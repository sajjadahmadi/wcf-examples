using System;
using System.Diagnostics;
using System.ServiceModel;
using System.Threading;
using System.Windows.Forms;
using CodeRunner.Client;

namespace CodeRunner
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new CounterForm());
        }
    }
}
