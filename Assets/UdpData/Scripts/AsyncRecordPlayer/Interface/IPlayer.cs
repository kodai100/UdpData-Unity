public interface IPlayer
{
    bool IsPlaying { get; }
    float Time { get; }
    void Play();
    void Update();
    void Stop();
}