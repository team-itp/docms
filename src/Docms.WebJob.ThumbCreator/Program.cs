using System.IO;

namespace Docms.WebJob.ThumbCreator
{
    // To learn more about Microsoft Azure WebJobs SDK, please see https://go.microsoft.com/fwlink/?LinkID=320976
    class Program
    {
        // Please set the following connection strings in app.config for this WebJob to run:
        // AzureWebJobsDashboard and AzureWebJobsStorage
        static void Main(string[] args)
        {
            var pathToApp = args.Length > 0 ? args[0] : "../../../Docms.Web";
            Functions.ConvertLocally(Path.Combine(pathToApp, "App_Data/Files"), Path.Combine(pathToApp, "App_Data/Thumbnails"));

            //var config = new JobHostConfiguration();

            //if (config.IsDevelopment)
            //{
            //    config.UseDevelopmentSettings();
            //}

            //var host = new JobHost(config);
            //// The following code ensures that the WebJob will be running continuously
            //host.RunAndBlock();
        }
    }
}
