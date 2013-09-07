using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Kernel.BFHAdmin.Common
{
    public static class Win32API
    {
        //WinAPI-Declaration for SendMessage
        [DllImport("user32.dll")]
        public static extern IntPtr SendMessage(IntPtr window, int message, int wparam, int lparam);

        public const int WM_VSCROLL = 0x115;
        public const int SB_BOTTOM = 7;
 
    }
}
