using DataAccess;
using System;
using System.Windows.Forms;

namespace Presentation
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();

            try
            {
                DatabaseBootstrapper.EnsureSchema();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Database bootstrap failed: " + ex.Message,
                    "Startup Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }

            Application.Run(new LoginForm());
        }
    }
}
