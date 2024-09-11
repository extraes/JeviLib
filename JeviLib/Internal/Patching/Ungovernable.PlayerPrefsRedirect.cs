using HarmonyLib;
using Jevil.Patching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Jevil.Internal.Patching
{
    internal static partial class Ungovernable
    {
        static readonly MethodInfo BadSetInt = Jevil.Utilities.AsInfo(UnityEngine.PlayerPrefs.SetInt);
        static readonly MethodInfo BadSetFloat = Jevil.Utilities.AsInfo(UnityEngine.PlayerPrefs.SetFloat);
        static readonly MethodInfo BadSetString = Jevil.Utilities.AsInfo(UnityEngine.PlayerPrefs.SetString);

        static readonly HarmonyMethod RedirectorTranspiler = Jevil.Utilities.ToHarmony(PlayerPrefsTranspiler);

        private static void TranspilePlayerPrefs(Assembly asm)
        {
#if DEBUG
            JeviLib.Log($"Transpiling PlayerPrefs usages for {asm.FullName}");
#endif
            int count = 0;
            try
            {
                foreach (Type type in asm.GetTypes())
                {
                    if (type.IsGenericType)
                    {
#if DEBUG
                        JeviLib.Warn($"Unable to patch methods on {type.FullName} - it is generic! This means skipping out on {type.GetMethods(Const.AllBindingFlags).Length} method(s)!");
#endif
                        continue;
                    }

                    foreach (MethodInfo method in type.GetMethods(Const.AllBindingFlags))
                    {


                        if (method.IsGenericMethod)
                        {
#if DEBUG
                            JeviLib.Warn("Unable to patch method " + method.FullDescription() + " - it is generic!");
#endif
                            continue;
                        }

                        Harmony.Patch(method, transpiler: RedirectorTranspiler);

                        count++;
                    }
                }

            }
            catch (Exception e)
            {
                JeviLib.Log($"Failed to patch all methods in {asm.GetName().Name}. See: {e}");
            }


#if DEBUG
            JeviLib.Log($"Done transpiling PlayerPrefs usages for {asm.FullName}");
#endif
        }

        static IEnumerable<CodeInstruction> PlayerPrefsTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            foreach (var instruction in instructions)
            {
                if (instruction.operand is not MethodInfo minf)
                {
                    yield return instruction;
                    continue;
                }
#if DEBUG
                else if (instruction.operand is not null)
                {
                    JeviLib.Log($"Instruction operand is of type '{instruction.opcode.OperandType}', operand is {instruction.operand} (Whole instruction: {instruction})");
                }
#endif

                if (minf == BadSetInt)
                    instruction.operand = PlayerPrefsReplacements.SetInt;
                else if (minf == BadSetFloat)
                    instruction.operand = PlayerPrefsReplacements.SetFloat;
                else if (minf == BadSetString)
                    instruction.operand = PlayerPrefsReplacements.SetString;
#if DEBUG
                else
                    continue;

                JeviLib.Log("Replaced call to a playerprefs method with: " + instruction.operand);
#endif

            }
        }
    }
}
