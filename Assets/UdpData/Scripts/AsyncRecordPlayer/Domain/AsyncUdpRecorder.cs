using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using UnityEngine;

public class AsyncUdpRecorder : IRecorder, IUdpReceivable, IDisposable
{

    public bool IsRecording { get; private set; }
    
    public long FileSize { get; private set; }

    Queue<TimeDataPair> recordQueue = new Queue<TimeDataPair>();

    Thread recorder;
    float startTime;
    float time;

    string recordFilePath;

    int receiveLimit;

    public AsyncUdpRecorder(string recordFilePath, int receiveLimit)
    {
        this.recordFilePath = recordFilePath;
        this.receiveLimit = receiveLimit;
    }

    ~AsyncUdpRecorder()
    {
        Dispose();
    }

    public void Start()
    {
        Stop();

        IsRecording = true;
        startTime = TimeUtility.GetCurrentTime();

        lock (recordQueue)
            recordQueue.Clear();

        recorder = new Thread(RecordLoop);
        recorder.Start();
    }

    public void Update()
    {
        if (IsRecording)
            time = TimeUtility.GetCurrentTime() - startTime;
    }


    // UDP Serverをデリゲートにしたほうがいい
    // またはこの関数をパブリックに公開して、向こう側で継承して関数の中に入れる
    public void OnReceivePacket(byte[] buffer, int length, IPEndPoint source)
    {
        // レコード中だったら
        if (IsRecording)
        {
            // 必要？
            var data = new byte[length];
            System.Buffer.BlockCopy(buffer, 0, data, 0, length);

            // レコードキューをロックして(この関数は別スレッドで動いてるから)
            lock (recordQueue)
            {
                // UDP Serverの受信リミットを超えていなければ、キューに追加
                if (0 < receiveLimit && recordQueue.Count < receiveLimit)
                    recordQueue.Enqueue(new TimeDataPair() { time = TimeUtility.GetCurrentTime() - startTime, data = data });
            }
        }
    }

    void RecordLoop()
    {
        using (var stream = new FileStream(recordFilePath, FileMode.Create))
        using (var writer = new BinaryWriter(stream))
        {
            while (IsRecording)
                try
                {
                    lock (recordQueue)
                    {
                        if (0 < recordQueue.Count)
                        {
                            var pair = recordQueue.Dequeue();
                            if (pair.data != null)
                            {
                                writer.Write(pair.time);
                                writer.Write(pair.data.Length);
                                writer.Write(pair.data);
                            }
                        }
                    }
                    FileSize = stream.Length;
                }
                catch (System.Exception e)
                {
                    writer.Close();
                    stream.Close();
                    throw new Exception(e.ToString());
                }

            writer.Close();
            stream.Close();
        }
    }

    public void Stop()
    {
        IsRecording = false;

        if (recorder != null)
        {
            recorder.Abort();
            recorder.Join();
            recorder = null;
        }
    }

    public void Dispose()
    {
        Stop();
    }
}
