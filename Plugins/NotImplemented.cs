using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Android_Cache_Viewer.Plugins
{
    class NotImplemented : CacheAbstract
    {
        public override Control showCache(string file)
        {
            return getContentControl(File.ReadAllBytes(file));
        }
    }
}
