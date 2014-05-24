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
    class CacheR0 : CacheAbstract
    {
        public override Control showCache(string file)
        {
            byte[] o = File.ReadAllBytes(file);
            TabControl tab = new TabControl();
            int idx = 0;
            int counter = 0;
            DataGridView dgv = null;
            DataTable table = null;
            byte[] url_data = null;
            int length = -1;
            string url = null;
            int end_offset = 0;
            StringBuilder sb = new StringBuilder();
            byte[] contentType = null;
            int val = -1;
            string ct = null;
            TabPage page = null;
            TabControl subtab = null;
            TabPage subdata = null;
            TabPage subpage = null;
            Control data_ctrl = new Label();
            while (o.Length > idx)
            {
                counter++;

                subtab = new TabControl();
                subdata = new TabPage("Data");           
                subpage = new TabPage("Details");                

                dgv = new DataGridView();
                table = new DataTable();
                table.Columns.Add("Offset", typeof(int));
                table.Columns.Add("Description", typeof(string));
                table.Columns.Add("Value", typeof(string));

                table.Rows.Add(0 + idx, "Constant ", displayBytes(o, 0 + idx, 4));
                table.Rows.Add(4 + idx, "Date/Time ", displayBytes(o, 4 + idx, 4));
                table.Rows.Add(8 + idx, "Counter ", displayBytes(o, 8 + idx, 1));
                table.Rows.Add(9 + idx, "Constant ", displayBytes(o, 9 + idx, 1));
                length = (int)o[10 + idx];
                table.Rows.Add(10 + idx, "Length of the URL", length);
                url_data = new byte[length];
                val = o[11 + idx];
                if (val == 1)
                    idx++;
                Array.Copy(o, 11 + idx, url_data, 0, length);
                url = Encoding.UTF8.GetString(url_data);
                table.Rows.Add(11 + idx, "URL of the cached data", url);
                table.Rows.Add(11 + length + idx, "Constant ", displayBytes(o, 11 + length + idx, 4));
                table.Rows.Add(15 + length + idx, "Unknown ", displayBytes(o, 15 + length + idx, 4));
                table.Rows.Add(19 + length + idx, "Constant ", displayBytes(o, 19 + length + idx, 4)); // 0x0 0x0 0x0 0x0
                table.Rows.Add(23 + length + idx, "Constant ", displayBytes(o, 23 + length + idx, 1));
                table.Rows.Add(24 + length + idx, "Length", displayBytes(o, 24 + length + idx, 2));

                end_offset = 0;
                sb.Clear();      
                int init = 26 + length + idx;
                bool image = false;
                for (int i = init; i < o.Length; i++)
                {
                    if (o[i] == 0x3A)
                    {
                        //data is PNG file.
                        if (o[init] == 0x89
                            && o[init+1] == 'P'
                            && o[init+2] == 'N'
                            && o[init+3] == 'G' 
                            && o[i - 1] == 0x82
                            && o[i - 2] == 0x60
                            && o[i - 3] == 0x42)
                        {                            
                            end_offset = i;
                            image = true;
                            break;                           
                        }
                        else if (o[init] == 0x89
                        && o[init + 1] == 'P'
                        && o[init + 2] == 'N'
                        && o[init + 3] == 'G'
                        && (o[i - 1] != 0x82
                        || o[i - 2] != 0x60
                        || o[i - 3] != 0x42))
                        {
                            sb.Append(Convert.ToChar(o[i]));
                            continue;
                        }
                        else if (o[i + 1] > 32)
                        {
                            sb.Append(Convert.ToChar(o[i]));
                            continue;
                        }
                        else //if (o[i + 2] > 0x61 && o[i + 3] > 0x61 && o[i + 4] > 0x61 && o[i + 5] > 0x61)
                        {
                            end_offset = i;
                            break;
                        }
                    }
                    else
                        sb.Append(Convert.ToChar(o[i]));
                }

                if (image)
                {
                    PictureBox data_ctrl2 = new PictureBox();

                    byte[] img_bytes = new byte[end_offset - init];
                    Array.Copy(o, init, img_bytes, 0, end_offset - init);
                    data_ctrl2.Image = ByteToImage(img_bytes);
                    data_ctrl2.Dock = DockStyle.Fill;
                    subdata.Controls.Remove(data_ctrl);
                    subdata.Controls.Add(data_ctrl2);
                }
                else
                {
                    data_ctrl.Text = sb.ToString();
                    subdata.Controls.Add(data_ctrl);
                }
                table.Rows.Add(26 + length + idx, "Data", sb.ToString());

                table.Rows.Add(end_offset, "Constant ", displayBytes(o, end_offset, 1));
                table.Rows.Add(end_offset + 1, "Content-Type Length", o[end_offset + 1]);

                contentType = new byte[o[end_offset + 1]];
                Array.Copy(o, end_offset + 2, contentType, 0, o[end_offset + 1]);
                ct = Encoding.UTF8.GetString(contentType);

                table.Rows.Add(end_offset + 2, "Content-Type", ct);
                idx = end_offset + 2+ct.Length;

                dgv.DataSource = table;
                dgv.Dock = DockStyle.Fill;
                dgv.ReadOnly = true;
                
                subpage.Controls.Add(dgv);
                subtab.Dock = DockStyle.Fill;
                subtab.Alignment = TabAlignment.Left;
                subtab.TabPages.Add(subdata);
                subtab.TabPages.Add(subpage);
                page = new TabPage(counter.ToString());
                page.Controls.Add(subtab);
                tab.TabPages.Add(page);

            }
            tab.Dock = DockStyle.Fill;
            return tab;
        }      
    }
}
