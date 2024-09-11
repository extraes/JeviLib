using System.Reflection;
using UnityEngine;

namespace Jevil;

/// <summary>
/// Constant values used in multiple places throughout this library.
/// <br>They may be useful in serialization or other calculations.</br>
/// </summary>
public static class Const
{
    /// <summary>
    /// The size of a <see cref="UnityEngine.Vector3"/> (3 <see cref="float"/>s).
    /// <para>Used for serialization, for offsets in byte arrays.</para>
    /// </summary>
    public const int SizeV3 = sizeof(float) * 3;
    /// <summary>
    /// <see cref="System.Math.PI"/> pre-casted into <see cref="float"/>.
    /// <para>Used extensively in <see cref="Tweening"/> to make interpolation smooth.</para>
    /// </summary>
    public const float FPI = (float)System.Math.PI;
    /// <summary>
    /// All relevant binding flags (Public, NonPublic, Instance, Static)
    /// </summary>
    public const BindingFlags AllBindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
    /// <summary>
    /// The name of the GameObject <see cref="Instances.Player_RigManager"/> is on.
    /// <para>Useful with <see cref="Extensions.InHierarchyOf(UnityEngine.Transform, string)"/></para>
    /// </summary>
    public const string RigManagerName = "[RigManager (Blank)]";
    /// <summary>
    /// The name of the more commonly used URP Lit <see cref="Shader"/>.
    /// </summary>
    public const string UrpLitName = "Universal Render Pipeline/Lit (PBR Workflow)";
    /// <summary>
    /// The name of the SSR-supporting, volumetrics-rendering SLZ LitMAS <see cref="Shader"/>.
    /// </summary>
    public const string LITMAS_NAME = "SLZ/LitMAS/LitMAS Opaque";
    /// <summary>
    /// Because <see cref="Material.mainTexture"/> doesn't work on URP Lit, you must use <see cref="Material.GetTexture(string)"/> and <see cref="Material.SetTexture(string, Texture)"/> with this.
    /// </summary>
    public const string UrpLitMainTexName = "_BaseMap";
    /// <summary>
    /// The domain used for Mod Stats requests.
    /// <para>See: <see cref="ModStats.StatsEntry"/>.</para>
    /// </summary>
    public const string MOD_STATS_DOMAIN = "https://stats.extraes.xyz/";
    /// <summary>
    /// The shader property ID of <see cref="UrpLitMainTexName"/> calculated using <see cref="Shader.PropertyToID(string)"/>
    /// </summary>
    public static readonly int UrpLitMainTexID = Shader.PropertyToID(UrpLitMainTexName);
}