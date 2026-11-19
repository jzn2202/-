using System;
using System.Drawing;
using System.Windows.Forms;

namespace ReminderMini
{
    internal sealed class DurationPickerForm : Form
    {
        private readonly Label errorLabel;
        internal readonly WheelNumberPicker HoursPicker;
        internal readonly WheelNumberPicker MinutesPicker;
        internal readonly WheelNumberPicker SecondsPicker;

        public int SelectedSeconds { get; private set; }

        public DurationPickerForm(int initialSeconds)
        {
            Text = "设置倒计时";
            Font = new Font("Microsoft YaHei UI", 9.5F);
            BackColor = TechTheme.Background;
            ForeColor = TechTheme.Text;
            Icon = TechTheme.CreateIcon();
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(540, 390);

            GridSurface surface = new GridSurface();
            surface.Dock = DockStyle.Fill;
            surface.Padding = new Padding(18);
            Controls.Add(surface);
            TechCard card = new TechCard();
            card.Dock = DockStyle.Fill;
            card.Padding = new Padding(23, 18, 23, 18);
            surface.Controls.Add(card);

            TableLayoutPanel layout = new TableLayoutPanel();
            layout.Dock = DockStyle.Fill;
            layout.BackColor = Color.Transparent;
            layout.ColumnCount = 1;
            layout.RowCount = 5;
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 29));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 29));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 51));
            card.Controls.Add(layout);

            Label title = new Label();
            title.Text = "设置倒计时";
            title.Dock = DockStyle.Fill;
            title.ForeColor = TechTheme.Text;
            title.Font = new Font("Microsoft YaHei UI", 17F, FontStyle.Bold);
            layout.Controls.Add(title, 0, 0);
            Label hint = new Label();
            hint.Text = "滚动滑轮、点击上下数字，或直接输入";
            hint.Dock = DockStyle.Fill;
            hint.ForeColor = TechTheme.Muted;
            layout.Controls.Add(hint, 0, 1);

            TableLayoutPanel wheels = new TableLayoutPanel();
            wheels.Dock = DockStyle.Fill;
            wheels.BackColor = Color.Transparent;
            wheels.ColumnCount = 3;
            wheels.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
            wheels.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
            wheels.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.34F));
            layout.Controls.Add(wheels, 0, 2);
            HoursPicker = new WheelNumberPicker("小时", 24);
            MinutesPicker = new WheelNumberPicker("分钟", 59);
            SecondsPicker = new WheelNumberPicker("秒", 59);
            WheelNumberPicker[] pickers = { HoursPicker, MinutesPicker, SecondsPicker };
            for (int i = 0; i < pickers.Length; i++)
            {
                pickers[i].Dock = DockStyle.Fill;
                pickers[i].Margin = new Padding(i == 0 ? 0 : 5, 0,
                    i == 2 ? 0 : 5, 0);
                pickers[i].InputChanged += delegate { errorLabel.Text = ""; };
                wheels.Controls.Add(pickers[i], i, 0);
            }
            initialSeconds = Math.Max(0, Math.Min(86400, initialSeconds));
            HoursPicker.Value = initialSeconds / 3600;
            MinutesPicker.Value = (initialSeconds / 60) % 60;
            SecondsPicker.Value = initialSeconds % 60;

            errorLabel = new Label();
            errorLabel.Text = "可设置 1 秒至 24 小时";
            errorLabel.Dock = DockStyle.Fill;
            errorLabel.ForeColor = TechTheme.Dim;
            layout.Controls.Add(errorLabel, 0, 3);

            TableLayoutPanel actions = new TableLayoutPanel();
            actions.Dock = DockStyle.Fill;
            actions.BackColor = Color.Transparent;
            actions.ColumnCount = 2;
            actions.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            actions.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            layout.Controls.Add(actions, 0, 4);
            TechButton cancel = new TechButton();
            cancel.Text = "取消";
            cancel.Kind = TechButtonKind.Secondary;
            cancel.DialogResult = DialogResult.Cancel;
            cancel.Dock = DockStyle.Fill;
            cancel.Margin = new Padding(0, 5, 6, 0);
            actions.Controls.Add(cancel, 0, 0);
            TechButton confirm = new TechButton();
            confirm.Text = "确认时长";
            confirm.Kind = TechButtonKind.Primary;
            confirm.Dock = DockStyle.Fill;
            confirm.Margin = new Padding(6, 5, 0, 0);
            confirm.Click += OnConfirm;
            actions.Controls.Add(confirm, 1, 0);
            AcceptButton = confirm;
            CancelButton = cancel;
            Shown += delegate { TechTheme.UseDarkTitleBar(this); HoursPicker.FocusInput(); };
            FormClosed += delegate { Icon.Dispose(); };
        }

        private void OnConfirm(object sender, EventArgs e)
        {
            int hours, minutes, seconds;
            if (!HoursPicker.TryRead(out hours) || !MinutesPicker.TryRead(out minutes) ||
                !SecondsPicker.TryRead(out seconds))
            {
                ShowError("请在每一列输入有效的数字。");
                return;
            }
            int total = hours * 3600 + minutes * 60 + seconds;
            if (total < 1 || total > 86400)
            {
                ShowError("时长必须在 1 秒到 24 小时之间。");
                return;
            }
            SelectedSeconds = total;
            DialogResult = DialogResult.OK;
            Close();
        }

        private void ShowError(string message)
        {
            errorLabel.Text = message;
            errorLabel.ForeColor = TechTheme.Danger;
        }
    }

    internal sealed class WheelNumberPicker : Panel
    {
        private readonly string unit;
        private readonly int maximum;
        private readonly TextBox editor;
        private int value;
        private bool updating;

        public event EventHandler InputChanged;

        public int Value
        {
            get { return value; }
            set
            {
                if (value < 0 || value > maximum) throw new ArgumentOutOfRangeException("value");
                this.value = value;
                updating = true;
                editor.Text = value.ToString("00");
                updating = false;
                Invalidate();
            }
        }

        internal TextBox Editor { get { return editor; } }

        public WheelNumberPicker(string unit, int maximum)
        {
            this.unit = unit;
            this.maximum = maximum;
            DoubleBuffered = true;
            BackColor = TechTheme.Card;
            Cursor = Cursors.Hand;
            editor = new TextBox();
            editor.BorderStyle = BorderStyle.None;
            editor.BackColor = TechTheme.CardHigh;
            editor.ForeColor = TechTheme.Text;
            editor.Font = new Font("Consolas", 22F, FontStyle.Bold);
            editor.TextAlign = HorizontalAlignment.Center;
            editor.MaxLength = 2;
            editor.Cursor = Cursors.IBeam;
            editor.KeyPress += delegate(object sender, KeyPressEventArgs e)
            {
                if (!char.IsControl(e.KeyChar) && (e.KeyChar < '0' || e.KeyChar > '9'))
                    e.Handled = true;
            };
            editor.TextChanged += delegate
            {
                if (updating) return;
                int parsed;
                if (TryRead(out parsed)) value = parsed;
                Invalidate();
                EventHandler handler = InputChanged;
                if (handler != null) handler(this, EventArgs.Empty);
            };
            editor.MouseWheel += delegate(object sender, MouseEventArgs e) { Wheel(e.Delta); };
            Controls.Add(editor);
            Resize += delegate { PositionEditor(); };
            Value = 0;
            PositionEditor();
        }

        public bool TryRead(out int number)
        {
            return int.TryParse(editor.Text, out number) && number >= 0 && number <= maximum;
        }

        public void FocusInput()
        {
            editor.Focus();
            editor.SelectAll();
        }

        internal void Step(int direction)
        {
            int count = maximum + 1;
            Value = ((value + direction) % count + count) % count;
        }

        private void Wheel(int delta)
        {
            if (delta == 0) return;
            int steps = Math.Max(1, Math.Abs(delta) / 120);
            Step((delta > 0 ? 1 : -1) * steps);
        }

        private void PositionEditor()
        {
            editor.Width = Math.Max(20, Width - 32);
            editor.Left = (Width - editor.Width) / 2;
            editor.Top = Math.Max(75, (Height - editor.Height) / 2);
            Invalidate();
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            Wheel(e.Delta);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button != MouseButtons.Left) return;
            int center = Height / 2;
            if (e.Y < center - 27) Step(1);
            else if (e.Y > center + 27) Step(-1);
            else FocusInput();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.Clear(TechTheme.Card);
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            int center = Height / 2;
            using (System.Drawing.Drawing2D.GraphicsPath path = TechTheme.Rounded(
                new Rectangle(5, center - 30, Math.Max(1, Width - 11), 60), 7))
            using (SolidBrush fill = new SolidBrush(TechTheme.CardHigh))
            using (Pen border = new Pen(TechTheme.CyanStrong))
            {
                e.Graphics.FillPath(fill, path);
                e.Graphics.DrawPath(border, path);
            }
            using (Font titleFont = new Font("Microsoft YaHei UI", 9F))
            using (Font previewFont = new Font("Consolas", 13F))
            {
                DrawText(e.Graphics, unit, titleFont, TechTheme.Muted,
                    new Rectangle(0, 0, Width, 20));
                DrawText(e.Graphics, ((value + 1) % (maximum + 1)).ToString("00"),
                    previewFont, TechTheme.Dim,
                    new Rectangle(0, center - 58, Width, 29));
                DrawText(e.Graphics, ((value + maximum) % (maximum + 1)).ToString("00"),
                    previewFont, TechTheme.Dim,
                    new Rectangle(0, center + 38, Width, 29));
            }
        }

        private static void DrawText(Graphics graphics, string text, Font font,
            Color color, Rectangle bounds)
        {
            TextRenderer.DrawText(graphics, text, font, bounds, color,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter |
                TextFormatFlags.NoPadding);
        }
    }
}
