using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using WMPLib;

namespace tiktokVideo
{
    internal class DManager
    {
        string path;
        public DManager(string _path)
        {
            path = _path;
        }
        public string Download(string fileName, string folderName, bool CustomInterface = true)
        {
            System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
            watch.Start();
            string[] data = File.ReadAllText(fileName).Split('\n');
            Directory.CreateDirectory(path + @"\" + folderName);
            List<Task> tasks = data.Select((url, index) =>
            {
                WebClient client = new WebClient();
                client.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");
                Task res;
                try
                {
                    res = client.DownloadFileTaskAsync(new Uri(url), path + $@"{folderName}\{index}.mp4");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    throw;
                }
                return res;
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
            return $"All File Download, time: {watch.ElapsedMilliseconds} ms";
        }

        void DrawInterface(List<Task> tasks)
        {
            bool run = true;
            while (run)
            {
                int count = 0;
                tasks.ForEach(task => {
                    if(!task.IsCompleted) count++;
                });
                run = count > 0;
                Console.Write($"\r{count}/{tasks.Count}\t\r");
                Task.WaitAny(tasks.ToArray());
            }
        }

        public string SortFiles(string folderName, TimeSpan videoTime)
        {
            var player = new WindowsMediaPlayer();

            List<string> allFiles = Directory.GetFiles(path + folderName, "*.*", SearchOption.AllDirectories).Where(s => s.EndsWith(".mp4")).ToList();
            TimeSpan allVideoTime = new TimeSpan(0);
            int countVideoFolder = 0;
            allFiles.ForEach(file => {
                allVideoTime += TimeSpan.FromSeconds(player.newMedia($"{file}").duration);
                if (allVideoTime > videoTime)
                {
                    allVideoTime = TimeSpan.Zero;
                    countVideoFolder++;
                }
            });
            if(countVideoFolder == 0)
                countVideoFolder = 1;
            List<List<string>> allDirectory = DivideArray(allFiles, countVideoFolder);
            int dirCount = 0;
            allDirectory.ForEach(dir => {
                Directory.CreateDirectory(path + folderName + @"\" + dirCount);
                dir.ForEach(file => {
                    File.Move(file, path + folderName + @"\" + dirCount + @"\" + Path.GetFileName(file));
                });
                dirCount++;
            });

            return $"Sort Video to {countVideoFolder} folders.";
        }
        List<List<string>> DivideArray(List<string> data, int size)
        {
            return data
                     .Select((x, i) => new { Index = i, Value = x })
                     .GroupBy(x => x.Index % size)
                     .Select(x => x.Select(v => v.Value).ToList())
                     .ToList();
        }

    }
}
