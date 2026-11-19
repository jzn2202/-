using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace ReminderMini
{
    internal static class Program
    {
        private const string MutexName = @"Local\ReminderMini_SingleInstance_6A9B3D18";
        internal static readonly int ActivationMessage = RegisterWindowMessage("ReminderMini_Show_6A9B3D18");

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern int RegisterWindowMessage(string message);

        [DllImport("user32.dll")]
        private static extern bool PostMessage(IntPtr window, int message, IntPtr wparam, IntPtr lparam);

        [STAThread]
        private static void Main()
        {
            bool firstInstance;
            using (Mutex mutex = new Mutex(true, MutexName, out firstInstance))
            {
                if (!firstInstance)
                {
                    PostMessage(new IntPtr(0xffff), ActivationMessage, IntPtr.Zero, IntPtr.Zero);
                    return;
                }

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new MainForm());
                GC.KeepAlive(mutex);
            }
        }
    }
}
