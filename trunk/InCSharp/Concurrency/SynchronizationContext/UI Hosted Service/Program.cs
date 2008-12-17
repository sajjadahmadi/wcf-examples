using System;
using System.Threading;
using System.Windows.Forms;

namespace CodeRunner
{
    static class Program
    {

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            ParameterizedThreadStart threadMethod = delegate(object baseAddress)
                    {
                        string address = baseAddress as string;
                        Application.Run(new HostForm(address));
                    };

            Thread thread1 = new Thread(threadMethod);
            thread1.Start("net.pipe://localhost/UIHostedSerice1");
            
            Thread thread2 = new Thread(threadMethod);
            thread2.Start("net.pipe://localhost/UIHostedSerice2");
        }
    }
}
