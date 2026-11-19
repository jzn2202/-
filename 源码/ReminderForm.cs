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
            BackColor = TechTheme.Background;
            ForeColor = TechTheme.Text;
            Icon = TechTheme.CreateIcon();
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = true;
            TopMost = true;
            StartPosition = FormStartPosition.Manual;
            ClientSize = new Size(520, 330);
            MinimumSize = Size;

            Rectangle area = Screen.FromPoint(Cursor.Position).WorkingArea;
            Location = new Point(area.Left + (area.Width - Width) / 2,
                area.Top + (area.Height - Height) / 2);

            GridSurface surface = new GridSurface();
            surface.Dock = DockStyle.Fill;
            surface.Padding = new Padding(16);
            Controls.Add(surface);
            TechCard card = new TechCard();
            card.Dock = DockStyle.Fill;
            card.Outline = TechTheme.Cyan;
            card.Padding = new Padding(24, 20, 24, 20);
            surface.Controls.Add(card);

            TableLayoutPanel layout = new TableLayoutPanel();
            layout.Dock = DockStyle.Fill;
            layout.BackColor = Color.Transparent;
            layout.ColumnCount = 1;
            layout.RowCount = 4;
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 27));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 47));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 53));
            card.Controls.Add(layout);

            Label eyebrow = new Label();
            eyebrow.Text = "◷  倒计时已完成";
            eyebrow.ForeColor = TechTheme.Cyan;
            eyebrow.Dock = DockStyle.Fill;
            layout.Controls.Add(eyebrow, 0, 0);
            Label title = new Label();
            title.Text = "时间到了";
            title.Font = new Font("Microsoft YaHei UI", 17F, FontStyle.Bold);
            title.ForeColor = TechTheme.Text;
            title.Dock = DockStyle.Fill;
            layout.Controls.Add(title, 0, 1);

            TextBox body = new TextBox();
            body.ReadOnly = true;
            body.Multiline = true;
            body.WordWrap = true;
            body.ScrollBars = ScrollBars.Vertical;
            body.Text = string.IsNullOrWhiteSpace(reminderText) ? "时间到了" : reminderText;
            body.Font = new Font("Microsoft YaHei UI", 12F);
            body.BackColor = TechTheme.Card;
            body.ForeColor = TechTheme.Text;
            body.BorderStyle = BorderStyle.None;
            body.Dock = DockStyle.Fill;
            body.Margin = new Padding(0, 2, 0, 7);
            body.TabStop = false;
            layout.Controls.Add(body, 0, 2);

            TechButton acknowledge = new TechButton();
            acknowledge.Text = "知道了";
            acknowledge.Kind = TechButtonKind.Primary;
            acknowledge.Dock = DockStyle.Fill;
            acknowledge.Margin = new Padding(0, 8, 0, 0);
            acknowledge.DialogResult = DialogResult.OK;
            layout.Controls.Add(acknowledge, 0, 3);
            AcceptButton = acknowledge;

            Shown += delegate
            {
                Activate();
                BringToFront();
                SetForegroundWindow(Handle);
                TechTheme.UseDarkTitleBar(this);
            };
            FormClosed += delegate { Icon.Dispose(); };
        }
    }
}
