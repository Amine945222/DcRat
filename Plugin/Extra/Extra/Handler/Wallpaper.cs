using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace Plugin.Handler
{
    public class Wallpaper
    {
        public static readonly uint SPI_SETDESKWALLPAPER = 0x14;
        public static readonly uint SPIF_UPDATEINIFILE = 0x01;
        public static readonly uint SPIF_SENDWININICHANGE = 0x02;

        [DllImport("user32.dll")]
        public static extern uint SystemParametersInfo(uint action, uint uParam, string vParam, uint winIni);

        public void Change(byte[] img, string exe)
        {
            var path1 = Path.Combine(Path.GetTempFileName() + exe);
            var path2 = Path.Combine(Path.GetTempFileName() + exe);
            File.WriteAllBytes(path1, img);

            using (var bmp = new Bitmap(path1))
            using (var graphics = Graphics.FromImage(bmp))
            {
                bmp.Save(path2, ImageFormat.Bmp);
            }

            using (var key = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop", true))
            {
                key.SetValue("WallpaperStyle", 2.ToString());
                key.SetValue("TileWallpaper", 0.ToString());
            }

            SystemParametersInfo(SPI_SETDESKWALLPAPER, 0, path2, SPIF_UPDATEINIFILE | SPIF_SENDWININICHANGE);
        }
    }
}