namespace SWeetJelly.Timer.App
{
    public interface ITimerTicker
    {
        void Tick(float deltaTime, float unscaledDeltaTime);
    }
}