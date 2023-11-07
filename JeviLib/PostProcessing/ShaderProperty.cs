using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Jevil.PostProcessing;

/// <summary>
/// Structure that makes it easier to identify and use shader properties on materials.
/// </summary>
/// <typeparam name="T"></typeparam>
public readonly struct ShaderProperty<T> where T : unmanaged
{
    private readonly int nameId;
    private readonly T startingValue;

    /// <summary>
    /// Creates a <see cref="ShaderProperty{T}"/> and assumes the initial value it represents to be it's C# default (Vector2/3/4.Zero, or just the number 0)
    /// </summary>
    /// <param name="propertyName"></param>
    public ShaderProperty(string propertyName)
    {
        nameId = Shader.PropertyToID(propertyName);
        startingValue = default;
    }

    /// <summary>
    /// Creates a <see cref="ShaderProperty{T}"/> and sets the initial value, in case <see cref="ResetOn(Material)"/> is ever called.
    /// </summary>
    /// <param name="propertyName"></param>
    /// <param name="getFrom"></param>
    public ShaderProperty(string propertyName, Material getFrom)
    {
        nameId = Shader.PropertyToID(propertyName);
        startingValue = GetValueFromMaterialImpl<T>(nameId, getFrom);
    }

    /// <summary>
    /// Sets a material's property back to the property's starting value
    /// <para>(as it was set at this <see cref="ShaderProperty{T}"/>'s construction, or the default value if this wasn't constructed with a material)</para>
    /// </summary>
    /// <param name="mat"></param>
    public void ResetOn(Material mat)
    {
        SetOn(mat, startingValue);
    }

    /// <summary>
    /// Retrieves a value from a material.
    /// </summary>
    /// <param name="mat">The material that holds the values to retrieve</param>
    /// <returns>The value that is set at <see cref="nameId"/> on <paramref name="mat"/>.</returns>
    public T GetFrom(Material mat)
    {
        return GetValueFromMaterialImpl<T>(nameId, mat);
    }

    /// <summary>
    /// Sets a value on <paramref name="mat"/>.
    /// </summary>
    /// <param name="mat">The material to be modified.</param>
    /// <param name="value">The value to set on the </param>
    /// <exception cref="NotSupportedException">This <see cref="ShaderProperty{T}"/> type <typeparamref name="T"/> is not used for materials.</exception>
    public void SetOn(Material mat, T value)
    {
        if (value is float vFloat)
        {
            mat.SetFloat(nameId, vFloat);
        }
        else if (value is int vInt)
        {
            mat.SetInteger(nameId, vInt);
        }
        else if (value is bool vBool)
        {
            mat.SetFloat(nameId, vBool ? 1 : 0);
        }
        else if (value is Color vColor)
        {
            mat.SetColor(nameId, vColor);
        }
        else if (value is Vector2 v2)
        {
            mat.SetVector(nameId, v2);
        }
        else if (value is Vector3 v3)
        {
            mat.SetVector(nameId, v3);
        }
        else if (value is Vector4 v4)
        {
            mat.SetVector(nameId, v4);
        }
        else throw new NotSupportedException($"Unsupported {nameof(ShaderProperty<T>)} type: {typeof(T).FullName}");
    }

    static TRet GetValueFromMaterialImpl<TRet>(int nameId, Material mat) where TRet : unmanaged
    {
        object holder;
        T value = default(T); // unlocks the powers of PATTERN MATCHING!
        if (value is float)
        {
            holder = mat.GetFloat(nameId);
        }
        else if (value is int)
        {
            holder = mat.GetInteger(nameId);
        }
        else if (value is bool)
        {
            holder = mat.GetFloat(nameId) != 0;
        }
        else if (value is Color)
        {
            holder = mat.GetColor(nameId);
        }
        else if (value is Vector2)
        {
            var vec = mat.GetVector(nameId);
            holder = new Vector2(vec.x, vec.y);
        }
        else if (value is Vector3)
        {
            var vec = mat.GetVector(nameId);
            holder = new Vector3(vec.x, vec.y, vec.z);
        }
        else if (value is Vector4)
        {
            holder = mat.GetVector(nameId);
        }
        else throw new NotSupportedException($"Unsupported Shader Property type: {typeof(TRet).FullName}");

        return (TRet)holder;
    }

    /// <summary>
    /// Creates a <see cref="ShaderProperty{T}"/> for the given property.
    /// </summary>
    /// <param name="name">Shader property name (e.g. "_MainTex", "_Color")</param>
    public static implicit operator ShaderProperty<T>(string name) => new(name);
}
