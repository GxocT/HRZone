using System;
using System.Net.Http;
using System.Text.RegularExpressions;
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
    [Inject]
    private readonly PluginConfig _config = null!;

    [Inject]
    private readonly Logger _logger = null!;

    private static readonly Regex PlainIntegerRegex = new("^\\d+$");

    private static readonly HttpClient HttpClient = new();

    private bool _updating;

    private string? _unusableFeedLink;

    protected override void Start()
    {
        _logger.Info("Starts updating HR");
        _updating = true;
        Task.Factory.StartNew(async () =>
        {
            _logger.Debug("Requesting HR data");

            while (_updating)
            {
                await UpdateHR();
                await Task.Delay(250);
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
            var res = await HttpClient.GetStringAsync(feedLink);
            _unusableFeedLink = null;
            if (TryParseHeartRate(res, _logger, out var hr) && _updating)
            {
                OnHeartRateDataReceived(hr);
            }
        }
        catch (HttpRequestException e)
        {
            _logger.Critical($"Failed to request HR: {e.Message}");
            _logger.Debug(e);
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
        }
    }
}
