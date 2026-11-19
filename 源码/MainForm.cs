using System;
using System.Drawing;
using System.IO;
using System.Media;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.Win32;

namespace ReminderMini
{
    internal sealed partial class MainForm : Form
    {
        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr handle);

        private readonly TimerEngine engine = new TimerEngine();
        private readonly Timer ticker = new Timer();
        private readonly NotifyIcon tray = new NotifyIcon();
        private int selectedSeconds;
        private TextBox messageBox;
        private CheckBox soundBox;
        private TextBox runMessageBox;
        private RingDisplay ring;
        private Label headingLabel;
        private Label statusLabel;
        private Label runStatusLabel;
        private Label soundStatusLabel;
        private Label durationHintLabel;
        private Label selectedDurationLabel;
        private Label validationLabel;
        private Panel setupPanel;
        private Panel runningPanel;
        private Button setDurationButton;
        private Button startButton;
        private Button pauseButton;
        private Button resetButton;
        private Button endButton;
        private Icon appIcon;
        private bool exiting;
        private bool sessionLocked;
        private bool pendingAlarm;
        private bool alarmShowing;
        private bool saveWarningShown;
        private SoundPlayer soundPlayer;
        private MemoryStream soundStream;

        public MainForm()
        {
            Text = "计时小程序";
            Font = new Font("Microsoft YaHei UI", 9.5F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = TechTheme.Background;
            ForeColor = TechTheme.Text;
            appIcon = TechTheme.CreateIcon();
            Icon = appIcon;
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.Sizable;
            MaximizeBox = false;
            ClientSize = new Size(790, 650);
            MinimumSize = new Size(720, 610);

            BuildInterface();
            AppSettings settings = SettingsStore.Load();
            int loadedSeconds;
            string ignoredError;
            selectedSeconds = DurationParser.TryParse(settings.Hours, settings.Minutes,
                settings.Seconds, out loadedSeconds, out ignoredError) ? loadedSeconds : 0;
            messageBox.Text = settings.Message ?? "";
            soundBox.Checked = settings.SoundEnabled;

            ContextMenuStrip menu = new ContextMenuStrip();
            menu.Items.Add("打开主窗口", null, delegate { ShowMainWindow(); });
            menu.Items.Add("退出", null, delegate { ExitApplication(); });
            tray.Icon = appIcon;
            tray.Text = "计时小程序";
            tray.ContextMenuStrip = menu;
            tray.Visible = true;
            tray.DoubleClick += delegate { ShowMainWindow(); };

            ticker.Interval = 250;
            ticker.Tick += delegate { CheckTimer(); };
            SystemEvents.PowerModeChanged += OnPowerModeChanged;
            SystemEvents.SessionSwitch += OnSessionSwitch;
            FormClosing += OnMainFormClosing;
            FormClosed += OnMainFormClosed;
            Shown += delegate { TechTheme.UseDarkTitleBar(this); };
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

        private void OnStartClicked(object sender, EventArgs e)
        {
            if (selectedSeconds < 1)
            {
                validationLabel.Text = "请先设置倒计时时长。";
                setDurationButton.Focus();
                return;
            }
            validationLabel.Text = "";
            engine.Start(selectedSeconds, DateTime.UtcNow);
            SaveSettings();
            ticker.Start();
            RefreshInterface();
        }

        private void OnSetDurationClicked(object sender, EventArgs e)
        {
            if (engine.State != TimerState.Idle) return;
            using (DurationPickerForm picker = new DurationPickerForm(selectedSeconds))
            {
                if (picker.ShowDialog(this) != DialogResult.OK) return;
                selectedSeconds = picker.SelectedSeconds;
            }
            validationLabel.Text = "";
            SaveSettings();
            RefreshInterface();
        }

        private void OnPauseClicked(object sender, EventArgs e)
        {
            if (engine.State == TimerState.Paused)
            {
                engine.Resume(DateTime.UtcNow);
                ticker.Start();
                RefreshInterface();
                return;
            }
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
            selectedSeconds = 0;
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

        private void RefreshInterface()
        {
            TimerState state = engine.State;
            bool editable = state == TimerState.Idle;
            setDurationButton.Enabled = editable;
            messageBox.Enabled = editable;
            startButton.Enabled = state == TimerState.Idle;
            pauseButton.Enabled = state == TimerState.Running || state == TimerState.Paused;
            pauseButton.Text = state == TimerState.Paused ? "继续" : "暂停";
            resetButton.Enabled = state != TimerState.Elapsed;
            endButton.Enabled = state != TimerState.Elapsed;
            statusLabel.Text = state == TimerState.Running ? "计时中" :
                state == TimerState.Paused ? "已暂停" :
                state == TimerState.Elapsed ? "时间到了" : "等待开始";

            TimeSpan remaining = state == TimerState.Idle ?
                TimeSpan.FromSeconds(selectedSeconds) : engine.Remaining(DateTime.UtcNow);
            bool idle = state == TimerState.Idle;
            setupPanel.Visible = idle;
            runningPanel.Visible = !idle;
            headingLabel.Text = idle ? "设置下一次提醒" : "专注当前任务";
            ring.TimeText = TimerEngine.FormatRemaining(remaining);
            ring.StateText = idle ? "准备就绪" : statusLabel.Text;
            ring.Progress = idle || selectedSeconds == 0 ? 1 :
                remaining.TotalSeconds / selectedSeconds;
            string durationText = selectedSeconds > 0 ?
                FormatDuration(selectedSeconds) : "尚未设置";
            durationHintLabel.Text = selectedSeconds > 0 ? durationText : "等待设置";
            selectedDurationLabel.Text = "当前时长  ·  " + durationText;
            string displayMessage = string.IsNullOrWhiteSpace(messageBox.Text) ?
                "时间到了" : messageBox.Text;
            if (runMessageBox.Text != displayMessage) runMessageBox.Text = displayMessage;
            runStatusLabel.Text = state == TimerState.Running ? "进行中" :
                state == TimerState.Paused ? "已暂停" : "时间到了";
            soundStatusLabel.Text = soundBox.Checked ? "弹窗 + 提示音" : "仅弹窗";
            if (!idle) runningPanel.BringToFront();
            else setupPanel.BringToFront();
        }

        private static string FormatDuration(int seconds)
        {
            if (seconds % 3600 == 0) return (seconds / 3600) + " 小时";
            if (seconds % 60 == 0) return (seconds / 60) + " 分钟";
            return TimerEngine.FormatRemaining(TimeSpan.FromSeconds(seconds));
        }

        private void SaveSettings()
        {
            AppSettings settings = new AppSettings();
            settings.Hours = selectedSeconds > 0 ? (selectedSeconds / 3600).ToString() : "";
            settings.Minutes = selectedSeconds > 0 ? ((selectedSeconds / 60) % 60).ToString() : "";
            settings.Seconds = selectedSeconds > 0 ? (selectedSeconds % 60).ToString() : "";
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
            tray.Visible = false;
            Hide();
            Close();
        }

        private void OnMainFormClosing(object sender, FormClosingEventArgs e)
        {
            if (!exiting && e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                Hide();
                SaveSettings();
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
            appIcon.Dispose();
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
