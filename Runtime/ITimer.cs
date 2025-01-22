namespace SWeetJelly.TimerService
{
    public interface ITimer
    {
        float CurrentTime { get; }
        bool IsRunning { get; }
        float Progress { get; }
        bool IsFinished { get; }

        void Start();
        void Stop();
        void Tick();
        void Resume();
        void Pause();
        void Reset();
        void Reset(float value);
    }
}