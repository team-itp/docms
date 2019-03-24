namespace Docms.Client.Tasks
{
    public interface ITask
    {
        bool IsCompleted { get; }
        void Next(object result);
    }
}
