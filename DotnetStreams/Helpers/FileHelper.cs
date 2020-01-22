using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace DotnetStreams.Helpers
{
    public class FileHelper
    {
        public static void WaitForFileToUnlock(string fileName)
        {
            // wait for file to unlock (this needs a timeout)
            while (fileIsLocked(fileName))
            {
                if (!System.IO.File.Exists(fileName))
                {
                    return;
                }
                Thread.Sleep(10);
            }
        }

        private static bool fileIsLocked(string fileName)
        {
            try
            {
                using (FileStream fs = System.IO.File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    fs.Close();
                }
                return false;
            }
            catch (IOException)
            {
                return true;
            }
        }
    }
}
