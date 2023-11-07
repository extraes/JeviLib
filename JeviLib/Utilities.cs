using BoneLib;
using BoneLib.RandomShit;
using Cysharp.Threading.Tasks;
using HarmonyLib;
using MelonLoader;
using PuppetMasta;
using SLZ.Interaction;
using SLZ.Marrow.Pool;
using SLZ.VRMK;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnhollowerBaseLib;
using UnityEngine;

namespace Jevil;

/// <summary>
/// A collection of utility methods I felt the need for while making Chaos. This may get refactored into a bunch of smaller classes
/// </summary>
public static class Utilities
{
#if DEBUG
    static Type inspectorManager;
#endif

    static IntPtr il2cppDomain = IL2CPP.il2cpp_domain_get(); // should just be thread agnostic
    static bool fusionLoaded = AppDomain.CurrentDomain.GetAssemblies().Any(asm => !asm.IsDynamic && asm.GetName().Name == "LabFusion");
    static bool? uniTasksNeedPatch;
    static bool? coroutinesNeedPatch;

    /// <summary>
    /// Tells whether the currently running game is installed from the Oculus store using a basic file check.
    /// </summary>
    /// <returns>Whether Boneworks_Oculus_Windows64.exe exists in the application path.</returns>
    public static bool IsSteamVersion()
    {
        return UnityEngine.Application.dataPath.EndsWith("BONELAB_Steam_Windows64_Data");
    }

    /// <summary>
    /// Uses <see cref="UnityEngine.Random.Range(int, int)"/> to determine what to return.
    /// </summary>
    /// <returns><see cref="Player.leftHand"/> or <see cref="Player.rightHand"/></returns>
    public static Hand GetRandomPlayerHand()
    {
        int randomNum = UnityEngine.Random.Range(0, 2);
        if (randomNum == 1)
            return Player.leftHand;
        else
            return Player.rightHand;
    }

    /// <summary>
    /// Spawns a <see cref="BoneLib"/> Ad and then moves it in front of, and makes it face, the player.
    /// </summary>
    /// <param name="str">The text to be displayed on the Ad.</param>
    /// <returns></returns>
    public static GameObject SpawnAd(string str)
    {
        GameObject ad = PopupBoxManager.CreateNewPopupBox(str);
        MoveAndFacePlayer(ad);
        return ad;
    }

    /// <summary>
    /// <see cref="UnityEngine.Object.FindObjectsOfTypeAll(Il2CppSystem.Type)"/> except LINQ Select'd to Cast to the given type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">A type extending <see cref="UnityEngine.Object"/></typeparam>
    /// <returns>The output of FindObjectsOfTypeAll, ran through Select. It is recommended you ToArray it to avoid running the selector every access.</returns>
    public static IEnumerable<T> FindAll<T>() where T : UnityEngine.Object
    {
        // cannot use LINQ .Cast<T> as that does not use the IL2CPP Cast method
        return GameObject.FindObjectsOfTypeAll(UnhollowerRuntimeLib.Il2CppType.Of<T>()).Select(obj => obj.Cast<T>());
    }

    /// <summary>
    /// Basically the <c>Split</c> method on <see cref="string"/>, but it returns a 2 element array instead. It's hard to describe without an example
    /// <code>
    /// Argsify("part1,part2,alsopart2", ','); // Returns { "part1", "part2,alsopart2" } 
    /// </code>
    /// </summary>
    /// <param name="data"></param>
    /// <param name="split"></param>
    /// <returns>The first element alone, with the following elements joined in one string.</returns>
    public static string[] Argsify(string data, char split = ',')
    {
        string[] args = data.Split(split);
        string arg = args[0];
        string argg = string.Join(split.ToString(), args.Skip(1));
        return new string[] { arg, argg };
    }

    /// <summary>
    /// Takes the byte representation of a <see cref="Vector3"/> and returns the Vector3 it represents. Useful for serialization.
    /// </summary>
    /// <param name="bytes">
    /// The byte representation of a Vector3, AKA the byte representation of 3 <see cref="float"/>s.
    /// <para>Debug builds have a check to warn in case the source array is too short for the given <paramref name="startIdx"/> or if it's not the correct size.</para>
    /// </param>
    /// <param name="startIdx">The index to start taking bytes from the <paramref name="bytes"/> array to represent as floats to construct a new <see cref="Vector3"/></param>
    /// <returns></returns>
    public static Vector3 DebyteV3(byte[] bytes, int startIdx = 0)
    {
#if DEBUG
        if (startIdx == 0 && bytes.Length != Const.SizeV3) JeviLib.Warn($"Trying to debyte a Vector3 of length {bytes.Length}, this is not the expected {Const.SizeV3} bytes!");
        if (startIdx + Const.SizeV3 > bytes.Length) JeviLib.Warn($"{bytes.Length} is too short for the given index of {startIdx}");
#endif
        return new Vector3(
            BitConverter.ToSingle(bytes, startIdx),
            BitConverter.ToSingle(bytes, startIdx + sizeof(float)),
            BitConverter.ToSingle(bytes, startIdx + sizeof(float) * 2));
    }

    /// <summary>
    /// Takes the output of <see cref="SerializeMesh(Vector3[])"/> and converts it into vertices (that can be piped into <see cref="MeshFilter.mesh"/>)
    /// <para>Can also be used to serialize a bunch of positions and rotations, and then deserialize them. IDK what for, that one's for you to decide.</para>
    /// </summary>
    /// <param name="bytes">Byte representation of the mesh vertices</param>
    /// <returns>The vertices</returns>
    public static Vector3[] DeserializeMesh(byte[] bytes)
    {
        Vector3[] result = new Vector3[bytes.Length / Const.SizeV3];
#if DEBUG
        if (bytes.Length % Const.SizeV3 != 0) JeviLib.Warn("Malformed byte array - not divisible by the size of a vector3");
#endif
        for (int i = 0; i < bytes.Length; i += Const.SizeV3)
        {
            result[i / Const.SizeV3] = DebyteV3(bytes, i);
        }

        return result;
    }

    /// <summary>
    /// Serializes a mesh into bytes to be saved or networked.
    /// </summary>
    /// <param name="vectors">Mesh vertices</param>
    /// <returns>An array of the mesh vertices, represented as bytes.</returns>
    public static byte[] SerializeMesh(Vector3[] vectors)
    {
        byte[] bytes = new byte[vectors.Length * sizeof(float) * 3];
        for (int i = 0; i < vectors.Length; i++)
        {
            int offset = i * sizeof(float) * 3;
            byte[] _vecB = vectors[i].ToBytes();
            Buffer.BlockCopy(_vecB, 0, bytes, offset, sizeof(float) * 3);
        }
        return bytes;
    }

    /// <summary>
    /// Moves an object in front of the player and rotates it to face the player, with its up remaining the global up.
    /// </summary>
    /// <param name="obj">The object to move and rotate</param>
    public static void MoveAndFacePlayer(GameObject obj)
    {
        Transform phead = Player.playerHead;
        Vector3 position = phead.position + phead.forward.normalized * 2;
        Quaternion rotation = Quaternion.LookRotation(obj.transform.position - phead.position, Vector3.up);
        obj.transform.SetPositionAndRotation(position, rotation);
    }

    /// <summary>
    /// So I basically made this method over the course of a few days in my Comp Sci class and I dont quite recall how it works aside from the rundown.
    /// <para>The first byte dictates how many arrays were in the source 2dArray. The following <c>[value of the first byte] * 2</c> bytes compose the rest of the header.</para>
    /// <para>The next pairs of bytes are used as ushorts that tell how long each corresponding array was</para>
    /// </summary>
    /// <param name="bytess">The source arrays to be joined together.</param>
    /// <returns>A single array consisting of a header and the values of the source arrays.</returns>
    /// <remarks>
    /// <para><b>Here's a rundown of how this method packs arrays. Note that ushorts are two bytes long, so this method will fail if one of your arrays is over 64KB in length.</b></para>
    /// <c>
    /// <br>Input: [</br>
    /// <br>....[1,2,3,4,5,6,7]</br>
    /// <br>....[2,3,4,5,6,7,8,9,10]</br>
    /// <br>]</br>
    /// <br>Output: [</br>
    /// <br>....2, (header, says how many things there are)</br>
    /// <br>....ushorts(7, 9)  (header, says lengths of elements, so method knows where to split)</br>
    /// <br>....1,2,3,4,5,6,7,</br>
    /// <br>....2,3,4,5,6,7,8,9,10</br>
    /// <br>]</br>
    /// </c></remarks>
    public static byte[] JoinBytes(byte[][] bytess)
    {
        byte arrayCount = (byte)bytess.Length;
        // bytes.Length - 1 because you dont put a delim at the end (unless youre weird i guess, but im not)
        List<ushort> indices = new(bytess.Length - 1);
        byte[] bytesJoined = new byte[(bytess.Length * 2 + 1) + bytess.Sum(b => b.Length)];
        // need to get the indices beforehand to compose the header
        foreach (byte[] arr in bytess)
        {
            indices.Add((ushort)arr.Length);
        }
        // Compose the header
        bytesJoined[0] = arrayCount;
        for (int i = 0; i < indices.Count; i++)
        {
            ushort idx = indices[i];
            BitConverter.GetBytes(idx).CopyTo(bytesJoined, i * sizeof(ushort) + 1);
        }

        ushort lastLen = 0;
        int index = arrayCount * 2 + 1;
        for (int i = 0; i < arrayCount; i++)
        {
            int thisLen = indices[i];
            Buffer.BlockCopy(bytess[i], 0, bytesJoined, index, thisLen);

            // set the next index
            lastLen = (ushort)thisLen;
            index += thisLen;
        }

        return bytesJoined;
    }

    /// <summary>
    /// Split an array of bytes recieved from <see cref="JoinBytes(byte[][])"/> into an array of arrays.
    /// </summary>
    /// <param name="bytes">The byte array with the proper header generated by JoinBytes This method will malfunction if the header is malformed.</param>
    /// <returns>A byte-for-byte copy of the original byte[][] before it was sent to <see cref="JoinBytes(byte[][])"/> and likely saved.</returns>
    public static byte[][] SplitBytes(byte[] bytes)
    {
        // Grab data from the header
        // get number of splits
        int splitsCount = bytes[0];
        // actually get the list of element sizes
        int[] splitIndices = new int[splitsCount];
        for (int i = 0; i < splitsCount; i++)
        {
            // add 1 to skip the byte that says how many split indicies there are
            ushort idx = BitConverter.ToUInt16(bytes, 1 + i * sizeof(ushort));
            splitIndices[i] = idx;
        }

        byte[][] res = new byte[splitsCount][];
        // start after the header
        int lastIdx = splitsCount * sizeof(ushort) + 1;
        for (int i = 0; i < res.Length; i++)
        {
            int len = splitIndices[i];
            // dont overallocate lest there be a dead byte
            byte[] arr = new byte[len];
            // copy the bytes
            Buffer.BlockCopy(bytes, lastIdx, arr, 0, len);


            // save our changes to the array 
            res[i] = arr;
            // perpetuate the nevereding cycle
            lastIdx += len;
        }
        return res;
    }

    /// <summary>
    /// Returns a user-friendly name from the name of a member. Only works with camelCase. WhateverCaseYouCallThis might work too. But not THIS_CASE or this_case.
    /// </summary>
    /// <param name="name">Member name, like "modToggleEnable"</param>
    /// <returns>Friendly name, like "Mod Enable Toggle"</returns>
    public static string GenerateFriendlyMemberName(string name)
    {
        StringBuilder builder = new(name.Length);
        int startIdx = 0;
        if (name.StartsWith("m_")) 
            startIdx += 2;

        builder.Append(char.ToUpper(name[startIdx]));

        for (int i = startIdx + 1; i < name.Length; i++)
        {
            // use char's cause theyre on the stack not heap, and i want to avoid allocating shit 
            char character = name[i];

            if (char.IsUpper(character))
            {
                builder.Append(' ');
                builder.Append(char.ToUpper(character));
            }
            else builder.Append(character);
        }
        return builder.ToString();
    }

    /// <summary>
    /// Redirect to <see cref="Extensions.Flatten(byte[][])"/>
    /// <br>Joins an array of bytes directly one after another, without regard for reproduceability. Use this for things of fixed size, like byte representations of <see cref="float"/>s or <see cref="int"/>s.</br>
    /// </summary>
    /// <param name="arrays">The arrays to be concatenated one after another.</param>
    /// <returns>An array of bytes whose length is equal to the sum of the lengths of the input arrays.</returns>
    public static byte[] Flatten(params byte[][] arrays) => Extensions.Flatten(arrays);

    /// <summary>
    /// Gets the byte representation of 
    /// </summary>
    /// <returns></returns>
    public static byte[] SerializeInFrontFacingPlayer()
    {
        byte[] res = new byte[Const.SizeV3 * 2];

        Vector3 inFrontOfPlayer = Player.playerHead.position + Player.playerHead.forward * 2;
        inFrontOfPlayer.ToBytes().CopyTo(res, 0);
        Quaternion.LookRotation(-Player.playerHead.forward).eulerAngles.ToBytes().CopyTo(res, Const.SizeV3);

        return res;
    }

    /// <summary>
    /// Reads an array of bytes as if contained a position and rotation, packed as follows: Position vector, followed immediately by an euler angles rotation.
    /// </summary>
    /// <param name="bytes">Source byte array. A warning will be logged if your array is too short for the data that is expected to be read off, likely followed by an error from the <see cref="BitConverter"/> class.</param>
    /// <param name="startIdx">The index at which to start reading the array.</param>
    /// <returns>A position and rotation, likely from <see cref="SerializeInFrontFacingPlayer"/></returns>
    public static (Vector3, Quaternion) DebytePosRot(byte[] bytes, int startIdx = 0)
    {
#if DEBUG
        if (startIdx == 0 && bytes.Length != Const.SizeV3 * 2) JeviLib.Warn($"Trying to debyte a posrot of length {bytes.Length}, this is not the expected {Const.SizeV3 * 2} bytes!");
        if (startIdx + Const.SizeV3 * 2 > bytes.Length) JeviLib.Warn($"{bytes.Length} is too short for the given index of {startIdx}");
#endif
        return
        (
            new Vector3
            (
                BitConverter.ToSingle(bytes, startIdx),
                BitConverter.ToSingle(bytes, startIdx + sizeof(float)),
                BitConverter.ToSingle(bytes, startIdx + sizeof(float) * 2)
            ),
            Quaternion.Euler
            (
                new Vector3
                (
                    BitConverter.ToSingle(bytes, startIdx + Const.SizeV3),
                    BitConverter.ToSingle(bytes, startIdx + Const.SizeV3 + sizeof(float)),
                    BitConverter.ToSingle(bytes, startIdx + Const.SizeV3 + sizeof(float) * 2)
                )
            )
        );
    }

    /// <summary>
    /// Gets all rigidbodies from the given <paramref name="puppet"/>, using <see cref="PuppetMaster.muscles"/> and LINQ.
    /// </summary>
    /// <param name="puppet">The puppet to get muscles from.</param>
    /// <returns>The rigidbodies of the muscles. I'm not sure if they're assured to be not <see langword="null"/>, so you may want to run </returns>
    public static IEnumerable<Rigidbody> GetMuscleRigidbodies(PuppetMaster puppet)
    {
        return puppet.muscles.Select(s => s.rigidbody);
    }

    /// <summary>
    /// Attempts to retrieve a <see cref="Type"/> using only its name and namespace, and an asynchronously created cache.
    /// <para>If the cache isn't built yet, this method will likely fail. Check via <see cref="JeviLib.DoneMappingNamespacesToAssemblies"/>.</para>
    /// </summary>
    /// <param name="namezpaze">The name of the namespace, without a leading or trailing period.</param>
    /// <param name="clazz">The Type's name.</param>
    /// <returns>The <see cref="Type"/> represented by the combined namespace and class name, or <see langword="null"/> if it wasn't found.</returns>
    public static Type GetTypeFromString(string namezpaze, string clazz)
    {
#if DEBUG
        if (!JeviLib.DoneMappingNamespacesToAssemblies) JeviLib.Log("Not done mapping namespaces to assemblies! Getting type from string may falsely return null!");
#endif

        if (JeviLib.namespaceAssemblies.TryGetValue(namezpaze, out ConcurrentBag<Assembly> asms))
        {

#if DEBUG
            JeviLib.Log($"Found the namespace '{namezpaze}' in {asms.Count} assemblies, attempting to find the type {clazz}");
#endif

            foreach (Assembly asm in asms)
            {
                Type t = asm.GetType(namezpaze + "." + clazz);
                if (t != null) return t;
            }

#if DEBUG
            JeviLib.Warn($"Unable to find a Type named '{clazz}' in the namespace '{namezpaze}' in any loaded Assemblies");
#endif
        }
#if DEBUG
        else JeviLib.Warn($"Unable to find the namespace '{namezpaze}' in any loaded assemblies (This is probably fine/expected behavior)");
#endif
        return null;
    }

    /// <summary>
    /// Get the <see cref="MethodInfo"/> behind any delegate. It can be a lambda or an actual method. See examples for examples.
    /// </summary>
    /// <typeparam name="TDelegate">Any delegate type, can be an Action (returns nothing/void) or a Func (returns something).</typeparam>
    /// <param name="method">Any delegate, can be an Action (returns nothing/void) or a Func (returns something).</param>
    /// <returns>The underlying <see cref="MethodInfo"/>, gotten from <see cref="Delegate.Method"/>.</returns>
    /// <example>
    /// <c>AsInfo(MethodNameHere);</c> That's it.
    /// <c>AsInfo(() => { return "top 10 things to do in michigan\n1. leave"; });</c> That's it.
    /// </example>
    public static MethodInfo AsInfo<TDelegate>(TDelegate method) where TDelegate : Delegate
    {
        // this is a bit chickenshit i think
        // it feels too easy
        return method.Method;
    }

    /// <summary>
    /// Creates a new harmony method from the delegate's MethodInfo.
    /// </summary>
    /// <typeparam name="TDelegate">Any delegate type, can be Func or Action</typeparam>
    /// <param name="method">a method or delegate</param>
    /// <returns>A <see cref="HarmonyMethod"/> of the delegate.</returns>
    public static HarmonyMethod ToHarmony<TDelegate>(TDelegate method) where TDelegate : Delegate
    {
        return AsInfo(method).ToNewHarmonyMethod();
    }

    /// <summary>
    /// Clones an instance of <see cref="BaseEnemyConfig.HealthSettings"/> because it's not a <see cref="UnityEngine.Object"/>, thus <see cref="UnityEngine.Object.Instantiate(UnityEngine.Object)"/> (cloning) doesn't work on it.
    /// </summary>
    /// <param name="hs">The health settings to be cloned.</param>
    /// <returns>A new instance of <see cref="BaseEnemyConfig.HealthSettings"/> with the values of the original.</returns>
    public static BaseEnemyConfig.HealthSettings CloneHealthSettings(BaseEnemyConfig.HealthSettings hs)
    {
        return new()
        {
            aggression = hs.aggression,
            irritability = hs.irritability,
            maxAppendageHp = hs.maxAppendageHp,
            maxHitPoints = hs.maxHitPoints,
            maxStunSeconds = hs.maxStunSeconds,
            minHeadImpact = hs.minHeadImpact,
            minLimbImpact = hs.minLimbImpact,
            minSpineImpact = hs.minSpineImpact,
            placability = hs.placability,
            stunRecovery = hs.stunRecovery,
            vengefulness = hs.vengefulness
        };
    }

    /// <summary>
    /// Creates a <see cref="Thread"/>, sets <see cref="Thread.IsBackground"/> to <see langword="true"/>, starts it, then returns it.
    /// </summary>
    /// <param name="call">An action to be invoked</param>
    /// <returns>A background thread.</returns>
    public static Thread StartBGThread(Action call)
    {
        Thread thread = new(() => { call(); });
        thread.IsBackground = true;
        thread.Start();
        return thread;
    }

    /// <summary>
    /// Creates a <see cref="Thread"/>, sets <see cref="Thread.IsBackground"/> to <see langword="true"/>, starts it, then returns it.
    /// </summary>
    /// <param name="call">An action to be invoked</param>
    /// <param name="param">The parameter to be passed into the call.</param>
    /// <returns>A background thread.</returns>
    public static Thread StartBGThread<T>(Action<T> call, T param)
    {
        Thread thread = new((object obj) => { call((T)obj); });
        thread.IsBackground = true;
        thread.Start(param);
        return thread;
    }

    /// <summary>
    /// Focuses something in a UnityExplorer inspector, if it is installed (and if this is in Debug mode).
    /// </summary>
    /// <param name="obj">Any object, can be a <see cref="Component"/>, <see cref="GameObject"/>, or even a normal Mono domain thing.</param>
    public static void InspectInUnityExplorer(object obj)
    {
#if DEBUG
        inspectorManager ??= GetTypeFromString("UnityExplorer", "InspectorManager");
        Type cobj = GetTypeFromString("UnityExplorer.CacheObject", "CacheObjectBase");
        // public static void Inspect(object obj, CacheObjectBase sourceCache = null)
        MethodInfo minf = inspectorManager?.GetMethod("Inspect", new Type[] { typeof(object), cobj });
        minf?.Invoke(null, new object[] { obj, null });
#endif
    }

    /// <summary>
    /// Focuses a <see cref="Type"/> in a UnityExplorer inspector using static reflection, if it is installed (and if this is in Debug mode).
    /// </summary>
    /// <param name="t">Any Type. Can be Mono domain or IL2CPP.</param>
    public static void InspectInUnityExplorer(Type t)
    {
#if DEBUG
        inspectorManager ??= GetTypeFromString("UnityExplorer", "InspectorManager");
        // public static void Inspect(object obj, CacheObjectBase sourceCache = null)
        MethodInfo minf = inspectorManager?.GetMethod("Inspect", new Type[] { typeof(Type) });
        minf?.Invoke(null, new object[] { t });

#endif
    }

    /// <summary>
    /// Gets a MethodInfo based entirely off of strings, so no direct assembly reference is required to interop with other mods.
    /// </summary>
    /// <param name="namezpaze">The Type's namespace. This will be used to look through assemblies with this namespace defined.</param>
    /// <param name="clazz">The Type's name.</param>
    /// <param name="method">The method's name.</param>
    /// <param name="paramTypes">The names of types of the parameters. <i>Do NOT include the namespaces in the names.</i></param>
    /// <returns></returns>
    public static MethodInfo GetMethodFromString(string namezpaze, string clazz, string method, string[] paramTypes = null)
    {
#if DEBUG
        if (!JeviLib.DoneMappingNamespacesToAssemblies)
        {
            JeviLib.Warn("Unlikely to get successfully get method from string if namespace mapping is incomplete!");
        }
#endif

        Type type = Utilities.GetTypeFromString(namezpaze, clazz);
        if (type == null)
        {
#if DEBUG
            JeviLib.Log($"Unable to find the namespace '{namezpaze}' in any loaded Assemblies.");
#endif
            return null;
        }

        MethodInfo minf = null;
        if (paramTypes == null) minf = type.GetMethodEasy(method);
        else
        {
            MethodInfo[] minfs = type.GetMethods(Const.AllBindingFlags).Where(m => m.Name == method).ToArray();
            foreach (MethodInfo meth in minfs)
            {
                string[] paramStrs = meth.GetParameters().Select(pi => pi.ParameterType.Name).ToArray();
                if (paramStrs.SequenceEqual(paramTypes))
                {
                    minf = meth;
                    break;
                }
            }
        }

#if DEBUG
        if (minf == null)
        {
            string extraText = paramTypes == null ? "" : $"that matches the given {paramTypes.Length} parameter(s).";
            JeviLib.Log($"The type {namezpaze}.{clazz} (from assembly {type.Assembly.GetName().Name}) doesn't have a method named {method} {extraText}");
        }
#endif

        return minf;
    }

    /// <summary>
    /// Returns <see langword="true"/> if the current platform is Ocul- I mean Faceb- I mean <i>META</i> Quest.
    /// </summary>
    /// <returns>If <see cref="MelonUtils.CurrentPlatform"/> is <c>3</c>.</returns>
    public static bool IsPlatformQuest()
        => MelonUtils.CurrentPlatform == (MelonPlatformAttribute.CompatiblePlatforms)3;

    /// <summary>
    /// Returns whether yielding WaitForSeconds/WaitForSecondsRealtime/another coroutine will work as expected, or if it will only yield for one frame.
    /// <br>JeviLib v2.0.0 restores expected functionality by replacing the IL2CPP support module (in this context "patching" it) on PCVR for better performance, but using Harmony to intercept coroutines and wrap them on Quest.</br>
    /// </summary>
    /// <returns><see langword="true"/> if yielding always waits only one frame. <see langword="false"/> if yielding works as expected (e.g. if the support module has been replaced).</returns>
    public static bool CoroutinesNeedPatch()
    {
        // returns 
        if (coroutinesNeedPatch.HasValue) return coroutinesNeedPatch.Value;
        if (IsPlatformQuest())
        {
            coroutinesNeedPatch = false;
            return coroutinesNeedPatch.Value;
        }

        Assembly supportModule = AppDomain.CurrentDomain.GetAssemblies().First(asm => !asm.IsDynamic && asm.Location.ToLower().Contains("support") && asm.Location.EndsWith(@"Il2Cpp.dll"));
        Type monoEnumeratorWrapper = supportModule.GetType("MelonLoader.Support.MonoEnumeratorWrapper");
        FieldInfo enumeratorWaitTimeField = monoEnumeratorWrapper.GetField("waitTime", Const.AllBindingFlags);
        coroutinesNeedPatch = enumeratorWaitTimeField == null;
#if DEBUG
        JeviLib.Log($"Defined fields in {monoEnumeratorWrapper.FullName}");
        foreach (FieldInfo field in monoEnumeratorWrapper.GetFields(Const.AllBindingFlags))
            JeviLib.Log($"{field.FieldType.Name} {field.Name}");
#endif
        return coroutinesNeedPatch.Value;
    }

    /// <summary>
    /// Returns whether a <see cref="Cysharp.Threading.Tasks.UniTask"/> can be <see langword="await"/>ed by mod code.
    /// <br>JeviLib v1.2.0 restores <see langword="await"/> functionality on UniTasks by modifying ("patching") <see cref="Cysharp.Threading.Tasks.UniTask.Awaiter"/> and <see cref="Cysharp.Threading.Tasks.UniTask{T}.Awaiter"/> to make them implement INotifyCompletion.</br>
    /// </summary>
    /// <returns><see langword="true"/> if UniTasks are unable to be <see langword="await"/>ed by mod code. <see langword="false"/> if UniTask.dll has already been patched.</returns>
    public static bool UniTasksNeedPatch()
    {
        if (uniTasksNeedPatch.HasValue) return uniTasksNeedPatch.Value;

        Type[] implementedInterfaces = typeof(Cysharp.Threading.Tasks.UniTask.Awaiter).GetInterfaces();
        uniTasksNeedPatch = implementedInterfaces.Length == 0;
#if DEBUG
        JeviLib.Log($"UniTask Awaiter implements {implementedInterfaces.Length} interfaces. Fix needs application? {uniTasksNeedPatch}");
#endif
        return uniTasksNeedPatch.Value;
    }

    /// <summary>
    /// Restarts the game.
    /// Note: on Android this just quits the game, as the methods necessary to perform a full restart have been stripped.
    /// </summary>
    public static void RestartGame()
    {
        if (IsPlatformQuest())
        {
            Application.Quit();
            return;
        }

        Process myProc = Process.GetCurrentProcess();
        new Process()
        {
            StartInfo = new ProcessStartInfo()
            {
                FileName = Application.dataPath.Replace("_Data", ".exe"), // this path should only be hit on win, i think im good
                WorkingDirectory = MelonUtils.BaseDirectory,
            },
        }.Start();
        Application.Quit();
    }

    /// <summary>
    /// Restarts (or quits if on Android) the game if JevilFixer patches are unapplied (if coroutines still need fixing or UniTasks still need fixing)
    /// </summary>
    public static void RestartIfPatchesUnapplied()
    {
        if (UniTasksNeedPatch() || CoroutinesNeedPatch())
        {
            JeviLib.Log("This game needs patches applied - restarting!");
            RestartGame();
        }
    }

    /// <summary>
    /// Returns whether the package with the given ID <paramref name="appId"/> is installed on the given system. This is currently borked.
    /// </summary>
    /// <param name="appId">The package app ID. Usually looks something like "com.StressLevelZero.BONELAB"</param>
    /// <returns><see langword="true"/> if the package's "launch intents" were found (IDK, ask that guy from the Unity forums), <see langword="false"/> if it wasn't or if the current platform is not android.</returns>
    public static bool IsAndroidPackageInstalled(string appId)
    {
        if (!IsPlatformQuest()) return false;
        throw new NotImplementedException();

        AndroidJavaClass up = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject ca = up.GetStatic<AndroidJavaObject>("currentActivity");
        AndroidJavaObject packageManager = ca.Call<AndroidJavaObject>("getPackageManager", new UnhollowerBaseLib.Il2CppReferenceArray<Il2CppSystem.Object>(0));
        AndroidJavaObject launchIntent = null;
        //if the app is installed, no errors. Else, doesn't get past next line
        try
        {
            Il2CppReferenceArray<Il2CppSystem.Object> parameter = new(1);
            parameter[0] = appId;
            //object[] parameters = { appId };
            launchIntent = packageManager.Call<AndroidJavaObject>("getLaunchIntentForPackage", parameter);
            //        
            //        ca.Call("startActivity",launchIntent);
        }
        catch (Exception ex)
        {
            JeviLib.Warn("Exception while checking package installation. Likely harmless: " + ex);
        }

        if (launchIntent == null)
            return false;

        return true;
    }

    /// <summary>
    /// Builds a loggable string that's similar to what's logged when calling NodeJS's (or whatever browser you have) <c>console.log</c> function.
    /// </summary>
    /// <param name="obj">The object to build a string from.</param>
    /// <param name="maxRecurseDepth">Determines the depth of how far to JS-like string for every single member of <paramref name="obj"/> and every member of every member, etc.</param>
    /// <returns>A <c>console.log</c>-like string. Does not include color codes.</returns>
    public static string BuildJavaScriptLogStr(object obj, int maxRecurseDepth = 3)
    {
        if (obj is null) return "<null>";
        if (obj is Il2CppSystem.Object il2obj && il2obj.WasCollected) return "<Collected in the IL2CPP domain>";

        StringBuilder sb = new StringBuilder();

        Internal.Utilities.ObjectDumper.InternalDump(0, "Logged object", obj, sb, new(), maxRecurseDepth);
        
        return sb.ToString();
    }

    /// <summary>
    /// Changes the strength (forces, torques, etc) of a <see cref="PhysHand"/>
    /// </summary>
    /// <param name="hand">The hand whose values will be change</param>
    /// <param name="mult">Strength multiplier</param>
    /// <param name="torque">Torque multiplier</param>
    public static void ChangeStrength(PhysHand hand, float mult = 200f, float torque = 10f)
    {

        /*
         * THIS CODE IS NOT MINE - THIS CODE IS NOT MINE - THIS CODE IS NOT MINE - THIS CODE IS NOT MINE - THIS CODE IS NOT MINE - THIS CODE IS NOT MINE
         * I GANKED THE FUCK OUT OF THIS CODE FROM LAKATRAZZ'S SPIDERMAN MOD
         *      IT'S NOT OPEN SOURCE SO I HOPE I DON'T GET SUED, BUT IF HE WANTS THIS GONE, FINE BY ME, ILL REMOVE IT
         *      I DNSPY'D THE SPIDERMAN MOD AND COPY PASTED THE SPIDERMAN.MODOPTIONS.MULTIPLYFORCES METHOD
         *      I THINK ITS FINE THO CAUSE I ASKED HIM AND HE SAID "just take code from any of my mods tbh i dont care"
         *      (https://discord.com/channels/563139253542846474/724595991675797554/913613134885974036)
         * THIS CODE IS NOT MINE - THIS CODE IS NOT MINE - THIS CODE IS NOT MINE - THIS CODE IS NOT MINE - THIS CODE IS NOT MINE - THIS CODE IS NOT MINE
         */

        hand.xPosForce = 90f * mult;
        hand.yPosForce = 90f * mult;
        hand.zPosForce = 340f * mult;
        hand.xNegForce = 90f * mult;
        hand.yNegForce = 200f * mult;
        hand.zNegForce = 360f * mult;
        hand.newtonDamp = 80f * mult;
        hand.angDampening = torque;
        hand.dampening = 0.2f * mult;
        hand.maxTorque = 30f * torque;
    }


    /// <summary>
    /// Returns whether BONELAB Fusion (AKA LabFusion.dll) is loaded (present in the current AppDomain)
    /// </summary>
    /// <returns>If an assembly with the title "LabFusion" is loaded.</returns>
    public static bool IsFusionLoaded()
    {
        return fusionLoaded;
    }

    /// <summary>
    /// Gives you the sibling indices from <paramref name="parent"/> to <paramref name="child"/>.
    /// <para>SSBL uses a similar method for saving constraints.</para>
    /// <para>Uses byte arrays to minimize memory allocation. If you need one that specifically uses <see langword="int"/>'s, ping me about it.</para>
    /// </summary>
    /// <param name="parent">The parent transform.</param>
    /// <param name="child">Child transform. Must not be the same as <paramref name="parent"/>. Can be null.</param>
    /// <returns>An array of hierarchy indices.</returns>
    public static byte[] GetHierarchyIndices(Transform parent, Transform child)
    {
        // precalculate depth to avoid overallocating
        int depthParent = HierarchalDepth(parent);
        int depthChild = HierarchalDepth(child);
        int arrSize = depthChild - depthParent;
#if DEBUG
        if (!child.IsChildOf(parent)) throw new ArgumentException($"Given transform {child.name} is not a child of the given parent {parent.name}. Parent full path: {parent.GetFullPath()} Child full path: {child.GetFullPath()}");
#endif
        byte[] ret = new byte[arrSize];

        // do reverse so loading can be done in forward order
        for (int i = arrSize - 1; i >= 0; i--)
        {
            ret[i] = (byte)child.GetSiblingIndex();
            child = child.parent;
        }

        return ret;
    }

    /// <summary>
    /// Gives you how deep <paramref name="transform"/> is in the hierarchy AKA how many parents it has.
    /// </summary>
    /// <param name="transform">Any transform. Can be null (will return 0).</param>
    /// <returns></returns>
    public static int HierarchalDepth(Transform transform)
    {
        if (transform == null) return 0;

        int depth = 0;

        while (transform.parent != null)
        {
            depth++;
            transform = transform.parent;
        }
        
        return depth;
    }

    /// <summary>
    /// Traverses the hierarchy down to an object. Use in tandem with <see cref="GetHierarchyIndices(Transform, Transform)"/>.
    /// <para>Uses byte arrays to minimize memory allocation.</para>
    /// </summary>
    /// <param name="transform">The parent transform. Cannot be null.</param>
    /// <param name="childIdxs">The transform children indices as returned from <see cref="GetHierarchyIndices(Transform, Transform)"/></param>
    /// <returns>The target transform as designated by <paramref name="childIdxs"/>.</returns>
    public static Transform TraverseHierarchy(Transform transform, byte[] childIdxs)
    {
        if (transform.INOC()) throw new ArgumentNullException(nameof(transform));

        Transform ret = transform;

        foreach (byte idx in childIdxs)
        {
            ret = ret.GetChild(idx);
        }

        return ret;
    }

    /// <summary>
    /// Creates a directory, but if the parent directory doesn't exist, it creates it. If the parent's parent doesn't exist, it creates it. And so on.
    /// </summary>
    /// <param name="directory">A folder, not a file path. See <see cref="Path.GetDirectoryName(string)"/>.</param>
    public static void CreateDirectoryRecursive(string directory)
    {
        string parentDir = Path.GetDirectoryName(directory);

        if (!Directory.Exists(parentDir)) CreateDirectoryRecursive(parentDir);
        
        Directory.CreateDirectory(directory);
    }

    /// <summary>
    /// Gets a byte array representing primitives. Only guaranteed to work with primitive types, like <see langword="int"/>, <see langword="ushort"/>, or <see langword="long"/>.
    /// </summary>
    /// <typeparam name="T">A primitive type. May work with vectors. Probably won't.</typeparam>
    /// <param name="primitives">An array of primitives.</param>
    /// <returns>An array of the bytes of primitives.</returns>
    public static unsafe byte[] SerializePrimitives<T>(T[] primitives) where T : unmanaged
    {
        int size = sizeof(T);
        byte[] ret = new byte[size * primitives.Length];

        fixed (byte* arrayPtr = ret)
        {
            for (int i = 0; i < primitives.Length; i++)
            {
                *(T*)(arrayPtr + size * i) = primitives[i];
            }
        }

        return ret;
    }

    /// <summary>
    /// Uses unsafe code to place a value in an array, "serializing" it.
    /// <para>This will work with all pre-defined "blittable" types, like ints and floats, but may not work for Vectors when using different unhollowed assemblies.</para>
    /// </summary>
    public static unsafe void SerializeInPlace<T>(byte[] array, T value, int offset = 0) where T : unmanaged
    {
#if DEBUG
        if (array.Length < offset + sizeof(T))
            JeviLib.Warn($"Trying to serialize a {sizeof(T)}-length value into an array of length {array.Length} with a {offset}-byte offset... This will not end well!");
#endif
        fixed (byte* arrayPtr = array)
        {
            T* arrayAsArrayTPtr = (T*)(arrayPtr + offset);
            *arrayAsArrayTPtr = value;
        }
    }

    // theoretically the generic version would work fine with Vector3's but its better to be safe than sorry.
    /// <summary>
    /// Does what <see cref="Extensions.ToBytes(Vector3)"/> does, but for an array <i>you</i> give it.
    /// </summary>
    public static unsafe void SerializeInPlace(byte[] array, Vector3 value, int offset = 0)
    {
        fixed (byte* arrayPtr = array)
        {
            *(float*)arrayPtr = value.x;
            *(float*)(arrayPtr + sizeof(float)) = value.y;
            *(float*)(arrayPtr + sizeof(float) * 2) = value.z;
        }
    }

    /// <summary>
    /// Effectively shorthand for <c>IL2CPP.il2cpp_thread_attach(IL2CPP.il2cpp_domain_get());</c>
    /// </summary>
    public static void AttachIl2CppToThread()
    {
        IL2CPP.il2cpp_thread_attach(il2cppDomain);
    }

    /// <summary>
    /// Determines whether OBS is running. Doesn't check to see if it's currently recording.
    /// </summary>
    /// <returns>Whether or not a process named 'obs64' was found on the computer, or <see langword="false"/> on Quest.</returns>
    public static bool IsOBSRunning()
    {
        return !IsPlatformQuest() && Process.GetProcessesByName("obs64").Length != 0;
    }
}