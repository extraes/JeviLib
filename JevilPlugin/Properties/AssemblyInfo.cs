using MelonLoader;
using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;

[assembly: AssemblyTitle(JevilPlugin.BuildInfo.Name)]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany(JevilPlugin.BuildInfo.Company)]
[assembly: AssemblyProduct(JevilPlugin.BuildInfo.Name)]
[assembly: AssemblyCopyright("Created by " + JevilPlugin.BuildInfo.Author)]
[assembly: AssemblyTrademark(JevilPlugin.BuildInfo.Company)]
[assembly: AssemblyCulture("")]
[assembly: ComVisible(false)]
//[assembly: Guid("")]
[assembly: AssemblyVersion(JevilPlugin.BuildInfo.Version)]
[assembly: AssemblyFileVersion(JevilPlugin.BuildInfo.Version)]
[assembly: NeutralResourcesLanguage("en")]
[assembly: MelonInfo(typeof(JevilPlugin.JevilPlugin), JevilPlugin.BuildInfo.Name, JevilPlugin.BuildInfo.Version, JevilPlugin.BuildInfo.Author, JevilPlugin.BuildInfo.DownloadLink)]


// Create and Setup a MelonModGame to mark a Mod as Universal or Compatible with specific Games.
// If no MelonModGameAttribute is found or any of the Values for any MelonModGame on the Mod is null or empty it will be assumed the Mod is Universal.
// Values for MelonModGame can be found in the Game's app.info file or printed at the top of every log directly beneath the Unity version.
[assembly: MelonGame("Stress Level Zero", "BONELAB")]