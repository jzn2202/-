using System;
using System.Globalization;

namespace ReminderMini
{
    internal enum TimerState { Idle, Running, Paused, Elapsed }

    internal sealed class TimerEngine
    {
        private TimeSpan configured;
        private TimeSpan remaining;
        private DateTime endUtc;

        public TimerState State { get; private set; }

        public TimerEngine()
        {
            State = TimerState.Idle;
        }

        public void Start(int seconds, DateTime nowUtc)
        {
            if (seconds < 1 || seconds > 86400) throw new ArgumentOutOfRangeException("seconds");
            configured = TimeSpan.FromSeconds(seconds);
            remaining = configured;
            endUtc = nowUtc.Add(remaining);
            State = TimerState.Running;
        }

        public bool Update(DateTime nowUtc)
        {
            if (State != TimerState.Running) return false;
            remaining = endUtc - nowUtc;
            if (remaining > TimeSpan.Zero) return false;
            remaining = TimeSpan.Zero;
            State = TimerState.Elapsed;
            return true;
        }

        public bool Pause(DateTime nowUtc)
        {
            if (State != TimerState.Running) return false;
            if (Update(nowUtc)) return true;
            State = TimerState.Paused;
            return false;
        }

        public void Resume(DateTime nowUtc)
        {
            if (State != TimerState.Paused) return;
            endUtc = nowUtc.Add(remaining);
            State = TimerState.Running;
        }

        public void Reset()
        {
            remaining = configured;
            State = TimerState.Idle;
        }

        public void Cancel()
        {
            configured = TimeSpan.Zero;
            remaining = TimeSpan.Zero;
            State = TimerState.Idle;
        }

        public TimeSpan Remaining(DateTime nowUtc)
        {
            if (State != TimerState.Running) return remaining;
            TimeSpan value = endUtc - nowUtc;
            return value > TimeSpan.Zero ? value : TimeSpan.Zero;
        }

        public static string FormatRemaining(TimeSpan value)
        {
            int seconds = (int)Math.Ceiling(Math.Max(0, value.TotalSeconds));
            return string.Format(CultureInfo.InvariantCulture, "{0:00}:{1:00}:{2:00}",
                seconds / 3600, (seconds / 60) % 60, seconds % 60);
        }
    }

    internal static class DurationParser
    {
        public static bool TryParse(string hoursText, string minutesText, string secondsText,
            out int totalSeconds, out string error)
        {
            totalSeconds = 0;
            error = null;
            string h = (hoursText ?? "").Trim();
            string m = (minutesText ?? "").Trim();
            string s = (secondsText ?? "").Trim();
            if (h.Length == 0 && m.Length == 0 && s.Length == 0)
            {
                error = "请设置时长。";
                return false;
            }
            int hours, minutes, seconds;
            if (!ReadPart(h, out hours) || !ReadPart(m, out minutes) || !ReadPart(s, out seconds))
            {
                error = "时长只能填写非负整数。";
                return false;
            }
            if (hours > 24 || minutes > 59 || seconds > 59)
            {
                error = "时长格式不正确：小时最多 24，分钟和秒最多 59。";
                return false;
            }
            long total = (long)hours * 3600 + minutes * 60 + seconds;
            if (total < 1 || total > 86400)
            {
                error = "时长必须在 1 秒到 24 小时之间。";
                return false;
            }
            totalSeconds = (int)total;
            return true;
        }

        private static bool ReadPart(string value, out int number)
        {
            number = 0;
            if (value.Length == 0) return true;
            foreach (char c in value) if (c < '0' || c > '9') return false;
            return int.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out number);
        }
    }
}
