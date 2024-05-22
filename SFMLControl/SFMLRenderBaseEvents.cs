using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFMLControl
{
    public class SFMLRenderBaseEvents(SFML.Graphics.RenderWindow renderWindow) : EventArgs
    {
        public SFML.Graphics.RenderWindow RenderWindow => renderWindow;
    }
}
