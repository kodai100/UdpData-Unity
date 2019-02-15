using System.Net;
using UnityEngine.Events;

public interface IUdpReceivable
{
    void OnReceivePacket(byte[] buffer, int length, IPEndPoint source);
}
