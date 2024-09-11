using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using HarmonyLib;
using Il2CppInterop.Generator.Contexts;
using Il2CppInterop.Generator.Utils;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Jevil.Il2CppInterop.Generator.Passes;

internal static class Pass61ImplementAwaiters
{
    public static void DoPass(RewriteGlobalContext context)
    {
        var corlib = context.GetAssemblyByName("mscorlib");
        var actionUntyped = corlib.GetTypeByName("System.Action");

        var actionConversionUntyped = actionUntyped.NewType.Methods.FirstOrDefault(m => m.Name == "op_Implicit") ?? throw new MissingMethodException("Untyped action conversion");

        foreach (var assemblyContext in context.Assemblies)
        {
            // dont actually import the references until they're needed
            Lazy<TypeReference> actionUntypedRef = new(() => assemblyContext.NewAssembly.MainModule.ImportReference(actionUntyped.OriginalType));
            Lazy<MethodReference> actionConversionUntypedRef = new(() => assemblyContext.NewAssembly.MainModule.ImportReference(actionConversionUntyped));
            Lazy<TypeReference> notifyCompletionRef = new(() => assemblyContext.NewAssembly.MainModule.ImportReference(typeof(INotifyCompletion)));
            Lazy<TypeReference> voidRef = new(() => assemblyContext.NewAssembly.MainModule.ImportReference(typeof(void)));
            foreach (var typeContext in assemblyContext.Types)
            {
                var interfaceImplementation = typeContext.OriginalType.Interfaces.FirstOrDefault(InterfaceImplementation => InterfaceImplementation.InterfaceType.Name == nameof(INotifyCompletion));
                if (interfaceImplementation is null)
                    continue;
                if (typeContext.OriginalType.IsInterface)
                    continue;

                var isGeneric = typeContext.OriginalType.ContainsGenericParameter;

                var awaiterType = typeContext.OriginalType;

                var onCompleteContext = typeContext.TryGetMethodByName(nameof(INotifyCompletion.OnCompleted));
                //System.Reflection.MethodInfo newOncompleteTyped = typeof(string).GetMethod("");
                MethodReference newOnComplete = typeContext.NewType.Methods.FirstOrDefault(m => m.Name == nameof(INotifyCompletion.OnCompleted));

                if (onCompleteContext is null || newOnComplete is null)
                    continue;

                #region Handle generic declaring types of nested types
                
                // many awaiters are nested types, so we need to handle generic declaring types
                // shit like UniTask<T>.Awaiter
                // our new OnCompletes cant just call Task<>.Awaiter.OnComplete
                //   this is probably hacky, only checking the single declaring type, but honestly, it works, i dont care.
                if (typeContext.NewType.DeclaringType?.HasGenericParameters ?? false)
                {
                    // "genericified" nah i gentrify neighborhoods
                    TypeReference declTypeGenericified = typeContext.NewType.DeclaringType.MakeGenericType(typeContext.NewType.DeclaringType.GenericParameters.ToArray());
                    //TypeReference awaiterTypeRetyped = new(typeContext.NewType.Namespace, typeContext.NewType.Name, typeContext.NewType.Module, typeContext.NewType.Scope, false)
                    //{
                    //    DeclaringType = declTypeGenericified,
                    //    //MetadataToken = typeContext.NewType.MetadataToken,
                    //};
                    TypeReference awaiterTypeRetyped = typeContext.NewType.MakeGenericType(typeContext.NewType.DeclaringType.GenericParameters.ToArray());

                    var parameters = newOnComplete.Parameters;
                    newOnComplete = new MethodReference(onCompleteContext.NewMethod.Name, onCompleteContext.NewMethod.ReturnType)
                    {
                        DeclaringType = awaiterTypeRetyped,
                        CallingConvention = onCompleteContext.NewMethod.CallingConvention,
                        HasThis = onCompleteContext.NewMethod.HasThis,
                        ExplicitThis = onCompleteContext.NewMethod.ExplicitThis,
                    };
                    foreach (var item in parameters)
                    {
                        newOnComplete.Parameters.Add(new ParameterDefinition(item.Name, item.Attributes, item.ParameterType));
                    }
                }

                #endregion
                
                //if (typeContext.NewType.HasGenericParameters)
                //{
                //    TypeReference genericType = typeContext.NewType;

                //    newOnComplete = genericType.Resolve().Methods.FirstOrDefault(m => m.Name == nameof(INotifyCompletion.OnCompleted));
                //}
                // this shouldnt happen but it does! awesome!

                var onCompletedAttr = MethodAttributes.Public | MethodAttributes.Final | MethodAttributes.Virtual | MethodAttributes.HideBySig | MethodAttributes.NewSlot;
                var onComplete = new MethodDefinition(nameof(INotifyCompletion.OnCompleted), onCompletedAttr, voidRef.Value);
                typeContext.NewType.Interfaces.Add(new(notifyCompletionRef.Value));
                typeContext.NewType.Methods.Add(onComplete);
                
                onComplete.Parameters.Add(new ParameterDefinition("continuation", ParameterAttributes.None, actionUntypedRef.Value));

                var onCompleteIl = onComplete.Body.GetILProcessor();

                onCompleteIl.Emit(OpCodes.Nop);
                onCompleteIl.Emit(OpCodes.Ldarg_0);
                onCompleteIl.Emit(OpCodes.Ldarg_1); // ldarg1 bc not static, so ldarg0 is "this" & ldarg1 is the parameter
                onCompleteIl.Emit(OpCodes.Call, actionConversionUntypedRef.Value);
                onCompleteIl.Emit(OpCodes.Call, newOnComplete);
                onCompleteIl.Emit(OpCodes.Nop);
                onCompleteIl.Emit(OpCodes.Ret);
            }
        }
    }

    static TypeReference MakeGenericType(this TypeReference self, params TypeReference[] arguments)
    {
        if (self.GenericParameters.Count != arguments.Length)
            throw new ArgumentException();

        var instance = new GenericInstanceType(self);
        foreach (var argument in arguments)
            instance.GenericArguments.Add(argument);

        return instance;
    }

    //public static MethodReference MakeGeneric(this MethodReference self, TypeReference declaringType)
    //{
    //    var reference = new MethodReference(self.Name, self.ReturnType)
    //    {
    //        Name = self.Name,
    //        DeclaringType = declaringType,
    //        HasThis = self.HasThis,
    //        ExplicitThis = self.ExplicitThis,
    //        ReturnType = self.ReturnType,
    //        CallingConvention = MethodCallingConvention.Generic,
    //    };

    //    foreach (var parameter in self.Parameters)
    //        reference.Parameters.Add(new ParameterDefinition
    //        (parameter.ParameterType));

    //    foreach (var generic_parameter in self.GenericParameters)
    //        reference.GenericParameters.Add(new GenericParameter(reference));

    //    return reference;
    //}
}
