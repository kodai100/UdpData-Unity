using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;

public class AsyncUdpPlayer : IPlayer, IDisposable
{

    public bool IsPlaying { get; private set; }
    public float Time { get; private set; }
    public long FileSize { get; private set; }

    [Header("field for debug")]
    public TimeDataPair[] playData;

    // 依存
    UdpSender sender;

    Thread player;

    string filePath;
    float startTime;

    public AsyncUdpPlayer(UdpSender sender, string playFilePath)
    {
        this.sender = sender;
        filePath = playFilePath;
    }

    ~AsyncUdpPlayer()
    {
        Dispose();
    }

    public void Update()
    {
        if (IsPlaying)
            Time = TimeUtility.GetCurrentTime() - startTime;
    }

    public void CreatePlayData()
    {
        var list = new List<TimeDataPair>();
        if (File.Exists(filePath))
        {
            var fileData = File.ReadAllBytes(filePath);
            using (var stream = new MemoryStream(fileData))
            using (var reader = new BinaryReader(stream))
            {
                var enl = false;
                while (!enl)
                {
                    try
                    {
                        var time = reader.ReadSingle();
                        var count = reader.ReadInt32();
                        var data = reader.ReadBytes(count);
                        list.Add(new TimeDataPair() { time = time, data = data });
                    }
                    catch (EndOfStreamException)
                    {
                        enl = true;
                    }
                }
            }
            playData = list.ToArray();
        }
    }

    // Resume?
    public void Resume()
    {
        Stop();

        IsPlaying = true;

        startTime = TimeUtility.GetCurrentTime();
        Time = 0;

        player = new Thread(PlayLoop);
        player.Start();
    }

    public void Play()
    {
        Stop();

        IsPlaying = true;
        startTime = TimeUtility.GetCurrentTime() - playStartTime;
        Time = playStartTime;

        this.filePath = filePath;
        if (!File.Exists(filePath))
            return;

        player = new Thread(PlayLoop);
        player.Start();
    }

    void PlayLoop()
    {
        var recordedData = File.ReadAllBytes(filePath);
        using (var stream = new MemoryStream(recordedData))
        using (var reader = new BinaryReader(stream))
        {
            FileSize = stream.Length;
            var startTime = Time;
            while (IsPlaying)
            {
                try
                {
                    var nextTime = reader.ReadSingle();
                    var count = reader.ReadInt32();
                    while (nextTime < startTime)
                    {
                        reader.ReadBytes(count);
                        nextTime = reader.ReadSingle();
                        count = reader.ReadInt32();
                    }


                    if (0 < count)
                    {
                        var data = reader.ReadBytes(count);
                        {
                            while (Time < nextTime && IsPlaying)
                            {

                            }
                            sender.Send(data);
                        }
                    }
                    else
                        IsPlaying = false;
                }
                catch (EndOfStreamException)
                {
                    IsPlaying = false;
                    reader.Close();
                    stream.Close();
                    return;
                }
                catch (System.Exception e)
                {
                    reader.Close();
                    stream.Close();
                    throw new Exception(e.ToString());
                    // exception = e.ToString();
                }
            }

            reader.Close();
            stream.Close();
        }
    }

    public void Stop()
    {
        IsPlaying = false;
        if (player != null)
        {
            player.Abort();
            player.Join();
            player = null;
        }
    }

    public void Dispose()
    {
        Stop();
    }
}
