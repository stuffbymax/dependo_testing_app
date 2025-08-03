// Program.cs

using System; // For [STAThread]
using System.Windows.Forms;

namespace dependo_testing_app
{
    internal static class Program
    {
        // This attribute tells the OS that this is a single-threaded UI application.
        [STAThread]
        static void Main()
        {
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            
            // This is where your app starts.
            // Make sure your main form class is named Form1.
            Application.Run(new Form1()); 
        }
    }
}