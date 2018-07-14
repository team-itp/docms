using Microsoft.Azure.WebJobs;
using System.IO;

namespace Docms.WebJob.ThumbCreator
{
    public class Functions
    {
        public static void ProcessQueueMessage([QueueTrigger("queue")] string message, TextWriter log)
        {
            log.WriteLine(message);
        }
    }
}
