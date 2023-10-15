using BoneLib.RandomShit;
using Cysharp.Threading.Tasks;
using Jevil.PostProcessing;
using Jevil.Spawning;
using Jevil.Tweening;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Jevil.IMGUI;

/// <summary>
/// Contains methods to draw things onto the IMGUI. Will only draw things in a DEBUG build.
/// </summary>
public static class DebugDraw
{
#if DEBUG
    internal static bool IsActive { get; private set; }
    private static readonly List<GUIToken> tokensActive = new();
    private static readonly List<GUIToken> tokensInactive = new(1) 
    { Button("Toggle JGUI", GUIPosition.TOP_RIGHT, () => Toggle()) };


    private static readonly int GuiGap = Utilities.IsPlatformQuest() ? 15 : 5;
    private static readonly int GuiCornerDist = Utilities.IsPlatformQuest() ? 350 : 20;

    // 0     1
    // 2     3
    private static int[] pagination = new int[4]; // TL, TR, BL, BR
    private static GUIToken[] paginateTokens = new GUIToken[12];
    private static bool[] paginates = new bool[] { true, true, true, true };
    private static int[] pageIdx = new int[4];

    private static GameObject tweenTarget;
    private static List<GUIToken> standardJevilTokens = new();
#endif

    /// <summary>
    /// Draws a box with the text <paramref name="text"/> inside, at <paramref name="position"/> on screen.
    /// </summary>
    /// <param name="text">The text to appear in the box.</param>
    /// <param name="position">The position of the text.</param>
    /// <returns>A <see cref="GUIToken"/> that can be sent to <see cref="Dont(GUIToken)"/> if you no longer wish for it to be drawn.</returns>
    public static GUIToken Text(string text, GUIPosition position)
    {
        GUIToken ret = new(text);
#if DEBUG
        ret.position = position;
        tokensActive.Add(ret);
#endif
        return ret;
    }

    /// <summary>
    /// Tracks a variable using a getter you provide and the ToString provided by that value.
    /// </summary>
    /// <param name="varName">The variable's name</param>
    /// <param name="position">The element's position on the screen</param>
    /// <param name="getter">The variable getter. Will be called in Update.</param>
    /// <returns>A <see cref="GUIToken"/> that can be sent to <see cref="Dont(GUIToken)"/> if you no longer wish for it to be drawn.</returns>
    /// <example>
    /// <c>TrackVariable(nameof(spawnedPoolees), GUIPosiiton.TOP_LEFT, () => spawnedPoolees);</c>
    /// <br>Will appear as "spawnedPoolees: 4" (assuming spawnedPoolees == 4)</br>
    /// </example>
    public static GUIToken TrackVariable<T>(string varName, GUIPosition position, Func<T> getter)
    {
        Func<object> boxedGetter = () => { return getter(); };
        GUIToken ret = new(varName, boxedGetter);
#if DEBUG
        ret.position = position;
        tokensActive.Add(ret);
#endif
        return ret;
    }

    /// <summary>
    /// Draws a button at <paramref name="position"/> on screen, labeled with <paramref name="text"/>, calling <paramref name="call"/> when pressed.
    /// </summary>
    /// <param name="text">The button's label</param>
    /// <param name="position">The position onscreen to draw the IMGUI element.</param>
    /// <param name="call">The call to invoke when the button is pressed.</param>
    /// <returns>A <see cref="GUIToken"/> that can be sent to <see cref="Dont(GUIToken)"/> if you no longer wish for it to be drawn.</returns>
    public static GUIToken Button(string text, GUIPosition position, Action call)
    {
        GUIToken ret = new(text, call);
#if DEBUG
        ret.position = position;
        // dont draw buttons on quest
        if (!Utilities.IsPlatformQuest()) 
            tokensActive.Add(ret);
#endif
        return ret;
    }

    /// <summary>
    /// Draw a text field with a button beside it, that automatically resizes based on whats inside it.
    /// </summary>
    /// <param name="startingText">The starting text inside the text area.</param>
    /// <param name="buttonText">The text in the button.</param>
    /// <param name="position">The position on screen to draw the IMGUI elements.</param>
    /// <param name="call">A delegate that will have the text in the text field passed into it when called.</param>
    /// <returns>A <see cref="GUIToken"/> that can be sent to <see cref="Dont(GUIToken)"/> if you no longer wish for it to be drawn.</returns>
    public static GUIToken TextButton(string startingText, string buttonText, GUIPosition position, Action<string> call)
    {
        GUIToken ret = new(startingText, buttonText, call);
#if DEBUG
        ret.position = position;
        tokensActive.Add(ret);
#endif
        return ret;
    }

    /// <summary>
    /// Draw a text field with a button beside it, that automatically resizes based on whats inside it. Button text is just "CALL".
    /// </summary>
    /// <param name="startingText">The starting text inside the text area.</param>
    /// <param name="position">The position on screen to draw the IMGUI elements.</param>
    /// <param name="call">A delegate that will have the text in the text field passed into it when called.</param>
    /// <returns>A <see cref="GUIToken"/> that can be sent to <see cref="Dont(GUIToken)"/> if you no longer wish for it to be drawn.</returns>
    public static GUIToken TextButton(string startingText, GUIPosition position, Action<string> call) 
        => TextButton(startingText, "CALL", position, call);

    /// <summary>
    /// Stops drawing a token.
    /// </summary>
    /// <param name="token">Any token, can be inactive, just means nothing will be done.</param>
    public static void Dont(GUIToken token)
    {
#if DEBUG
        int idx = tokensActive.IndexOf(token);
        if (idx != -1) tokensActive.RemoveAt(idx);
#endif
    }

    /// <summary>
    /// Toggles JeviLib IMGUI. 
    /// <br>After being called there will be a button in the top right of the screen to toggle Jevil IMGUI again.</br>
    /// </summary>
    /// <returns>Whether JGUI is active after being toggled.</returns>
    public static bool Toggle()
    {
#if DEBUG
        IsActive = !IsActive;
        return IsActive;
#else
        return default;
#endif
    }


#if DEBUG
    internal static void PerformDraw()
    {

        try
        {
            List<GUIToken> tokens = IsActive ? tokensActive : tokensInactive;

            try
            {
                foreach (GUIToken tkn in tokens)
                {
                    try
                    {
                        if (tkn.type == GUIType.TRACKER) tkn.SetText(tkn.txtAlt + ": " + tkn.getter().ToString());
                    }
                    catch (Exception ex)
                    {
                        JeviLib.Error($"Exception while grabbing variable for IMGUI:\n\t\t{ex.GetType().FullName} '{ex.Message}'\n\t\t\t@ {ex.TargetSite.DeclaringType.FullName}.{ex.TargetSite.Name} (in {ex.Source})");
                    }
                }
            }
            catch (Exception ex)
            {
                JeviLib.Error($"Exception while enumerating {nameof(GUIType)}.{GUIType.TRACKER} tokens. Are you calling DebugDraw in a variable checker?");
                JeviLib.Error($"Exception information: Source = '{ex.Source}'; Exception = {ex}");
            }

            // assumed 12px for a button. default is 10 but im tryna make it a bit roomy
            int pxForOneButton = (GuiGap * 2) + 15;
            int drawnPerColumn = ((Screen.height - GuiCornerDist) / 2 / pxForOneButton) - 4;

            // these are only being enumerated once, so keeping them in an IEnumerable instead of ToArraying isn't a concern
            IEnumerable<GUIToken> topLeft = tokens.Where(t => t.position == GUIPosition.TOP_LEFT);
            IEnumerable<GUIToken> topRight = tokens.Where(t => t.position == GUIPosition.TOP_RIGHT);
            IEnumerable<GUIToken> bottomLeft = tokens.Where(t => t.position == GUIPosition.BOTTOM_LEFT);
            IEnumerable<GUIToken> bottomRight = tokens.Where(t => t.position == GUIPosition.BOTTOM_RIGHT);

            //if (paginate)
            //{
            //    topLeft = topLeft.Skip(pageIdx * drawnPerColumn).Take(drawnPerColumn);
            //    topRight = topRight.Skip(pageIdx * drawnPerColumn).Take(drawnPerColumn);
            //    bottomLeft = bottomLeft.Skip(pageIdx * drawnPerColumn).Take(drawnPerColumn);
            //    bottomRight = bottomRight.Skip(pageIdx * drawnPerColumn).Take(drawnPerColumn - paginateTokens.Length).Prepend(paginateTokens);
            //}

            // Draw top left
            int screenCorner = 0;
            int maxWidth = 0;
            int xStart = GuiCornerDist;
            int yStart = GuiCornerDist;
            if (paginates[screenCorner])
                topLeft = topLeft.Skip(pagination[screenCorner] * drawnPerColumn).Take(drawnPerColumn);
            if (IsActive) topLeft = paginateTokens.Skip(screenCorner * 3).Take(3).Concat(topLeft);
            foreach (GUIToken token in topLeft)
            {
                if (maxWidth < token.width) maxWidth = token.width;
                Rect rekt = new(xStart, yStart, maxWidth, token.height);
                DrawToken(rekt, token);
                yStart += token.height + GuiGap;

                if (yStart + token.height + GuiGap > Screen.height / 2)
                {
                    yStart = GuiCornerDist;
                    xStart += maxWidth;
                    maxWidth = 0;
                }
            }

            // Draw top right
            screenCorner++;
            maxWidth = 0;
            xStart = Screen.width - GuiCornerDist;
            yStart = GuiCornerDist;
            if (paginates[screenCorner])
                topRight = topRight.Skip(pagination[screenCorner] * drawnPerColumn).Take(drawnPerColumn);
            if (IsActive) topRight = paginateTokens.Skip(screenCorner * 3).Take(3).Concat(topRight);
            foreach (GUIToken token in topRight)
            {
                if (maxWidth < token.width) maxWidth = token.width;
                Rect rekt = new(xStart - maxWidth, yStart, maxWidth, token.height);
                DrawToken(rekt, token);
                yStart += token.height + GuiGap;

                if (yStart + token.height + GuiGap > Screen.height / 2)
                {
                    yStart = GuiCornerDist;
                    xStart -= maxWidth;
                    maxWidth = 0;
                }
            }

            // Draw bottom left
            screenCorner++;
            maxWidth = 0;
            xStart = GuiCornerDist;
            yStart = Screen.height - GuiCornerDist;
            if (paginates[screenCorner])
                bottomLeft = bottomLeft.Skip(pagination[screenCorner] * drawnPerColumn).Take(drawnPerColumn);
            if (IsActive) bottomLeft = paginateTokens.Skip(screenCorner * 3).Take(3).Concat(bottomLeft);
            foreach (GUIToken token in bottomLeft)
            {
                if (maxWidth < token.width) maxWidth = token.width;
                Rect rekt = new(xStart, yStart - token.height, maxWidth, token.height);
                DrawToken(rekt, token);
                yStart -= token.height + GuiGap;

                if (yStart - token.height - GuiGap < Screen.height / 2)
                {
                    yStart = Screen.height - GuiCornerDist;
                    xStart += maxWidth;
                    maxWidth = 0;
                }
            }

            // Draw bottom right
            screenCorner++;
            maxWidth = 0;
            xStart = Screen.width - GuiCornerDist;
            yStart = Screen.height - GuiCornerDist;
            if (paginates[screenCorner])
                bottomRight = bottomRight.Skip(pagination[screenCorner] * drawnPerColumn).Take(drawnPerColumn);
            if (IsActive) bottomRight = paginateTokens.Skip(screenCorner * 3).Take(3).Concat(bottomRight);
            foreach (GUIToken token in bottomRight)
            {
                if (maxWidth < token.width) maxWidth = token.width;
                Rect rekt = new(xStart - maxWidth, yStart - token.height, maxWidth, token.height);
                DrawToken(rekt, token);
                yStart -= token.height + GuiGap;

                if (yStart - token.height - GuiGap < Screen.height / 2)
                {
                    yStart = Screen.height - GuiCornerDist;
                    xStart -= maxWidth;
                    maxWidth = 0;
                }
            }
        }
        catch (Exception ex)
        {
            JeviLib.Error($"Exception thrown while drawing IMGUI:\n\t{ex}");
        }
    }


    private static void DrawToken(Rect rect, GUIToken token)
    {
        switch (token.type)
        {
            case GUIType.TRACKER:
            case GUIType.TEXT:
                GUI.Box(rect, token.txt);
                break;
            case GUIType.BUTTON:
                if (GUI.Button(rect, token.txt)) token.call();
                break;
            case GUIType.TEXT_BUTTON:
                token.SetText(GUI.TextField(rect, token.txt));

                Rect button = rect;
                button.width = token.txtAlt.Length * 7f + 12f;
                if (token.position == GUIPosition.TOP_LEFT || token.position == GUIPosition.BOTTOM_LEFT)
                    button.x += rect.width + GuiGap;
                else button.x -= +GuiGap + button.width;

                if (GUI.Button(button, token.txtAlt)) token.callStr(token.txt);
                break;
            default:
                break;
        }
    }

    internal static void InitTokens()
    {
        standardJevilTokens.Add(Button("Remove JeviLib Debug tokens", GUIPosition.TOP_RIGHT, ClearStandardTokens));
        standardJevilTokens.Add(Button("Spawn cube", GUIPosition.TOP_LEFT, () => { tweenTarget = GameObject.CreatePrimitive(PrimitiveType.Cube); tweenTarget.GetComponent<Renderer>().material.shader = Shader.Find(Const.UrpLitName); }));
        standardJevilTokens.Add(Button("Pos -> V3.One", GUIPosition.TOP_LEFT, () => { tweenTarget.transform.TweenPosition(Vector3.one, 1); }));
        standardJevilTokens.Add(Button("Pos -> -V3.One", GUIPosition.TOP_LEFT, () => { tweenTarget.transform.TweenPosition(-Vector3.one, 1); }));
        standardJevilTokens.Add(Button("Scl -> V3.One", GUIPosition.TOP_LEFT, () => { tweenTarget.transform.TweenLocalScale(Vector3.one, 1); }));
        standardJevilTokens.Add(Button("Scl -> V3.Zero", GUIPosition.TOP_LEFT, () => { tweenTarget.transform.TweenLocalScale(Vector3.zero, 1); }));
        standardJevilTokens.Add(Button("Rot -> Euler(V3.Zero)", GUIPosition.TOP_LEFT, () => { tweenTarget.transform.TweenRotation(Quaternion.Euler(0, 0, 0), 1); }));
        standardJevilTokens.Add(Button("Rot -> Euler(0,180,0)", GUIPosition.TOP_LEFT, () => { tweenTarget.transform.TweenRotation(Quaternion.Euler(0, 180, 0), 1); }));
        standardJevilTokens.Add(Button("Test spawning", GUIPosition.TOP_LEFT, TestSpawning));
        standardJevilTokens.Add(Button("Test UniTask async", GUIPosition.TOP_LEFT, TestUniTaskAsync));
        standardJevilTokens.Add(Button("PBM.CNSPU", GUIPosition.TOP_RIGHT, () => { PopupBoxManager.CreateNewShibePopup(); }));

            
        for (int i = 0; i < paginateTokens.Length / 3; i++)
        {
            int _i = i;
            paginateTokens[i * 3] = new GUIToken("Paginate", void () => paginates[_i] = !paginates[_i]);
            paginateTokens[i * 3 + 1] = new GUIToken("Pg++", void () => pagination[_i]++);
            paginateTokens[i * 3 + 2] = new GUIToken("Pg--", void () => pagination[_i]--);
        }

        
        foreach (Type type in typeof(PostProcessingMaterials).GetNestedTypes())
        {
            MethodInfo enableMethod = type.GetMethod("Enable", Const.AllBindingFlags);
            MethodInfo disableMethod = type.GetMethod("Disable", Const.AllBindingFlags);

            if (enableMethod == null || disableMethod == null)
            {
                JeviLib.Log($"PostProcessingMaterials JGUI mapping failure: {type.FullName} missing dis/enable method!");
                continue;
            }

            Action enable = (Action)enableMethod.CreateDelegate(typeof(Action));
            Action disable = (Action)disableMethod.CreateDelegate(typeof(Action));
            standardJevilTokens.Add(Button("Enable FX: " + type.Name, GUIPosition.TOP_RIGHT, enable));
            standardJevilTokens.Add(Button("Disable FX: " + type.Name, GUIPosition.TOP_RIGHT, disable));

        }
    }


    private static void ClearStandardTokens()
    {
        foreach (GUIToken token in standardJevilTokens)
        {
            Dont(token);
        }

        standardJevilTokens.Clear();
    }

    private static void TestSpawning()
    {
        Barcodes.SpawnAsync(JevilBarcode.MP5, Vector3.zero, Quaternion.identity);
    }

    private static async void TestUniTaskAsync() // async void ! bad !!!
    {
        JeviLib.Log("Waiting 2sec");
        await UniTask.Delay(Il2CppSystem.TimeSpan.FromSeconds(2), DelayType.UnscaledDeltaTime, PlayerLoopTiming.Update, new Il2CppSystem.Threading.CancellationToken());
        JeviLib.Log("Hello after waiting 2sec!");
        JeviLib.Log("Are we still on the main thread?");
        GameObject.CreatePrimitive(PrimitiveType.Cube);
        JeviLib.Log("If we're still here, then YES we are on the main thread! UniTask and JeviLib did its job!");
        JeviLib.Log("Testing UniTask patches. Waiting 3sec on each.");
        await UniTask.Delay(3000, true);
        JeviLib.Log("Check-in 1");
        await UniTask.Delay(3000, DelayType.UnscaledDeltaTime);
        JeviLib.Log("Check-in 2");
        await UniTask.Delay(Il2CppSystem.TimeSpan.FromSeconds(3), true);
        JeviLib.Log("Check-in 3! All checks passed!");
    }
#endif
}
