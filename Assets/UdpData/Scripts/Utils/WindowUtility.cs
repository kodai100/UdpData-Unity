using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindowUtility
{

    public static Rect CalculateCenteredWindowRect(int width, int height)
    {
        var x = Screen.width / 2f - width / 2f;
        var y = Screen.height / 2f - height / 2f;
        return new Rect(new Vector2(x, y), new Vector2(width, height));
    }
}
