using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Osu_Music
{
    public class Singleton
    {
        private static Singleton instance;
        private static string path;
        public static int beatmapCount;
        public static int originalBeatmapCount;
        public static string OutputDirectory;
        public static bool cancel;
        public static bool Artwork;
        public static bool SingleAlbum;
        public static int Selection;
        private Singleton() {}

        public static Singleton Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Singleton();
                }
                return instance;
            }
        }

        public static void CancelProcess()
        {
            path = null;
            OutputDirectory = null;
            cancel = false;
            Selection = 0;
        }

        public static string GetPath()
        {
            try
            {
                path = FilePaths[0];
                FilePaths.RemoveAt(0);
                return path;
            }
            catch
            {
                return null;
            }
        }

        public static int threadCount = 0;

        public static List<string> FilePaths = new List<string>();
    }
}
