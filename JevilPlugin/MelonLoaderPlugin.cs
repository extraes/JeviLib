using HarmonyLib;
using Il2CppInterop.Generator.Passes;
using Jevil.Il2CppInterop.Generator.Passes;
using MelonLoader;
using MelonLoader.Utils;
using Mono.Cecil;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace JevilPlugin
{
    public static class BuildInfo
    {
        public const string Name = "JevilPlugin"; // Name of the Mod.  (MUST BE SET)
        public const string Author = "extraes"; // Author of the Mod.  (Set as null if none)
        public const string Company = null; // Company that made the Mod.  (Set as null if none)
        public const string Version = "1.0.0"; // Version of the Mod.  (MUST BE SET)
        public const string DownloadLink = null; // Download Link for the Mod.  (Set as null if none)
    }

    public class JevilPlugin : MelonPlugin
    {
        public JevilPlugin() : base() => instance = this;
        internal static JevilPlugin instance;

        public override void OnApplicationEarlyStart()
        {
            Type pass61 = typeof(Pass60AddImplicitConversions).Assembly.GetType("Il2CppInterop.Generator.Passes.Pass61ImplementAwaiters");

            bool needsRerunAsmGen = GetUniTaskInterfaceCount() == 0;
            
            if (!needsRerunAsmGen && !MelonLaunchOptions.Il2CppAssemblyGenerator.ForceRegeneration)
                return;

            if (pass61 is null)
            {
                Log("Patching assembly generation...");
                MethodInfo originalMethod = InfoOf(Pass60AddImplicitConversions.DoPass);
                HarmonyMethod postfix = new(InfoOf(Pass61ImplementAwaiters.DoPass));
                HarmonyInstance.Patch(originalMethod, postfix: postfix);
            }
            else
            {
                Log("Assembly generation does not need to be patched.");
            }

            Log("Forcing assembly regeneration...");
            Type asmGenOptions = typeof(MelonLaunchOptions.Il2CppAssemblyGenerator);
            PropertyInfo forceRegen = asmGenOptions.GetProperty(nameof(MelonLaunchOptions.Il2CppAssemblyGenerator.ForceRegeneration), BindingFlags.Public | BindingFlags.Static);
            forceRegen.SetValue(null, true);
        }

        MethodInfo InfoOf(Delegate d) => d.Method;

        static int GetUniTaskInterfaceCount()
        {
            string uniTaskPath = Path.Combine(MelonEnvironment.Il2CppAssembliesDirectory, "Il2CppUniTask.dll");
            if (!File.Exists(uniTaskPath))
                return 0;

            using AssemblyDefinition assembly = AssemblyDefinition.ReadAssembly(uniTaskPath);
            TypeDefinition uniTask = assembly.MainModule.GetType("Il2CppCysharp.Threading.Tasks.UniTask");
            TypeDefinition awaiter = uniTask.NestedTypes.FirstOrDefault(t => t.Name == "Awaiter");
#if DEBUG
            Log($"UniTask awaiter implements {awaiter.Interfaces.Count} interfaces.");
#endif
            return awaiter.Interfaces.Count;
        }

        #region MelonLogger replacements

        internal static void Log(string str) => instance.LoggerInstance.Msg(str);
        internal static void Log(object obj) => instance.LoggerInstance.Msg(obj?.ToString() ?? "null");
        internal static void Warn(string str) => instance.LoggerInstance.Warning(str);
        internal static void Warn(object obj) => instance.LoggerInstance.Warning(obj?.ToString() ?? "null");
        internal static void Error(string str) => instance.LoggerInstance.Error(str);
        internal static void Error(object obj) => instance.LoggerInstance.Error(obj?.ToString() ?? "null");

        #endregion
    }
}
