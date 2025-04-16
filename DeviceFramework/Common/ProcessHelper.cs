using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using TIM.Devices.Framework.Common.Extensions;

namespace TIM.Devices.Framework.Common
{
    public class ProcessHelper
    {
        #region DllImport

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern Boolean ShowWindow(IntPtr hWnd, Int32 nCmdShow);

        #endregion

        #region Constants

        private const int SHOWMAXIMIZED = 1;

        #endregion

        public static Process FindRunningInstanceWithSameName()
        {
            // http://stackoverflow.com/questions/17956286/maximize-already-running-window-c-sharp

            try
            {
                Process currentProcess = Process.GetCurrentProcess();
                Process[] processes = Process.GetProcessesByName(currentProcess.ProcessName);

                return processes.FirstOrDefault(
                    p =>
                        p.Id != currentProcess.Id &&
                        p.ProcessName.Equals(currentProcess.ProcessName, StringComparison.Ordinal) &&
                        p.SessionId == currentProcess.SessionId);
            }
            catch
            {
                return null;
            }
        }

        public static void ShowRunningInstanceInForeground(Process process)
        {
            try
            {
                ShowWindow(process.MainWindowHandle, SHOWMAXIMIZED);
                SetForegroundWindow(process.MainWindowHandle);
            }
            catch (Exception ex)
            {
                ex.LogWarning();
            }
        }
    }
}