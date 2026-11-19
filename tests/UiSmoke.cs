using System;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using ReminderMini;

internal static class UiSmoke
{
    private static T Field<T>(object target, string name)
    {
        return (T)target.GetType().GetField(name,
            BindingFlags.NonPublic | BindingFlags.Instance).GetValue(target);
    }

    private static void Invoke(object target, string name)
    {
        target.GetType().GetMethod(name,
            BindingFlags.NonPublic | BindingFlags.Instance).Invoke(target, null);
    }

    private static bool ContainsText(Control root, string expected)
    {
        TextBox box = root as TextBox;
        if (box != null && box.Text == expected) return true;
        foreach (Control child in root.Controls)
            if (ContainsText(child, expected)) return true;
        return false;
    }

    [STAThread]
    private static void Main()
    {
        Console.WriteLine("界面检查启动");
        string path = Path.Combine(Path.GetTempPath(), "ReminderMiniUiSmoke_" +
            Guid.NewGuid().ToString("N") + ".xml");
        SettingsStore.PathOverride = path;
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        MainForm main = new MainForm();
        main.Opacity = 0;
        main.ShowInTaskbar = false;
        string failure = null;
        bool completed = false;
        bool popupSeen = false;
        DateTime deadline = DateTime.UtcNow.AddSeconds(12);
        Timer probe = new Timer();
        probe.Interval = 100;

        main.Shown += delegate
        {
            Console.WriteLine("主窗口已显示");
            try
            {
                main.Size = main.MinimumSize;
                Application.DoEvents();
                if (Field<RingDisplay>(main, "ring").Width < 200 ||
                    Field<TextBox>(main, "messageBox").Height < 60)
                    throw new Exception("最小窗口尺寸下，倒计时或文字输入区过小。");
                if (!Field<Panel>(main, "setupPanel").Visible)
                    throw new Exception("待开始时未显示设置区。");
                Field<TextBox>(main, "hoursBox").Text = "0";
                Field<TextBox>(main, "minutesBox").Text = "0";
                Field<TextBox>(main, "secondsBox").Text = "1";
                Field<TextBox>(main, "messageBox").Text = "界面流程测试";
                Field<CheckBox>(main, "soundBox").Checked = false;
                Console.WriteLine("即将点击开始");
                Field<Button>(main, "startButton").PerformClick();
                Console.WriteLine("已点击开始");
                if (Field<TimerEngine>(main, "engine").State != TimerState.Running)
                    throw new Exception("开始按钮未启动倒计时。");
                if (!Field<Panel>(main, "runningPanel").Visible)
                    throw new Exception("计时中未显示操作区。");
                Console.WriteLine("即将关闭主窗口");
                main.Close();
                Console.WriteLine("已关闭主窗口");
                if (main.Visible) throw new Exception("关闭主窗口后未隐藏到托盘。");
                Console.WriteLine("后台计时已启动");
                probe.Start();
            }
            catch (Exception ex)
            {
                failure = ex.Message;
                Invoke(main, "ExitApplication");
            }
        };

        probe.Tick += delegate
        {
            try
            {
                if (DateTime.UtcNow > deadline) throw new Exception("等待提醒弹窗超时。");
                if (!popupSeen)
                {
                    foreach (Form form in Application.OpenForms)
                    {
                        if (form is ReminderForm)
                        {
                            Console.WriteLine("已发现提醒弹窗");
                            if (!form.TopMost) throw new Exception("提醒弹窗未置顶。");
                            if (!ContainsText(form, "界面流程测试"))
                                throw new Exception("提醒弹窗未显示设定文字。");
                            popupSeen = true;
                            form.Close();
                            break;
                        }
                    }
                    return;
                }

                if (Field<TimerEngine>(main, "engine").State != TimerState.Idle) return;
                Console.WriteLine("提醒结束，检查输入");
                Invoke(main, "ShowMainWindow");
                if (!main.Visible) throw new Exception("无法从托盘恢复主窗口。");
                if (Field<TextBox>(main, "messageBox").Text != "界面流程测试")
                    throw new Exception("正常提醒后未保留输入。");
                Field<TextBox>(main, "secondsBox").Text = "10";
                Field<Button>(main, "startButton").PerformClick();
                Field<Button>(main, "pauseButton").PerformClick();
                if (Field<TimerEngine>(main, "engine").State != TimerState.Paused ||
                    Field<Button>(main, "pauseButton").Text != "继续")
                    throw new Exception("暂停按钮状态不正确。");
                Field<Button>(main, "pauseButton").PerformClick();
                if (Field<TimerEngine>(main, "engine").State != TimerState.Running)
                    throw new Exception("继续按钮未恢复计时。");
                Field<Button>(main, "resetButton").PerformClick();
                if (Field<TimerEngine>(main, "engine").State != TimerState.Idle ||
                    Field<TextBox>(main, "messageBox").Text != "界面流程测试")
                    throw new Exception("重置未保留输入并停止计时。");
                Field<Button>(main, "startButton").PerformClick();
                Field<Button>(main, "endButton").PerformClick();
                if (Field<TextBox>(main, "messageBox").Text != "" ||
                    Field<TextBox>(main, "secondsBox").Text != "")
                    throw new Exception("结束后未清空输入。");
                if (SettingsStore.Load().Message != "")
                    throw new Exception("结束后未清空保存的输入。");
                completed = true;
                probe.Stop();
                Invoke(main, "ExitApplication");
            }
            catch (Exception ex)
            {
                failure = ex.Message;
                probe.Stop();
                Invoke(main, "ExitApplication");
            }
        };

        Application.Run(main);
        probe.Dispose();
        if (File.Exists(path)) File.Delete(path);
        if (failure != null) throw new Exception(failure);
        if (!completed || !popupSeen) throw new Exception("界面流程未完成。");
        Console.WriteLine("界面流程检查通过：后台计时、弹窗、暂停继续、重置、清空输入。");
    }
}
