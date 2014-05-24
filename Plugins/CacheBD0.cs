using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Android_Cache_Viewer.Plugins
{
    class CacheBD0 : CacheAbstract
    {
        public override Control showCache(string file)
        {
            byte[] o = File.ReadAllBytes(file);
            TabControl tab = new TabControl();
            int idx = 0;
            int counter = 0;
            DataGridView dgv = null;
            DataTable table = null;
            byte[] data = null;
            int length = -1;
            StringBuilder sb = new StringBuilder();
            TabPage page = null;
            //string datastr = null;
            while (o.Length > idx)
            {
                counter++;

                dgv = new DataGridView();
                table = new DataTable();
                table.Columns.Add("Offset", typeof(int));
                table.Columns.Add("Description", typeof(string));
                table.Columns.Add("Value", typeof(string));
                //
                int offset = -1;
                for (int i = 0; i < o.Length-4;i++)
                {
                    if(o[i+idx]==0xFF && o[i+idx+1]==0xFF && o[i+idx+2]==0xFF && o[i+idx+3]==0xFF)
                    {
                        offset = i;
                        break;
                    }
                }

                table.Rows.Add(0 + idx, "Unknown", displayBytes(o, 0 + idx, offset));
                table.Rows.Add(idx + offset + 0, "Constant", displayBytes(o, idx + offset + 0, 4));
                table.Rows.Add(idx + offset + 4, "Unknown", displayBytes(o, idx + offset + 4, 4));
                table.Rows.Add(idx + offset + 8, "Date/Time", displayBytes(o, idx + offset + 8, 4));

                length = (int)(o[idx + offset + 12] * Math.Pow(256, 3) + o[idx + offset + 13] * Math.Pow(256, 2) + o[idx + offset + 14] * 256 + o[idx + offset + 15]);
                table.Rows.Add(idx + offset + 12, "Data Length", length);
                data = new byte[length];
                Array.Copy(o, idx + offset + 16, data, 0, length);

                //datastr = Encoding.UTF8.GetString(data).Trim();     
                int offset2=idx + offset + 16;
                table.Rows.Add(offset2, "Data", "");
                int l = 0;
                for (int i = 0; i < data.Length;i++ )
                {
                    if(data[i]==0x0A)//sep
                    {
                        table.Rows.Add(offset2 + i, "Seperator", displayBytes(o,offset2 + i,1));
                        i++;
                        l = data[i];
                        table.Rows.Add(offset2 + i, "Record Length",Convert.ToInt16(l));
                        i++;
                        table.Rows.Add(offset2 + i, "Record", displayString(o, offset2 + i, l));
                        i += l;
                    }
                }

                //table.Rows.Add(last_i, "Record", displayString(o, offset2 + last_i, data.Length - last_i));


                dgv.DataSource = table;
                dgv.Dock = DockStyle.Fill;
                dgv.ReadOnly = true;

                page = new TabPage(counter.ToString());
                page.Controls.Add(dgv);
                tab.TabPages.Add(page);

                idx = offset2 + length;
                //break;
            }
            tab.Dock = DockStyle.Fill;
            return tab;
        }
    }
}