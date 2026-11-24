using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using HRZone.Configuration;
using HRZone.Data.DataSources;
using IPA.Utilities.Async;
using UnityEngine;
using Zenject;
using IPALogger = IPA.Logging.Logger;

namespace HRZone;

internal enum MenuFeedStatus
{
    Disabled,
    NotConfigured,
    Failed,
    HasValue,
}

// Menu-scoped feed poller shared by the menu display and the Server tab preview.
// The Menu container is never disposed entering a level (verified on a live install), so the
// loop pauses on this GameObject's activeInHierarchy — the menu scene goes inactive in a level —
// to keep exactly one request stream at the configured URL. ModEnable is read on every iteration
// like FeedLink, so turning the mod off in the settings screen stops the stream right away.
internal sealed class MenuHRFeed : MonoBehaviour, IInitializable, IDisposable
{
    private const int POLL_INTERVAL_MS = 1000;
    private const int REQUEST_TIMEOUT_MS = 3000;

    private static readonly HttpClient HttpClient = new();

    [Inject]
    private readonly PluginConfig _config = null!;

    [Inject]
    private readonly IPALogger _logger = null!;

    private CancellationTokenSource? _cts;

    private string? _unusableFeedLink;

    private bool _paused;

    public MenuFeedStatus Status { get; private set; } = MenuFeedStatus.NotConfigured;

    public int? CurrentHR { get; private set; }

    // false until the first request outcome (or a not-configured check) has been published,
    // so consumers can tell "no state yet" from the initial NotConfigured snapshot
    public bool HasState { get; private set; }

    public event Action<MenuFeedStatus, int?>? StateChanged;

    public void Initialize()
    {
        _logger.Info("Menu feed poller started");
        var cts = new CancellationTokenSource();
        _cts = cts;
        _ = Task.Run(() => PollLoop(cts.Token));
    }

    public void Dispose()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;
    }

    private async Task PollLoop(CancellationToken token)
    {
        try
        {
            while (!token.IsCancellationRequested)
            {
                if (!await IsMenuSceneActive(token))
                {
                    if (!_paused)
                    {
                        _paused = true;
                        _logger.Info("Menu scene inactive, menu feed polling paused");
                    }
                }
                else
                {
                    if (_paused)
                    {
                        _paused = false;
                        _logger.Info("Menu scene active, menu feed polling resumed");
                    }

                    if (!_config.ModEnable) Publish(MenuFeedStatus.Disabled, null);
                    else await RequestOnce(token);
                }

                await Task.Delay(POLL_INTERVAL_MS, token);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.Trace("Menu feed polling stopped");
        }
    }

    private async Task<bool> IsMenuSceneActive(CancellationToken token)
    {
        var tcs = new TaskCompletionSource<bool>();
        _ = UnityMainThreadTaskScheduler.Factory.StartNew(() => tcs.TrySetResult(gameObject.activeInHierarchy), token);
        return await tcs.Task;
    }

    private async Task RequestOnce(CancellationToken loopToken)
    {
        var url = _config.FeedLink;

        if (string.IsNullOrEmpty(url))
        {
            // an empty URL must never issue a request and never log
            Publish(MenuFeedStatus.NotConfigured, null);
            return;
        }

        if (url == _unusableFeedLink)
        {
            // a relative or malformed link cannot start working on its own: it is logged once,
            // retried only when the player edits it
            return;
        }

        try
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(loopToken);
            cts.CancelAfter(REQUEST_TIMEOUT_MS);
            using var response = await HttpClient.GetAsync(url, HttpCompletionOption.ResponseContentRead, cts.Token);
            var body = await response.Content.ReadAsStringAsync();
            _unusableFeedLink = null;

            if (WebRequest.TryParseHeartRate(body, null, out var hr))
            {
                Publish(MenuFeedStatus.HasValue, hr);
            }
            else
            {
                _logger.Debug($"Menu feed response from {url} carried no valid heart rate");
                Publish(MenuFeedStatus.Failed, null);
            }
        }
        catch (OperationCanceledException) when (!loopToken.IsCancellationRequested)
        {
            _logger.Debug($"Menu feed request to {url} timed out after {REQUEST_TIMEOUT_MS} ms");
            Publish(MenuFeedStatus.Failed, null);
        }
        catch (OperationCanceledException)
        {
            // the menu scope went away mid-request
        }
        catch (Exception e) when (e is InvalidOperationException or UriFormatException or NotSupportedException)
        {
            _unusableFeedLink = url;
            _logger.Warn($"Invalid menu feed URI: {url}");
            Publish(MenuFeedStatus.NotConfigured, null);
        }
        catch (HttpRequestException e)
        {
            _logger.Debug($"Menu feed request failed: {e.Message}");
            Publish(MenuFeedStatus.Failed, null);
        }
        catch (Exception e)
        {
            _logger.Warn($"Error requesting menu feed: {e.Message}");
            Publish(MenuFeedStatus.Failed, null);
        }
    }

    private void Publish(MenuFeedStatus status, int? hr)
    {
        if (HasState && Status == status && CurrentHR == hr) return;

        UnityMainThreadTaskScheduler.Factory.StartNew(() =>
        {
            Status = status;
            CurrentHR = hr;
            HasState = true;
            StateChanged?.Invoke(status, hr);
        }, CancellationToken.None);
    }
}
