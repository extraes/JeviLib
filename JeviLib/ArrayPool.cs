using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Text;
using System.Threading.Tasks;
using Tomlet.Exceptions;

namespace Jevil;

/// <summary>
/// Class used to pool arrays, to prevent unnecessary memory allocation.
/// </summary>
/// <typeparam name="T">Any type.</typeparam>
public static class ArrayPool<T>
{
    /// <summary>
    /// A disposible struct used to ensure an array gets returned after it is rented.
    /// </summary>
    /// <param name="Rented">The array that was rented from the current <see cref="ArrayPool{T}"/></param>
    /// <param name="Length">The length that was requested. This may not align with the <see cref="Rented"/> array's actual length.</param>
    public record struct RentScope(T[] Rented, int Length) : IDisposable 
    {
        /// <summary>
        /// Returns the rented array
        /// </summary>
        public void Dispose()
        {
            Return(Rented);
        }
    }


    static List<T[]> allArrays = new();
    static List<T[]> freeArrays = new();

    /// <summary>
    /// Retrieves an array to be used temporarily.
    /// Remember to call <see cref="Return(T[])"/>.
    /// </summary>
    /// <param name="length">The desired length, or minimum length, of the array to be returned.</param>
    /// <param name="allowOversized">Whether or not it's okay to return an array that is longer than the requested size.</param>
    /// <returns></returns>
    public static T[] Rent(int length, bool allowOversized = false)
    {
        int retIdx = BinarySearch(freeArrays, length);

        if (retIdx == freeArrays.Count)
        {
            T[] array = CreateArray(length);
            return array;
        }

        T[] ret = freeArrays[retIdx];

        if (!allowOversized && ret.Length != length)
        {
            T[] array = CreateArray(length);
            return array;
        }

        freeArrays.RemoveAt(retIdx);
        return ret;
    }

    /// <summary>
    /// Returns an array to the ArrayPool
    /// </summary>
    public static void Return(T[] arr)
    {
#if DEBUG
        if (!allArrays.Contains(arr))
            throw new InvalidOperationException("Do not return an array you have created! Only return rented arrays!");
        if (freeArrays.Contains(arr))
            throw new InvalidOperationException("You cannot return an array that hasn't been rented, or you can't return an array twice!");
#endif
        int idx = BinarySearch(freeArrays, arr.Length);
        InsertOrAppend(freeArrays, idx, arr);
    }

    /// <summary>
    /// Returns a <see cref="RentScope"/> that, when used with C#'s <see langword="using"/> scope, will be used to <see cref="Return(T[])"/> the array automatically.
    /// </summary>
    /// <param name="length">The desired length, or minimum length, of the array to be returned.</param>
    /// <param name="allowOversized">Whether or not it's okay to return an array that is longer than the requested size.</param>
    /// <returns></returns>
    public static RentScope RentTemp(int length, bool allowOversized = false)
    {
        T[] arr = Rent(length, allowOversized);
        return new RentScope(arr, length);
    }

    /// <summary>
    /// I wrote this binary search implementation from memory and it fucking worked first try, so I should write some docs about the behavior I've found it exhibits.
    /// <para>This will return the length of <paramref name="orderedList"/> if <paramref name="desiredLength"/> is too large for the elements in <paramref name="orderedList"/>, so its output cannot be fed directly into an indexer or into .Insert(...).</para>
    /// <para>Similarly, it will return 0 if the items in <paramref name="orderedList"/> are too large.</para>
    /// <para>If there are items sized 6 and 8 elements long, and <paramref name="desiredLength"/> is 7, this method will return the index of the 8-length element.</para>
    /// </summary>
    /// <param name="orderedList">The list in order. Debug builds will pre-check for order.</param>
    /// <param name="desiredLength">The minimum (inclusive) length array to look for in the list.</param>
    /// <returns></returns>
    static int BinarySearch(List<T[]> orderedList, int desiredLength)
    {
#if DEBUG
        int len = orderedList.FirstOrDefault()?.Length ?? 0;
        foreach (T[] arr in orderedList)
        {
            if (arr.Length > len)
                len = arr.Length;
            else if (arr.Length < len)
            {
                JeviLib.Error("BinarySearch can only be performed on a list that is in order! The list will be sorted to rectify this.");
                JeviLib.Error($"(Don't worry, unless you're {JevilBuildInfo.NAME}'s developer, {JevilBuildInfo.AUTHOR}, this isn't your fault, but it should be reported)");
                JeviLib.Error("Stack trace: " + new System.Diagnostics.StackTrace(1));
                orderedList.Sort(ArrayLengthComparer.Instance);
                break;
            }
        }
#endif
        return BinarySearchImpl(orderedList, desiredLength, 0, orderedList.Count);
    }

    static int BinarySearchImpl(List<T[]> orderedList, int desiredLength, int startIdx, int endIdx)
    {
        if (startIdx == endIdx)
            return startIdx;

        int lookAtIdx = (startIdx + endIdx) / 2;
        T[] arr = orderedList[lookAtIdx];
        if (arr.Length > desiredLength)
            return BinarySearchImpl(orderedList, desiredLength, startIdx, lookAtIdx);
        if (arr.Length < desiredLength)
            return BinarySearchImpl(orderedList, desiredLength, lookAtIdx + 1, endIdx);
        else return lookAtIdx;
    }
    
    static void InsertOrAppend(List<T[]> list, int idx, T[] item)
    {
        if (idx == list.Count)
            list.Add(item);
        else list.Insert(idx, item);
    }

    static T[] CreateArray(int length)
    {
        T[] arr = new T[length];
        int idx = BinarySearch(allArrays, length);
        InsertOrAppend(allArrays, idx, arr);
        return arr;
    }

    private class ArrayLengthComparer : IComparer<T[]>
    {
        public static readonly ArrayLengthComparer Instance = new();

        public int Compare(T[] x, T[] y)
        {
            return x.Length - y.Length;
        }
    }

}
