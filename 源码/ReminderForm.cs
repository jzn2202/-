using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace ReminderMini
{
    internal sealed class ReminderForm : Form
    {
        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr handle);

        public ReminderForm(string reminderText)
        {
            Text = "时间到了";
            Font = new Font("Microsoft YaHei UI", 10F);
            Icon = SystemIcons.Information;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = true;
            TopMost = true;
            StartPosition = FormStartPosition.Manual;
            ClientSize = new Size(520, 320);
            MinimumSize = Size;

            Rectangle area = Screen.FromPoint(Cursor.Position).WorkingArea;
            Location = new Point(area.Left + (area.Width - Width) / 2,
                area.Top + (area.Height - Height) / 2);

            Label title = new Label();
            title.Text = "时间到了";
            title.Font = new Font("Microsoft YaHei UI", 17F, FontStyle.Bold);
            title.AutoSize = false;
            title.Location = new Point(24, 18);
            title.Size = new Size(470, 42);
            Controls.Add(title);

            TextBox body = new TextBox();
            body.ReadOnly = true;
            body.Multiline = true;
            body.WordWrap = true;
            body.ScrollBars = ScrollBars.Vertical;
            body.Text = string.IsNullOrWhiteSpace(reminderText) ? "时间到了" : reminderText;
            body.Font = new Font("Microsoft YaHei UI", 12F);
            body.Location = new Point(24, 72);
            body.Size = new Size(472, 175);
            body.TabStop = false;
            Controls.Add(body);

            Button acknowledge = new Button();
            acknowledge.Text = "知道了";
            acknowledge.Font = new Font("Microsoft YaHei UI", 10F);
            acknowledge.Size = new Size(110, 38);
            acknowledge.Location = new Point(386, 265);
            acknowledge.DialogResult = DialogResult.OK;
            Controls.Add(acknowledge);
            AcceptButton = acknowledge;

            Shown += delegate
            {
                Activate();
                BringToFront();
                SetForegroundWindow(Handle);
            };
        }
    }
}
