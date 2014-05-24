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
    class WebViewCache : CacheAbstract
    {
        Dictionary<string, byte[]> internal_files = null;
        int last_file = 0;

        public override Control showCache(string file)
        {
            internal_files = new Dictionary<string, byte[]>();
            TabControl tab = new TabControl();
            string cache_dir = file.Substring(0, file.Length - Path.GetFileName(file).Length);

            tab.TabPages.Add(getIndexPage(cache_dir));
            tab.TabPages.Add(getDataPage(cache_dir, 0));
            tab.TabPages.Add(getDataPage(cache_dir, 1));
            tab.TabPages.Add(getDataPage(cache_dir, 2));
            tab.TabPages.Add(getDataPage(cache_dir, 3));

            string str_data = null;
            byte[] bytes = null;
            foreach(string ifile in internal_files.Keys)
            {
                bytes = internal_files[ifile];
                str_data = Encoding.UTF8.GetString(bytes).Trim();
                if (str_data!="")
                {
                    TabPage page = new TabPage(ifile);
                    page.Controls.Add(getContentControl(bytes));   
                    tab.TabPages.Add(page);
                }
            }
            string file_idx = null;
            for (int i = 0; i <= last_file;i++ )
            {
                file_idx = cache_dir+"/f_" + i.ToString("X").PadLeft(6, '0');
                if (!File.Exists(file_idx))
                    continue;
                bytes = File.ReadAllBytes(file_idx);
                TabPage page = new TabPage("f_" + i.ToString("X").PadLeft(6, '0'));
                page.Controls.Add(getContentControl(bytes));                
                tab.TabPages.Add(page);
            }

            tab.Dock = DockStyle.Fill;
            return tab;
        }

        private TabPage getDataPage(string cache_dir, int p)
        {
            BinaryReader reader_data = new BinaryReader(new FileStream(cache_dir + "data_" + p.ToString(), FileMode.Open),Encoding.ASCII);
            TabPage page = new TabPage("data_" + p.ToString());
            DataGridView dgv = new DataGridView();
            DataTable table = new DataTable();
            page.Controls.Add(dgv);

            table.Columns.Add("Offset", typeof(int));
            table.Columns.Add("Description", typeof(string));
            table.Columns.Add("Value", typeof(string));

            table.Rows.Add(reader_data.BaseStream.Position, "Index Magic", "0x" + reader_data.ReadUInt32().ToString("X"));
            long position = reader_data.BaseStream.Position;
            string minor_version = Convert.ToString(reader_data.ReadUInt16());
            string major_version = Convert.ToString(reader_data.ReadUInt16());
            table.Rows.Add(position, "Version", major_version + "." + minor_version);
            table.Rows.Add(reader_data.BaseStream.Position, "Index of file", reader_data.ReadInt16());
            table.Rows.Add(reader_data.BaseStream.Position, "Next file", reader_data.ReadInt16());
            position = reader_data.BaseStream.Position;
            int block_size = reader_data.ReadInt32();
            switch (block_size)
            {
                case 36:
                    table.Rows.Add(position, "Block Size", block_size + " (Rankings)");
                    break;
                case 256:
                    table.Rows.Add(position, "Block Size", block_size + " (Entry)");
                    break;
                case 1024:
                    table.Rows.Add(position, "Block Size", block_size + " (Sparse)");
                    break;
                case 4096:
                    table.Rows.Add(position, "Block Size", block_size);
                    break;
            }
            
            position = reader_data.BaseStream.Position;
            int entry_count = reader_data.ReadInt32();
            table.Rows.Add(position, "Stored Entry Count", entry_count);
            position = reader_data.BaseStream.Position;
            int max_entries = reader_data.ReadInt32();
            table.Rows.Add(position, "Max Entries", max_entries);
            for (int i = 0; i < 4;i++)
                table.Rows.Add(reader_data.BaseStream.Position, "Empty Entries ["+i.ToString()+"]", reader_data.ReadInt32());
            for (int i = 0; i < 4; i++)
                table.Rows.Add(reader_data.BaseStream.Position, "Last Position [" + i.ToString() + "]", reader_data.ReadInt32());
            table.Rows.Add(reader_data.BaseStream.Position, "Update Tracker", reader_data.ReadInt32());
            for (int i = 0; i < 5; i++)
                table.Rows.Add(reader_data.BaseStream.Position, "User [" + i.ToString() + "]", reader_data.ReadInt32());
            uint value = 0;
            for (int i = 0; i < 2028; i++)
            {
                position = reader_data.BaseStream.Position;
                value = reader_data.ReadUInt32();                
                if(value != 0)
                    table.Rows.Add(position, "AllocBitmap [" + i.ToString("X").PadLeft(4, '0') + "]", value);
            }
            //////////
            if (reader_data.BaseStream.Position != reader_data.BaseStream.Length)
            {
                UInt64 last_used = 0;
                Int32 current_state = 0;
                UInt32 hash = 0;
                Int32 keylen = 0;
                byte[] bytes = new byte[]{};
                byte[] bytes_tmp = null;
                int size_incr = 0;
                for (int i = 0; i < max_entries; i++)
                {
                    switch (block_size)
                    {
                        case 36:
                            position = reader_data.BaseStream.Position;
                            last_used = reader_data.ReadUInt64();
                            if (last_used != 0)
                            {
                                table.Rows.Add(position, "Entry [" + i.ToString("X").PadLeft(4, '0') + "] Last Used", last_used.ToString());
                                table.Rows.Add(reader_data.BaseStream.Position, "             Last Modified", reader_data.ReadUInt64().ToString());
                                table.Rows.Add(reader_data.BaseStream.Position, "             Next", "0x" + reader_data.ReadUInt32().ToString("X").PadLeft(8, '0'));
                                table.Rows.Add(reader_data.BaseStream.Position, "             Previous", "0x" + reader_data.ReadUInt32().ToString("X").PadLeft(8, '0'));
                                table.Rows.Add(reader_data.BaseStream.Position, "             Content Address", "0x" + reader_data.ReadUInt32().ToString("X").PadLeft(8, '0'));
                                table.Rows.Add(reader_data.BaseStream.Position, "             Dirty Flag", reader_data.ReadInt32() == 1 ? "Yes" : "No");
                                reader_data.ReadInt32();
                            }
                            else
                            {
                                reader_data.ReadBytes(28);
                            }
                            break;

                        case 256:
                            hash = reader_data.ReadUInt32();
                            reader_data.BaseStream.Seek(16, SeekOrigin.Current);
                            current_state = reader_data.ReadInt32();
                            reader_data.BaseStream.Seek(-24, SeekOrigin.Current);
                            if (current_state == 0 && hash != 0)
                            {
                                table.Rows.Add(reader_data.BaseStream.Position, "Entry [" + i.ToString("X").PadLeft(4, '0') + "] Hash", reader_data.ReadUInt32().ToString("X").PadLeft(8, '0'));
                                table.Rows.Add(reader_data.BaseStream.Position, "             Next", "0x" + reader_data.ReadUInt32().ToString("X").PadLeft(8, '0'));
                                table.Rows.Add(reader_data.BaseStream.Position, "             Rankings Node", "0x" + reader_data.ReadUInt32().ToString("X").PadLeft(8, '0'));
                                table.Rows.Add(reader_data.BaseStream.Position, "             Reuse Count", "0x" + reader_data.ReadInt32());
                                table.Rows.Add(reader_data.BaseStream.Position, "             Refetch Count", "0x" + reader_data.ReadInt32());
                                table.Rows.Add(reader_data.BaseStream.Position, "             Current State", "0x" + reader_data.ReadInt32());
                                table.Rows.Add(reader_data.BaseStream.Position, "             Creation Time", "0x" + reader_data.ReadUInt64());
                                position = reader_data.BaseStream.Position;
                                keylen = reader_data.ReadInt32();
                                table.Rows.Add(position, "             Key Length", keylen);
                                table.Rows.Add(reader_data.BaseStream.Position, "             Address of long key (Optional)", "0x" + reader_data.ReadUInt32());
                                for (int j = 0; j < 4; j++)
                                    table.Rows.Add(reader_data.BaseStream.Position, "             Data Size [" + j + "]", "0x" + reader_data.ReadInt32());
                                for (int j = 0; j < 4; j++)
                                    table.Rows.Add(reader_data.BaseStream.Position, "             Data Address [" + j + "]", "0x" + reader_data.ReadUInt32().ToString("X").PadLeft(8, '0'));
                                table.Rows.Add(reader_data.BaseStream.Position, "             Entry Flag", reader_data.ReadUInt32() == 1 ? "Parent" : "Child");
                                for (int j = 0; j < 5; j++)
                                    reader_data.ReadInt32();
                                keylen = 0;
                                table.Rows.Add(reader_data.BaseStream.Position, "             Key", new string(reader_data.ReadChars(keylen)));
                                reader_data.ReadBytes(256 - 24 * 4 - keylen);
                            }
                            else
                            {
                                reader_data.ReadBytes(256);
                            }
                            break;

                        case 1024:
                            if (reader_data.PeekChar() == 0)
                            {
                                if (bytes.Length > 0)
                                    internal_files.Add("data_" + p.ToString() + ": Entry" + i, bytes);
                                bytes = new byte[] { };
                                size_incr = 0;
                            }
                            else
                            {
                                size_incr += 1024;
                                Array.Resize(ref bytes, size_incr);
                                bytes_tmp = reader_data.ReadBytes(1024);
                                Array.Copy(bytes_tmp, 0, bytes, size_incr - 1024, bytes_tmp.Length);
                            }
                            break;

                        case 4096:
                            if (reader_data.PeekChar()==0)
                            {
                                if (bytes.Length > 0)
                                    internal_files.Add("data_" + p.ToString() + ": Entry" + i, bytes);
                                bytes = new byte[] { };
                                size_incr = 0;
                            }
                            else
                            {
                                size_incr += 4096;
                                Array.Resize(ref bytes, size_incr);
                                bytes_tmp = reader_data.ReadBytes(4096);
                                Array.Copy(bytes_tmp, 0, bytes, size_incr - 4096, bytes_tmp.Length);
                            }
                            break;
                        default:
                            break;
                    }

                }
            }

            dgv.DataSource = table;
            dgv.Dock = DockStyle.Fill;
            dgv.ReadOnly = true;
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;            
            reader_data.Close();
            return page;
        }

        private TabPage getIndexPage(string cache_dir)
        {
            BinaryReader reader_index = new BinaryReader(new FileStream(cache_dir + "index", FileMode.Open));
            TabPage page = null;
            DataGridView dgv = new DataGridView();
            DataTable table = new DataTable();
            table.Columns.Add("Offset", typeof(int));
            table.Columns.Add("Description", typeof(string));
            table.Columns.Add("Value", typeof(string));


            table.Rows.Add(reader_index.BaseStream.Position, "Index Magic", "0x" + reader_index.ReadUInt32().ToString("X"));

            long position = reader_index.BaseStream.Position;
            string minor_version = Convert.ToString(reader_index.ReadUInt16());
            string major_version = Convert.ToString(reader_index.ReadUInt16());

            table.Rows.Add(position, "Version", major_version + "." + minor_version);
            table.Rows.Add(reader_index.BaseStream.Position, "Number of entries", reader_index.ReadInt32());
            table.Rows.Add(reader_index.BaseStream.Position, "Total size", reader_index.ReadInt32());
            last_file = reader_index.ReadInt32();
            table.Rows.Add(reader_index.BaseStream.Position, "Last external file created", "f_" + reader_index.ReadInt32().ToString("X").PadLeft(6, '0'));
            table.Rows.Add(reader_index.BaseStream.Position, "Dirty Flag", reader_index.ReadInt32() == 1 ? "Yes" : "No");
            table.Rows.Add(reader_index.BaseStream.Position, "Storage for usage data", reader_index.ReadUInt32() == 1 ? "Yes" : "No");
            //
            position = reader_index.BaseStream.Position;
            int size = reader_index.ReadInt32();
            //if (size == 0)
            //    size = 0x10000;

            table.Rows.Add(position, "Actual size of the table", size);
            table.Rows.Add(reader_index.BaseStream.Position, "Signals a previous crash", reader_index.ReadInt32() == 1 ? "Yes" : "No");
            table.Rows.Add(reader_index.BaseStream.Position, "Id of an ongoing test", reader_index.ReadInt32());

            //DateTime dt = UnixTimeStampToDateTime(reader.ReadUInt64());
            table.Rows.Add(reader_index.BaseStream.Position, "Creation time for this set of files", reader_index.ReadUInt64().ToString());
            table.Rows.Add(reader_index.BaseStream.Position, "Pad", displayBytes(reader_index.ReadBytes(52 * 4), 0, 52 * 4));
            table.Rows.Add(reader_index.BaseStream.Position, "Padded content", displayBytes(reader_index.ReadBytes(4 * 2), 0, 4 * 2));
            table.Rows.Add(reader_index.BaseStream.Position, "Cache Filled Flag", reader_index.ReadInt32() == 1 ? "Yes" : "No");

            for (int i = 0; i < 5; i++)
                table.Rows.Add(reader_index.BaseStream.Position, "Sizes [" + i + "]", reader_index.ReadInt32());
            for (int i = 0; i < 5; i++)
                table.Rows.Add(reader_index.BaseStream.Position, "Heads cache address [" + i + "]", reader_index.ReadUInt32());
            for (int i = 0; i < 5; i++)
                table.Rows.Add(reader_index.BaseStream.Position, "Tails cache address [" + i + "]", reader_index.ReadUInt32());

            table.Rows.Add(reader_index.BaseStream.Position, "Transaction cache address", reader_index.ReadUInt32());
            table.Rows.Add(reader_index.BaseStream.Position, "Actual in-flight operation", reader_index.ReadInt32());
            table.Rows.Add(reader_index.BaseStream.Position, "In-flight operation list", reader_index.ReadInt32());
            table.Rows.Add(reader_index.BaseStream.Position, "Pad", displayBytes(reader_index.ReadBytes(4 * 7), 0, 4 * 7));
            uint cacheAddr = 0;
            //UInt16 cacheAddr16_01 = 0;
            //UInt16 cacheAddr16_02 = 0;
            byte[] bb = new byte[4];
            for (int st = 0; st < size; st++)
            {
                position = reader_index.BaseStream.Position;
                //cacheAddr16_01 = reader_index.ReadUInt16();
                //cacheAddr16_02 = reader_index.ReadUInt16();

                //cacheAddr = cacheAddr16_01 * (uint)256 + cacheAddr16_02;
                cacheAddr = reader_index.ReadUInt32();
                if (cacheAddr != 0)
                {
                    table.Rows.Add(position, "Cache Addresses [" + st.ToString("X").PadLeft(4, '0') + "]", "0x"+cacheAddr.ToString("X").PadLeft(8, '0'));
                }
            }

            dgv.DataSource = table;
            dgv.Dock = DockStyle.Fill;
            dgv.ReadOnly = true;
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            page = new TabPage("index");
            page.Controls.Add(dgv);
            reader_index.Close();
            return page;
        }
    }
}