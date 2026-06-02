namespace SWeetJelly.Timer.Contract
{
#nullable enable

    public enum TimerState
    {
        Idle = 0,
        Running,
        Paused,
        Stopped,
        Completed,
        Disposed
    }
}