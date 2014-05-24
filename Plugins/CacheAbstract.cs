using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Android_Cache_Viewer.Plugins
{
    class CacheAbstract : CacheInterface
    {
        public virtual Control showCache(string file)
        {
            Label lbl = new Label();
            lbl.Text = "\r\nNot Implemented!";
            lbl.Dock = DockStyle.Fill;
            return lbl;
        }

        public static Image LoadImage(byte[] imageBytes)
        {
            Bitmap image = null;
            byte[] mbytes = new byte[imageBytes.Length];
            imageBytes.CopyTo(mbytes, 0);
            MemoryStream inStream = new MemoryStream(mbytes);
            image = new Bitmap(Image.FromStream(inStream));
            inStream.Close();
            //
            return image;
        }

        public static string displayBytes(byte[] o, int offset, int length)
        {
            StringBuilder sb = new StringBuilder();
            for(int i=offset;i<offset+length;i++)
            {
                sb.Append("0x"+Convert.ToInt16(o[i]).ToString("X") + " ");
            }
            //
            string s = sb.ToString().Trim();
            string[] arr = s.Trim().Split(new char[] { ' ' });
            string prev="";
            int count = 1;
            StringBuilder ss = new StringBuilder();
            foreach(string a in arr)
            {
                if(prev==a)
                    count++;
                else
                {
                    ss.Append(prev);
                    ss.Append(" ");
                    if (count > 1)
                    {
                        ss.Append("(");
                        ss.Append(count);
                        ss.Append(" times) ");
                    }
                    count = 1;
                    prev = a;
                }
            }
            ss.Append(prev);
            ss.Append(" ");
            if (count > 1)
            {
                ss.Append("(");
                ss.Append(count);
                ss.Append(" times) ");
            }

            return ss.ToString().Trim();
        }

        public static Control getContentControl(byte[] bytes)
        {
            Control ctrl = null;
            if (bytes.Length > 5)
            {
                if ((bytes[0] == 'G' && bytes[1] == 'I' && bytes[2] == 'F') || (bytes[0] == 0xFF && bytes[1] == 0xD8) || (bytes[0] == 0x89 && bytes[1] == 0x50 && bytes[2] == 0x4E && bytes[3] == 0x47))
                {
                    ctrl = new PictureBox();
                    ((PictureBox)ctrl).Image = LoadImage(bytes);
                }
                else if (bytes[0] == 0x1f && bytes[1] == 0x8b)
                {
                    var bigStream = new GZipStream(new MemoryStream(bytes), CompressionMode.Decompress);
                    var bigStreamOut = new System.IO.MemoryStream();
                    bigStream.CopyTo(bigStreamOut);
                    return getContentControl(bigStreamOut.ToArray());
                }
                else if (bytes[0] == 0x53 && bytes[1] == 0x51 && bytes[2] == 0x4C && bytes[3] == 0x69 && bytes[4] == 0x74 && bytes[5] == 0x65)
                {
                    DataTable dt = new DataTable();
                    string rnd_file = Path.GetTempPath() + "/" + Path.GetRandomFileName();
                    File.WriteAllBytes(rnd_file, bytes);
                    string data_src = @"" + rnd_file;
                    SQLiteConnection cnn = new SQLiteConnection("Data Source=" + data_src);
                    cnn.Open();
                    SQLiteCommand mycommand = new SQLiteCommand(cnn);
                    mycommand.CommandText = "SELECT name FROM sqlite_master WHERE type='table'";
                    SQLiteDataReader reader = mycommand.ExecuteReader();

                    dt.Load(reader);

                    mycommand.Dispose();
                    reader.Dispose();

                    List<string> tables = new List<string>();
                    for (int row = 0; row < dt.Rows.Count; row++)
                        tables.Add(dt.Rows[row][0].ToString());
                    //
                    ctrl = new TabControl();
                    foreach (string table in tables)
                    {
                        TabPage page = new TabPage(table);
                        page.Controls.Add(getTablePage(rnd_file, table, cnn));
                        ctrl.Controls.Add(page);
                    }
                    cnn.Dispose();
                }
                else
                {
                    ctrl = new TextBox();
                    ((TextBox)ctrl).Multiline = true;
                    ((TextBox)ctrl).ScrollBars = ScrollBars.Both;
                    ((TextBox)ctrl).Text = displayFriendlyBytes(bytes, 0, bytes.Length);
                    ((TextBox)ctrl).ReadOnly = true;
                }
            }
            else
            {
                ctrl = new TextBox();
                ((TextBox)ctrl).Multiline = true;
                ((TextBox)ctrl).ScrollBars = ScrollBars.Both;
                ((TextBox)ctrl).Text = displayFriendlyBytes(bytes, 0, bytes.Length);
                ((TextBox)ctrl).ReadOnly = true;
            }
            ctrl.Dock = DockStyle.Fill;
            return ctrl;
        }

        private static DataGridView getTablePage(string rnd_file, string tablename, SQLiteConnection cnn)
        {
            DataGridView dgv = new DataGridView();
            DataTable table = new DataTable();            
            SQLiteCommand mycommand = new SQLiteCommand(cnn);
            mycommand.CommandText = "SELECT * FROM " + tablename;
            SQLiteDataReader reader = mycommand.ExecuteReader();
            table.Load(reader);
            mycommand.Dispose();
            reader.Dispose();
            dgv.DataSource = table;            
            dgv.Dock = DockStyle.Fill;
            return dgv;
        }

        public static string displayFriendlyBytes(byte[] o, int offset, int length)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = offset; i < offset + length; i++)
            {
                if (o[i] >= 32 && o[i] < 127 || (o[i] == '\r' || o[i] == '\n'))
                    sb.Append(Convert.ToChar(o[i]));
                else
                {
                    sb.Append(".");
                }
            }
            return sb.ToString().Trim();
        }

        public static string displayHex(byte[] o, int offset, int length)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = offset; i < offset + length; i++)
            {
                sb.Append(Convert.ToInt16(o[i]).ToString("X").PadLeft(2,'0'));
            }
            return "0x"+sb.ToString().Trim();
        }

        public static int getInteger(byte[] o, int offset, int length)
        {
            int no = 0;
            for (int i = offset + length - 1; i >= offset; i--)
            {
                no = no + (o[i] * (int)Math.Pow(256, offset + length - 1 - i));
            }
            return no;
        }

        public static long getLong(byte[] o, int offset, int length)
        {
            long no = 0;
            for (int i = offset + length - 1; i >= offset; i--)
            {
                no = no + (o[i] * (long)Math.Pow(256, offset + length - 1 - i));
            }
            return no;
        }

        public static int getIntegerR(byte[] o, int offset, int length)
        {
            int no = 0;
            for (int i = offset; i < offset + length; i++)
            {
               no = no + (o[i]*(int)Math.Pow(256, i - offset));
            }
            return no;
        }

        public static long getLongR(byte[] o, int offset, int length)
        {
            long no = 0;
            for (int i = offset; i < offset + length; i++)
            {
                no = no + (o[i] * (long)Math.Pow(256, i - offset));
            }
            return no;
        }

        public static string displayString(byte[] o, int offset, int length)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = offset; i < offset + length; i++)
            {
                sb.Append(Convert.ToChar(o[i]));
            }
            return sb.ToString().Trim();
        }

        public static Bitmap ByteToImage(byte[] blob)
        {
            MemoryStream mStream = new MemoryStream();
            byte[] pData = blob;
            mStream.Write(pData, 0, Convert.ToInt32(pData.Length));
            Bitmap bm = new Bitmap(mStream, false);
            mStream.Dispose();
            return bm;

        }

        public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }

        public static DateTime JavaTimeStampToDateTime(double javaTimeStamp)
        {
            // Java timestamp is millisecods past epoch
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(Math.Round(javaTimeStamp / 1000)).ToLocalTime();
            return dtDateTime;
        }
    }
}
