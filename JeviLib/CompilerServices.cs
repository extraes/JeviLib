using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Runtime.CompilerServices;

/// <summary>
/// Define the type "IsExternalInit" so that the compiler lets me use <c>init</c> properties and record types.
/// </summary>
public static class IsExternalInit { }


/// <summary>
/// Define the type "IsUnmanagedAttribute" so that the compiler lets me use <c>: unmanaged</c> type constraints.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
[AttributeUsage(AttributeTargets.All)]
public sealed class IsUnmanagedAttribute : Attribute { }


internal sealed class AsyncMethodBuilderAttribute : Attribute 
{
    public Type BuilderType { get; private set; }
    public AsyncMethodBuilderAttribute(Type builderType)
    {
        BuilderType = builderType;
    }
}

#if !NET5_0_OR_GREATER
/// <summary>
/// Allows a method to see the expression used to create a given input parameter. The expression will be given as a string and set by the compiler.
/// <para><i>A paste of the .NET Core attribute of the same name.</i></para>
/// </summary>
[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
public sealed class CallerArgumentExpressionAttribute : Attribute
{
    /// <summary>
    /// The parameter this attribute references.
    /// </summary>
    public string ParameterName { get; private set; }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="parameterName">The parameter this attribute references.</param>
    public CallerArgumentExpressionAttribute(string parameterName)
    {
        ParameterName = parameterName;
    }
}
#endif