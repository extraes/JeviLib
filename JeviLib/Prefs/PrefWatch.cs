using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jevil.Prefs;


/// <summary>
/// Designates a method to be executed when a preference field with the given name is changed in BoneMenu (but NOT MelonPreferences).
/// <para>As always, use a debug build when making mods that use this, as you'll get more descriptive error messages if things are amiss. A release build will use more optimized and less warn-y code.</para>
/// </summary>
[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
public sealed class PrefWatch : Attribute
{
    internal readonly string fieldName;
    
    /// <summary>
    /// Designates a method to be executed when a preference field with the given name is changed in BoneMenu or MelonPreferences.
    /// </summary>
    /// <param name="fieldName">The field to watch for changes to.</param>
    public PrefWatch(string fieldName)
    {
        this.fieldName = fieldName;
    }
}
