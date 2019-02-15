using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeUtility
{
    public static float GetCurrentTime()
    {
        return (float)Environment.TickCount / 1000.0f;
    }
    
}

[System.Serializable]
public struct TimeDataPair
{
    public float time;
    public byte[] data;
}