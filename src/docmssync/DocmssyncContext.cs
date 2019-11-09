using System;
using System.Drawing;
using System.Windows.Forms;

namespace docmssync
{
    class DocmssyncContext : ApplicationContext
    {
        public static DocmssyncContext Context { get; private set; }

        static DocmssyncContext()
        {
            Context = new DocmssyncContext();
        }

        private readonly NotifyIcon _trayIcon;

        private readonly Timer _timer;

        private DocmssyncContext()
        {
            _timer = new Timer();
            _timer.Interval = (int)TimeSpan.FromSeconds(30).TotalMilliseconds;
            _timer.Tick += new EventHandler(TimerTick);

            var contextMenu = new ContextMenuStrip();
            contextMenu.Items.Add("開始", null, new EventHandler(StartApp));
            contextMenu.Items.Add("停止", null, new EventHandler(StopApp));
            contextMenu.Items.Add("再起動", null, new EventHandler(RestartApp));
            contextMenu.Items.Add("終了", null, new EventHandler(ExitMonitor));

            var icon = new Icon(this.GetType().Assembly.GetManifestResourceStream("docmssync.icon.ico"));
            _trayIcon = new NotifyIcon()
            {
                Icon = icon,
                ContextMenuStrip = contextMenu,
                Visible = true
            };
            _timer.Start();
        }

        private void StartApp(object sender, EventArgs e)
        {
            DocmssyncActions.StartApp();
        }

        private void StopApp(object sender, EventArgs e)
        {
            DocmssyncActions.StopApp();
        }

        private void RestartApp(object sender, EventArgs e)
        {
            DocmssyncActions.StopApp();
            DocmssyncActions.StartApp();
        }

        public void OnApplicationStarted()
        {
            _trayIcon.BalloonTipTitle = "docmssync";
            _trayIcon.BalloonTipText = "同期を開始します。";
            _trayIcon.ShowBalloonTip(10000);
        }

        public void OnApplicationStopped()
        {
            _trayIcon.BalloonTipTitle = "docmssync";
            _trayIcon.BalloonTipText = "同期を終了します。";
            _trayIcon.ShowBalloonTip(10000);
        }

        public void OnError(Exception e)
        {
            _trayIcon.BalloonTipTitle = "docmssync";
            _trayIcon.BalloonTipText = e.Message;
            _trayIcon.ShowBalloonTip(10000);
        }

        private void ExitMonitor(object sender, EventArgs e)
        {
            _timer.Stop();
            _trayIcon.Visible = false;
            Application.Exit();
        }

        private void TimerTick(object sender, EventArgs e)
        {
            DocmssyncActions.UpdateAppStatus();
        }
    }
}
