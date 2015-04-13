using Common;
using System;
using System.IO.Pipes;
using System.Security.Principal;
using System.Windows.Forms;

namespace UptimeClient
{
    public partial class MainForm : Form
    {
        private string _currentUptime;
        private bool _exiting;
        private bool _minimized;

        public MainForm()
        {
            InitializeComponent();
            textBox.Text = GetUptime();            
        }

        private string GetUptime()
        {
            var result = "";

            try
            {
                var pipe = new NamedPipeClientStream(
                    ".",
                    "uptimepipe",
                    PipeDirection.In,
                    PipeOptions.None,
                    TokenImpersonationLevel.Impersonation);

                pipe.Connect(5000);
                var stream = new StreamString(pipe);
                result = stream.ReadString();
                pipe.Close();
            }
            catch (TimeoutException)
            {
                textBox.Text = "timeout";
            }

            _currentUptime = result.Split('\n')[0];
            return result;
        }

        private void MainForm_Resize(object sender, EventArgs e)
        {
            if (FormWindowState.Minimized == WindowState)
            {
                notifyIcon.Visible = true;
                notifyIcon.BalloonTipTitle = "Uptime client";
                notifyIcon.BalloonTipText = "Uptime client is still running.";
                notifyIcon.BalloonTipIcon = ToolTipIcon.Info;

                if (!_minimized)
                {
                    _minimized = true;
                    notifyIcon.ShowBalloonTip(500);
                }
                notifyIcon.ContextMenuStrip = contextMenuStrip;
                Hide();
            }

            else if (FormWindowState.Normal == WindowState)
            {
                notifyIcon.Visible = false;
            }
        }

        private void notifyIcon_DoubleClick(object sender, EventArgs e)
        {
            Show();
            WindowState = FormWindowState.Normal;
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            textBox.Text = GetUptime();
            notifyIcon.Text = _currentUptime;
        }

        private void restoreMenuItem_Click(object sender, EventArgs e)
        {
            Show();
            WindowState = FormWindowState.Normal;
        }

        private void exitMenuItem_Click(object sender, EventArgs e)
        {
            _exiting = true;
            Close();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_exiting) return;
            WindowState = FormWindowState.Minimized;
            e.Cancel = true;
        }

        private void notifyIcon_MouseClick(object sender, MouseEventArgs e)
        {
            Show();
            WindowState = FormWindowState.Normal;
        }
    }
}
