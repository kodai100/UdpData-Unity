using System.Net;

public interface IRecorder
{
    bool IsRecording { get; }
    void Start();
    void Update();
    void Stop();
    void OnReceivePacket(byte[] data, int length, IPEndPoint ip);
}
