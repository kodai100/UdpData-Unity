using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.IO;
using UnityEngine;

// UDPServer継承してるのは意味が分かりにくい
public class AsyncRecordPlayer : MonoBehaviour
{

    public UdpSender sender;
    public UdpRecordServer server;

    // 依存
    public IRecorder recorder;
    public IPlayer player;

    public void StartRecording()
    {
        recorder.Stop();
        recorder = new AsyncUdpRecorder("path", 10);
        server.SetReceivedCallbackFunc(recorder.OnReceivePacket);

        recorder.Start();
    }

    public void Play(string filePath, float startTime)
    {

        player.Stop();

        player = new AsyncUdpPlayer(sender, "path");
        player.Play();
    }

    public void Stop()
    {
        player.Stop();
        recorder.Stop();
    }
    
    private void Update()
    {
        recorder.Update();
        player.Update();
    }

}
