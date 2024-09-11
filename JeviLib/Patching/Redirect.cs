using HarmonyLib;
using Il2CppCysharp.Threading.Tasks;
using MelonLoader;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Jevil.Patching;

/// <summary>
/// A class to make prefixes easier to create.
/// <para>This is functionally different from <see cref="Hook"/> due to the use of Prefixes instead of Postfixes. If you want to modify some fields before a method runs or replace a method entirely, use this class.</para>
/// <para>It is important to use the Debug build of JeviLib when testing anything using <see cref="Hook"/> or <see cref="Redirect"/> because debug builds have more specific errors.</para>
/// </summary>
public static class Redirect
{
    const string RESULT_PARAM_NAME = "__result";
    delegate ParameterExpression ParamExpMake(Type type, string name, bool isByRef);

    static readonly ParamExpMake ParameterExpression_Make = (ParamExpMake)typeof(ParameterExpression).GetMethod("Make", BindingFlags.Static | BindingFlags.NonPublic).CreateDelegate(typeof(ParamExpMake));
    static int redirections;
    /// <summary>
    /// The length, in ms, to wait to acquire the threaded spinlock. Lock logging statements do not abide by <see cref="DisableLogging"/>.
    /// <para>Defaults to <c>1000</c>ms, aka 1 second.</para>
    /// <br>If it fails to acquire a lock within the given time, it will queue another call.</br>
    /// </summary>
    public static int lockTimeout = 1000;
    static SpinLock spinLock = new();

    // fine to use because internals are be visible to the dynamically created assembly called "JeviLib Dynamic Patch Assembly Host" (see AssemblyInfo.cs, the InternalsVisibleTo attr)
    internal static List<Delegate> redirectionDelegates = new();
    internal static Delegate GetDelegate(int idx) => redirectionDelegates[idx]; // didnt feel like learning how to have an expression get 
    private static readonly MethodInfo GetDelegate_Info = typeof(Redirect).GetMethod(nameof(GetDelegate), BindingFlags.Static | BindingFlags.NonPublic);
    /// <summary>
    /// Get all methods dynamically created from <see cref="FromMethod{TDelegate}(MethodInfo, TDelegate, bool)"/> and <see cref="FromDelegate{TDelegateSource, TDelegateDest}(TDelegateSource, TDelegateDest, bool)"/>.
    /// </summary>
    public static IEnumerable<MethodInfo> GetConditionalDisableMethods()
        => DynTools.DynamicModuleBuilder.GetTypes()
                                        .Select(t => t.GetMethods(Const.AllBindingFlags))
                                        .Flatten()
                                        .Where(m => m.Name.Contains("Redirect_"));

    // todo: support reference parameters for changing params

    /// <summary>
    /// Redirection to <see cref="FromMethod{TDelegate}(MethodInfo, TDelegate, bool)"/>. See that method's summary and remarks unless you value 'trial and error' and 'fucking around and finding out' over your time.
    /// <para>This is one of the few JeviLib methods that will actually <see langword="throw"/> when something is amiss, so make sure you have your ducks in a row.</para>
    /// </summary>
    /// <typeparam name="TDelegateSource">Any method type</typeparam>
    /// <typeparam name="TDelegateDest">Something invokable like <see cref="Action"/></typeparam>
    /// <param name="toBeRedirected">
    /// The method being replaced/prefixed. Can be static or instanced.
    /// <para>This applies on a method-wide basis, not just to the single instance you have a reference to (assuming the method isn't static).</para>
    /// </param>
    /// <param name="toBeRan">The invokable to be executed before <paramref name="toBeRedirected"/></param>
    /// <param name="skipOriginal">Whether to fully replace the method being redirected (<see langword="true"/>) or to just prefix it (<see langword="false"/>).</param>
    /// <example>
    /// Redirect.FromDelegate(Physics.ClosestPoint, ClosestPointButBetter, true);
    /// Redirect.FromDelegate(Physics.ClosestPoint, ClosestPointChangePreState, false);
    /// </example>
    public static void FromDelegate<TDelegateSource, TDelegateDest>(TDelegateSource toBeRedirected, TDelegateDest toBeRan, bool skipOriginal = false) where TDelegateSource : Delegate where TDelegateDest : Delegate
        => FromMethod(toBeRedirected.Method, toBeRan, skipOriginal);

    /// <summary>
    /// Redirect the given method to the given delegate, attempting to automatically resolve parameter types. See remarks on parameters to learn more on how this method operates.
    /// <para>This is one of the few JeviLib methods that will actually <see langword="throw"/> when something is amiss, so make sure you have your ducks in a row.</para>
    /// <para><b>THIS IS IRREVERSIBLE!!!</b> MelonLoader does not support unpatching on IL2CPP games!</para>
    /// </summary>
    /// <typeparam name="TDelegate">Any delegate, can be a <see cref="Func{T, TResult}"/> or an <see cref="Action{T}"/>.</typeparam>
    /// <param name="toBeRedirected">The method to be skipped and have its execution passed to the other method.</param>
    /// <param name="toBeRan">Any delegate. If it is a <see cref="Func{TResult}"/> (that is, it returns a value) it have its value post-execution automatically assigned to the original's return value.</param>
    /// <param name="skipOriginal">Whether or not to skip the original method. Most recommended if you </param>
    /// <exception cref="ArgumentNullException"></exception>
    /// <remarks>
    /// <para><b>Remarks:</b></para>
    /// <para>This method dymanically creates methods which it attaches to an internal class. That method calls your delegate, passing in every parameter you specify.</para>
    /// <para>It determines the parameters to pass in by matching types in order. If <paramref name="toBeRedirected"/> is declared with parameters like so, <c>(string goofBall, bool bazingoid, Pancake[] hotcakes)</c>, and your delegate <paramref name="toBeRan"/> is declared with the parameters <c>(string someShit, Pancake[] wowza)</c>, this method will resolve that when determining shared parameters and their types, and will call it as expected. <i>It does not look for parameter names.</i></para>
    /// <para>If your delegate's return type isn't <see langword="void"/> and matches <paramref name="toBeRedirected"/>'s return type, an extra parameter will be added to the dynamically created method, called <c>__result</c>. When a Harmony patch is created, Harmony looks through the parameters and sees that, and knows that any value assigned to it will become the return value of the patched method.</para>
    /// <para>If your delegate takes a parameter that is the same as <paramref name="toBeRedirected"/>.DeclaringType, it will add a parameter called <c>__instance</c> and pass that to your redirection.</para>
    /// <para>If there's a parameter your redirect has that isn't the instance type or whose type isn't in the normal parameter list, this method will likely fail.</para>
    /// </remarks>
    public static void FromMethod<TDelegate>(MethodInfo toBeRedirected, TDelegate toBeRan, bool skipOriginal = false) where TDelegate : Delegate
    {
        bool lockTaken = false;
#if DEBUG
        if (spinLock.IsHeld) JeviLib.Log("Redirect will have to wait, the spin lock is presently held by another thread. Timing out in " + lockTimeout + ".");
#endif
        try
        {
            spinLock.TryEnter(lockTimeout, ref lockTaken);
            // cover my ass to make sure shit doesnt break while messing around in such a critical field
            if (toBeRedirected == null) throw new ArgumentNullException(nameof(toBeRedirected));
            if (toBeRan == null) throw new ArgumentNullException(nameof(toBeRan));
            if (toBeRedirected.DeclaringType is null) throw new NullReferenceException("Method to be patched has no declaring type!");
            
            if (toBeRan.Method.IsStatic && toBeRan.Method.GetParameters().Length == 0 && toBeRan.Method.ReturnType == typeof(void))
            {
                Log("Method to be ran is not a replacement, void, parameterless, and static; doing direct patch without dynamic method creation.");
                JeviLib.instance.HarmonyInstance.Patch(toBeRedirected, toBeRan.Method.ToNewHarmonyMethod());
                return;
            }

            // have a redirection-specific identifier
            int thisRedirNum = redirections++;
            int thisDelegateIdx = redirectionDelegates.Count;
            MethodBuilder bob = CreateRedirectMethod(toBeRedirected, toBeRan, "Redirect_" + thisRedirNum, !skipOriginal);
            
            Type createdType = bob.DeclaringType ?? throw new NullReferenceException("Declaring type was null after creation!");
            Log($"Compiled patch method and created runtime type!");
            Log($"Created: <asm={createdType.Assembly.GetName().Name}> <module={createdType.Module.Name}> {createdType.FullName}");


            //MethodBase dynInfo = MethodBase.GetMethodFromHandle(bob.MethodHandle);
            //SymbolExtensions.GetMethodInfo(dynInfo);
            HarmonyMethod hPrefix = new(bob.GetMethodInfo());

            JeviLib.instance.HarmonyInstance.Patch(toBeRedirected, prefix: hPrefix);
        }
        finally
        {
            if (lockTaken) spinLock.Exit();
            else
            {
                JeviLib.Log("FAILURE!!! Redirect failed to acquire the lock after " + lockTimeout + "ms! Queueing call after 500ms!");
                Waiting.CallDelayed.CallAction(() => FromMethod(toBeRedirected, toBeRan, skipOriginal), 0.5f, true);
            } 
        }
    }


    internal static MethodBuilder CreateRedirectMethod(MethodInfo toBeRedirected, Delegate toBeRan, string uniqueId, bool retval)
    {
#if DEBUG
        if (toBeRedirected.DeclaringType is null)
            throw new NullReferenceException("Declaring type of method to be patched is null!");
#endif
        bool addRefResult = toBeRan.Method.ReturnType != typeof(void) && toBeRan.Method.ReturnType == toBeRedirected.ReturnType;
        
        Type delegateType = toBeRan.GetType();
        // keep the parameterinfo's of the source method, but only the ones that are used. hopefully kept in order
        List<ParameterInfo> parameters = DynTools.RemoveUnmatchedParameters(toBeRedirected, toBeRan.Method);
        // convert to parameterexpressions for Linq.Expressions
        List<ParameterExpression> paramExps = DynTools.GetMethodParameters(parameters, toBeRedirected.DeclaringType, toBeRedirected.IsStatic);

        if (addRefResult)
        {
            if (retval)
            {
                Log($"Original method '{toBeRedirected.Name}' isn't going to be skipped but '{toBeRan.Method.Name}' returns a value as if skipping the original... Did you use the wrong value for skipOriginal?");
            }

            // a "ref bool" parameter isnt a "bool" parameter, its a "bool&", so we need to get the reference type of the return type
            Type referenceType = DynTools.GetReferenceType(toBeRedirected.ReturnType);
            ParameterExpression resultParamExp = ParameterExpression_Make(toBeRedirected.ReturnType, RESULT_PARAM_NAME, true);
            //ParameterExpression resultParamExp = Expression.Parameter(referenceType, RESULT_PARAM_NAME);
            paramExps.Add(resultParamExp);
        }
        
        // get it, bob the builder, har har
        TypeBuilder tb = DynTools.GetTypeBuilder(uniqueId);
        FieldBuilder actionHolder = tb.DefineField("forwardTo", delegateType, FieldAttributes.Private | FieldAttributes.Static);
        MethodBuilder bob = tb.DefineMethod($"{toBeRedirected.Name}_{uniqueId}",
                                            MethodAttributes.Public | MethodAttributes.Static, // method has to be static to be called from harmony
                                            CallingConventions.Standard,
                                            typeof(bool),
                                            paramExps.Select(pi => pi.IsByRef && !pi.Type.IsByRef ? DynTools.GetReferenceType(pi.Type) : pi.Type).ToArray());
        for (int i = 0; i < paramExps.Count; i++)
        {
            ParameterExpression paramExp = paramExps[i];
            bob.DefineParameter(i + 1, ParameterAttributes.None, paramExp.Name);
        }

        ILGenerator ilGen = bob.GetILGenerator();
        Label jumpIfNeeded = addRefResult ? ilGen.DefineLabel() : default;
        MethodInfo invokeMethod = DynTools.GetInvokeMethod(delegateType);
        ilGen.DeclareLocal(typeof(bool));


        ilGen.Emit(OpCodes.Nop);
        if (addRefResult)
        {
            // loads __result so it can be stind.*'d to set its value
            DynTools.EmitLoadArgs(ilGen, paramExps.Count, paramExps.Count - 1); // just loads last arg (the __result)
        }
        ilGen.Emit(OpCodes.Ldsfld, actionHolder);
        DynTools.EmitLoadArgs(ilGen, addRefResult ? paramExps.Count - 1 : paramExps.Count);
        ilGen.Emit(OpCodes.Callvirt, invokeMethod);
        if (addRefResult)
        {
            DynTools.EmitStoreInstruction(ilGen, toBeRedirected.ReturnType);
            ilGen.Emit(retval ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0);
            ilGen.Emit(OpCodes.Stloc_0);
            ilGen.Emit(OpCodes.Br_S, jumpIfNeeded);
            ilGen.MarkLabel(jumpIfNeeded);
            ilGen.Emit(OpCodes.Ldloc_0);
        }
        else
        {
            ilGen.Emit(retval ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0);
        }
        ilGen.Emit(OpCodes.Ret);
        //   // {
        //   IL_0000: nop
        //   // tenParameterAction(a, b, c, d, e, f, g, h, j, k);
        //   IL_0001: ldsfld class [System.Runtime] System.Action`10<int32, int32, int32, int32, int32, int32, int32, int32, int32, int32> JeviLib.Research.ActionCalling::tenParameterAction
        //   IL_0006: ldarg.0
        //   IL_0007: ldarg.1
        //   IL_0008: ldarg.2
        //   IL_0009: ldarg.3
        //   IL_000a: ldarg.s e
        //   IL_000c: ldarg.s f
        //   IL_000e: ldarg.s g
        //   IL_0010: ldarg.s h
        //   IL_0012: ldarg.s j
        //   IL_0014: ldarg.s k
        //   IL_0016: callvirt instance void class [System.Runtime] System.Action`10<int32, int32, int32, int32, int32, int32, int32, int32, int32, int32>::Invoke(!0, !1, !2, !3, !4, !5, !6, !7, !8, !9)
        //   // }
        //   IL_001b: nop
        //   IL_001c: ret
        Type createdType = tb.CreateType() ?? throw new NullReferenceException("Created type is null");
        FieldInfo actionHolderBuilt = createdType.GetField(actionHolder.Name, BindingFlags.NonPublic | BindingFlags.Static) ?? throw new NullReferenceException("Delegate-holding field is null");
        actionHolderBuilt.SetValue(null, toBeRan);

        if (Debugger.IsAttached)
        {
            MethodInfo meth = createdType.GetMethod(bob.Name) ?? throw new NullReferenceException("Method null after type create");
            var insts = PatchProcessor.GetOriginalInstructions(meth);

        }

        return bob;
    }

    #region Logging
    /// <summary>
    /// Whether to allow the (frankly copious amounts of) log statements in <see cref="FromMethod{TDelegate}(MethodInfo, TDelegate, bool)"/> to output to the log file.
    /// <para>Debug builds default to false, release builds default to true.</para>
    /// </summary>
    public static bool DisableLogging { get => DynTools.disableLogging; set { DynTools.disableLogging = value; } }
    private static void Log(object obj) => Log(obj?.ToString() ?? "<null>");
    private static void Log(string str)
    {
        if (!DynTools.disableLogging) JeviLib.Log("REDIRECTOR -> " + str, ConsoleColor.DarkGray);
    }
    #endregion
}
