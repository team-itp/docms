using Docms.Client.Configuration;
using Docms.Client.InterprocessCommunication;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace docmssync
{
    class DocmssyncContext : ApplicationContext
    {
        private Process clientApp;

        private NotifyIcon trayIcon;

        public DocmssyncContext()
        {
            StartProcess();
            var icon = new Icon(this.GetType().Assembly.GetManifestResourceStream("docmssync.icon.ico"));
            var contextMenu = new ContextMenuStrip();
            contextMenu.Items.Add("終了", null, (s, e) =>
            {
                trayIcon.Visible = false;
                StopProcess();
                Application.Exit();
            });

            trayIcon = new NotifyIcon()
            {
                Icon = icon,
                ContextMenuStrip = contextMenu,
                Visible = true
            };
        }

        private void StartProcess()
        {
            var processes = Process.GetProcessesByName("Docms.Client.App");
            if (processes.Length > 0)
            {
                clientApp = processes.First();
            }
            else
            {
                var filename = Path.GetFullPath("Docms.Client.App.exe");
                clientApp = Process.Start(filename);
            }
        }

        private void StopProcess()
        {
            try
            {
                Process.GetProcessById(clientApp.Id);
                if (EventWaitHandle.TryOpenExisting(Constants.StopProcessHandle, out var handle))
                {
                    handle.Set();
                }
                if (!clientApp.WaitForExit(10000))
                {
                    clientApp.Kill();
                }
            }
            catch (ArgumentException)
            {
            }
        }
    }
}
