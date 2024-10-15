using System;
using System.Drawing;
using System.Windows.Forms;

namespace Server.Handle_Packet
{
    public class HandleLogs
    {
        public void Addmsg(string Msg, Color color)
        {
            try
            {
                var LV = new ListViewItem();
                LV.Text = DateTime.Now.ToLongTimeString();
                LV.SubItems.Add(Msg);
                LV.ForeColor = color;

                if (Program.form1.InvokeRequired)
                    Program.form1.Invoke((MethodInvoker)(() =>
                    {
                        lock (Settings.LockListviewLogs)
                        {
                            Program.form1.listView2.Items.Insert(0, LV);
                        }
                    }));
                else
                    lock (Settings.LockListviewLogs)
                    {
                        Program.form1.listView2.Items.Insert(0, LV);
                    }
            }
            catch
            {
            }
        }
    }
}