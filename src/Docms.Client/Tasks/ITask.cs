namespace Docms.Client.Tasks
{
    public interface ITask
    {
        void Next(params object[] args);
    }
}
