using Semver;
using System;

namespace Jevil;

/// <summary>
/// Values defined to warn your users to update their JeviLib version if necessary
/// </summary>
public static class JevilBuildInfo
{
    internal const string NAME = "JeviLib"; // Name of the Mod.  (MUST BE SET)
    internal const string AUTHOR = "extraes"; // Author of the Mod.  (Set as null if none)
    internal const string COMPANY = null; // Company that made the Mod.  (Set as null if none)

    /// <summary>
    /// The version of JeviLib your mod will be built against.
    /// </summary>
    public const string VERSION = "2.3.0";

    /// <summary>
    /// The Thunderstore page for JeviLib. I'm not sure why you'd need this but go for it.
    /// </summary>
    public const string DOWNLOAD_LINK = "https://bonelab.thunderstore.io/package/extraes/JeviLibBL/";

    /// <summary>
    /// Whether your mod was built with a debug version of JeviLib (it should be).
    /// </summary>
    public const bool DEBUG
#if DEBUG
        = true;
#else
        = false;
#endif
    /// <summary>
    /// Whether your mod was built with a version of JeviLib that was built with a self-contained (no external mod deps) configuration.
    /// </summary>
    public const bool SELFCONTAINED
#if SELFCONTAINED
        = true;
#else
        = false;
#endif

    /// <summary>
    /// The version of JeviLib your mod is running with at runtime. This may be out of date due to users not updating dependencies.
    /// </summary>
    public static string RuntimeVersion() => VERSION;

    /// <summary>
    /// Whether your mod is running with a debug version of JeviLib.
    /// </summary>
    public static bool RuntimeDebug() => DEBUG;

    /// <summary>
    /// If <paramref name="version"/> >= <see cref="VERSION"/>.
    /// <br>The suggested use case is something like: <c>if (!RuntimeVersionGreaterThanOrEqual(VERSION)) WarnUserThatJeviLibIsOutdated();</c></br>
    /// </summary>
    /// <param name="version"></param>
    /// <returns><see langword="true"/> if the currently executing JeviLib version is more recent or the same as the one given, <see langword="false"/> if it's outdated.</returns>
    public static bool RuntimeVersionGreaterThanOrEqual(string version)
    {
        SemVersion compiledVer = SemVersion.Parse(version);
        SemVersion runtimeVer = SemVersion.Parse(VERSION);

        return runtimeVer >= compiledVer;
    }

    /// <summary>
    /// Whether your mod is running with a self-contained version of JeviLib.
    /// </summary>
    public static bool RuntimeSelfContained() => SELFCONTAINED;
}
