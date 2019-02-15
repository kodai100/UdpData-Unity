using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class UdpRecordServer : UdpServer
{
    Action<byte[], int, IPEndPoint> onReceivedPacket;

    protected override void OnReadPacket(byte[] buffer, int length, IPEndPoint source)
    {
        onReceivedPacket(buffer, length, source);
    }

    public void SetReceivedCallbackFunc(Action<byte[], int, IPEndPoint> callback)
    {
        onReceivedPacket = callback;
    }
}
