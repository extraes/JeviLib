using HarmonyLib;
using MelonLoader;
using MelonLoader.Utils;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

namespace Jevil.Internal.Patching;

internal static partial class Ungovernable
{
    static MelonLogger.Instance MethodEntryLogger = new("JevilProfiler");
    static readonly HarmonyMethod LogPrefix = Jevil.Utilities.ToHarmony(LogMethodEntry);
    static readonly HarmonyMethod LogPostfix = Jevil.Utilities.ToHarmony(LogMethodExit);
    static readonly HashSet<Assembly> PatchedAsms = new();
    static readonly Dictionary<Assembly, StreamWriter> writers = new();
    static readonly ConcurrentQueue<string> logs = new();
    static readonly Thread logThread = new(() =>
    {
        while (true)
        {
            if (logs.TryDequeue(out string? log))
                writers[JeviLib.instance.Assembly].WriteLine(log);
        }
    });

    private static void LogMethodEntry()
    {
        // this is so ass and should be replaced with a dynamic method implementation, but I do not care atm.
        StackTrace st = new(1);
        MethodBase? originalMethod = st.GetFrame(0)?.GetMethod();
        Assembly? assembly = originalMethod?.DeclaringType?.Assembly;

#if DEBUG
        if (originalMethod?.DeclaringType?.Assembly is not null && !PatchedAsms.Contains(originalMethod.DeclaringType.Assembly))
            Debugger.Break();
#endif
        logs.Enqueue($"{originalMethod?.DeclaringType?.Name}.{originalMethod?.Name} called.");
        //MethodEntryLogger.Msg($"{originalMethod?.DeclaringType?.Name}.{originalMethod?.Name} called.");
    }

    private static void LogMethodExit()
    {
        StackTrace st = new(1);
        MethodBase? originalMethod = st.GetFrame(0)?.GetMethod();
        Assembly? assembly = originalMethod?.DeclaringType?.Assembly;
        logs.Enqueue(originalMethod.FullDescription() + " exited.");
        //MethodEntryLogger.Msg(originalMethod.FullDescription() + " exited.");
    }

    private static StreamWriter CreateLogFileFor(Assembly asm)
    {
        return File.CreateText(Path.Join(MelonEnvironment.MelonLoaderLogsDirectory, (asm.GetName().Name ?? "unk") + ".log"));
    }

    private static void PatchAllMethods(Assembly asm)
    {
        if (logThread.ThreadState != System.Threading.ThreadState.Running)
            logThread.Start();
        PatchedAsms.Add(asm);
        if (!writers.ContainsKey(JeviLib.instance.Assembly))
            writers[JeviLib.instance.Assembly] = CreateLogFileFor(JeviLib.instance.Assembly);

#if DEBUG
        int counter = 0;
#endif
        foreach (Type type in asm.GetTypes())
        {
            foreach (MethodInfo item in type.GetMethods(Const.AllBindingFlags | BindingFlags.DeclaredOnly))
            {
                try
                {
                    if (item.IsGenericMethod || type.IsGenericType || type.IsAbstract || type.IsInterface || type.Assembly != asm)
                        continue;
#if DEBUG
                    JeviLib.Log($"Patching {item.FullDescription()} from {item?.DeclaringType?.Assembly.FullName ?? "KYSKYSKYSKYS"}");
#endif

                    Harmony.Patch(item, prefix: LogPrefix, postfix: LogPostfix);
#if DEBUG
                    counter++;
#endif
                }
                catch (Exception e)
                {
                    JeviLib.Warn("Failed to patch method " + item.FullDescription() + " in " + asm.FullName + " - " + e);
                }
            }
        }
#if DEBUG
        JeviLib.Log("Harmony-surrounded " + counter + " method(s) in " + asm.FullName);
#endif
    }
}
