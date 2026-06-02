namespace SWeetJelly.Timer.Contract
{
    public interface IProgressable
    {
        float Progress { get; }
        bool IsFinished { get; }
    }
}