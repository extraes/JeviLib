# Type definitions for GlobalGameManager derivatives

As found from BinaryNinja's parsing of UnityPlayer.dll and its associated PDB from Unity 2021.3.5f1's editor folder.

Not everything'll be in here, as I don't have a need for everything. 

# PlayerSettings
```cpp
class __base(GlobalGameManager, 0) PlayerSettings
{
    struct PlayerSettings::VTable* vtable;
    __inherited int32_t Object::m_InstanceID;
    __inherited union
    {
        uint32_t m_MemLabelIdentifier;
        uint32_t m_TemporaryFlags;
        uint32_t m_HideFlags;
        uint32_t m_IsPersistent;
        uint32_t m_CachedTypeIndex;
    } __bitfieldc;
    __inherited struct EventEntry* Object::m_EventIndex;
    __inherited class ScriptingGCHandle Object::m_MonoReference;
    struct UnityGUID productGUID;
    class core::basic_string<char,core::StringStorageDefault<char> > cloudProjectId;
    uint8_t cloudEnabled;

    class core::basic_string<char,core::StringStorageDefault<char> > projectName;
    class core::basic_string<char,core::StringStorageDefault<char> > organizationId;
    class core::basic_string<char,core::StringStorageDefault<char> > companyName;
    class core::basic_string<char,core::StringStorageDefault<char> > productName;
    class core::basic_string<char,core::StringStorageDefault<char> > version;
    struct PlayerSettingsSplashScreen m_SplashScreenSettings;
    class PPtr<Texture2D> m_HolographicTrackingLossScreen;
    class PPtr<Texture2D> defaultCursor;
    class Vector2f cursorHotspot;
    uint8_t androidProfiler;
    int32_t defaultScreenOrientation;
    int32_t targetDevice;
    uint8_t androidFilterTouchesWhenObscured;
    uint8_t androidEnableSustainedPerformanceMode;
    uint8_t useOnDemandResources;
    int32_t accelerometerFrequency;
    int32_t defaultScreenWidth;
    int32_t defaultScreenHeight;
    int32_t defaultWebScreenWidth;
    int32_t defaultWebScreenHeight;
    int32_t displayResolutionDialog;
    struct AspectRatios m_SupportedAspectRatios;
    int32_t m_StereoRenderingPath;
    int32_t m_ActiveColorSpace;
    uint8_t m_MTRendering;
    uint8_t m_MobileMTRenderingBaked;

    struct Hash128 m_MobileApplicationIdentifierHash;
    struct dynamic_array<int,0> m_StackTraceTypes;
    int32_t androidShowActivityIndicatorOnLoading;
    int32_t iosShowActivityIndicatorOnLoading;
    int32_t androidBlitType;
    uint8_t iosUseCustomAppBackgroundBehavior;
    uint8_t iosAllowHTTPDownload;
    uint8_t uiAutoRotateToPortrait;
    uint8_t uiAutoRotateToPortraitUpsideDown;
    uint8_t uiAutoRotateToLandscapeRight;
    uint8_t uiAutoRotateToLandscapeLeft;
    uint8_t uiUseAnimatedAutoRotation;
    uint8_t uiUse32BitDisplayBuffer;
    uint8_t uiDisableDepthAndStencilBuffers;
    uint8_t uiPreserveFramebufferAlpha;
    uint8_t defaultIsNativeResolution;
    uint8_t macRetinaSupport;
    uint8_t runInBackground;
    uint8_t resetResolutionOnWindowResize;
    uint8_t captureSingleScreen;
    uint8_t muteOtherAudioSources;
    uint8_t prepareIOSForRecording;
    uint8_t forceIOSSpeakersWhenRecording;
    uint8_t hideHomeButton;
    int32_t deferSystemGesturesMode;
    uint8_t submitAnalytics;
    uint8_t usePlayerLog;
    uint8_t bakeCollisionMeshes;
    uint8_t visibleInBackground;
    uint8_t allowFullscreenSwitch;
    int32_t macFullscreenMode;
    int32_t d3d11FullscreenMode;
    enum FullscreenMode fullscreenMode;
    uint8_t forceSingleInstance;
    uint8_t useFlipModelSwapchain;
    uint8_t resizableWindow;
    uint8_t gpuSkinning;
    uint8_t xboxPIXTextureCapture;
    uint8_t xboxEnableAvatar;
    uint8_t xboxEnableKinect;
    uint8_t xboxEnableKinectAutoTracking;
    uint32_t xboxSpeechDB;
    uint8_t xboxEnableFitness;
    uint8_t xboxEnableHeadOrientation;
    uint8_t xboxEnableGuest;
    uint8_t xboxEnablePIXSampling;
    uint8_t metalFramebufferOnly;
    uint8_t vulkanEnableSetSRGBWrite;
    uint32_t vulkanNumSwapchainBuffers;
    uint8_t vulkanEnableLateAcquireNextImage;
    uint8_t vulkanEnablePreTransform;
    uint8_t vulkanEnableCommandBufferRecycling;
    uint8_t mipStripping;
    int32_t numberOfMipsStripped;
    uint8_t virtualTexturingSupportEnabled;
    uint8_t useMacAppStoreValidation;

    class core::basic_string<char,core::StringStorageDefault<char> > macAppStoreCategory;
    int32_t xboxOneResolution;
    int32_t xboxOneSResolution;
    int32_t xboxOneXResolution;
    int32_t xboxOneMonoLoggingLevel;
    int32_t xboxOneLoggingLevel;
    uint8_t xboxOneDisableEsram;
    uint8_t xboxOneEnableTypeOptimization;
    uint32_t xboxOnePresentImmediateThreshold;
    int32_t switchQueueCommandMemory;
    int32_t switchQueueControlMemory;
    int32_t switchQueueComputeMemory;
    int32_t switchNVNShaderPoolsGranularity;
    int32_t switchNVNDefaultPoolsGranularity;
    int32_t switchNVNOtherPoolsGranularity;
    int32_t switchNVNMaxPublicTextureIDCount;
    int32_t switchNVNMaxPublicSamplerIDCount;
    int32_t stadiaPresentMode;
    int32_t stadiaTargetFramerate;

    class core::basic_string<char,core::StringStorageDefault<char> > absoluteURL;
    class core::basic_string<char,core::StringStorageDefault<char> > bundleVersion;
    class core::basic_string<char,core::StringStorageDefault<char> > AndroidLicensePublicKey;
    struct dynamic_array<PPtr<Object>,0> preloadedAssets;
    enum WSAInputSource metroInputSource;
    uint8_t wsaTransparentSwapchain;
    uint8_t m_HolographicPauseOnTrackingLoss;
    uint8_t xboxOneDisableKinectGpuReservation;
    uint8_t xboxOneEnable7thCore;
    struct VRSettings vrSettings;
    uint8_t isWsaHolographicRemotingEnabled;
    uint8_t enableFrameTimingStats;
    uint8_t enableOpenGLProfilerGPURecorders;
    uint8_t useHDRDisplay;
    enum D3DHDRDisplayBitDepth D3DHDRBitDepth;

    struct dynamic_array<int,0> m_ColorGamuts;
    int32_t activeInputHandler;
    int32_t targetPixelDensity;
    int32_t resolutionScalingMode;
    int32_t androidSupportedAspectRatio;
    float androidMaxAspectRatio;
    uint8_t androidStartInFullscreen;
    uint8_t androidRenderOutsideSafeArea;
    uint8_t androidUseSwappy;
    uint8_t androidResizableWindow;
    int32_t androidDefaultWindowWidth;
    int32_t androidDefaultWindowHeight;
    int32_t androidMinimumWindowWidth;
    int32_t androidMinimumWindowHeight;
    enum FullscreenMode androidFullscreenMode;
    enum OpenGLESVersion playerMinOpenGLESVersion;
    enum MemorylessMode framebufferDepthMemorylessMode;

    struct dynamic_array<core::basic_string<char,core::StringStorageDefault<char> >,0> qualitySettingsNames;
    uint8_t legacyClampBlendShapeWeights;

    class core::basic_string<char,core::StringStorageDefault<char> > playerDataPath;
    uint8_t forceSRGBBlit;
};
```
