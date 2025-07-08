using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Jevil.PostProcessing;

/// <summary>
/// Represents a custom texture that will be passed into all postprocessing shaders, with a given property ID.
/// </summary>
public readonly struct GlobalTextureDescriptor
{
    static List<int> inUseIds = new();

    internal readonly int propertyId;
    internal readonly Texture tex;
    internal readonly string name;

    /// <summary>
    /// Creates a <see cref="GlobalTextureDescriptor"/>.
    /// </summary>
    /// <param name="propertyId">Shader property ID, given from <see cref="Shader.PropertyToID(string)"/></param>
    /// <param name="tex">The texture to be passed into postprocessing shaders</param>
    /// <exception cref="ArgumentNullException">The texture is null</exception>
    public GlobalTextureDescriptor(int propertyId, Texture tex)
    {
        if (tex == null) throw new ArgumentNullException(nameof(tex));
        this.propertyId = propertyId;
        this.tex = tex;
        this.name = tex.name;
    }

    /// <summary>
    /// Creates a <see cref="GlobalTextureDescriptor"/>.
    /// </summary>
    /// <param name="propertyName">Shader property ID, will be passed into <see cref="Shader.PropertyToID(string)"/></param>
    /// <param name="tex">The texture to be passed into postprocessing shaders</param>
    /// <exception cref="ArgumentNullException">The texture is null</exception>
    public GlobalTextureDescriptor(string propertyName, Texture tex)
    {
        if (tex == null) throw new ArgumentNullException(nameof(tex));
        this.propertyId = Shader.PropertyToID(propertyName);
        this.tex = tex;
        this.name = tex.name;
    }

    internal bool IsValid => tex != null && propertyId != 0;
}

