using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jevil.Patching;

/// <summary>
/// Designates an assembly to be called "ungovernable", so JeviLib knows to handle it appropriately when it's loaded.
/// <para/>See <see cref="UngovernableType"/>
/// </summary>
[AttributeUsage(AttributeTargets.Assembly)]
public sealed class UngovernableAttribute : Attribute
{
    internal readonly UngovernableType type;

    /// <summary>
    /// Decorates an assembly with the given <see cref="UngovernableType"/>
    /// </summary>
    /// <param name="type"></param>
    public UngovernableAttribute(UngovernableType type) : base()
    {
        this.type = type;
    }
}
