using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Jevil;

/// <summary>
/// <see cref="ArrayPool{T}"/> made specifically for <see langword="byte"/>s, with special helpers for serializing/deserializing data.
/// </summary>
public static class ByteArrayPool
{
    static List<byte[]> vectorArrays = new();

    /// <summary>
    /// Represents a fixed-length vector-sized array.
    /// <para>See <see cref="Const.SizeV3"/></para>.
    /// </summary>
    /// <param name="Rented">A rented vector-sized array.</param>
    public record struct RentedVector(byte[] Rented) : IDisposable
    {
        /// <summary>
        /// Returns the rented array
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            ReturnVector(Rented);
        }
    }

    /// <summary>
    /// Rents a byte array that is just large enough to store a Vector3, or 3 floats.
    /// </summary>
    /// <returns>A byte array with the same length as <see cref="Const.SizeV3"/>.</returns>
    public static byte[] RentVector()
    {
        if (vectorArrays.Count == 0)
            return new byte[Const.SizeV3];

        byte[] arr = vectorArrays[0];
        vectorArrays.RemoveAt(0);
        return arr;
    }

    /// <summary>
    /// Returns a struct that, when used with C#'s <see langword="using"/> feature, will ensure your rented array is returned when it is no longer in use.
    /// </summary>
    /// <returns>A <see cref="RentedVector"/> scope.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static RentedVector RentVectorTemp()
    {
        return new(RentVector());
    }

    /// <summary>
    /// Returns a rented vector-sized array.
    /// </summary>
    /// <param name="vectorArray">An array rented from <see cref="RentVector"/>. If the array's length is not <see cref="Const.SizeV3"/>, an exception will be thrown (IN DEBUG BUILDS!)</param>
    /// <exception cref="Exception">Only present in debug builds. The array was not of the right length.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ReturnVector(byte[] vectorArray)
    {
#if DEBUG
        if (vectorArray.Length != Const.SizeV3)
            throw new Exception("Cannot return an array that is not the right size! This method is only intended to be used to return arrays that have been rented using RentVector.");
        if (vectorArrays.Contains(vectorArray))
            throw new Exception("That array has already been returned! Do not double-return an array!");
#endif
        vectorArrays.Add(vectorArray);
    }

    /// <summary>
    /// Retrieves an array to be used temporarily.
    /// Remember to call <see cref="Return(byte[])"/>.
    /// </summary>
    /// <param name="length">The desired length, or minimum length, of the array to be returned.</param>
    /// <param name="allowOversized">Whether or not it's okay to return an array that is longer than the requested size.</param>
    /// <returns></returns>
    public static byte[] Rent(int length, bool allowOversized = false)
    {
        return ArrayPool<byte>.Rent(length, allowOversized);
    }

    /// <summary>
    /// Returns a <see cref="ArrayPool{T}.RentScope"/> that, when used with C#'s <see langword="using"/> scope, will be used to <see cref="Return(byte[])"/> the array automatically.
    /// </summary>
    /// <param name="length">The desired length, or minimum length, of the array to be returned.</param>
    /// <param name="allowOversized">Whether or not it's okay to return an array that is longer than the requested size.</param>
    /// <returns></returns>
    public static ArrayPool<byte>.RentScope RentTemp(int length, bool allowOversized = false)
    {
        return ArrayPool<byte>.RentTemp(length, allowOversized);
    }

    /// <summary>
    /// Returns a byte array to the <see cref="ArrayPool{T}"/>. This is a direct call in release builds.
    /// </summary>
    /// <param name="nonVectorArray">A byte array. If it is the size of a Vector3, debug builds will log a warning.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Return(byte[] nonVectorArray)
    {
#if DEBUG
        if (nonVectorArray.Length == Const.SizeV3)
        {
            JeviLib.Warn($"This array is the size of a Vector3. Did you mean to use {nameof(ReturnVector)}? For info, see call stack below:");
            JeviLib.Warn(new StackTrace(1));
        }
#endif
        ArrayPool<byte>.Return(nonVectorArray);
    }
}
