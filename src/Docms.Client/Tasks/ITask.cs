using System.Threading.Tasks;

namespace Docms.Client.Tasks
{
    public interface ITask
    {
        bool IsCompleted { get; }
        Task ExecuteAsync();
    }
}
