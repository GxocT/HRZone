using System;
using System.Linq;
using System.Reflection;
using IPA.Loader;

namespace HRZone.Utils;

public static class Utils
{
    #region replay check

    private static readonly Lazy<MethodBase?> ScoreSaber_playbackEnabled = new(() =>
    {
        var meta = Plugin.Instance.ScoreSaberMeta;
        if (meta == null)
        {
            Plugin.Logger.Info("ScoreSaber is not installed or disabled");
            return null;
        }

        var method = meta.Assembly.GetType("ScoreSaber.Core.ReplaySystem.HarmonyPatches.PatchHandleHMDUnmounted")
            ?.GetMethod("Prefix", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

        if (method == null)
        {
            Plugin.Logger.Warn("ScoreSaber replay check method not found");
            return null;
        }

        return method;
    });

    private static readonly Lazy<MethodBase?> GetBeatLeaderIsStartedAsReplay = new(() =>
    {
        var meta = Plugin.Instance.BeatLeaderMeta;
        if (meta == null)
        {
            Plugin.Logger.Info("BeatLeader is not installed or disabled");
            return null;
        }

        var method = meta.Assembly.GetType("BeatLeader.Replayer.ReplayerLauncher")
            ?.GetProperty("IsStartedAsReplay", BindingFlags.Static | BindingFlags.Public)?.GetGetMethod(false);

        if (method == null)
        {
            Plugin.Logger.Warn("BeatLeader ReplayerLauncher.IsStartedAsReplay not found");
            return null;
        }

        return method;
    });

    internal static bool IsInSSReplay()
    {
        try
        {
            var method = ScoreSaber_playbackEnabled.Value;
            return method != null && (bool)method.Invoke(null, null) == false;
        }
        catch (Exception e)
        {
            Plugin.Logger.Warn($"ScoreSaber replay detection failed: {e}");
            return false;
        }
    }

    internal static bool IsInBLReplay()
    {
        try
        {
            var method = GetBeatLeaderIsStartedAsReplay.Value;
            return method != null && (bool)method.Invoke(null, null);
        }
        catch (Exception e)
        {
            Plugin.Logger.Warn($"BeatLeader replay detection failed: {e}");
            return false;
        }
    }

    internal static bool IsInReplay() => IsInSSReplay() || IsInBLReplay();

    #endregion

    internal static bool IsModEnabled(string id) => FindEnabledPluginMetadata(id) != null;

    internal static PluginMetadata? FindEnabledPluginMetadata(string id)
    {
        return PluginManager.EnabledPlugins.FirstOrDefault(x => x.Id == id);
    }
}
