using System;
using System.Threading;
using System.Windows.Forms;

namespace Docms.Client.App
{
    static class Program
    {
        /// <summary>
        /// アプリケーションのメイン エントリ ポイントです。
        /// </summary>
        [STAThread]
        static void Main()
        {
            using (new Mutex(true, "Docms.Client.App-4ce6c823-50bc-4117-96c6-2d4d464e485f", out var created))
            {
                if (!created)
                {
                    System.Windows.Forms.Application.Exit();
                }
                else
                {
                    System.Windows.Forms.Application.EnableVisualStyles();
                    System.Windows.Forms.Application.SetCompatibleTextRenderingDefault(true);
                    System.Windows.Forms.Application.Run(DocmssyncContext.Context);
                }
            }
        }
    }
}
