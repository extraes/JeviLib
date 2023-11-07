namespace Jevil.Unsafe;

public static partial class GlobalGameManagers
{
    /// <summary>
    /// enum ManagerContext::Managers : int32_t
    /// <para/> Taken from a decompiled UnityPlayer.dll with the UnityPlayer_Win64_player_il2cpp_x64.pdb debug file.
    /// </summary>
    public enum ManagerIndex : int
    {
        /// <summary>
        /// Instructs GetManagerFromContext to give you a <b>pointer to</b> PlayerSettings
        /// </summary>
        kPlayerSettings = 0x0,
        /// <summary>
        /// Instructs GetManagerFromContext to give you a <b>pointer to</b> the native InputManager
        /// </summary>
        kInputManager = 0x1,
        /// <summary>
        /// Instructs GetManagerFromContext to give you a <b>pointer to</b> the TagManager
        /// </summary>
        kTagManager = 0x2,
        /// <summary>
        /// Instructs GetManagerFromContext to give you a <b>pointer to</b> the AudioManager
        /// </summary>
        kAudioManager = 0x3,
        /// <summary>
        /// Instructs GetManagerFromContext to give you a <b>pointer to</b> the shader name registry
        /// </summary>
        kShaderNameRegistry = 0x4,
        /// <summary>
        /// Instructs GetManagerFromContext to give you a <b>pointer to</b> MonoManager. Unsure if this does anything on IL2CPP. It likely just manages user code (as in, code written by developers within the Unity project, not your MelonLoader code)
        /// </summary>
        kMonoManager = 0x5,
        /// <summary>
        /// Instructs GetManagerFromContext to give you a <b>pointer to</b> the native GraphicsSettings
        /// </summary>
        kGraphicsSettings = 0x6,
        /// <summary>
        /// Instructs GetManagerFromContext to give you a <b>pointer to</b> TimeManager
        /// </summary>
        kTimeManager = 0x7,
        /// <summary>
        /// Instructs GetManagerFromContext to give you a <b>pointer to</b> DelayedCallManager
        /// </summary>
        kDelayedCallManager = 0x8,
        /// <summary>
        /// Instructs GetManagerFromContext to give you a <b>pointer to</b> the PhysicsManager
        /// </summary>
        kPhysicsManager = 0x9,
        /// <summary>
        /// Instructs GetManagerFromContext to give you a <b>pointer to</b> BuildSettings.
        /// </summary>
        kBuildSettings = 0xa,
        /// <summary>
        /// Instructs GetManagerFromContext to give you a <b>pointer to</b> the native QualitySettings
        /// </summary>
        kQualitySettings = 0xb,
        /// <summary>
        /// Instructs GetManagerFromContext to give you a <b>pointer to</b> the ResourceManager
        /// </summary>
        kResourceManager = 0xc,
        /// <summary>
        /// Instructs GetManagerFromContext to give you a <b>pointer to</b> NavMeshProjectSettings
        /// </summary>
        kNavMeshProjectSettings = 0xd,
        /// <summary>
        /// Instructs GetManagerFromContext to give you a <b>pointer to</b> the 2D PhysicsSettings
        /// </summary>
        kPhysics2DSettings = 0xe,
        /// <summary>
        /// Instructs GetManagerFromContext to give you a <b>pointer to</b> the ClusterInputManager (i do not know what this is)
        /// </summary>
        kClusterInputManager = 0xf,
        /// <summary>
        /// Instructs GetManagerFromContext to give you a <b>pointer to</b> the RuntimeInitializeOnLoad manager
        /// </summary>
        kRuntimeInitializeOnLoadManager = 0x10,
        /// <summary>
        /// Instructs GetManagerFromContext to give you a <b>pointer to</b> UnityConnectSettings (because of course)
        /// </summary>
        kUnityConnectSettings = 0x11,
        /// <summary>
        /// Instructs GetManagerFromContext to give you a <b>pointer to</b> the StreamingManager
        /// </summary>
        kStreamingManager = 0x12,
        /// <summary>
        /// Instructs GetManagerFromContext to give you a <b>pointer to</b> the VFXManager
        /// </summary>
        kVFXManager = 0x13,
        /// <summary>
        /// Instructs GetManagerFromContext to give you a <b>pointer to</b> the GlobalManagerCount. What this is for I do not know, because all the managers are listed here in this enum.
        /// </summary>
        kGlobalManagerCount = 0x14,
        /// <summary>
        /// Instructs GetManagerFromContext to give you a <b>pointer to</b> the FirstLevelManager
        /// </summary>
        kFirstLevelManager = 0x14,
        /// <summary>
        /// Instructs GetManagerFromContext to give you a <b>pointer to</b> OcclusionCullingSettings
        /// </summary>
        kOcclusionCullingSettings = 0x14,
        /// <summary>
        /// Instructs GetManagerFromContext to give you a <b>pointer to</b> RenderSettings
        /// </summary>
        kRenderSettings = 0x15,
        /// <summary>
        /// Instructs GetManagerFromContext to give you a <b>pointer to</b> LightmapSettings
        /// </summary>
        kLightmapSettings = 0x16,
        /// <summary>
        /// Instructs GetManagerFromContext to give you a <b>pointer to</b> NavMeshSettings
        /// </summary>
        kNavMeshSettings = 0x17,
        /// <summary>
        /// Instructs GetManagerFromContext to give you a <b>pointer to</b> ManagerCount. Because this is somehow different from GlobalManagerCount? tf?
        /// </summary>
        kManagerCount = 0x18,
        /// <summary>
        /// Instructs GetManagerFromContext to give you a <b>pointer to</b> LevelGameManagerCount. Again, this is somehow different from GlobalManagerCount and ManagerCount?
        /// </summary>
        kLevelGameManagerCount = 0x4
    };


}
