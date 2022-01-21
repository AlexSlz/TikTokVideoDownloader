using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using WMPLib;

namespace tiktokVideo
{
    internal class FileManager
    {
        private string path;
        public FileManager(string _path)
        {
            path = _path;
        }
        System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
        public string Download(string fileName, string folderName, bool CustomInterface = true)
        {
            watch.Start();
            string[] data = File.ReadAllText(fileName).Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            if (Directory.Exists(path + @"\" + folderName))
                Directory.Delete(path + @"\" + folderName, true);
            Directory.CreateDirectory(path + @"\" + folderName);

            List<Task> tasks = data.Select((url, index) =>
            {
                WebClient client = new WebClient();
                client.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");
                return client.DownloadFileTaskAsync(new Uri(url), path + $@"{folderName}\{index}.mp4");
            }).ToList();
            if (CustomInterface)
            {
                DrawInterface(tasks);
            }
            else
            {
                Task.WaitAll(tasks.ToArray());
            }
            watch.Stop();
            return $"All File Download, time: {watch.ElapsedMilliseconds} ms.";
        }

        private void DrawInterface(List<Task> tasks)
        {
            bool run = true;
            while (run)
            {
                int count = 0;
                tasks.ForEach(task => {
                    if(!task.IsCompleted) count++;
                });
                run = count > 0;
                Console.Write($"\r\t{count}/{tasks.Count}\t\r");
                Task.WaitAny(tasks.ToArray());
            }
        }

        public string SortFiles(string folderName, TimeSpan videoTime)
        {
            Console.WriteLine("Start sorting files.");
            List<string> allFiles = Directory.GetFiles(path + folderName, "*.*", SearchOption.AllDirectories).Where(s => s.EndsWith(".mp4")).ToList();
            int countVideoFolder = GetCountDirToSort(allFiles, videoTime);
            List<List<string>> allDirectory = DivideArray(allFiles, countVideoFolder);
            int dirCount = 0;
            allDirectory.ForEach(dir => {
                Directory.CreateDirectory($@"{path}{folderName}\{dirCount}");
                dir.ForEach(file => {
                    File.Move(file, $@"{path}{folderName}\{dirCount}\{Path.GetFileName(file)}");
                });
                dirCount++;
            });

            return $"All files are sorted in {countVideoFolder} folders.";
        }

        private int GetCountDirToSort(List<string> allFiles, TimeSpan videoTime)
        {
            var player = new WindowsMediaPlayer();
            int countVideoFolder = 0;
            TimeSpan allVideoTime = new TimeSpan(0);
            allFiles.ForEach(file => {
                allVideoTime += TimeSpan.FromSeconds(player.newMedia($"{file}").duration);
                if (allVideoTime > videoTime)
                {
                    allVideoTime = TimeSpan.Zero;
                    countVideoFolder++;
                }
            });
            return (countVideoFolder == 0) ? 1 : countVideoFolder;
        }

        private List<List<string>> DivideArray(List<string> data, int size)
        {
            return data
                     .Select((x, i) => new { Index = i, Value = x })
                     .GroupBy(x => x.Index % size)
                     .Select(x => x.Select(v => v.Value).ToList())
                     .ToList();
        }
        public string Render(string folderName, string inPutFolder = "input_videos", string outPutFolder = "output")
        {
            Console.WriteLine("Start render.");
            watch.Start();
            List<string> allDirectories = Directory.GetDirectories(path + folderName).ToList();
            int renderIndex = 0;
            allDirectories.ForEach(dir => {
                SendToRenderFolder(dir, inPutFolder);
                Console.WriteLine($"- Render #{renderIndex} Start.");
                System.Diagnostics.Process renderProcess = System.Diagnostics.Process.Start("ffmpeg_parser.exe");
                renderProcess.WaitForExit();
                RenameRenderVideo(outPutFolder);
                Console.WriteLine($"- Render #{renderIndex++} End.");
            });
            watch.Stop();
            return $"All Video Render, time: {watch.ElapsedMilliseconds} ms.";
        }
        private void SendToRenderFolder(string dir, string inPutFolder)
        {
            inPutFolder = Path.Combine(path, "..", inPutFolder);
            Directory.Delete(inPutFolder, true);
            Directory.CreateDirectory(inPutFolder);
            List<string> allFiles = Directory.GetFiles(dir, "*.*", SearchOption.AllDirectories).Where(s => s.EndsWith(".mp4")).ToList(); ;
            allFiles.ForEach(file =>
            {
                File.Move(file, Path.Combine(Path.Combine(inPutFolder, Path.GetFileName(file))));
            });
        }
        public void RenameRenderVideo(string outPutFolder)
        {
            outPutFolder = Path.Combine(path, "..", outPutFolder, $"{DateTime.Now.ToString("dd_MM_yyyy-hh_mm_ss_ms")}");
            Directory.CreateDirectory(outPutFolder);
            List<string> allFiles = Directory.GetFiles(Path.Combine(outPutFolder, ".."), "*.*", SearchOption.AllDirectories).Where(s => s.EndsWith(".mp4")).ToList();
            allFiles.ForEach(file => {
                File.Move(file, Path.Combine(outPutFolder, Path.GetFileName(file)));
            });
        }
    }
}
