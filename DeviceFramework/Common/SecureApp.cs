using System;
using System.Windows.Forms;

namespace TIM.Devices.Framework.Common
{
    /// <summary>
    /// Secure apps, use it within a using-block
    /// </summary>
    /// <example>
    /// using (SecureApp MySecApp = new SecureApp(CrashWithException))
    /// {
    ///     // YOUR CODE
    /// }
    /// </example>
    public class SecureApp : IDisposable
    {
        private Action<Exception> MyExAction;

        public SecureApp(Action<Exception> MyExAction)
        {
            this.MyExAction = MyExAction;

            Application.ThreadException += new System.Threading.ThreadExceptionEventHandler(Application_ThreadException);
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            MyExAction((Exception)e.ExceptionObject);
        }

        private void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            MyExAction(e.Exception);
        }

        public void Dispose()
        {
            Application.ThreadException -= new System.Threading.ThreadExceptionEventHandler(Application_ThreadException);
            AppDomain.CurrentDomain.UnhandledException -= new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
        }
    }
}