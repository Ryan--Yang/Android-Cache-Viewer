using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Android_Cache_Viewer.Plugins
{
    interface CacheInterface 
    {
        Control showCache(string file);        
    }
}
