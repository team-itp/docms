using System;
using System.Threading;
using System.Windows.Forms;

namespace docmssync
{
    static class Program
    {
        /// <summary>
        /// アプリケーションのメイン エントリ ポイントです。
        /// </summary>
        [STAThread]
        static void Main()
        {
            using (new Mutex(true, "docmssync-4ce6c823-50bc-4117-96c6-2d4d464e485f", out var created))
            {
                if (!created)
                {
                    Application.Exit();
                }
                else
                {
                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(true);
                    Application.Run(DocmssyncContext.Context);
                }
            }
        }
    }
}
