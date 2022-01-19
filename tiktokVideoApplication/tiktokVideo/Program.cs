using System;
using System.Linq;
using System.Threading;
using WMPLib;

namespace tiktokVideo
{
    internal class Program
    {
        static string path = AppDomain.CurrentDomain.BaseDirectory;
        static void Main(string[] args)
        {
            if (args.Length != 0)
            {
                foreach(string arg in args)
                {
                    if(arg.Contains(@"\") && arg.Contains(".txt"))
                    {
                        string fileName = arg.Split('\\').Last();
                        string folderName = fileName.Split('.')[0];
                        //string filePath = arg.Remove(arg.Length - fileName.Length);
                        // Код скачки/
                        DManager dManager = new DManager(path);
                        Console.WriteLine(dManager.Download(fileName, folderName));
                        Console.WriteLine(dManager.SortFiles(folderName, GetTime()));

                    }
                }
            }

            Thread.Sleep(3000);
            //Console.ReadLine();
            System.Diagnostics.Process.GetCurrentProcess().Kill();
        }

        public static TimeSpan GetTime()
        {
            Console.WriteLine("Minute:");
            int min = Int32.Parse(Console.ReadLine());
            Console.WriteLine("Second:");
            int sec = Int32.Parse(Console.ReadLine());
            return new TimeSpan(0, min, sec);
        }

    }
}
