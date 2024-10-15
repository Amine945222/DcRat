using System;
using System.Windows.Forms;

namespace Server
{
    internal static class Program
    {
        public static Form1 form1;

        /*
         *                         _                  _
         *                        | |                | |
         *  __ ___      ____ _  __| | __ _ _ __   ___| |__  _   _ _ __
         * / _` \ \ /\ / / _` |/ _` |/ _` | '_ \ / __| '_ \| | | | '_ \
         *| (_| |\ V  V / (_| | (_| | (_| | | | | (__| | | | |_| | | | |
         * \__, | \_/\_/ \__, |\__,_|\__,_|_| |_|\___|_| |_|\__,_|_| |_|
         *    | |           | |
         *    |_|           |_|
         *
         */
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            form1 = new Form1();
            Application.Run(form1);
        }
    }
}