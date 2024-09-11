using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using MethodBody = Mono.Cecil.Cil.MethodBody;

namespace Jevil.Patching;

internal static class SupportModuleOverwriter
{
    public static Action<string> Log;
    public static Action<string> Error;

    /// <summary>
    /// SUMMARYING A PRIVATE MEMBER, I KNOW
    /// <br>CURRSMPATH IS IMPLICITLY INSIDE [BASEDIR]/USERDATA/JEVILSM</br>
    /// <br>IF THIS IS WRONG, THE PROGRAM WILL FAIL EXECUTION</br>
    /// </summary>
    public static void Execute(string newSmPath, string currSmPath)
    {

        return;
#if DEBUG
        if (Log == null || Error == null)
            throw new NullReferenceException("Logging callbacks cannot be null.");
#endif

        Log($"Current support module path: {currSmPath}");
        Log($"Going to write support module to: {newSmPath}");

        string jsmFolder = Path.GetDirectoryName(newSmPath);
        string backupsFolder = Path.Combine(Path.GetDirectoryName(newSmPath), "Backups");
        string backupFile = Path.Combine(backupsFolder, "Il2Cpp.bak.dll");
        using (FileStream dllWriteStream = File.Create(newSmPath))
        {
            Assembly asm = Assembly.GetExecutingAssembly();
            string resourcePath = asm.GetManifestResourceNames().First(p => p.EndsWith("Resources.Il2Cpp.dll"));
            using Stream stream = asm.GetManifestResourceStream(resourcePath);
            stream.CopyTo(dllWriteStream);
        }

        if (!Directory.Exists(jsmFolder))
        {
            Log("Creating new JevilSM folder in UserData");
            Directory.CreateDirectory(jsmFolder);
        }

        Log("Wrote new support module to disk in working directory at " + newSmPath);
        if (!Directory.Exists(backupsFolder))
        {
            Directory.CreateDirectory(backupsFolder);
            Log("Created backup directory " + backupsFolder);
        }
        Log("Backing up current support module to " + backupFile);
        File.Copy(currSmPath, backupFile, true);
        Log("Successfully wrote support module backup.");

        Log("Replacing enumerator wrapper...");
        MemoryStream newSupportModule = GetAssemblyStream(newSmPath);
        Log("New support module successfully replaced and written to in-memory stream.");
        
        Log("Overwriting the support module at " + currSmPath);
        File.WriteAllBytes(currSmPath, newSupportModule.ToArray());
        // VVV DOESNT WORK!!! currSmOverwritten DOESNT RECIEVE ANY DATA!!! WHY? I DONT KNOW!!!
        //using (FileStream currSmOverwritten = File.OpenWrite(currSmPath))
        //{
        //    newSupportModule.CopyTo(currSmOverwritten);
        //    currSmOverwritten.Flush();
        //}
        Log("Wrote new IL2CPP support module.");
    }

    // this is only abstracted into a method so i can easily add some cecil if needed
    private static MemoryStream GetAssemblyStream(string newSmPath)
    {
        MemoryStream memoryStream = new();
        File.OpenRead(newSmPath).CopyTo(memoryStream);
        return memoryStream;
    }
}
