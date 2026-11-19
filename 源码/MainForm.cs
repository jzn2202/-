using System;
using System.Drawing;
using System.IO;
using System.Media;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.Win32;

namespace ReminderMini
{
    internal sealed class MainForm : Form
    {
        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr handle);

        private readonly TimerEngine engine = new TimerEngine();
        private readonly Timer ticker = new Timer();
        private readonly NotifyIcon tray = new NotifyIcon();
        private TextBox hoursBox;
        private TextBox minutesBox;
        private TextBox secondsBox;
        private TextBox messageBox;
        private CheckBox soundBox;
        private Label countdownLabel;
        private Label statusLabel;
        private Button startButton;
        private Button pauseButton;
        private Button resetButton;
        private Button endButton;
        private bool exiting;
        private bool sessionLocked;
        private bool pendingAlarm;
        private bool alarmShowing;
        private bool saveWarningShown;
        private SoundPlayer soundPlayer;
        private MemoryStream soundStream;

        public MainForm()
        {
            Text = "提醒小程序";
            Font = new Font("Microsoft YaHei UI", 10F);
            Icon = SystemIcons.Information;
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            ClientSize = new Size(520, 450);
            MinimumSize = Size;

            BuildInterface();
            AppSettings settings = SettingsStore.Load();
            hoursBox.Text = settings.Hours ?? "";
            minutesBox.Text = settings.Minutes ?? "";
            secondsBox.Text = settings.Seconds ?? "";
            messageBox.Text = settings.Message ?? "";
            soundBox.Checked = settings.SoundEnabled;

            ContextMenuStrip menu = new ContextMenuStrip();
            menu.Items.Add("打开主窗口", null, delegate { ShowMainWindow(); });
            menu.Items.Add("退出", null, delegate { ExitApplication(); });
            tray.Icon = SystemIcons.Information;
            tray.Text = "提醒小程序";
            tray.ContextMenuStrip = menu;
            tray.Visible = true;
            tray.DoubleClick += delegate { ShowMainWindow(); };

            ticker.Interval = 250;
            ticker.Tick += delegate { CheckTimer(); };
            SystemEvents.PowerModeChanged += OnPowerModeChanged;
            SystemEvents.SessionSwitch += OnSessionSwitch;
            FormClosing += OnMainFormClosing;
            FormClosed += OnMainFormClosed;
            RefreshInterface();
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == Program.ActivationMessage)
            {
                ShowMainWindow();
                return;
            }
            base.WndProc(ref m);
        }

        private void BuildInterface()
        {
            Label heading = new Label();
            heading.Text = "设置倒计时";
            heading.Font = new Font("Microsoft YaHei UI", 16F, FontStyle.Bold);
            heading.Location = new Point(28, 20);
            heading.Size = new Size(460, 42);
            Controls.Add(heading);

            Label durationLabel = new Label();
            durationLabel.Text = "时长（1 秒至 24 小时）";
            durationLabel.Location = new Point(30, 74);
            durationLabel.Size = new Size(450, 25);
            Controls.Add(durationLabel);

            hoursBox = AddTimeBox(30, "时");
            minutesBox = AddTimeBox(145, "分");
            secondsBox = AddTimeBox(260, "秒");

            Label messageLabel = new Label();
            messageLabel.Text = "提醒文字（可留空）";
            messageLabel.Location = new Point(30, 147);
            messageLabel.Size = new Size(450, 25);
            Controls.Add(messageLabel);

            messageBox = new TextBox();
            messageBox.Multiline = true;
            messageBox.ScrollBars = ScrollBars.Vertical;
            messageBox.Location = new Point(30, 176);
            messageBox.Size = new Size(460, 112);
            Controls.Add(messageBox);

            soundBox = new CheckBox();
            soundBox.Text = "到点播放提示音";
            soundBox.Location = new Point(30, 300);
            soundBox.Size = new Size(230, 27);
            Controls.Add(soundBox);

            countdownLabel = new Label();
            countdownLabel.Font = new Font("Consolas", 25F, FontStyle.Bold);
            countdownLabel.TextAlign = ContentAlignment.MiddleCenter;
            countdownLabel.Location = new Point(30, 328);
            countdownLabel.Size = new Size(330, 48);
            Controls.Add(countdownLabel);

            statusLabel = new Label();
            statusLabel.TextAlign = ContentAlignment.MiddleLeft;
            statusLabel.Location = new Point(370, 337);
            statusLabel.Size = new Size(120, 28);
            Controls.Add(statusLabel);

            startButton = AddButton("开始", 30, OnStartClicked);
            pauseButton = AddButton("暂停", 147, OnPauseClicked);
            resetButton = AddButton("重置", 264, OnResetClicked);
            endButton = AddButton("结束", 381, OnEndClicked);

            hoursBox.TextChanged += delegate { RefreshIdleDisplay(); };
            minutesBox.TextChanged += delegate { RefreshIdleDisplay(); };
            secondsBox.TextChanged += delegate { RefreshIdleDisplay(); };
        }

        private TextBox AddTimeBox(int x, string unit)
        {
            TextBox box = new TextBox();
            box.Location = new Point(x, 105);
            box.Size = new Size(63, 31);
            box.TextAlign = HorizontalAlignment.Center;
            Controls.Add(box);
            Label label = new Label();
            label.Text = unit;
            label.Location = new Point(x + 71, 109);
            label.Size = new Size(32, 24);
            Controls.Add(label);
            return box;
        }

        private Button AddButton(string text, int x, EventHandler handler)
        {
            Button button = new Button();
            button.Text = text;
            button.Location = new Point(x, 394);
            button.Size = new Size(108, 38);
            button.Click += handler;
            Controls.Add(button);
            return button;
        }

        private void OnStartClicked(object sender, EventArgs e)
        {
            if (engine.State == TimerState.Paused)
            {
                engine.Resume(DateTime.UtcNow);
            }
            else
            {
                int seconds;
                string error;
                if (!DurationParser.TryParse(hoursBox.Text, minutesBox.Text, secondsBox.Text,
                    out seconds, out error))
                {
                    MessageBox.Show(this, error, "时长无效", MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }
                engine.Start(seconds, DateTime.UtcNow);
                SaveSettings();
            }
            ticker.Start();
            RefreshInterface();
        }

        private void OnPauseClicked(object sender, EventArgs e)
        {
            if (engine.Pause(DateTime.UtcNow))
            {
                HandleElapsed();
                return;
            }
            ticker.Stop();
            RefreshInterface();
        }

        private void OnResetClicked(object sender, EventArgs e)
        {
            ticker.Stop();
            pendingAlarm = false;
            engine.Reset();
            SaveSettings();
            RefreshInterface();
        }

        private void OnEndClicked(object sender, EventArgs e)
        {
            ticker.Stop();
            pendingAlarm = false;
            engine.Cancel();
            hoursBox.Clear();
            minutesBox.Clear();
            secondsBox.Clear();
            messageBox.Clear();
            SaveSettings();
            RefreshInterface();
        }

        private void CheckTimer()
        {
            if (exiting) return;
            if (engine.Update(DateTime.UtcNow))
            {
                HandleElapsed();
                return;
            }
            if (pendingAlarm && !sessionLocked && !alarmShowing)
            {
                ShowAlarm();
                return;
            }
            RefreshInterface();
        }

        private void HandleElapsed()
        {
            ticker.Stop();
            pendingAlarm = true;
            RefreshInterface();
            if (!sessionLocked) BeginInvoke(new MethodInvoker(ShowAlarm));
        }

        private void ShowAlarm()
        {
            if (!pendingAlarm || sessionLocked || exiting || alarmShowing) return;
            pendingAlarm = false;
            alarmShowing = true;
            if (soundBox.Checked) PlayReminderSound();
            using (ReminderForm popup = new ReminderForm(messageBox.Text))
            {
                popup.ShowDialog();
            }
            alarmShowing = false;
            engine.Reset();
            RefreshInterface();
            SaveSettings();
        }

        private void PlayReminderSound()
        {
            try
            {
                if (soundPlayer != null) soundPlayer.Dispose();
                if (soundStream != null) soundStream.Dispose();
                soundStream = new MemoryStream(MakeTone());
                soundPlayer = new SoundPlayer(soundStream);
                soundPlayer.Load();
                soundPlayer.Play();
            }
            catch { /* A sound device failure must not prevent the visible reminder. */ }
        }

        private static byte[] MakeTone()
        {
            const int sampleRate = 16000;
            const int samples = 6400;
            using (MemoryStream stream = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                writer.Write(new char[] { 'R', 'I', 'F', 'F' });
                writer.Write(36 + samples * 2);
                writer.Write(new char[] { 'W', 'A', 'V', 'E' });
                writer.Write(new char[] { 'f', 'm', 't', ' ' });
                writer.Write(16);
                writer.Write((short)1);
                writer.Write((short)1);
                writer.Write(sampleRate);
                writer.Write(sampleRate * 2);
                writer.Write((short)2);
                writer.Write((short)16);
                writer.Write(new char[] { 'd', 'a', 't', 'a' });
                writer.Write(samples * 2);
                for (int i = 0; i < samples; i++)
                {
                    double fade = Math.Min(1.0, Math.Min(i / 500.0, (samples - i) / 900.0));
                    double wave = Math.Sin(2.0 * Math.PI * 740.0 * i / sampleRate);
                    writer.Write((short)(wave * fade * 9000));
                }
                return stream.ToArray();
            }
        }

        private void RefreshIdleDisplay()
        {
            if (engine.State == TimerState.Idle) RefreshInterface();
        }

        private void RefreshInterface()
        {
            TimerState state = engine.State;
            bool editable = state == TimerState.Idle;
            hoursBox.Enabled = editable;
            minutesBox.Enabled = editable;
            secondsBox.Enabled = editable;
            messageBox.Enabled = editable;
            startButton.Enabled = state == TimerState.Idle || state == TimerState.Paused;
            startButton.Text = state == TimerState.Paused ? "继续" : "开始";
            pauseButton.Enabled = state == TimerState.Running;
            resetButton.Enabled = state != TimerState.Elapsed;
            endButton.Enabled = state != TimerState.Elapsed;
            statusLabel.Text = state == TimerState.Running ? "计时中" :
                state == TimerState.Paused ? "已暂停" :
                state == TimerState.Elapsed ? "时间到了" : "等待开始";

            TimeSpan remaining;
            if (state == TimerState.Idle)
            {
                int seconds;
                string error;
                remaining = DurationParser.TryParse(hoursBox.Text, minutesBox.Text,
                    secondsBox.Text, out seconds, out error)
                    ? TimeSpan.FromSeconds(seconds) : TimeSpan.Zero;
            }
            else remaining = engine.Remaining(DateTime.UtcNow);
            countdownLabel.Text = TimerEngine.FormatRemaining(remaining);
        }

        private void SaveSettings()
        {
            AppSettings settings = new AppSettings();
            settings.Hours = hoursBox.Text;
            settings.Minutes = minutesBox.Text;
            settings.Seconds = secondsBox.Text;
            settings.Message = messageBox.Text;
            settings.SoundEnabled = soundBox.Checked;
            try { SettingsStore.Save(settings); }
            catch (Exception ex)
            {
                if (!saveWarningShown)
                {
                    saveWarningShown = true;
                    MessageBox.Show(this, "无法保存上次输入：" + ex.Message,
                        "保存失败", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        private void ShowMainWindow()
        {
            if (exiting) return;
            ShowInTaskbar = true;
            Show();
            WindowState = FormWindowState.Normal;
            BringToFront();
            Activate();
            SetForegroundWindow(Handle);
        }

        private void ExitApplication()
        {
            exiting = true;
            ticker.Stop();
            SaveSettings();
            tray.Visible = false;
            Close();
        }

        private void OnMainFormClosing(object sender, FormClosingEventArgs e)
        {
            if (!exiting && e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                SaveSettings();
                ShowInTaskbar = false;
                Hide();
            }
            else SaveSettings();
        }

        private void OnMainFormClosed(object sender, FormClosedEventArgs e)
        {
            SystemEvents.PowerModeChanged -= OnPowerModeChanged;
            SystemEvents.SessionSwitch -= OnSessionSwitch;
            ticker.Dispose();
            tray.Visible = false;
            tray.Dispose();
            if (soundPlayer != null) soundPlayer.Dispose();
            if (soundStream != null) soundStream.Dispose();
        }

        private void OnPowerModeChanged(object sender, PowerModeChangedEventArgs e)
        {
            if (e.Mode == PowerModes.Resume) CheckTimerOnUiThread();
        }

        private void OnSessionSwitch(object sender, SessionSwitchEventArgs e)
        {
            if (e.Reason == SessionSwitchReason.SessionLock) sessionLocked = true;
            if (e.Reason == SessionSwitchReason.SessionUnlock)
            {
                sessionLocked = false;
                CheckTimerOnUiThread();
            }
        }

        private void CheckTimerOnUiThread()
        {
            if (!IsDisposed && IsHandleCreated)
            {
                try { BeginInvoke(new MethodInvoker(CheckTimer)); }
                catch (InvalidOperationException) { }
            }
        }
    }
}
