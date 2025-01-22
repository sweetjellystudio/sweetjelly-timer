namespace SWeetJelly.TimerService
{
    public interface ITimerManager
    {
        void RegisterTimer(ITimer timer);
        void UnregisterTimer(ITimer timer);
        void UpdateTimers();
        void Clear();
    }
}