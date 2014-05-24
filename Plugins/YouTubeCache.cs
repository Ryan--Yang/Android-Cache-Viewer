using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Android_Cache_Viewer.Plugins
{
    class YouTubeCache  : CacheAbstract
    {
        public override Control showCache(string file)
        {
            byte[] obj = File.ReadAllBytes(file);
            return getContentControl(obj.Skip(0x95).ToArray());
        }
    }
}
