using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Jevil.Patching;

internal static class UniTaskCeciler
{
    public static Action<string> Log;
    public static Action<string> Error;

    public static void Execute(string unitaskAsmPath, string il2cppMscorlibPath, string interopRuntimePath, string userData)
    {
#if DEBUG
        if (Log == null || Error == null) 
            throw new NullReferenceException("Logging callbacks cannot be null.");
#endif

        Log($"UniTask assembly path: {unitaskAsmPath}");
        Log($"Unhollowed IL2CPP mscorlib assembly path: {il2cppMscorlibPath}");
        Log($"Il2CppInterop.Runtime assembly path: {interopRuntimePath}");

        string backupsFolder = Path.Combine(userData, "JevilSM", "Backups");
        string backupFile = Path.Combine(backupsFolder, "Il2CppUniTask.bak.dll");

        if (!Directory.Exists(backupsFolder))
        {
            Directory.CreateDirectory(backupsFolder);
            Log("Created backup directory " + backupsFolder);
        }
        Log("Backing up current UniTask.dll to " + backupFile);
        File.Copy(unitaskAsmPath, backupFile, true);
        Log("Successfully wrote UniTask.dll backup.");
        
        // Added copious log statements
        Log("Creating memory stream for modified UniTask.dll");
        using MemoryStream tempStream = new();
        Log("Success!");

        Log("Reading Il2CppInterop for type reference...");
        TypeDefinition il2cppObjectRef = AssemblyDefinition.ReadAssembly(interopRuntimePath).MainModule.GetType("Il2CppInterop.Runtime.InteropTypes.Il2CppObjectBase");
        Log("Success!");

        Log("Reading Il2Cppmscorlib for type references...");
        AssemblyDefinition il2Mscorlib = AssemblyDefinition.ReadAssembly(il2cppMscorlibPath);
        TypeDefinition il2ActionGeneric = il2Mscorlib.MainModule.GetType("Il2CppSystem.Action`1");
        TypeDefinition il2ActionUntyped = il2Mscorlib.MainModule.GetType("Il2CppSystem.Action");
        Log("Success!");

        Log("Getting conversion methods from Il2Cppmscorlib...");
        MethodDefinition monoActionToIl2ActionGenericDef = il2ActionGeneric.Methods.First(m => m.Name == "op_Implicit");
        MethodDefinition monoActionToIl2ActionUntypedDef = il2ActionUntyped.Methods.First(m => m.Name == "op_Implicit");
        Log("Success!");

        Log("Reading .NET Framework mscorlib for type references...");
        AssemblyDefinition mscorlib = AssemblyDefinition.ReadAssembly(typeof(INotifyCompletion).Assembly.Location);
        TypeDefinition notifyCompletionDef = mscorlib.MainModule.GetType(typeof(INotifyCompletion).FullName);
        TypeDefinition voidDef = mscorlib.MainModule.GetType(typeof(void).FullName);
        TypeDefinition actionGenericDef = mscorlib.MainModule.GetType(typeof(Action).FullName + "`1");
        TypeDefinition actionUntypedDef = mscorlib.MainModule.GetType(typeof(Action).FullName);
        TypeDefinition attributeDef = mscorlib.MainModule.GetType(typeof(System.Attribute).FullName);
        TypeDefinition typeDef = mscorlib.MainModule.GetType(typeof(Type).FullName);
        Log("Success!");

        Log("Loading UniTask.dll to modify...");
        AssemblyDefinition asm = AssemblyDefinition.ReadAssembly(unitaskAsmPath);
        ModuleDefinition utMod = asm.MainModule;
        Log("Success!");

        Log("Importing references for UniTask...");
        MethodReference monoActionToIl2ActionGenericRef = utMod.ImportReference(monoActionToIl2ActionGenericDef);
        MethodReference monoActionToIl2ActionUntypedRef = utMod.ImportReference(monoActionToIl2ActionUntypedDef);
        TypeReference voidRef = utMod.ImportReference(voidDef);
        TypeReference actionGenericRef = utMod.ImportReference(actionGenericDef);
        TypeReference actionUntypedRef = utMod.ImportReference(actionUntypedDef);
        TypeReference notifyCompletionRef = utMod.ImportReference(notifyCompletionDef);
        TypeReference attributeRef = utMod.ImportReference(attributeDef);
        TypeReference typeRef = utMod.ImportReference(typeDef);
        Log("Success!");


        Log("Looking for UniTask type definition");
        TypeDefinition uniTaskDef = utMod.GetType("Il2CppCysharp.Threading.Tasks.UniTask");
        Log("Success!");

        Log("Looking for UniTask Awaiter type definitions...");
        TypeDefinition uniTaskAwaiterGeneric = utMod.GetType("Il2CppCysharp.Threading.Tasks.UniTask`1/Awaiter");
        TypeDefinition uniTaskAwaiterUntyped = utMod.GetType("Il2CppCysharp.Threading.Tasks.UniTask/Awaiter");
        TypeDefinition yieldAwaiter = utMod.GetType("Il2CppCysharp.Threading.Tasks.YieldAwaitable/Awaiter");
        TypeDefinition switchToMainThreadAwaiter = utMod.GetType("Il2CppCysharp.Threading.Tasks.SwitchToMainThreadAwaitable/Awaiter");
        TypeDefinition switchToThreadPoolAwaiter = utMod.GetType("Il2CppCysharp.Threading.Tasks.SwitchToThreadPoolAwaitable/Awaiter");
        Log("Success!");


        Log("Looking for UniTask Awaiter OnCompleted method definitions...");
        const string unhollowedMethodName = nameof(INotifyCompletion.OnCompleted);
        const string altMethodName = nameof(ICriticalNotifyCompletion.UnsafeOnCompleted);

        MethodDefinition uniTaskAwaiterOnCompletedGeneric = uniTaskAwaiterGeneric.Methods.FirstOrDefault(m => m.Name == unhollowedMethodName) ?? uniTaskAwaiterGeneric.Methods.First(m => m.Name == altMethodName);
        Log(" - Found OnCompleted for the generic awaiter...");
        MethodDefinition uniTaskAwaiterOnCompletedUntyped = uniTaskAwaiterUntyped.Methods.FirstOrDefault(m => m.Name == unhollowedMethodName) ?? uniTaskAwaiterUntyped.Methods.First(m => m.Name == altMethodName);
        Log(" - Found OnCompleted for the untyped awaiter...");
        MethodDefinition yieldAwaiterOnCompleted = yieldAwaiter.Methods.FirstOrDefault(m => m.Name == unhollowedMethodName) ?? yieldAwaiter.Methods.First(m => m.Name == altMethodName);
        Log(" - Found OnCompleted for the yield awaiter...");
        MethodDefinition mainThreadOnCompleted = switchToMainThreadAwaiter.Methods.FirstOrDefault(m => m.Name == unhollowedMethodName) ?? switchToMainThreadAwaiter.Methods.First(m => m.Name == altMethodName);
        Log(" - Found OnCompleted for the main thread awaiter...");
        MethodDefinition threadPoolOnCompleted = switchToThreadPoolAwaiter.Methods.FirstOrDefault(m => m.Name == unhollowedMethodName) ?? switchToThreadPoolAwaiter.Methods.First(m => m.Name == altMethodName);
        Log(" - Found OnCompleted for the thread pool awaiter...");
        Log("Success!");

        // Clear previous runs
        Log("Checking for evidence of previous runs & undoing them...");
        if (uniTaskAwaiterGeneric.Interfaces.Any(i => i.InterfaceType == null) || uniTaskAwaiterGeneric.Interfaces.Count(i => i.InterfaceType.Name == nameof(INotifyCompletion)) >= 1)
            uniTaskAwaiterGeneric.Interfaces.Clear();
        if (uniTaskAwaiterUntyped.Interfaces.Any(i => i.InterfaceType == null) || uniTaskAwaiterUntyped.Interfaces.Count(i => i.InterfaceType.Name == nameof(INotifyCompletion)) >= 1)
            uniTaskAwaiterUntyped.Interfaces.Clear();
        if (yieldAwaiter.Interfaces.Any(i => i.InterfaceType == null) || yieldAwaiter.Interfaces.Count(i => i.InterfaceType.Name == nameof(INotifyCompletion)) >= 1)
            yieldAwaiter.Interfaces.Clear();
        if (switchToMainThreadAwaiter.Interfaces.Any(i => i.InterfaceType == null) || switchToMainThreadAwaiter.Interfaces.Count(i => i.InterfaceType.Name == nameof(INotifyCompletion)) >= 1)
            switchToMainThreadAwaiter.Interfaces.Clear();
        if (switchToThreadPoolAwaiter.Interfaces.Any(i => i.InterfaceType == null) || switchToThreadPoolAwaiter.Interfaces.Count(i => i.InterfaceType.Name == nameof(INotifyCompletion)) >= 1)
            switchToThreadPoolAwaiter.Interfaces.Clear();
        var md1 = uniTaskAwaiterGeneric.Methods.FirstOrDefault(m => m.Name == unhollowedMethodName && !m.Parameters.FirstOrDefault().ParameterType.FullName.ToLower().Contains("il2"));
        var md2 = uniTaskAwaiterUntyped.Methods.FirstOrDefault(m => m.Name == unhollowedMethodName && !m.Parameters.FirstOrDefault().ParameterType.FullName.ToLower().Contains("il2"));
        var md3 = yieldAwaiter.Methods.FirstOrDefault(m => m.Name == unhollowedMethodName && !m.Parameters.FirstOrDefault().ParameterType.FullName.ToLower().Contains("il2"));
        var md4 = switchToMainThreadAwaiter.Methods.FirstOrDefault(m => m.Name == unhollowedMethodName && !m.Parameters.FirstOrDefault().ParameterType.FullName.ToLower().Contains("il2"));
        var md5 = switchToThreadPoolAwaiter.Methods.FirstOrDefault(m => m.Name == unhollowedMethodName && !m.Parameters.FirstOrDefault().ParameterType.FullName.ToLower().Contains("il2"));
        var td1 = utMod.GetType("System.Runtime.CompilerServices.AsyncMethodBuilderAttribute");
        if (md1 != null) uniTaskAwaiterGeneric.Methods.Remove(md1);
        if (md2 != null) uniTaskAwaiterUntyped.Methods.Remove(md2);
        if (md3 != null) yieldAwaiter.Methods.Remove(md3);
        if (md4 != null) switchToMainThreadAwaiter.Methods.Remove(md4);
        if (md5 != null) switchToThreadPoolAwaiter.Methods.Remove(md5);
        if (td1 != null) utMod.Types.Remove(td1);

        Log("Success!");

        // Create new methods to host action conversion
        Log("Creating new method definitions for UniTask Awaiters...");
        // Copy method attributes from System.Runtime.CompilerServices.TaskAwaiter.OnCompleted.
        // If this is not done, using UniTask.Awaiter (generic and untyped) will throw "System.TypeLoadException: VTable setup of type Cysharp.Threading.Tasks.UniTask+Awaiter failed"
        MethodAttributes onCompletedAttr = MethodAttributes.Public | MethodAttributes.Final | MethodAttributes.Virtual | MethodAttributes.HideBySig | MethodAttributes.NewSlot;
        MethodDefinition notifyCompletionGeneric = new(nameof(INotifyCompletion.OnCompleted), onCompletedAttr, voidRef);
        MethodDefinition notifyCompletionUntyped = new(nameof(INotifyCompletion.OnCompleted), onCompletedAttr, voidRef);
        MethodDefinition yieldComplete = new(nameof(INotifyCompletion.OnCompleted), onCompletedAttr, voidRef) ;
        MethodDefinition mainThreadComplete = new(nameof(INotifyCompletion.OnCompleted), onCompletedAttr, voidRef) ;
        MethodDefinition threadPoolComplete = new(nameof(INotifyCompletion.OnCompleted), onCompletedAttr, voidRef) ;
        TypeReference genericActionRefMatchesTask = actionGenericRef.MakeGenericInstanceType(uniTaskAwaiterGeneric.GenericParameters[0]);
        notifyCompletionGeneric.Parameters.Add(new ParameterDefinition("jevilSaysHi", ParameterAttributes.None, actionUntypedRef));
        notifyCompletionUntyped.Parameters.Add(new ParameterDefinition("jevilSaysHi", ParameterAttributes.None, actionUntypedRef));
        yieldComplete.Parameters.Add(new ParameterDefinition("jevilSaysHi", ParameterAttributes.None, actionUntypedRef));
        mainThreadComplete.Parameters.Add(new ParameterDefinition("jevilSaysHi", ParameterAttributes.None, actionUntypedRef));
        threadPoolComplete.Parameters.Add(new ParameterDefinition("jevilSaysHi", ParameterAttributes.None, actionUntypedRef));
        Log("Success!");

        // Create method bodies
        Log("Filling out method bodies for UniTask Awaiters...");
        Log(" - Creating bodies & getting IL Processors...");
        MethodBody genericBody = new(notifyCompletionGeneric);
        MethodBody untypedBody = new(notifyCompletionGeneric);
        MethodBody yieldBody = new(yieldComplete);
        MethodBody mainThreadBody = new(mainThreadComplete);
        MethodBody threadPoolBody = new(threadPoolComplete);
        ILProcessor genericProc = genericBody.GetILProcessor();
        ILProcessor untypedProc = untypedBody.GetILProcessor();
        ILProcessor yieldProc = yieldBody.GetILProcessor();
        ILProcessor mainThreadProc = mainThreadBody.GetILProcessor();
        ILProcessor threadPoolProc = threadPoolBody.GetILProcessor();
        Log(" - Success!");

        // pasting the IL from some sample methods real quick
        Log(" - Filling in method body for the generic awaiter...");
        untypedProc.Emit(OpCodes.Nop);
        genericProc.Emit(OpCodes.Ldarg_0);
        genericProc.Emit(OpCodes.Ldarg_1); //ldarg1 because not static method
        genericProc.Emit(OpCodes.Call, monoActionToIl2ActionUntypedRef);
        genericProc.Emit(OpCodes.Call, uniTaskAwaiterOnCompletedGeneric);
        untypedProc.Emit(OpCodes.Nop);
        genericProc.Emit(OpCodes.Ret);
        Log(" - Success!");

        Log(" - Filling in method body for the non-generic awaiter...");
        untypedProc.Emit(OpCodes.Nop);
        untypedProc.Emit(OpCodes.Ldarg_0);
        untypedProc.Emit(OpCodes.Ldarg_1); //ldarg1 because not static method
        untypedProc.Emit(OpCodes.Call, monoActionToIl2ActionUntypedRef);
        untypedProc.Emit(OpCodes.Call, uniTaskAwaiterOnCompletedUntyped);
        untypedProc.Emit(OpCodes.Nop);
        untypedProc.Emit(OpCodes.Ret);
        Log(" - Success!");

        Log(" - Filling in method body for yield awaiter");
        yieldProc.Emit(OpCodes.Nop);
        yieldProc.Emit(OpCodes.Ldarg_0);
        yieldProc.Emit(OpCodes.Ldarg_1);
        yieldProc.Emit(OpCodes.Call, monoActionToIl2ActionUntypedRef);
        yieldProc.Emit(OpCodes.Call, yieldAwaiterOnCompleted);
        yieldProc.Emit(OpCodes.Nop);
        yieldProc.Emit(OpCodes.Ret);
        Log(" - Success!");

        Log(" - Filling in method body for main thread awaiter");
        mainThreadProc.Emit(OpCodes.Nop);
        mainThreadProc.Emit(OpCodes.Ldarg_0);
        mainThreadProc.Emit(OpCodes.Ldarg_1);
        mainThreadProc.Emit(OpCodes.Call, monoActionToIl2ActionUntypedRef);
        mainThreadProc.Emit(OpCodes.Call, yieldAwaiterOnCompleted);
        mainThreadProc.Emit(OpCodes.Nop);
        mainThreadProc.Emit(OpCodes.Ret);
        Log(" - Success!");

        Log(" - Filling in method body for thread pool awaiter");
        threadPoolProc.Emit(OpCodes.Nop);
        threadPoolProc.Emit(OpCodes.Ldarg_0);
        threadPoolProc.Emit(OpCodes.Ldarg_1);
        threadPoolProc.Emit(OpCodes.Call, monoActionToIl2ActionUntypedRef);
        threadPoolProc.Emit(OpCodes.Call, yieldAwaiterOnCompleted);
        threadPoolProc.Emit(OpCodes.Nop);
        threadPoolProc.Emit(OpCodes.Ret);
        Log(" - Success!");

        Log(" - Assigning and optimizing method bodies");
        notifyCompletionGeneric.Body = genericBody;
        notifyCompletionUntyped.Body = untypedBody;
        yieldComplete.Body = yieldBody;
        mainThreadComplete.Body = mainThreadBody;
        threadPoolComplete.Body = threadPoolBody;
        notifyCompletionGeneric.Body.Optimize();
        notifyCompletionUntyped.Body.Optimize();
        yieldComplete.Body.Optimize();
        mainThreadComplete.Body.Optimize();
        threadPoolComplete.Body.Optimize();
        Log(" - Success!");

        Log(" - Adding new methods to UniTask Awaiters...");
        uniTaskAwaiterGeneric.Methods.Add(notifyCompletionGeneric);
        uniTaskAwaiterUntyped.Methods.Add(notifyCompletionUntyped);
        yieldAwaiter.Methods.Add(yieldComplete);
        yieldAwaiter.Methods.Add(mainThreadComplete);
        yieldAwaiter.Methods.Add(threadPoolComplete);
        Log(" - Success!");

        Log("Success!");

        Log("Making UniTask Awaiters implement interface...");
        uniTaskAwaiterGeneric.Interfaces.Add(new InterfaceImplementation(notifyCompletionRef));
        uniTaskAwaiterUntyped.Interfaces.Add(new InterfaceImplementation(notifyCompletionRef));
        yieldAwaiter.Interfaces.Add(new InterfaceImplementation(notifyCompletionRef));
        switchToMainThreadAwaiter.Interfaces.Add(new InterfaceImplementation(notifyCompletionRef));
        switchToThreadPoolAwaiter.Interfaces.Add(new InterfaceImplementation(notifyCompletionRef));
        Log("Success!");


        // This aint happening cuz IAsyncStateMachine shit. If someone wants to work through this by themself, go ahead and do it, 'cause I'm not.
        //Log("Working on the UniTask builder...");
        //Log("Creating AsyncMethodBuilderAttribute...");
        //TypeDefinition asyncBuilderAttr = new("System.Runtime.CompilerServices", "AsyncMethodBuilderAttribute", TypeAttributes.Sealed | TypeAttributes.BeforeFieldInit | TypeAttributes.NotPublic, attributeRef);

        //Log(" - Creating BuilderType property...");
        //PropertyDefinition builderTypeProp = new("BuilderType", PropertyAttributes.None, typeRef);
        //FieldDefinition builderTypeBacking = new($"<{builderTypeProp.Name}>k__BackingField", FieldAttributes.Private, typeRef);

        //MethodDefinition builderTypePropGet = new("get_" + builderTypeProp.Name, MethodAttributes.RTSpecialName | MethodAttributes.Public, typeRef);

        //Log(" - Filling in BuilderType getter...");
        //ILProcessor builderTypeProc = builderTypePropGet.Body.GetILProcessor();
        //builderTypeProc.Emit(OpCodes.Ldarg_0);
        //builderTypeProc.Emit(OpCodes.Ldfld, builderTypeBacking);
        //builderTypeProc.Emit(OpCodes.Ret);

        //Log(" - Filled in BuilderType getter. Binding backing field and property to AsyncMethodBuilderAttribute.");
        //asyncBuilderAttr.Fields.Add(builderTypeBacking);
        //asyncBuilderAttr.Properties.Add(builderTypeProp);




        Log("The moment of truth: Writing new UniTask assembly...");
        asm.Write(tempStream);
        Log("Success! The hard part's over, UniTask is now patched!");

        Log("Cleaning up temporary items...");

        Log(" - Flushing & closing file writer...");
        tempStream.Flush();
        tempStream.Dispose();
        Log(" - Success!");

        Log(" - Closing UniTask.dll (Disposing)...");
        asm.Dispose();
        Log(" - Success!");

        Log(" - Copying temporary file to UniTask.dll path...");
        File.WriteAllBytes(unitaskAsmPath, tempStream.ToArray());
        Log(" - Success!");

        Log("Success!");
    }
}
