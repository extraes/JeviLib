using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Jevil;

/// <summary>
/// Comparer for Unity Objects. Special because it uses the overridden equality operator for them.
/// </summary>
/// <typeparam name="T"></typeparam>
public class UnityObjectComparer<T> : IEqualityComparer<T> where T : UnityEngine.Object?
{
    /// <summary>
    /// An instance that can be passed into <see cref="Enumerable.Distinct{TSource}(IEnumerable{TSource}, IEqualityComparer{TSource})"/> to remove duplicate elements from a <see cref="List{T}"/> or <see cref="Dictionary{TKey, TValue}"/>.
    /// </summary>
    public static IEqualityComparer<T> Instance { get; } = new UnityObjectComparer<T>();

    /// <summary>
    /// Redirect to <c><paramref name="x"/> == <paramref name="y"/></c>
    /// </summary>
    /// <param name="x">Any object of type <typeparamref name="T"/>. May be null.</param>
    /// <param name="y">Any object of type <typeparamref name="T"/>. May be null.</param>
    /// <returns>Whether the two are equal.</returns>
    public bool Equals(T? x, T? y)
    {
        bool xNull = x.INOC();
        bool yNull = y.INOC();
        bool bothNull = xNull && yNull;
        bool oneNull = xNull || yNull;
        bool nullMismatch = bothNull != oneNull;
        return !nullMismatch && x == y; // avoid the throwing behavior of the == operator
    }

    /// <summary>
    /// Redirect to <see cref="UnityEngine.Object.GetHashCode"/>.
    /// </summary>
    /// <param name="obj">Any Unity object</param>
    /// <returns><see cref="UnityEngine.Object.GetHashCode"/>.</returns>
    public int GetHashCode(T obj) => obj.GetHashCode();
}