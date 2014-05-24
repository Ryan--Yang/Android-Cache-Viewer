using Android_Cache_Viewer.Plugins;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Android_Cache_Viewer
{
    class CacheIdentity
    {
        public static CacheInterface Identify(String filename)
        {
            if (filename.EndsWith(".cache"))
                return new YouTubeCache();
            else if (Path.GetFileNameWithoutExtension(filename).Equals("imgcache"))
                return new Gallery3dCache();
            else if (filename.EndsWith("cache_r.0"))
                return new CacheR0();
            else if (filename.EndsWith("cache_bd.0"))
                return new CacheBD0();
            else if (filename.EndsWith(".db"))
                return new SQLiteDB();
            else if (filename.EndsWith("index") || Path.GetFileName(filename).StartsWith("data_")||Path.GetFileName(filename).StartsWith("f_"))
                return new WebViewCache();
            else
                return new NotImplemented();
        }
    }
}
