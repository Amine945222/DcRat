using System.Reflection;
using System.Windows.Forms;

namespace Server.Helper
{
    public static class ListviewDoubleBuffer
    {
        public static void Enable(ListView listView)
        {
            var aProp = typeof(Control).GetProperty("DoubleBuffered", BindingFlags.NonPublic | BindingFlags.Instance);
            aProp.SetValue(listView, true, null);
        }
    }
}