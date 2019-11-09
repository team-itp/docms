using System.Drawing;
using System.Windows.Forms;

namespace docmssync
{
    class DocmssyncContext : ApplicationContext
    {
        private NotifyIcon trayIcon;

        public DocmssyncContext()
        {
            var icon = new Icon(this.GetType().Assembly.GetManifestResourceStream("docmssync.icon.ico"));
            var contextMenu = new ContextMenuStrip();
            contextMenu.Items.Add("終了", null, (s, e) =>
            {
                trayIcon.Visible = false;
                Application.Exit();
            });

            trayIcon = new NotifyIcon()
            {
                Icon = icon,
                ContextMenuStrip = contextMenu,
                Visible = true
            };
        }
    }
}
