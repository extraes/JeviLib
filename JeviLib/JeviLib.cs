using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Il2CppCysharp.Threading.Tasks;
using Il2CppSLZ.Marrow;
using Il2CppSLZ.Marrow.Audio;
using Il2CppSLZ.Marrow.SceneStreaming;
using Jevil.IMGUI;
using Jevil.Internal.Patching;
using Jevil.Patching;
using Jevil.PostProcessing;
using Jevil.Spawning;
using Jevil.Tweening;
using MelonLoader;
using UnityEngine;
using UnityEngine.SceneManagement;
using DebugDraw = Jevil.IMGUI.DebugDraw;
using HarmonyLib;
using MelonLoader.Utils;
using Il2CppOculus.Platform.Models;


#if !SELFCONTAINED
using BoneLib;
using BoneLib.BoneMenu;
using BoneLib.RandomShit;
#endif

namespace Jevil;

/// <summary>
/// The JeviLib <see cref="MelonMod"/> class. There's not much of note here.
/// </summary>
public class JeviLib : MelonMod
{
    /// <summary>
    /// Initializes a new instance of the <see cref="JeviLib"/> class.
    /// https://cdn.discordapp.com/attachments/646885826776793099/976272724324401172/IMG_2559.jpg
    /// </summary>
    public JeviLib() : base() => instance = this;
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    internal static JeviLib instance;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    internal readonly new Assembly Assembly = typeof(JeviLib).Assembly; // melonloader's favorite word is "Obsolete"

    internal static ConcurrentDictionary<string, ConcurrentBag<Assembly>> namespaceAssemblies = new();
    internal static event Action? onUpdateCallback;
    internal static event Action? onNamespaceAssembliesCompleted;
    internal static int unityMainThread;

    static readonly ConcurrentQueue<string> toLog = new();
    static Stopwatch mainThreadInvokeTimer = new();
    static Task<string>? nsCacheTask;

    /// <summary>
    /// Gets a value indicating whether the asynchronously-built map of namespaces to assemblies is done being created. JeviLib waits for this to finish in its OnInitializeMelon method.
    /// <para><see cref="Utilities.GetTypeFromString(string, string)"/> will fail if this is <see langword="false"/>.</para>
    /// </summary>
    public static bool DoneMappingNamespacesToAssemblies { get; private set; }

    /// <summary>
    /// Determines whether methods that <i>must</i> use external mod deps (and aren't fully removed because of that) should throw <see cref="NotImplementedException"/>s.
    /// </summary>
    public static bool SelfContainedThrowNotImplemented { get; private set; }


#if false // only break glass in case of emergency: aka when patching starts crashing the game for unknown reasons
    static FieldInfo fi1 = typeof(PatchProcessor).GetField("instance", Const.AllBindingFlags);
    static FieldInfo fi2 = typeof(PatchProcessor).GetField("original", Const.AllBindingFlags);
    static void HarmonyPatchPrefix(PatchProcessor __instance)
    {
#if !DEBUG
    #error remove this, dumbass
#endif

        Log("fi1 " + fi1);
        Log("fi2 " + fi2);
        HarmonyLib.Harmony harmony = (HarmonyLib.Harmony)fi1.GetValue(__instance)!;
        MethodBase mb = (MethodBase)fi2.GetValue(__instance)!;
        Log($"Harmony instance with ID '{harmony.Id}' is patching {mb.FullDescription()}");
    }
#endif

    /// <summary>
    /// https://media.discordapp.net/attachments/919014401187643435/958026151383691344/freeze-1.gif
    /// </summary>
    public void _OnEarlyInitializeMelon()
    {
#if DEBUG
        // launch debugger because, for some inane reason, melonloader doesnt thoroughly test jack shit
        bool launchDbg = MelonLaunchOptions.Core.IsDebug
                      && typeof(MelonEnvironment).Assembly.GetName().Version?.ToString() == "0.6.4.0"
                      && !Utilities.IsPlatformQuest();
        
        Log("ML isDbg: " + MelonLaunchOptions.Core.IsDebug);
        Log("ML ver: " + typeof(MelonEnvironment).Assembly.GetName().Version?.ToString() ?? "null");
        Log("On Quest: " + Utilities.IsPlatformQuest());
        Log("Need launch dbgr: " + launchDbg);
        Log(Debugger.IsAttached);
        if (launchDbg && !Debugger.IsAttached)
        {
            Debugger.Launch();
        }
#endif

        Stopwatch sw = Stopwatch.StartNew();
        
#if DEBUG
        Stopwatch submoduleInitSW = Stopwatch.StartNew();
        //HarmonyInstance.Patch(typeof(PatchProcessor).GetMethod(nameof(PatchProcessor.Patch)), prefix: Utilities.ToHarmony(HarmonyPatchPrefix));

        DebugDraw.InitTokens();
#endif

        if (Utilities.IsPlatformQuest())
            AndroidAsyncUnfucker.Init();

        HarmonyInstance.PatchAll();

        Barcodes.Init();

        PostProcessingManager.Init();

        Ungovernable.Init();

#if DEBUG
        submoduleInitSW.Stop();
        LoggerInstance.Msg(System.ConsoleColor.Blue, $"JeviLib submodules initialized in {submoduleInitSW.ElapsedMilliseconds}ms");
#endif

        nsCacheTask = Task.Run(this.GetNamespaces);

#if DEBUG && !SELFCONTAINED
        Hooking.OnLevelLoaded += (li) => { OnSceneWasInitialized(-1, li.barcode); };
#endif

        sw.Stop();
        LoggerInstance.Msg(System.ConsoleColor.Blue, $"Pre-initialized {nameof(JeviLib)} v{JevilBuildInfo.VERSION}{(JevilBuildInfo.DEBUG ? " Debug (Development)" : "")} in {sw.ElapsedMilliseconds}ms");
    }

    /// <summary>
    /// Initializes JeviLib
    /// </summary>
    public override void OnInitializeMelon()
    {
        _OnEarlyInitializeMelon();

        Stopwatch sw = Stopwatch.StartNew();

#if DEBUG
        Log("This version of " + nameof(JeviLib) + " has been built with the DEBUG compiler flag!");
        Log("Functionality will remain in tact for the most part, however there will be extra log points to warn you if there is anything worrying about your usage of the library.");
        Log("You should only be using this build if you create code mods, and not if you simply use mods. Do not rely on the extra checks in this build, or require the use of a debug build for your production code.");
#endif

        if (nsCacheTask is not null)
        {
            if (!nsCacheTask.IsCompleted)
                Log("Waiting for namespace assembly cache task to complete.");

            string nsCacheLog = nsCacheTask.GetAwaiter().GetResult();

            Log(nsCacheLog);
        }
        else
        {
            Log("Namespace cache task is null... What?");
        }

#if SELFCONTAINED
        try
        {
            Hook.OntoMethod(typeof(RigManager).GetMethod(nameof(RigManager.OnEnable), BindingFlags.Public | BindingFlags.Instance) ?? throw new MissingMethodException("RigManager.OnEnable"), () => { OnSceneWasInitialized(-1, SceneStreamer.Session.Level.name); });
        }
        catch (Exception ex)
        {
            Error("Exception while initializing RigManager.OnEnable patch: " + ex);
        }
#endif

#if DEBUG

#if !SELFCONTAINED
        Log("Creating BoneMenu for jevil postprocess testing...");
        var mcat = Page.Root.CreatePage("Test Jevil PostFX (debug only)", Color.white);

        foreach (Type postproc in typeof(SharedPostProcessingMaterials).GetNestedTypes())
        {
            Log("Creating FunctionElements for " + postproc.FullName);
            foreach (MethodInfo method in postproc.GetMethods())
            {
                if (method.Name.Contains("able"))
                    mcat.CreateFunction(method.Name + " " + postproc.Name, Color.white, () => method.Invoke(null, Array.Empty<object>()));
            }
        }
#endif

        SharedPostProcessingMaterials.Depth.DepthPow.SetOn(SharedPostProcessingMaterials.Depth.Material, 1);
        SharedPostProcessingMaterials.Depth.DepthMult.SetOn(SharedPostProcessingMaterials.Depth.Material, 1);
        SharedPostProcessingMaterials.Depth.UseColor.SetOn(SharedPostProcessingMaterials.Depth.Material, false);
        SharedPostProcessingMaterials.Pixelate.PixelsPerAxis.SetOn(SharedPostProcessingMaterials.Pixelate.Material, 100);
#endif

        CreateNeverCancel();

        LoggerInstance.Msg("Device memory statistics:");
        LoggerInstance.Msg(" - Total memory: " + SystemInfo.systemMemorySize);
        LoggerInstance.Msg(" - Used memory (will likely spike when game starts): " + Process.GetCurrentProcess().PeakWorkingSet64 / 1024 / 1024);
        LoggerInstance.Msg(System.ConsoleColor.Blue, $"Completed initialization of {nameof(JeviLib)} v{JevilBuildInfo.VERSION}{(JevilBuildInfo.DEBUG ? " Debug" : "")} in {sw.ElapsedMilliseconds}ms");
    }

    private static void CreateNeverCancel()
    {
        // Initialize NeverCollect/NeverCancel for generic Tweens
        GameObject go = new(nameof(NeverCollect));
        NeverCollect nc = go.AddComponent<NeverCollect>();
        go.Persist();
        nc.Persist();
        Instances.NeverCancel = go;
    }

    /// <summary>
    /// Update things that are always changing, like Tweens.
    /// </summary>
    public override void OnUpdate()
    {
        onUpdateCallback.InvokeSafeSync();
        Tweener.UpdateAll();

        if (!AsyncUtilities.mainThreadCallbacks.IsEmpty)
        {
            while (AsyncUtilities.mainThreadCallbacks.TryDequeue(out var mte))
            {
                try
                {
                    mte.execute();
                }
                catch (Exception ex)
                {
                    Error("Exception while executing a main-thread callback");
                    Error(ex);
                    if (mte.completer.UnsafeGetStatus() != UniTaskStatus.Pending) continue;

                    mte.completer.exception = new(Il2CppSystem.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(new Il2CppSystem.Exception(ex.ToString())));
                }

                mte.completer.TrySetResult();
            }
        }

        mainThreadInvokeTimer.Restart();
    }

    ///// <summary>
    ///// kjr
    ///// </summary>
    ///// <param name="buildIndex"></param>
    ///// <param name="sceneName"></param>
    //public override void OnSceneWasLoaded(int buildIndex, string sceneName)
    //{
    //    Log($"OSWL CALLED, PASSING TO OSWI: PARAMS: IDX={buildIndex}, NAME={sceneName}");
    //    OnSceneWasInitialized(buildIndex, sceneName);
    //}

    /// <summary>
    /// Update references in <see cref="Instances"/>
    /// </summary>
    public override void OnSceneWasInitialized(int buildIndex, string sceneName)
    {
        //return;
#if DEBUG
        Log($"OSWI CALLED: PARAMS: IDX={buildIndex}, NAME={sceneName}");
#endif
        if (!Instances.Player_RigManager.INOC()) return;

        Waiting.WaitForSceneInit.currSceneIdx = SceneManager.GetActiveScene().buildIndex;
        foreach (IDictionary item in Instances.instanceCachesToClear)
        {
            item.Clear();
        }
#if DEBUG
        Log("Cleared instance caches!");

        Stopwatch sw = Stopwatch.StartNew();
#endif
        // this should obviously never happen, but IL2CPP (and Unity 2021.3.5 i guess) is a whore that never stops sucking
        if (Instances.NeverCancel.INOC())
            CreateNeverCancel();

        // Grab the necessary references when the scene starts. 
        Instances.Player_RigManager =
            GameObject.FindObjectsOfType<Il2CppSLZ.Marrow.RigManager>().FirstOrDefault(r => r.gameObject.scene != default)!;
        if (Instances.Player_RigManager.INOC())
            return;
        Instances.Player_BodyVitals =
            GameObject.FindObjectOfType<Il2CppSLZ.Bonelab.BodyVitals>();
        Instances.Player_PhysicsRig =
            Instances.Player_RigManager.physicsRig;
        Instances.Player_Health =
            GameObject.FindObjectOfType<Il2CppSLZ.Marrow.Player_Health>();
        Instances.Audio2dManager =
            GameObject.FindObjectOfType<Audio2dManager>();
        Instances.MusicMixer = //todo: get mixer names from runtime game
            Instances.Audio2dManager.mixer.FindMatchingGroups("Music").First();
        Instances.SFXMixer =
            Instances.Audio2dManager.mixer.FindMatchingGroups("SFX").First();
        // Separate cameras because it's better this way, I think. It's more distinguishable even if it requires two lines to keep the two "in sync"
        Instances.RigCameras =
            GameObject.FindObjectsOfType<Camera>().Where(c => c.transform.IsChildOfRigManager()).ToArray();
        Instances.SpectatorCam =
            Instances.RigCameras.FirstOrDefault(c => c.name == "Spectator Camera");
        Instances.InHeadsetCam =
            Instances.RigCameras.FirstOrDefault(c => c.name == "Head");

#if SELFCONTAINED
        Transform pHead = Instances.Player_PhysicsRig.m_head;
#else
        Transform pHead = Player.Head;
#endif

        GameObject musicPlayer = new("JeviLib Music Player");
        musicPlayer.transform.parent = pHead.transform;
        Instances.MusicPlayer = musicPlayer.AddComponent<AudioPlayer>();
        Instances.MusicPlayer._source = musicPlayer.AddComponent<AudioSource>();
        Instances.MusicPlayer.source.outputAudioMixerGroup = Instances.MusicMixer;
        Instances.MusicPlayer._defaultVolume = 0.1f;
        Instances.MusicPlayer.source.volume = 0.1f;
        Instances.MusicPlayer.enabled = true;

        GameObject sfxPlayer = new("JeviLib SFX Player");
        sfxPlayer.transform.parent = pHead.transform;
        Instances.SFXPlayer = sfxPlayer.AddComponent<AudioPlayer>();
        Instances.SFXPlayer._source = sfxPlayer.AddComponent<AudioSource>();
        Instances.SFXPlayer.source.outputAudioMixerGroup = Instances.SFXMixer;
        Instances.SFXPlayer._defaultVolume = 0.25f;
        Instances.SFXPlayer.source.volume = 0.25f;
        Instances.SFXPlayer.enabled = true;

#if DEBUG
        Log("Found our instances in " + sw.ElapsedMilliseconds + "ms.");
#endif
    }

#if DEBUG
    /// <summary>
    /// Draw <see cref="GUIToken"/>s from <see cref="DebugDraw"/>.
    /// <para>Only exists in debug builds.</para>
    /// </summary>
    public override void OnGUI()
    {
        DebugDraw.PerformDraw();
    }
#endif

    private async Task<string> GetNamespaces()
    {
        Log("Getting namespaces from assemblies now");
        Stopwatch sw = Stopwatch.StartNew();

        Assembly[] asms = AppDomain.CurrentDomain.GetAssemblies();
        Assembly[][] assemblies = asms.SplitByProcessors().Select(e => e.ToArray()).ToArray();
#if DEBUG
        Log($"Getting namespaces from " + assemblies.Sum(a => a.Length) + " assemblies, split across " + assemblies.Length + " threads");
#endif

        Thread[] threads = new Thread[assemblies.Length];

        for (int i = 0; i < threads.Length; i++)
        {
            Thread dictThread = new(this.PopulateDictionary_ThreadStart)
            {
                IsBackground = true
            };
            dictThread.Start(assemblies[i]);
            threads[i] = dictThread;
        }
#if DEBUG
        Log($"Split list & started threads in {sw.ElapsedMilliseconds} ms.");
#endif
        while (threads.Any(t => t.ThreadState != System.Threading.ThreadState.Stopped)) await Task.Yield();

        sw.Stop();

        DoneMappingNamespacesToAssemblies = true;
        onNamespaceAssembliesCompleted.InvokeSafeParallel();
        return $"Cached all {namespaceAssemblies.Count} namespaces and their respective assemblies in {sw.ElapsedMilliseconds}ms";
    }

    private void PopulateDictionary_ThreadStart(object? obj) => this.PopulateDictionary((Assembly[])obj!);

    private void PopulateDictionary(Assembly[] section)
    {
        string currAsmTitle = string.Empty;
        try
        {
            for (int i = 0; i < section.Length; i++)
            {
                Assembly currentAsm = section[i];
                currAsmTitle = currentAsm.FullName ?? "<unnamed>";
                if (currAsmTitle.Contains("JeviLib")) continue; // causes quest enumerator wrapper to fail
                if (currentAsm.IsDynamic) continue; // avoid exceptions from Redirect and Hooking
                
                Type[] types = currentAsm.GetTypes();

                IEnumerable<string> namespaces = types.Select(t => t.Namespace).NoNull().Distinct();

                foreach (string ns in namespaces)
                {
                    ConcurrentBag<Assembly>? asms;
                    if (!namespaceAssemblies.TryGetValue(ns ?? "", out asms))
                    {
                        asms = new ConcurrentBag<Assembly>();
                        namespaceAssemblies.TryAdd(ns ?? "", asms);
                    }
                    asms.Add(currentAsm);
                }
            }
        }
        catch (Exception ex)
        {
#if DEBUG
            // apparently jevilib is throwing System.TypeLoadException and IDFK why
            Error("Caught exception while populating namespace dictionary for assembly " + currAsmTitle, ex);
#endif
        }
    }

    private static IEnumerable<MethodInfo> GetLogMethods()
    {
        IEnumerable<MethodInfo> logMethods = new List<MethodInfo>();
        string[] methodnames = { nameof(MelonLogger.Msg), nameof(MelonLogger.Warning), nameof(MelonLogger.Error), };
        Type[] types = { typeof(MelonLogger), typeof(MelonLogger.Instance) };

        foreach (Type type in types)
        {
            foreach (string method in methodnames)
            {
                logMethods = logMethods.Concat(type.GetMethods(method));
            }
        }
        return logMethods;
    }

    #region MelonLogger replacements

    internal static void Log(string str, ConsoleColor conCol = System.ConsoleColor.Gray) => instance.LoggerInstance.Msg(conCol, str);
    internal static void Log(object obj, ConsoleColor conCol = System.ConsoleColor.Gray) => instance.LoggerInstance.Msg(conCol, obj?.ToString() ?? "null");
    internal static void Warn(string str) => instance.LoggerInstance.Warning(str);
    internal static void Warn(object obj) => instance.LoggerInstance.Warning(obj?.ToString() ?? "null");
    internal static void Error(string str) => instance.LoggerInstance.Error(str);
    internal static void Error(object obj) => instance.LoggerInstance.Error(obj?.ToString() ?? "null");
    internal static void Error(string str, Exception ex) => instance.LoggerInstance.Error(str ?? "null", ex);

    #endregion

#if DEBUG
    private void NotifiedLowRAM()
    {
        JeviLib.Warn("BONELAB has been notified that your system has very little free RAM left! Double check that! Consider uninstalling some items!");
    }
#endif
}