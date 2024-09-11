namespace JeviLib.Research;

public static class FuncCalling
{
    private record class TestClass1(byte B);
    private record struct TestStruct1(byte B);
    private record struct TestStruct2(ushort S);
    private record struct TestStruct3(byte B, ushort S);
    private record struct TestStruct4(uint I);

    static Func<int, int, byte> twoParameterFunc = (_, _) => default;

    static byte TwoParameterCaller(int a, int b)
    {
        return twoParameterFunc(a, b);
    }

    static void TwoParameterCaller(int a, int b, ref byte c)
    {
        c = twoParameterFunc(a, b);

        /*
         *  .method private hidebysig static 
         *      void TwoParameterObjectCaller (
         *          int32 a,
         *          int32 b,
         *          object& c
         *      ) cil managed 
         *  {
         *      // Method begins at RVA 0x21d2
         *      // Header size: 1
         *      // Code size: 16 (0x10)
         *      .maxstack 8
         *
         *      // {
         *      IL_0000: nop
         *      // c = twoParameterObjectFunc(a, b);
         *      IL_0001: ldarg.2
         *      IL_0002: ldsfld class [System.Runtime]System.Func`3<int32, int32, object> JeviLib.Research.FuncCalling::twoParameterObjectFunc
         *      IL_0007: ldarg.0
         *      IL_0008: ldarg.1
         *      IL_0009: callvirt instance !2 class [System.Runtime]System.Func`3<int32, int32, object>::Invoke(!0, !1)
         *      IL_000e: stind.ref
         *      // }
         *      IL_000f: ret
         *  } // end of method FuncCalling::TwoParameterObjectCaller
         */
    }

    static Func<int, int, object> twoParameterObjectFunc = (_, _) => default!;

    static object TwoParameterObjectCaller(int a, int b)
    {
        return twoParameterObjectFunc(a, b);
    }

    static bool TwoParameterRefCaller(int a, int b, ref object c)
    {
        c = twoParameterObjectFunc(a, b);
        return true;
    }

    static Func<int, int, TestClass1> twoParameterClassFunc1 = (_, _) => default!;

    static void TwoParameterClassCaller(int a, int b, ref TestClass1 c)
    {
        c = twoParameterClassFunc1(a, b);
    }

    static void TwoParameterObjectCaller(int a, int b, ref object c)
    {
        c = twoParameterObjectFunc(a, b);
    }

    static Func<int, int, TestStruct1> twoParameterStructFunc1 = (_, _) => default!;

    static void TwoParameterStructCaller1(int a, int b, ref TestStruct1 c)
    {
        c = twoParameterStructFunc1(a, b);
    }

    static Func<int, int, TestStruct2> twoParameterStructFunc2 = (_, _) => default!;

    static void TwoParameterStructCaller2(int a, int b, ref TestStruct2 c)
    {
        c = twoParameterStructFunc2(a, b);
    }

    static Func<int, int, TestStruct3> twoParameterStructFunc3 = (_, _) => default!;

    static void TwoParameterStructCaller3(int a, int b, ref TestStruct3 c)
    {
        c = twoParameterStructFunc3(a, b);
    }

    static Func<int, int, TestStruct4> twoParameterStructFunc4 = (_, _) => default!;

    static void TwoParameterStructCaller4(int a, int b, ref TestStruct4 c)
    {
        c = twoParameterStructFunc4(a, b);
    }


    static Func<int, int, int, int, int, int, int, int, int, byte> nineParameterFunc = (_, _, _, _, _, _, _, _, _) => default;

    static bool NineParameterCaller(int a, int b, int c, int d, int e, int f, int g, int h, int j)
    {
        nineParameterFunc(a, b, c, d, e, f, g, h, j);
        return false;
    }


    static bool NineParameterCaller(int a, int b, int c, int d, int e, int f, int g, int h, int j, ref object k)
    {
        k = nineParameterFunc(a, b, c, d, e, f, g, h, j);
        return true;
    }

    static void Log(string message)
    {
        Console.WriteLine(message);
    }

    static Func<bool> flunk = () => default;
    static bool NoParameterCaller()
    {
        return !flunk();
    }

    static bool NoParameterCaller2()
    {
        Log("j");
        return !flunk();
    }

    static bool NoParameterCaller3()
    {
        bool v = !flunk();
        Log("j " + v);
        return v;
    }
}
