using System;
using System.IO;
using System.Xml.Serialization;
using ReminderMini;

internal static class TimerTests
{
    private static int checks;

    private static void Assert(bool condition, string name)
    {
        checks++;
        if (!condition) throw new Exception("失败：" + name);
    }

    private static void Main()
    {
        int seconds;
        string error;
        Assert(DurationParser.TryParse("0", "40", "0", out seconds, out error)
            && seconds == 2400, "40 分钟");
        Assert(DurationParser.TryParse("", "", "1", out seconds, out error)
            && seconds == 1, "最短 1 秒");
        Assert(DurationParser.TryParse("24", "0", "0", out seconds, out error)
            && seconds == 86400, "最长 24 小时");
        Assert(!DurationParser.TryParse("24", "0", "1", out seconds, out error),
            "超过 24 小时");
        Assert(!DurationParser.TryParse("", "", "", out seconds, out error),
            "清空后不可开始");
        Assert(!DurationParser.TryParse("1", "60", "0", out seconds, out error),
            "分钟超界");
        Assert(!DurationParser.TryParse("-1", "0", "0", out seconds, out error),
            "负数无效");

        DateTime t0 = new DateTime(2026, 11, 19, 0, 0, 0, DateTimeKind.Utc);
        TimerEngine timer = new TimerEngine();
        timer.Start(60, t0);
        Assert(timer.State == TimerState.Running, "开始");
        Assert(TimerEngine.FormatRemaining(timer.Remaining(t0.AddMilliseconds(200))) == "00:01:00",
            "倒计时向上显示秒数");
        Assert(!timer.Pause(t0.AddSeconds(20)), "暂停时尚未到点");
        Assert(timer.State == TimerState.Paused &&
            TimerEngine.FormatRemaining(timer.Remaining(t0.AddHours(3))) == "00:00:40",
            "暂停期间时间不流逝");
        timer.Resume(t0.AddHours(3));
        Assert(!timer.Update(t0.AddHours(3).AddSeconds(39)), "继续后尚未到点");
        Assert(timer.Update(t0.AddHours(3).AddSeconds(40)), "继续后到点");
        Assert(!timer.Update(t0.AddHours(3).AddSeconds(41)), "到点只触发一次");
        timer.Reset();
        Assert(timer.State == TimerState.Idle &&
            TimerEngine.FormatRemaining(timer.Remaining(t0)) == "00:01:00", "重置");
        timer.Start(60, t0);
        Assert(timer.Update(t0.AddHours(2)), "睡眠后按实际经过时间到点");
        timer.Cancel();
        Assert(timer.State == TimerState.Idle && timer.Remaining(t0) == TimeSpan.Zero,
            "结束取消");

        AppSettings settings = new AppSettings();
        settings.Message = "喝水\n休息一下";
        settings.SoundEnabled = false;
        using (MemoryStream stream = new MemoryStream())
        {
            XmlSerializer serializer = new XmlSerializer(typeof(AppSettings));
            serializer.Serialize(stream, settings);
            stream.Position = 0;
            AppSettings restored = (AppSettings)serializer.Deserialize(stream);
            Assert(restored.Message == settings.Message && !restored.SoundEnabled,
                "中文多行设置保存与读取");
        }
        Console.WriteLine("通过 " + checks + " 项计时检查。");
    }
}
