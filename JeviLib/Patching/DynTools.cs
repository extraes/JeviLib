using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Jevil.Patching;

internal static class DynTools
{
    public const string DYNAMIC_ASM_NAME = "JeviLib Dynamic Patch Assembly Host";
    public static readonly ModuleBuilder DynamicModuleBuilder;
    public static readonly HarmonyLib.Harmony Harmony = new("JeviLib.Patching.SharedHarmony");

    static DynTools()
    {
        AssemblyBuilder asmBuilder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(DYNAMIC_ASM_NAME), AssemblyBuilderAccess.Run);
        if (Debugger.IsAttached)
        {
            Type debuggable = typeof(DebuggableAttribute);
            ConstructorInfo ctor = debuggable.GetConstructor(new Type[] { typeof(DebuggableAttribute.DebuggingModes) }) ?? throw new MissingMethodException("Debuggable ctor");


            CustomAttributeBuilder debuggableBuilder = new(ctor, new object[] {
                DebuggableAttribute.DebuggingModes.DisableOptimizations | DebuggableAttribute.DebuggingModes.Default
            });
        }


        DynamicModuleBuilder = asmBuilder.DefineDynamicModule("Modulo");
    }

    static readonly Dictionary<Type, OpCode> StoreInstructions = new()
    {
        { typeof(sbyte),    OpCodes.Stind_I1 },
        { typeof(byte),     OpCodes.Stind_I1 },
        { typeof(short),    OpCodes.Stind_I2 },
        { typeof(ushort),   OpCodes.Stind_I2 },
        { typeof(int),      OpCodes.Stind_I4 },
        { typeof(uint),     OpCodes.Stind_I4 },
        { typeof(long),     OpCodes.Stind_I8 },
        { typeof(ulong),    OpCodes.Stind_I8 },
        { typeof(float),    OpCodes.Stind_R4 },
        { typeof(double),   OpCodes.Stind_R8 },
        { typeof(IntPtr),   OpCodes.Stind_I },
        { typeof(UIntPtr),  OpCodes.Stind_I },
        { typeof(bool),     OpCodes.Stind_I1 },
        { typeof(char),     OpCodes.Stind_I2 },
    };

    private static Dictionary<Type, MethodInfo> delegateInvokeMethods = new();
    private static Dictionary<Type, Type> referenceTypes = new();

    internal static bool disableLogging =
#if DEBUG
        false;
    //private static readonly MethodInfo GetDelegate_Info = typeof(Hook).GetMethod(nameof(GetDelegate), BindingFlags.Static | BindingFlags.NonPublic) ?? throw new MissingMethodException("");
#else
        true;
#endif


    public static TypeBuilder GetTypeBuilder(string extraIdentifier)
    {
        return DynamicModuleBuilder.DefineType("DynamicMethodHost_" + extraIdentifier);
    }

    internal static MethodInfo GetMethodInfo(this MethodBuilder builder)
    {
        Type declaringType = builder.DeclaringType ?? throw new NullReferenceException("Type was null");

        return declaringType?.GetMethod(builder.Name, Const.AllBindingFlags) ?? throw new NullReferenceException("Method was not found/was null");
    }


    internal static List<ParameterInfo> RemoveUnmatchedParameters(MethodBase source, MethodInfo dest)
    {
        List<ParameterInfo> srcParams = source.GetParameters().ToList();
        ParameterInfo[] destParams = dest.GetParameters();
        List<ParameterInfo> ret = new();
        bool gotInstanceParameter = false;

        foreach (ParameterInfo param in destParams)
        {
            if (!gotInstanceParameter && param.ParameterType == source.DeclaringType)
            {
                Log($"Skipping delegate parameter {param.ParameterType} {param.Name} because it's the first parameter that's the same type as the type that's going to be patched.");
                gotInstanceParameter = true;
                ret.Add(param); // should work methinks
                continue;
            }

            ParameterInfo? pinf = srcParams.FirstOrDefault(p => p.ParameterType == param.ParameterType);
            if (pinf is null)
            {
                throw new InvalidHarmonyPatchArgumentException($"Patch parameter has no matching paramter in original method! {param.ParameterType.FullName} {param.Name}", source, dest);
            }
            srcParams.Remove(pinf);
            ret.Add(pinf);
        }
        Log("Patch will have " + ret.Count + " parameters passed from patch to redirect.");

        return ret;
    }

    // this method was made back when using Expressions was possible, but .NET 6 doesnt let them compile to methods,
    // so this is just used as one of the steps in the process.
    internal static List<ParameterExpression> GetMethodParameters(IEnumerable<ParameterInfo> infos, Type instanceType, bool isStatic)
    {
        List<ParameterExpression> ret = new(infos.Count());
        bool alreadyInstanced = isStatic;
        foreach (ParameterInfo pinf in infos)
        {
            string name = pinf.Name ?? throw new NullReferenceException("Parameter cannot be nameless!");

            if (!alreadyInstanced && pinf.ParameterType == instanceType)
            {
                Log($"Replacing parameter name '{pinf.Name}' with '__instance'");
                name = "__instance";
                alreadyInstanced = true;
            }

            ret.Add(Expression.Parameter(pinf.ParameterType, name));
        }
        return ret;
    }

    internal static void EmitLoadArgs(ILGenerator ilGen, int argCount, int startAt = 0)
    {
        for (int i = startAt; i < argCount; i++)
        {
            switch (i)
            {
                case 0: ilGen.Emit(OpCodes.Ldarg_0); break;
                case 1: ilGen.Emit(OpCodes.Ldarg_1); break;
                case 2: ilGen.Emit(OpCodes.Ldarg_2); break;
                case 3: ilGen.Emit(OpCodes.Ldarg_3); break;
                default: ilGen.Emit(OpCodes.Ldarg_S, i); break;
            }
        }
    }

    internal static void EmitStoreInstruction(ILGenerator ilGen, Type type)
    {
        if (StoreInstructions.TryGetValue(type, out OpCode opCode))
            ilGen.Emit(opCode);
        else if (type.IsValueType)
            ilGen.Emit(OpCodes.Stobj, type);
        else
            ilGen.Emit(OpCodes.Stind_Ref);
    }

    internal static Type GetReferenceType(Type normalType)
    {
        if (referenceTypes.TryGetValue(normalType, out Type? refType))
            return refType;
        refType = normalType.MakeByRefType(); // also works: "refType = Type.GetType(normalType.FullName + "&");" i wish i was joking
        referenceTypes[normalType] = refType;
        return refType;
    }

    internal static MethodInfo GetInvokeMethod(Type type)
    {
        if (delegateInvokeMethods.TryGetValue(type, out MethodInfo? methodInfo))
            return methodInfo;

        MethodInfo invokeMethod = type.GetMethod("Invoke") ?? throw new MissingMethodException($"Delegate '{type.FullDescription()}' has no 'Invoke' method!");
        delegateInvokeMethods[type] = invokeMethod;
        return invokeMethod;
    }

    #region Logging
    private static void Log(object obj) => Log(obj?.ToString() ?? "<null>");
    private static void Log(string str)
    {
        if (!disableLogging) JeviLib.Log("DYNAMICTOOLS -> " + str, ConsoleColor.DarkGray);
    }
    #endregion
}
