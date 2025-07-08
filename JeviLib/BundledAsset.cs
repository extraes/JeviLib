using Il2CppCysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Il2CppInterop.Runtime;
using UnityEngine;

namespace Jevil;

/// <summary>
/// Represents a bundled asset, with its path coming from the bundle manifest/project path.
/// <para>Using this class, your asset can be lazy-loaded, cached, loaded asynchronously, or loaded synchronously.</para>
/// </summary>
/// <typeparam name="T">Any unity object. Not recommended to be a component, but a <see cref="Material"/> or <see cref="GameObject"/> is fine.</typeparam>
public sealed class BundledAsset<T> where T : UnityEngine.Object
{
#if DEBUG
    static Dictionary<AssetBundle, string[]> assetPaths = new(UnityObjectComparer<AssetBundle>.Instance);
#endif

    /// <summary>
    /// The path which holds an object.
    /// </summary>
    public readonly string path;

    AssetBundle? bundle;
    readonly bool hide;
    T? asset;

    /// <summary>
    /// Creates a <see cref="BundledAsset{T}"/> reference and optionally immediately loads it.
    /// </summary>
    /// <param name="bundle">The assetbundle that contains the asset.</param>
    /// <param name="path">The asset's path within the <paramref name="bundle"/>.</param>
    /// <param name="hideWhenPersisting">Sets the object's <see cref="HideFlags"/> to <see cref="HideFlags.HideAndDontSave"/>. This is recommended for prefabs you're going to clone.</param>
    /// <param name="loadImmediately">Immediately loads the asset, blocking the main thread.</param>
    public BundledAsset(AssetBundle bundle, string path, bool hideWhenPersisting = true, bool loadImmediately = false)
    {
#if DEBUG
        if (bundle == null) throw new ArgumentNullException(nameof(bundle));
        if (string.IsNullOrWhiteSpace(path)) throw new ArgumentException("Asset path cannot be null, empty, or only whitespace.", nameof(path));
#endif

        this.bundle = bundle;
        this.path = path;
        this.hide = hideWhenPersisting;

        bundle.Persist();

        if (loadImmediately) Get();
    }

    /// <summary>
    /// Creates a bundled asset reference without binding it to an AssetBundle.
    /// <para>You must call <see cref="Bind(AssetBundle, bool)"/> or <see cref="BindAsync(AssetBundle)"/> before trying to retrieve the asset.</para>
    /// </summary>
    /// <param name="path">The asset's path within an AssetBundle.</param>
    /// <param name="hideWhenPersisting">Sets the object's <see cref="HideFlags"/> to <see cref="HideFlags.HideAndDontSave"/>. This is recommended for prefabs you're going to clone.</param>
    public BundledAsset(string path, bool hideWhenPersisting = true)
    {
        this.path = path;
        this.hide = hideWhenPersisting;
    }

    /// <summary>
    /// Binds a bundled asset to the given <see cref="AssetBundle"/>.
    /// </summary>
    /// <param name="bundle">The <see cref="AssetBundle"/> that contains an asset of type <typeparamref name="T"/> at the previously given path.</param>
    /// <param name="loadImmediately">Whether or not to immediately load the asset.</param>
    public void Bind(AssetBundle bundle, bool loadImmediately = false)
    {
#if DEBUG
        if (this.bundle) JeviLib.Warn("It's not recommended to live-switch the AssetBundle reference of a bundled asset! Asset path: " + path);
        if (bundle == null) throw new ArgumentNullException(nameof(bundle));
#endif

        this.bundle = bundle;
        bundle.Persist();

        if (loadImmediately) Get();
    }

    /// <summary>
    /// Assigns an <see cref="AssetBundle"/> reference to an asset and immediately begins loading the asset.
    /// </summary>
    /// <param name="bundle">The <see cref="AssetBundle"/> that contains an asset of type <typeparamref name="T"/> at the previously given path.</param>
    /// <returns>An asynchronous operation for the loading of the asset.</returns>
    public Task<T> BindAsync(AssetBundle bundle)
    {
#if DEBUG
        if (this.bundle) JeviLib.Warn("It's not recommended to live-switch the AssetBundle reference of a bundled asset! Asset path: " + path);
        if (bundle == null) throw new ArgumentNullException(nameof(bundle));
#endif

        this.bundle = bundle;
        bundle.Persist();

        return GetAsync();
    }

    /// <summary>
    /// If the asset is loaded, it gets immediately returned. If not, it gets loaded in a manner that blocks the main thread.
    /// </summary>
    /// <returns>Your loaded asset.</returns>
    public T Get()
    {
        if (asset == null)
        {
#if DEBUG
            if (bundle == null) throw new InvalidOperationException("BundledAsset be associated with an existing assetbundle! You have either failed to bind an AssetBundle to this asset or the provided AssetBundle was collected! Asset path: " + path);
#endif

            asset = bundle.LoadAsset(path).Cast<T>();

            asset.Persist(hide);
        }

        return asset;
    }

    /// <summary>
    /// Retrieves an asset asynchronously, or returns it immediately if you 
    /// </summary>
    /// <returns>Your loaded asset.</returns>
    public async Task<T> GetAsync()
    {
        if (asset == null)
        {
#if DEBUG
            if (bundle == null) 
                throw new InvalidOperationException("BundledAsset be associated with an existing assetbundle! You have either failed to bind an AssetBundle to this asset or the provided AssetBundle was collected! Asset path: " + path);
            if (!GetPaths().Contains(path.ToLower()))
            {
                foreach(string str in GetPaths())
                    JeviLib.Log($"Bundle {bundle.name} - {str}");

                throw new InvalidOperationException($"Assetbundle '{bundle.name}' does not contain any asset at path {path} - Logged all asset paths");
            }
#endif

            var abr = bundle.LoadAssetAsync(path);
            await abr.ToUniTask();
#if DEBUG
            if (abr.asset.TryCast<T>() == null)
            {
                throw new InvalidCastException($"Asset type {abr.asset.GetIl2CppType().FullName} does not match {typeof(T).FullName}");
            }
#endif
            asset = abr.asset.Cast<T>();

            asset.Persist(hide);
        }

        return asset;
    }

#if DEBUG
    string[] GetPaths()
    {
        if (bundle == null)
            throw new InvalidOperationException("AssetBundle is null! Cannot retrieve asset paths.");

        if (assetPaths.TryGetValue(bundle, out string[]? paths)) return paths;
        {
            paths = bundle.GetAllAssetNames();
            assetPaths.Add(bundle, paths);
        }

        return paths;
    }
#endif

    /// <summary>
    /// Converts a <see cref="BundledAsset{T}"/> to its loaded counterpart.
    /// <para>This will block the main thread if the asset has not been loaded yet.</para>
    /// </summary>
    /// <param name="bundledAsset">The bundled asset reference being converted.</param>
    public static implicit operator T(BundledAsset<T> bundledAsset)
    {
        return bundledAsset.Get();
    }
}
