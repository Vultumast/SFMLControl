using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFMLControl
{
    public class RenderLoopArguments(IntPtr hwnd)
    {
        public IntPtr HWND => hwnd;
    }
}
