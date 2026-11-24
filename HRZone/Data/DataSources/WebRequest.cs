using System;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using HRZone.Configuration;
using HRZone.Data.DataSources.Base;
using IPA.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Zenject;

namespace HRZone.Data.DataSources;

internal sealed class WebRequest : DataSource
{
    private const int CONNECT_ATTEMPTS = 4;
    private const int CONNECT_SPACING_MS = 3000;
    private const int POLL_INTERVAL_MS = 1000;
    private const int REQUEST_TIMEOUT_MS = 3000;

    [Inject]
    private readonly PluginConfig _config = null!;

    [Inject]
    private readonly Logger _logger = null!;

    private static readonly Regex PlainIntegerRegex = new("^\\d+$");

    private static readonly HttpClient HttpClient = new();

    private bool _updating;

    private string? _unusableFeedLink;

    private int _connectAttempts;

    private bool _connected;

    private bool _givenUp;

    protected override void Start()
    {
        _logger.Info("Starts updating HR");
        _updating = true;
        Task.Factory.StartNew(async () =>
        {
            _logger.Debug("Requesting HR data");

            while (_updating)
            {
                // A given-up source stays idle but alive: the poll loop must keep running
                // so a corrected FeedLink is noticed once the player edits it.
                if (!_givenUp)
                {
                    await UpdateHR();
                }

                await Task.Delay(_connected ? POLL_INTERVAL_MS : CONNECT_SPACING_MS);
            }
        });
    }

    protected override void Stop()
    {
        _updating = false;
    }

    internal static bool TryParseHeartRate(string body, Logger? logger, out int hr)
    {
        if (PlainIntegerRegex.IsMatch(body) && int.TryParse(body, out hr))
        {
            return true;
        }

        try
        {
            var bpmToken = JObject.Parse(body)["bpm"];
            if (bpmToken != null)
            {
                hr = bpmToken.ToObject<int>();
                return true;
            }

            logger?.Warn("Json received does not contain necessary field");
            logger?.Warn(body);
        }
        catch (JsonReaderException)
        {
            logger?.Critical($"Invalid json received: {body}");
        }

        hr = default;
        return false;
    }

    private async Task UpdateHR()
    {
        var feedLink = _config.FeedLink;

        if (feedLink == _unusableFeedLink)
        {
            // a relative or malformed link cannot start working on its own, so only retry once the player edits it
            return;
        }

        try
        {
            using var cts = new CancellationTokenSource();
            cts.CancelAfter(REQUEST_TIMEOUT_MS);
            var response = await HttpClient.GetAsync(feedLink, HttpCompletionOption.ResponseContentRead, cts.Token);
            var res = await response.Content.ReadAsStringAsync();
            _unusableFeedLink = null;
            if (TryParseHeartRate(res, _logger, out var hr))
            {
                if (!_connected)
                {
                    _connected = true;
                    _logger.Info($"Feed connected on connect attempt {_connectAttempts + 1}");
                }

                if (_updating)
                {
                    OnHeartRateDataReceived(hr);
                }
            }
            else
            {
                CountFailedConnectAttempt("response carried no valid heart rate");
            }
        }
        catch (OperationCanceledException)
        {
            CountFailedConnectAttempt($"request timed out after {REQUEST_TIMEOUT_MS} ms");
        }
        catch (HttpRequestException e)
        {
            _logger.Critical($"Failed to request HR: {e.Message}");
            _logger.Debug(e);
            CountFailedConnectAttempt("request failed");
        }
        catch (Exception e) when (e is InvalidOperationException or UriFormatException or NotSupportedException)
        {
            _unusableFeedLink = feedLink;
            _logger.Warn(string.IsNullOrEmpty(feedLink) ? "Request URI is not set" : $"Invalid request URI: {feedLink}");
        }
        catch (Exception e)
        {
            _logger.Warn($"Error Requesting HR data: {e.Message}");
            _logger.Warn(e);
            CountFailedConnectAttempt("request failed");
        }
    }

    // Network failures, timeouts and unparseable bodies are all failed attempts; skipped
    // unusable-URL polls never reach here, so an invalid URL can never exhaust the budget.
    private void CountFailedConnectAttempt(string reason)
    {
        if (_connected) return;

        _connectAttempts++;
        if (_connectAttempts < CONNECT_ATTEMPTS) return;

        _givenUp = true;
        _logger.Warn($"Giving up on the feed after {CONNECT_ATTEMPTS} failed connect attempts: {reason}; it stays unused until the next level");
    }
}
