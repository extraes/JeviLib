using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jevil;

/// <summary>
/// Like <see cref="Lazy{T}"/>, but for Unity objects, with support for reinitializing. <br/>
/// If you're using this for asset loading, consider using <see cref="BundledAsset{T}"/> instead.
/// </summary>
/// <typeparam name="T">Any unity object</typeparam>
public sealed class ULazy<T> where T : UnityEngine.Object
{
    private T? value;
    private Func<T> valueMaker;
    bool persist;
    bool hide;

    /// <summary>
    /// Creates a new lazy value-holder with the provided value maker and settings.
    /// </summary>
    /// <param name="valueMaker">Any method or lambda that returns <typeparamref name="T"/>.</param>
    /// <param name="persistOnCreate"></param>
    /// <param name="hideOnCreate"></param>
    public ULazy(Func<T> valueMaker, bool persistOnCreate = true, bool hideOnCreate = true)
    {
        this.valueMaker = valueMaker;
        persist = persistOnCreate;
        hide = hideOnCreate;
    }

    /// <summary>
    /// If the value exists, returns it. If not, creates it and returns it using the settings provided in the constructor.
    /// </summary>
    public T Value
    {
        get
        {
            if (value.INOC())
            {
                value = valueMaker();
                if (persist)
                    value.Persist(hide);
                else if (hide)
                    value.hideFlags = UnityEngine.HideFlags.HideAndDontSave;

#if DEBUG
                if (value.INOC())
                    throw new InvalidOperationException("Value maker returned null. This should not happen!");
#endif
            }    
            return value;
        }
    }
    
    /// <summary>
    /// Returns true if the value has been created and is not null.
    /// </summary>
    public bool ValueExists => !value.INOC();
}
