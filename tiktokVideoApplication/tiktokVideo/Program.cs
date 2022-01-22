using System;
using System.IO;
using System.Linq;
using System.Threading;

namespace tiktokVideo
{
    internal class Program
    {
        static string path = AppDomain.CurrentDomain.BaseDirectory;
        static void Main(string[] args)
        {
            //args = new string[] { @"\video.txt" };
            //Render Extension
            Directory.CreateDirectory(path + "add_video");
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
                    try
                    {
                        File.Copy("logo.png", Path.Combine(path, "logo.png"));
                    }
                    catch
                    {}
                if (File.Exists("between.mp4"))
                    try
                    {
                        File.Copy("between.mp4", Path.Combine(path, "insert_between", "between.mp4"));
                    }
                    catch
                    { }
                if (File.Exists("intro.webm"))
                    try
                    {
                        File.Copy("intro.webm", Path.Combine(path, "intro.webm"));
                    }
                    catch
                    { }
                if (File.Exists("end.mp4"))
                    try
                    {
                        File.Copy("end.mp4", Path.Combine(path, "end.mp4"));
                    }
                    catch
                    { }
                path += @"\download_video\";
            }
            FileManager fManager = new FileManager(path);
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
                        Console.WriteLine(fManager.Download(fileName, folderName));
                        Console.WriteLine("Sort? [true/false]");
                        if(Boolean.Parse(ReadLine("true")))
                            Console.WriteLine(fManager.SortFiles(folderName, GetTime()));
                        if (renderExtension)
                        {
                            Console.WriteLine("Render? [true/false]");
                            if (Boolean.Parse(ReadLine("true")))
                                Console.WriteLine(fManager.Render(folderName));
                        }
                    }
                }
            }
            else
            {
                DrawMenu(fManager, renderExtension);
            }

            Thread.Sleep(3000);

            System.Diagnostics.Process.GetCurrentProcess().Kill();
        }

        public static void DrawMenu(FileManager fManager, bool renderExtension)
        {
            Console.WriteLine("Menu:\n");
            Console.WriteLine("- 1. Download.");
            Console.WriteLine("- 2. Just SortFile.");
            if (renderExtension)
            {
                Console.WriteLine("- 3. Just Render one file. Or WhiteSpace to Render All files.");
            }
            string menu = Console.ReadLine();
            switch (menu)
            {
                case "1":
                    Console.WriteLine("Enter FileName");
                    string Name = Console.ReadLine();
                    fManager.Download(Name, Path.GetFileNameWithoutExtension(Name));
                    break;
                case "2":
                    Console.WriteLine("Enter FolderName");
                    fManager.SortFiles(Console.ReadLine(), GetTime());
                    break;
                case "3":
                    Console.WriteLine("Enter FolderName \\n SecondFolderName");
                    fManager.Render(Console.ReadLine(), Console.ReadLine());
                    break;
                default:
                    Console.WriteLine("ok");
                    break;
            }
        }

        public static TimeSpan GetTime()
        {
            Console.WriteLine("Enter time to sort videos by time:");
            Console.WriteLine("Minute(10):");
            int min;
            Int32.TryParse(ReadLine("10"), out min);
            Console.WriteLine("Second(0):");
            int sec;
            Int32.TryParse(ReadLine("0"), out sec);
            return new TimeSpan(0, min, sec);
        }
        delegate string ReadLineDelegate();
        public static string ReadLine(string standard, int timeoutms = 7000)
        {
            ReadLineDelegate d = Console.ReadLine;
            IAsyncResult result = d.BeginInvoke(null, null);
            result.AsyncWaitHandle.WaitOne(timeoutms);
            if (result.IsCompleted)
            {
                string res = d.EndInvoke(result);
                if (res == "")
                    res = standard;
                return res;
            }
            return standard;
        }

    }
}
