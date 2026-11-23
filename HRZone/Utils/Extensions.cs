using System.Diagnostics;
using IPA.Logging;

namespace HRZone.Utils;

public static class Extensions
{
    [Conditional("DEBUG")]
    internal static void Spam(this Logger logger, string s)
    {
        logger.Trace(s);
    }
}
