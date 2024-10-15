using System;
using System.IO;
using System.Media;
using System.Text;

namespace Plugin.Handler
{
    internal class HandlePlayAudio
    {
        private const string Alphabet = "abcdefghijklmnopqrstuvwxyz";
        public static Random Random = new Random();

        public void Play(byte[] wavfile)
        {
            var fullPath = Path.Combine(Path.GetTempPath(), GetRandomString(6) + ".wav");

            using (var fs = new FileStream(fullPath, FileMode.Create))
            {
                fs.Write(wavfile, 0, wavfile.Length);
            }


            var sp = new SoundPlayer(fullPath);
            sp.Load();
            sp.Play();
        }

        public static string GetRandomString(int length)
        {
            var randomName = new StringBuilder(length);
            for (var i = 0; i < length; i++)
                randomName.Append(Alphabet[Random.Next(Alphabet.Length)]);

            return randomName.ToString();
        }
    }
}