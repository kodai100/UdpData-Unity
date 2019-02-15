using System.Collections.Generic;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.Events;

// UDPの受信サーバーを構築するクラス
public abstract class UdpServer : MonoBehaviour
{
    public int localPort = 8888;
    public int receiveLimit = 10;
    public string errorMsg;

    Socket udp;
    Thread reader;
    byte[] receiveBuffer;

    // サーバーの起動
    public void StartServer(int port)
    {
        StopServer();   // 一回止める

        // ソケットを開く
        udp = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

        localPort = port;
        udp.Bind(new IPEndPoint(IPAddress.Any, localPort));

        // 別スレッドでリッスン
        reader = new Thread(Reader);
        reader.Start();
    }

    public void StopServer()
    {
        if (udp != null)
        {
            // ソケットを閉じて
            udp.Close();
            udp = null;
        }
        if (reader != null)
        {
            // スレッドを停止する
            reader.Abort();
            reader.Join();  // 終了を待ち合わせ
            reader = null;
        }
    }


    /// <summary>
    /// MonoBehaviour
    /// </summary>
    void Start()
    {
        receiveBuffer = new byte[1 << 16];  // 16ビット確保
        StartServer(localPort);
    }

    void OnApplicationQuit()
    {
        StopServer();
    }

    // 受信したUDPを読む別スレッドの関数
    void Reader()
    {
        var clientEndpoint = new IPEndPoint(IPAddress.Any, 0);

        // ソケットが開いていて
        while (udp != null && udp.IsBound)
        {
            try
            {
                var fromendpoint = (EndPoint)clientEndpoint;
                var length = udp.ReceiveFrom(receiveBuffer, ref fromendpoint);
                var fromipendpoint = fromendpoint as IPEndPoint;
                if (length == 0 || fromipendpoint == null)
                    continue;

                OnReadPacket(receiveBuffer, length, fromipendpoint);
            }
            catch (System.Exception e) { OnRaiseError(e); }
        }
    }

    // インタフェースにすればいい
    protected abstract void OnReadPacket(byte[] buffer, int length, IPEndPoint source);

    protected virtual void OnRaiseError(System.Exception e)
    {
        errorMsg = e.ToString();
    }
}
