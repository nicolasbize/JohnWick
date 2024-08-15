using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ColorHelper
{
    public static Color UnselectedColor {
        get {
            ColorUtility.TryParseHtmlString("#59594E", out Color unselectedColor);
            return unselectedColor;
        }
    }

    public static Color SelectedColor {
        get {
            ColorUtility.TryParseHtmlString("#D2D27C", out Color unselectedColor);
            return unselectedColor;
        }
    }
}
