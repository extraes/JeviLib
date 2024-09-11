using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Rendering;

namespace Jevil.Internal.SelfTesting;

internal static class Patching
{
    public static class Hook
    {
        static void NoTarget()
        {
            JeviLib.Log($"Target (none)");
        }

        static void NoIntercept()
        {
            JeviLib.Log($"Intercept (none)");
        }

        static void ThreeTarget(int one, int two, int three)
        {
            JeviLib.Log($"Target: {one} {two} {three}");
        }

        static void ThreeIntercept(int one, int two, int three)
        {
            JeviLib.Log($"Intercept: {one} {two} {three}");
        }

        static void SixTarget(int one, int two, int three, int four, int five, int six)
        {
            JeviLib.Log($"Target: {one} {two} {three} {four} {five} {six}");
        }

        static void SixIntercept(int one, int two, int three, int four, int five, int six)
        {
            JeviLib.Log($"Intercept: {one} {two} {three} {four} {five} {six}");
        }

        static long TwoReturn(int one, int two)
        {
            JeviLib.Log($"Target: {one} + {two} = {one + two}");
            return one + two;
        }

        static long TwoReturnIntercept(int one, int two)
        {
            JeviLib.Log($"Intercept: {one} * {two} = {one * two}");
            return one * two;
        }

        public static void RunTestHook()
        {
            JeviLib.Log("Running targets before patching");
            NoTarget();
            ThreeTarget(1, 2, 3);
            SixTarget(1, 2, 3, 4, 5, 6);

            JeviLib.Log("Patching NoTarget");
            Jevil.Patching.Hook.OntoDelegate(NoTarget, NoIntercept);
            JeviLib.Log("Running notarget after patching");
            NoTarget();

            JeviLib.Log("Patching ThreeTarget");
            Jevil.Patching.Hook.OntoDelegate(ThreeTarget, ThreeIntercept);
            JeviLib.Log("Running threetarget after patching");
            ThreeTarget(1, 2, 3);

            JeviLib.Log("Patching SixTarget");
            Jevil.Patching.Hook.OntoDelegate(SixTarget, SixIntercept);
            JeviLib.Log("Running six target after patching");
            SixTarget(1, 2, 3, 4, 5, 6);
        }


        public static void RunTestRedirect(bool skipOriginal)
        {
            JeviLib.Log("Running targets before patching");
            NoTarget();
            ThreeTarget(1, 2, 3);
            SixTarget(1, 2, 3, 4, 5, 6);
            JeviLib.Log("TwoReturn(2, 3) outputs: " + TwoReturn(2, 3));

            JeviLib.Log("Patching NoTarget");
            Jevil.Patching.Redirect.FromDelegate(NoTarget, NoIntercept, skipOriginal);
            JeviLib.Log("Running notarget after patching");
            NoTarget();

            JeviLib.Log("Patching ThreeTarget");
            Jevil.Patching.Redirect.FromDelegate(ThreeTarget, ThreeIntercept, skipOriginal);
            JeviLib.Log("Running threetarget after patching");
            ThreeTarget(1, 2, 3);

            JeviLib.Log("Patching SixTarget");
            Jevil.Patching.Redirect.FromDelegate(SixTarget, SixIntercept, skipOriginal);
            JeviLib.Log("Running six target after patching");
            SixTarget(1, 2, 3, 4, 5, 6);

            JeviLib.Log("Patching TwoReturn");
            Jevil.Patching.Redirect.FromDelegate(TwoReturn, TwoReturnIntercept, skipOriginal);
            JeviLib.Log("TwoReturn(2, 3) outputs: " + TwoReturn(2, 3));
        }

        static Func<bool> halfOfTheTime = () => UnityEngine.Random.value > 0.5f;
        static bool HalfTheTime()
        {
            return UnityEngine.Random.value > 0.5f;
        }

        //public static void RunTestDisable()
        //{
        //    JeviLib.Log("Running targets before patching");
        //    NoTarget();
        //    ThreeTarget(1, 2, 3);
        //    SixTarget(1, 2, 3, 4, 5, 6);
        //    JeviLib.Log("TwoReturn(2, 3) outputs: " + TwoReturn(2, 3));

        //    JeviLib.Log("Patching NoTarget");
        //    Jevil.Patching.Disable.When(HalfTheTime, NoIntercept);
        //    JeviLib.Log("Running notarget after patching");
        //    NoTarget();
        //    NoTarget();
        //    NoTarget();
        //    NoTarget();

        //    JeviLib.Log("Patching ThreeTarget");
        //    Jevil.Patching.Disable.When(HalfTheTime, ThreeIntercept);
        //    JeviLib.Log("Running threetarget after patching");
        //    ThreeTarget(1, 2, 3);
        //    ThreeTarget(1, 2, 3);
        //    ThreeTarget(1, 2, 3);
        //    ThreeTarget(1, 2, 3);

        //    JeviLib.Log("Patching SixTarget");
        //    Jevil.Patching.Disable.When(HalfTheTime, SixIntercept);
        //    JeviLib.Log("Running six target after patching");
        //    SixTarget(1, 2, 3, 4, 5, 6);
        //    SixTarget(1, 2, 3, 4, 5, 6);
        //    SixTarget(1, 2, 3, 4, 5, 6);
        //    SixTarget(1, 2, 3, 4, 5, 6);
        //}

        public static void RunTestDisable()
        {
            throw new NotImplementedException();
            //JeviLib.Log("Running targets before patching");
            //NoTarget();
            //ThreeTarget(1, 2, 3);
            //SixTarget(1, 2, 3, 4, 5, 6);
            //JeviLib.Log("TwoReturn(2, 3) outputs: " + TwoReturn(2, 3));

            //JeviLib.Log("Patching NoTarget");
            //Jevil.Patching.Disable.FromMethod(typeof(Hook).GetMethod(nameof(NoIntercept), Const.AllBindingFlags));
            //JeviLib.Log("Running notarget after patching");
            //NoTarget();

            //JeviLib.Log("Patching ThreeTarget");
            //Jevil.Patching.Disable.FromMethod(typeof(Hook).GetMethod(nameof(ThreeIntercept), Const.AllBindingFlags));
            //JeviLib.Log("Running threetarget after patching");
            //ThreeTarget(1, 2, 3);

            //JeviLib.Log("Patching SixTarget");
            //Jevil.Patching.Disable.FromMethod(typeof(Hook).GetMethod(nameof(SixIntercept), Const.AllBindingFlags));
            //JeviLib.Log("Running six target after patching");
            //SixTarget(1, 2, 3, 4, 5, 6);
        }

        delegate long Deal(int one, int two);
        static Deal d;
        static bool Example(int one, int two, ref long __result)
        {
            __result = d(one, two);
            return false;
        }

    }

}
