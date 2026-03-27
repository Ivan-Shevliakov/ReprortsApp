using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ShowNotificationSystemWindows
{
    internal class Program
    {
        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        [DllImport("user32.dll")]
        static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        const int SW_HIDE = 0;
        const int SW_SHOW = 5;
        const uint WM_CLOSE = 0x0010;
        static void Main(string[] args)
        {
            var handle = GetConsoleWindow();
            ShowWindow(handle, SW_HIDE);
            foreach (string s in args)
            {
                if (s == "Warning")
                {
                    Form1 form1 = new Form1();
                    form1.ShowDialog();

                }
                else if (s == "Attention")
                {
                    Form2 form2 = new Form2();
                    form2.ShowDialog();
                }
            }

            PostMessage(handle, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);

        }
    }
}
