using System;
using System.Windows.Forms;

namespace VisualiazdorLogica
{
    public static class Program
    {
        // Entry point
        [STAThread]
        public static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Frontend());
        }
    }
}