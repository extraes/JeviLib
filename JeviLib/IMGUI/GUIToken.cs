using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Jevil.IMGUI;

/// <summary>
/// Represents an IMGUI element to be drawn in OnGUI.
/// </summary>
public sealed class GUIToken
{
    /// <summary>
    /// The position where this GUIToken will be drawn.
    /// </summary>
    public GUIPosition position;
    /// <summary>
    /// Width of the IMGUI box. Is automatically set by <see cref="SetText(string)"/> to <c>(3 * text.Length) + 5</c>
    /// </summary>
    public int width;
    /// <summary>
    /// Height of the IMGUI box. Defaults to 10.
    /// </summary>
    public int height = 20;
    internal Func<object?> getter;
    internal Action call;
    internal Action<string> callStr;
    internal string txt;
    internal string txtAlt;
    internal GUIType type;

    internal GUIToken(string text)
    {
#if DEBUG
        type = GUIType.TEXT;
        SetText(text);
#endif
    }

    internal GUIToken(string text, Func<object?> call)
    {
#if DEBUG
        type = GUIType.TRACKER;
        this.getter = call;
        this.txtAlt = text;
        SetText(text);
#endif
    }

    internal GUIToken(string text, Action call)
    {
#if DEBUG
        type = GUIType.BUTTON;
        this.call = call;
        txtAlt = text;
        SetText(text);
#endif
    }

    internal GUIToken(string text, string btnTxt, Action<string> call)
    {
#if DEBUG
        type = GUIType.TEXT_BUTTON;
        this.callStr = call;
        this.txtAlt = btnTxt;
        SetText(text);
#endif
    }

    /// <summary>
    /// Set the text of the GUIToken. Can be spammed as it has an equality check.
    /// </summary>
    /// <param name="text">The new text to be set.</param>
    public void SetText(string text)
    {
#if DEBUG
        if (text == txt) return;
        txt = text;
        //width = text.Length * 8 + 15;
        GUIStyle.none.CalcMinMaxWidth(new GUIContent(text), out float minW, out float maxW);
        width = Mathf.CeilToInt(maxW + 10);
#endif
    }
}
