using MelonLoader;
using MelonLoader.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Jevil.Patching;

internal static class AssemblyPatcher
{
    public static void Init()
    {
        if (Utilities.IsPlatformQuest())
        {
            PersistentFixQuest();
            
            if (Utilities.CoroutinesNeedPatch() || Utilities.UniTasksNeedPatch())
                OneTimeFixQuest();
        }
        else
        {
            OneTimeFixWindows();
        }
    }

    static void PersistentFixQuest()
    {
        try
        {
            //QuestEnumeratorRewrapper.Init();
        }
        catch (Exception ex)
        {
            JeviLib.Warn("Exception while initializing harmony-based coroutine fix: " + ex);
        }
    }

    static void OneTimeFixQuest()
    {
        JeviLib.Warn("Since you're on Quest, JeviLib will attempt a live-update of mod files");

        // need to MANUALLY FUCKING LOAD cecil.rocks because MONO ANDROID DOESNT LOAD IT, THE DUMB CUNT
        string _cecilLocation = typeof(Mono.Cecil.AssemblyDefinition).Assembly.Location;
        string _cecilRocksLocation = Path.Combine(Path.GetDirectoryName(_cecilLocation)!, "Mono.Cecil.Rocks.dll");
        JeviLib.Log("Loading Cecil.Rocks from " + _cecilRocksLocation);
        byte[] _cecilRocksAsm = File.ReadAllBytes(_cecilRocksLocation);
        Assembly.Load(_cecilRocksAsm);

        string userDataFolder = MelonEnvironment.UserDataDirectory;
        string jsmPath = Path.Combine(userDataFolder, "JevilSM");
        string logPath = Path.Combine(jsmPath, "FixerLog.log");
        string newSmPath = Path.Combine(jsmPath, "Il2Cpp.dll");
        string unitaskPath = Path.Combine(userDataFolder, "..", "melonloader", "etc", "managed", "UniTask.dll");
        string il2mscorlibPath = Path.Combine(userDataFolder, "..", "melonloader", "etc", "managed", "Il2Cppmscorlib.dll");
        string unhollowerBasePath = Path.Combine(userDataFolder, "..", "melonloader", "etc", "managed", "UnhollowerBaseLib.dll");
        string unityCoreModulePath = Path.Combine(userDataFolder, "..", "melonloader", "etc", "managed", "UnityEngine.CoreModule.dll");
        if (!Directory.Exists(jsmPath))
        {
            JeviLib.Log("Created JevilSM directory: " + jsmPath);
            Directory.CreateDirectory(jsmPath);
        }

        JeviLib.Log("MelonLoader assembly updating can now begin.");
        JeviLib.Log("JeviLib custom support module does not work properly on Android. A harmony based solution has been implemented instead");

        if (!typeof(Vector3).GetMethod("ToString", Array.Empty<Type>())!.IsVirtual)
        {
            JeviLib.Log("UnityEngine CoreModule modification can now begin");
            throw new NotImplementedException();
            //todo
            //UnityCoreModuleCeciler.Log = (str) => JeviLib.Log("UnityCoreModuleCeciler: " + str);
            //UnityCoreModuleCeciler.Error = (str) => JeviLib.Error("UnityCoreModuleCeciler: " + str);
            //UnityCoreModuleCeciler.Execute(unityCoreModulePath, il2mscorlibPath, userDataFolder);
            JeviLib.Log("UnityEngine CoreModule modification finished successfully");
            JeviLib.Log("Critical mod files have been modified. It is recommended you restart your game.");
        }

        if (Utilities.UniTasksNeedPatch())
        {
            JeviLib.Log("UniTask modification can now begin.");
            throw new NotImplementedException();
            //todo
            //UniTaskCeciler.Log = (str) => JeviLib.Log("UniTaskCeciler: " + str);
            //UniTaskCeciler.Error = (str) => JeviLib.Error("UniTaskCeciler: " + str);
            //UniTaskCeciler.Execute(unitaskPath, il2mscorlibPath, unhollowerBasePath, MelonEnvironment.UserDataDirectory);
            JeviLib.Log("UniTask modification has finished.");
            JeviLib.Log("Critical mod files have been modified. It is recommended you restart your game.");
        }
    }

    private static void OneTimeFixWindows()
    {
        bool unitasksNeedFixing = Utilities.UniTasksNeedPatch();
        bool enumeratorsNeedFixing = Utilities.CoroutinesNeedPatch();

        if (unitasksNeedFixing)
            JeviLib.Warn("UniTasks need fixing!");
        if (enumeratorsNeedFixing)
            JeviLib.Warn("MonoEnumeratorWrapper needs fixing!");

        if (unitasksNeedFixing || enumeratorsNeedFixing)
            JeviLib.Log("Starting fork process to watch for fixes!");
        else return;

        string jevilSmFolder = Path.Combine(MelonUtils.UserDataDirectory, "JevilSM");
        if (!Directory.Exists(jevilSmFolder)) Directory.CreateDirectory(jevilSmFolder);

        string file = Path.Combine(jevilSmFolder, "JevilFixer.exe");
        JeviLib.instance.MelonAssembly.Assembly.UseEmbeddedResource("Jevil.Resources.ForkProcess.exe", bytes => File.WriteAllBytes(file, bytes));
        JeviLib.Log($"Wrote fork process to " + file);

        Process fork = new()
        {
            StartInfo = new()
            {
                Arguments = MelonUtils.UserDataDirectory,
                CreateNoWindow = !JevilBuildInfo.DEBUG,
                UseShellExecute = true,
                FileName = file,
            },
        };
        fork.Exited += (s, e) => { JeviLib.Log("Fork process exited. This is likely to properly fork itself."); };
        fork.Start();
        JeviLib.Log($"Fork process started with PID {fork.Id}");
    }
}
