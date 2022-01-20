using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace tiktokVideo
{
    internal class Program
    {
        static string path = AppDomain.CurrentDomain.BaseDirectory;
        static void Main(string[] args)
        {
            //Render Extension
            bool renderExtension = File.Exists(path + "ffmpeg_parser.exe");
            if(renderExtension)
            {
                path += @"data\";
                Directory.CreateDirectory(path);
                Directory.CreateDirectory(path + @"\output");
                Directory.CreateDirectory(path + @"\insert_between");
                Directory.CreateDirectory(path + @"\input_videos");
                Directory.CreateDirectory(path + @"\download_video");
                if (File.Exists("logo.png"))
                    File.Copy("logo.png", path + "logo.png");
                path += @"\download_video\";
            }
            if (args.Length != 0)
            {
                foreach(string arg in args)
                {
                    if(arg.Contains(@"\") && arg.Contains(".txt"))
                    {
                        string fileName = arg.Split('\\').Last();
                        string folderName = fileName.Split('.')[0];
                        //string filePath = arg.Remove(arg.Length - fileName.Length);
                        // Download Code
                        FileManager fManager = new FileManager(path);
                        Console.WriteLine(fManager.Download(fileName, folderName));
                        Console.WriteLine(fManager.SortFiles(folderName, GetTime()));
                        if(renderExtension)
                            Console.WriteLine(fManager.Render(folderName, "input_videos"));
                    }
                }
            }

            Thread.Sleep(1500);

            System.Diagnostics.Process.GetCurrentProcess().Kill();
        }

        public static TimeSpan GetTime()
        {
            Console.WriteLine("Minute:");
            int min;
            Int32.TryParse(ReadLine(), out min);
            Console.WriteLine("Second:");
            int sec;
            Int32.TryParse(ReadLine(), out sec);
            if (min == 0 && sec == 0)
                min = 10;
            return new TimeSpan(0, min, sec);
        }

        delegate string ReadLineDelegate();
        public static string ReadLine(int timeoutms = 5000)
        {
            ReadLineDelegate d = Console.ReadLine;
            IAsyncResult result = d.BeginInvoke(null, null);
            result.AsyncWaitHandle.WaitOne(timeoutms);
            if (result.IsCompleted)
            {
                return d.EndInvoke(result);
            }
            return "0";
        }

    }
}
