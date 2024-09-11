namespace JeviLib.Research;

public static class ActionCalling
{
    static Action noParameterAction = () => { };

    static void NoParameterCaller()
    {
        noParameterAction();
    }

    static Action<int> oneParameterAction = (_) => { };

    static void OneParameterCaller()
    {
        oneParameterAction(0);
    }

    static Action<int, int> twoParameterAction = (_, _) => { };

    static void TwoParameterCaller()
    {
        twoParameterAction(0, 0);
    }

    static Action<int, int, int, int, int, int, int, int, int, int> tenParameterAction = (_, _, _, _, _, _, _, _, _, _) => { };

    static void TenParameterCaller(int a, int b, int c, int d, int e, int f, int g, int h, int j, int k)
    {
        tenParameterAction(a, b, c, d, e, f, g, h, j, k);
    }


    static Action<int, int, int> threeParameterAction = (_, _, _) => { };

    static void ThreeParameterCaller(int a, int b, int c)
    {
        threeParameterAction(a, b, c);
    }
}