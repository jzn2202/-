using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace ReminderMini
{
    internal static class TechTheme
    {
        public static readonly Color Background = Color.FromArgb(10, 20, 34);
        public static readonly Color Card = Color.FromArgb(16, 31, 48);
        public static readonly Color CardHigh = Color.FromArgb(20, 40, 59);
        public static readonly Color Input = Color.FromArgb(11, 28, 44);
        public static readonly Color Border = Color.FromArgb(44, 69, 87);
        public static readonly Color Text = Color.FromArgb(240, 248, 251);
        public static readonly Color Muted = Color.FromArgb(168, 191, 202);
        public static readonly Color Dim = Color.FromArgb(119, 149, 164);
        public static readonly Color Cyan = Color.FromArgb(84, 216, 229);
        public static readonly Color CyanStrong = Color.FromArgb(25, 187, 207);
        public static readonly Color Danger = Color.FromArgb(240, 155, 155);

        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr window, int attribute,
            ref int value, int valueSize);

        [DllImport("user32.dll")]
        private static extern bool DestroyIcon(IntPtr handle);

        public static void UseDarkTitleBar(Form form)
        {
            try
            {
                int enabled = 1;
                if (DwmSetWindowAttribute(form.Handle, 20, ref enabled, 4) != 0)
                    DwmSetWindowAttribute(form.Handle, 19, ref enabled, 4);
            }
            catch { /* Older Windows versions keep their normal title bar. */ }
        }

        public static Icon CreateIcon()
        {
            using (Bitmap bitmap = new Bitmap(32, 32))
            using (Graphics graphics = Graphics.FromImage(bitmap))
            {
                graphics.SmoothingMode = SmoothingMode.AntiAlias;
                graphics.Clear(Color.Transparent);
                using (GraphicsPath tile = Rounded(new Rectangle(1, 1, 30, 30), 7))
                using (SolidBrush background = new SolidBrush(Color.FromArgb(17, 65, 79)))
                using (Pen border = new Pen(Cyan, 1.5F))
                {
                    graphics.FillPath(background, tile);
                    graphics.DrawPath(border, tile);
                }
                using (Pen circle = new Pen(Color.FromArgb(178, 244, 248), 2F))
                using (Pen hand = new Pen(Color.White, 2F))
                {
                    graphics.DrawEllipse(circle, 9, 9, 14, 14);
                    graphics.DrawLine(hand, 16, 16, 16, 11);
                    graphics.DrawLine(hand, 16, 16, 19, 18);
                }
                using (SolidBrush dot = new SolidBrush(Cyan))
                    graphics.FillEllipse(dot, 14.5F, 4.5F, 3F, 3F);
                IntPtr handle = bitmap.GetHicon();
                try { return (Icon)Icon.FromHandle(handle).Clone(); }
                finally { DestroyIcon(handle); }
            }
        }

        public static GraphicsPath Rounded(Rectangle rectangle, int radius)
        {
            int diameter = radius * 2;
            GraphicsPath path = new GraphicsPath();
            path.AddArc(rectangle.Left, rectangle.Top, diameter, diameter, 180, 90);
            path.AddArc(rectangle.Right - diameter, rectangle.Top, diameter, diameter, 270, 90);
            path.AddArc(rectangle.Right - diameter, rectangle.Bottom - diameter,
                diameter, diameter, 0, 90);
            path.AddArc(rectangle.Left, rectangle.Bottom - diameter,
                diameter, diameter, 90, 90);
            path.CloseFigure();
            return path;
        }
    }

    internal sealed class GridSurface : Panel
    {
        public GridSurface()
        {
            DoubleBuffered = true;
            BackColor = TechTheme.Background;
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            e.Graphics.Clear(TechTheme.Background);
            using (Pen grid = new Pen(Color.FromArgb(19, 66, 112, 131)))
            {
                for (int x = 0; x < Width; x += 28)
                    e.Graphics.DrawLine(grid, x, 0, x, Height);
                for (int y = 0; y < Height; y += 28)
                    e.Graphics.DrawLine(grid, 0, y, Width, y);
            }
        }
    }

    internal sealed class TechCard : Panel
    {
        public Color Outline { get; set; }

        public TechCard()
        {
            DoubleBuffered = true;
            BackColor = TechTheme.Card;
            Outline = TechTheme.Border;
            ResizeRedraw = true;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using (GraphicsPath path = TechTheme.Rounded(
                new Rectangle(1, 1, Math.Max(1, Width - 3), Math.Max(1, Height - 3)), 10))
            using (Pen border = new Pen(Outline, 1F))
                e.Graphics.DrawPath(border, path);
        }
    }

    internal enum TechButtonKind { Primary, Secondary, Danger }

    internal sealed class TechCheckBox : CheckBox
    {
        public TechCheckBox()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer, true);
            BackColor = TechTheme.Card;
            ForeColor = TechTheme.Text;
            Cursor = Cursors.Hand;
        }

        protected override void OnCheckedChanged(EventArgs e)
        {
            base.OnCheckedChanged(e);
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.Clear(TechTheme.Card);
            Rectangle box = new Rectangle(1, Math.Max(1, (Height - 16) / 2), 15, 15);
            using (SolidBrush fill = new SolidBrush(Checked ? TechTheme.CyanStrong : TechTheme.Input))
            using (Pen border = new Pen(Checked ? TechTheme.Cyan : TechTheme.Border))
            {
                e.Graphics.FillRectangle(fill, box);
                e.Graphics.DrawRectangle(border, box);
            }
            if (Checked)
            {
                using (Pen mark = new Pen(Color.FromArgb(6, 26, 35), 2F))
                {
                    e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    e.Graphics.DrawLines(mark, new Point[] {
                        new Point(4, box.Top + 8), new Point(7, box.Top + 11),
                        new Point(13, box.Top + 4) });
                }
            }
            TextRenderer.DrawText(e.Graphics, Text, Font,
                new Rectangle(22, 0, Math.Max(0, Width - 22), Height), ForeColor,
                TextFormatFlags.VerticalCenter | TextFormatFlags.Left |
                TextFormatFlags.NoPadding | TextFormatFlags.EndEllipsis);
            if (Focused && ShowFocusCues)
                ControlPaint.DrawFocusRectangle(e.Graphics,
                    new Rectangle(21, 2, Math.Max(1, Width - 22), Math.Max(1, Height - 4)));
        }
    }

    internal sealed class TechButton : Button
    {
        private bool hovering;
        public TechButtonKind Kind { get; set; }

        public TechButton()
        {
            Kind = TechButtonKind.Secondary;
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer, true);
            Font = new Font("Microsoft YaHei UI", 9.5F, FontStyle.Bold);
            Height = 44;
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            hovering = true;
            Invalidate();
            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            hovering = false;
            Invalidate();
            base.OnMouseLeave(e);
        }

        protected override void OnEnabledChanged(EventArgs e)
        {
            Invalidate();
            base.OnEnabledChanged(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            // The unpainted corners otherwise expose Button's default light background.
            e.Graphics.Clear(TechTheme.Card);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            Color fill = Kind == TechButtonKind.Primary ? TechTheme.CyanStrong :
                Kind == TechButtonKind.Danger ? TechTheme.Card : TechTheme.CardHigh;
            Color border = Kind == TechButtonKind.Primary ? TechTheme.Cyan :
                Kind == TechButtonKind.Danger ? Color.FromArgb(95, 70, 83) : TechTheme.Border;
            Color foreground = Kind == TechButtonKind.Primary ? Color.FromArgb(6, 26, 35) :
                Kind == TechButtonKind.Danger ? TechTheme.Danger : TechTheme.Text;
            if (hovering && Enabled)
            {
                fill = Kind == TechButtonKind.Primary ? TechTheme.Cyan :
                    Kind == TechButtonKind.Danger ? Color.FromArgb(49, 35, 48) :
                    Color.FromArgb(29, 58, 76);
            }
            if (!Enabled)
            {
                fill = TechTheme.Card;
                border = Color.FromArgb(38, 58, 71);
                foreground = TechTheme.Dim;
            }
            using (GraphicsPath path = TechTheme.Rounded(
                new Rectangle(1, 1, Math.Max(1, Width - 3), Math.Max(1, Height - 3)), 7))
            using (SolidBrush brush = new SolidBrush(fill))
            using (Pen pen = new Pen(border, 1F))
            {
                e.Graphics.FillPath(brush, path);
                e.Graphics.DrawPath(pen, path);
            }
            TextRenderer.DrawText(e.Graphics, Text, Font, ClientRectangle, foreground,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter |
                TextFormatFlags.NoPadding | TextFormatFlags.EndEllipsis);
            if (Focused && ShowFocusCues)
                ControlPaint.DrawFocusRectangle(e.Graphics,
                    Rectangle.Inflate(ClientRectangle, -5, -5), foreground, fill);
        }
    }

    internal sealed class DarkTextArea : Panel
    {
        private readonly DarkScrollTextBox editor;
        private readonly DarkScrollTrack track;
        private readonly bool multiline;

        public TextBox Editor { get { return editor; } }

        public DarkTextArea(bool multiline)
        {
            this.multiline = multiline;
            DoubleBuffered = true;
            BackColor = TechTheme.Input;
            editor = new DarkScrollTextBox();
            editor.Multiline = multiline;
            editor.WordWrap = true;
            editor.ScrollBars = ScrollBars.None;
            editor.BorderStyle = BorderStyle.None;
            editor.BackColor = TechTheme.Input;
            editor.ForeColor = TechTheme.Text;
            editor.Font = multiline ? new Font("Microsoft YaHei UI", 10F) :
                new Font("Consolas", 14F, FontStyle.Bold);
            Controls.Add(editor);
            if (multiline)
            {
                track = new DarkScrollTrack(editor);
                Controls.Add(track);
                editor.ScrollPositionChanged += delegate { track.Invalidate(); };
                editor.TextChanged += delegate { track.Invalidate(); };
                editor.Resize += delegate { track.Invalidate(); };
            }
            Resize += delegate { LayoutEditor(); };
            LayoutEditor();
        }

        private void LayoutEditor()
        {
            if (multiline)
            {
                editor.SetBounds(10, 8, Math.Max(20, Width - 32), Math.Max(20, Height - 16));
                track.SetBounds(Math.Max(0, Width - 18), 6, 12, Math.Max(20, Height - 12));
                track.Invalidate();
            }
            else
            {
                editor.Width = Math.Max(20, Width - 14);
                editor.Left = 7;
                editor.Top = Math.Max(2, (Height - editor.Height) / 2);
            }
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            using (Pen pen = new Pen(TechTheme.Border))
                e.Graphics.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
        }
    }

    internal sealed class DarkScrollTextBox : TextBox
    {
        public event EventHandler ScrollPositionChanged;

        protected override void WndProc(ref Message message)
        {
            int code = message.Msg;
            base.WndProc(ref message);
            if (code == 0x0115 || code == 0x020A || code == 0x0100 ||
                code == 0x0102 || code == 0x0005)
            {
                EventHandler handler = ScrollPositionChanged;
                if (handler != null) handler(this, EventArgs.Empty);
            }
        }
    }

    internal sealed class DarkScrollTrack : Control
    {
        private const int GetLineCount = 0x00BA;
        private const int GetFirstLine = 0x00CE;
        private const int LineScroll = 0x00B6;

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr window, int message,
            IntPtr wParam, IntPtr lParam);

        private readonly TextBox editor;
        private bool dragging;
        private int dragOffset;

        public DarkScrollTrack(TextBox editor)
        {
            this.editor = editor;
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer, true);
            BackColor = TechTheme.Input;
            Cursor = Cursors.Hand;
        }

        private int TotalLines
        {
            get { return editor.IsHandleCreated ?
                Math.Max(1, SendMessage(editor.Handle, GetLineCount, IntPtr.Zero,
                    IntPtr.Zero).ToInt32()) : 1; }
        }

        private int VisibleLines
        {
            get { return Math.Max(1, editor.ClientSize.Height / Math.Max(1, editor.Font.Height)); }
        }

        private int FirstLine
        {
            get { return editor.IsHandleCreated ?
                SendMessage(editor.Handle, GetFirstLine, IntPtr.Zero,
                    IntPtr.Zero).ToInt32() : 0; }
        }

        private Rectangle Thumb(int total, int visible, int first)
        {
            int usable = Math.Max(1, Height - 8);
            int thumbHeight = total <= visible ? usable :
                Math.Max(22, (int)Math.Round((double)usable * visible / total));
            thumbHeight = Math.Min(usable, thumbHeight);
            int max = Math.Max(0, total - visible);
            int top = 4 + (max == 0 ? 0 :
                (int)Math.Round((double)(usable - thumbHeight) * Math.Min(first, max) / max));
            return new Rectangle(3, top, Math.Max(3, Width - 6), thumbHeight);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.Clear(BackColor);
            int total = TotalLines;
            int visible = VisibleLines;
            using (SolidBrush rail = new SolidBrush(Color.FromArgb(32, 57, 72)))
                e.Graphics.FillRectangle(rail, Width / 2 - 2, 4, 4, Math.Max(1, Height - 8));
            using (SolidBrush thumb = new SolidBrush(total > visible ?
                TechTheme.CyanStrong : Color.FromArgb(63, 102, 117)))
                e.Graphics.FillRectangle(thumb, Thumb(total, visible, FirstLine));
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button != MouseButtons.Left) return;
            Rectangle thumb = Thumb(TotalLines, VisibleLines, FirstLine);
            dragOffset = thumb.Contains(e.Location) ? e.Y - thumb.Top : thumb.Height / 2;
            dragging = true;
            Capture = true;
            ScrollTo(e.Y - dragOffset);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (dragging) ScrollTo(e.Y - dragOffset);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            dragging = false;
            Capture = false;
            base.OnMouseUp(e);
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            ScrollBy(-Math.Sign(e.Delta) * 3);
        }

        private void ScrollTo(int top)
        {
            int total = TotalLines;
            int visible = VisibleLines;
            int max = Math.Max(0, total - visible);
            int thumbHeight = Thumb(total, visible, FirstLine).Height;
            int travel = Math.Max(1, Height - 8 - thumbHeight);
            int desired = (int)Math.Round((double)Math.Max(0,
                Math.Min(travel, top - 4)) * max / travel);
            ScrollBy(desired - FirstLine);
        }

        private void ScrollBy(int lines)
        {
            if (lines == 0 || !editor.IsHandleCreated) return;
            SendMessage(editor.Handle, LineScroll, IntPtr.Zero, new IntPtr(lines));
            Invalidate();
        }
    }

    internal sealed class RingDisplay : Control
    {
        private string timeText = "00:40:00";
        private string stateText = "准备就绪";
        private double progress = 1;

        public string TimeText { get { return timeText; } set { timeText = value; Invalidate(); } }
        public string StateText { get { return stateText; } set { stateText = value; Invalidate(); } }
        public double Progress { get { return progress; } set { progress = Math.Max(0, Math.Min(1, value)); Invalidate(); } }

        public RingDisplay()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer, true);
            BackColor = TechTheme.Card;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.Clear(BackColor);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            int diameter = Math.Max(80, Math.Min(Width - 28, Height - 20));
            Rectangle circle = new Rectangle((Width - diameter) / 2,
                (Height - diameter) / 2, diameter, diameter);
            circle.Inflate(-5, -5);
            using (Pen track = new Pen(Color.FromArgb(42, 70, 84), 6F))
            using (Pen arc = new Pen(TechTheme.Cyan, 6F))
            {
                track.StartCap = LineCap.Round;
                track.EndCap = LineCap.Round;
                arc.StartCap = LineCap.Round;
                arc.EndCap = LineCap.Round;
                e.Graphics.DrawEllipse(track, circle);
                if (progress > 0) e.Graphics.DrawArc(arc, circle, -90, (float)(359.9 * progress));
            }
            Rectangle middle = new Rectangle(circle.Left + 8, circle.Top + diameter / 2 - 47,
                circle.Width - 16, 110);
            using (Font caption = new Font("Microsoft YaHei UI", 9F))
            using (Font digits = new Font("Consolas", diameter > 210 ? 26F : 21F, FontStyle.Bold))
            using (Font status = new Font("Microsoft YaHei UI", 8.5F, FontStyle.Bold))
            {
                DrawCentered(e.Graphics, "距离提醒", caption, TechTheme.Muted,
                    new Rectangle(middle.Left, middle.Top, middle.Width, 25));
                DrawCentered(e.Graphics, timeText, digits, TechTheme.Text,
                    new Rectangle(middle.Left, middle.Top + 24, middle.Width, 50));
                DrawCentered(e.Graphics, stateText, status, TechTheme.Cyan,
                    new Rectangle(middle.Left, middle.Top + 78, middle.Width, 25));
            }
        }

        private static void DrawCentered(Graphics graphics, string text, Font font,
            Color color, Rectangle bounds)
        {
            TextRenderer.DrawText(graphics, text, font, bounds, color,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter |
                TextFormatFlags.NoPadding | TextFormatFlags.EndEllipsis);
        }
    }
}
