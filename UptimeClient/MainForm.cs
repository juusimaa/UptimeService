using Common;
using System;
using System.IO.Pipes;
using System.Security.Principal;
using System.Windows.Forms;

namespace UptimeClient
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void updateButton_Click(object sender, EventArgs e)
        {
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
                textBox.Text = stream.ReadString();
                pipe.Close();
            }
            catch (TimeoutException)
            {
                textBox.Text = "timeout";
            }
        }
    }
}
