using Mono.Cecil;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jevil.Patching;

internal static class UnityCoreModuleCeciler
{
    public static Action<string> Log;
    public static Action<string> Error;

    public static void Execute(string unityCorePath, string il2cppMscorlibPath, string userData)
    {

        //todo
        return;
        Log("UnityEngine CoreModule path: " + unityCorePath);
        Log("IL2CPP MSCorlib path: " + il2cppMscorlibPath);
        
        string backupsFolder = Path.Combine(userData, "JevilSM", "Backups");
        string unityBackupFile = Path.Combine(backupsFolder, "UnityEngine.CoreModule.bak.dll");
        string mscorlibBackupFile = Path.Combine(backupsFolder, "Il2cppmscorlib.bak.dll");

        if (!Directory.Exists(backupsFolder))
        {
            Directory.CreateDirectory(backupsFolder);
            Log("Created backup directory " + backupsFolder);
        }
        Log("Backing up current UnityEngine.CoreModule.dll to " + unityBackupFile);
        File.Copy(unityCorePath, unityBackupFile, true);
        Log("Successfully wrote UnityEngine.CoreModule.dll backup.");

        Log("Backing up current Il2Cppmscorlib.dll to " + mscorlibBackupFile);
        File.Copy(unityCorePath, mscorlibBackupFile, true);
        Log("Successfully wrote Il2Cppmscorlib.dll backup.");

        Log("Reading UnityEngine.CoreModule...");
        AssemblyDefinition unityAsm = AssemblyDefinition.ReadAssembly(unityCorePath);
        ModuleDefinition unityModDef = unityAsm.MainModule;
        Log("Read successfully!");

        Log("Reading IL2CPP MSCorlib...");
        AssemblyDefinition mscorAsm = AssemblyDefinition.ReadAssembly(il2cppMscorlibPath);
        ModuleDefinition mscorModDef = mscorAsm.MainModule;
        Log("Read successfully!");

        Log("Iterating type definitions...");
        int count = 0;
        foreach (TypeDefinition type in unityModDef.Types.Concat(mscorModDef.Types))
        {
            foreach (MethodDefinition method in type.Methods)
            {
                bool isToString = method.Name == "ToString" && method.Parameters.Count == 0;
                bool isGetHashCode = method.Name == "GetHashCode" && method.Parameters.Count == 0;
                bool isEquals = method.Name == "Equals" && method.Parameters.Count == 1;
                if (isToString || isGetHashCode || isEquals)
                {
                    Log("Setting virtual flag to true on method: " + method.FullName);
                    method.IsVirtual = true;
                    count++;
                }
            }
        }
        Log($"Finished iterating type definitions. Modified {count} types/methods.");

        Log("Writing Unity core assembly to memory...");
        using MemoryStream ms = new();
        unityAsm.Write(ms);
        unityAsm.Dispose();
        Log("Success! Now writing to disk...");
        File.WriteAllBytes(unityCorePath, ms.ToArray());
        Log("Success!");

        Log("Writing Unity core assembly to memory...");
        using MemoryStream ms2 = new();
        mscorAsm.Write(ms2);
        mscorAsm.Dispose();
        Log("Success! Now writing to disk...");
        File.WriteAllBytes(il2cppMscorlibPath, ms2.ToArray());
        Log("Success!");
    }
}
