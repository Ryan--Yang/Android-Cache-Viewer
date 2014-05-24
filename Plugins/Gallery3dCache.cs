using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Android_Cache_Viewer.Plugins
{
    class Gallery3dCache : CacheAbstract
    {
        public override Control showCache(string file)
        {
            if (!file.EndsWith(".idx"))
            {
                byte[] obj = File.ReadAllBytes(file);
                return getContentControl(obj.Skip(0x46).ToArray());
            }
            else
                return new Label();
        }
    }
}