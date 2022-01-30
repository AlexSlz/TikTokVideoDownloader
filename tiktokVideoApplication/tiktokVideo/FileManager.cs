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
        private string addPath;
        public FileManager(string _path)
        {
            path = _path;
            addPath = Path.Combine(path, (path.Contains("download_video")) ? @"..\.." : "", "add_video");
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
            if (Directory.Exists(addPath))
            {
                CopyAddFile(folderName, data.Length);
            }
            return $"All File Download, time: {watch.ElapsedMilliseconds} ms.";
        }
        private void CopyAddFile(string folderName, int lastIndex = 0)
        {
            List<string> allFiles = Directory.GetFiles(addPath, "*.*", SearchOption.AllDirectories).Where(s => s.EndsWith(".mp4")).ToList();
            allFiles.ForEach(file => { File.Copy(file, Path.Combine(path, folderName, $"{lastIndex++}.mp4")); });
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
            allDirectory.ForEach(dir =>
            {
                Directory.CreateDirectory($@"{path}{folderName}\{dirCount}");
                dir.ForEach(file =>
                {
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
                    Console.WriteLine($"Folder{countVideoFolder++} - Time - {allVideoTime}");
                    allVideoTime = TimeSpan.Zero;
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
        public string Render(string folderName, string secondFolderName = "", string inPutFolder = "input_videos", string outPutFolder = "output")
        {
            Console.WriteLine("Start render.");
            watch.Start();
            List<string> allDirectories = Directory.GetDirectories(Path.Combine(path, folderName, secondFolderName)).ToList();
            int renderIndex = 0;
            if(allDirectories.Count == 0)
            {
                RenderVideo(Path.Combine(path, folderName, secondFolderName), inPutFolder, renderIndex++);
            }
            allDirectories.ForEach(dir => {
                RenderVideo(dir, inPutFolder, renderIndex++);
            });
            watch.Stop();
            return $"All Video Render, time: {watch.ElapsedMilliseconds} ms.";
        }

        private void RenderVideo(string dir, string inPutFolder, int renderIndex)
        {
            SendToRenderFolder(dir, inPutFolder);
            Console.WriteLine($"- Render #{renderIndex} Start.");
            System.Diagnostics.Process renderProcess = System.Diagnostics.Process.Start("ffmpeg_parser.exe");
            renderProcess.WaitForExit();
            Console.WriteLine($"- Render #{renderIndex} End.");
        }

        private void SendToRenderFolder(string dir, string inPutFolder)
        {
            inPutFolder = Path.Combine(path, "..", inPutFolder);
            Directory.Delete(inPutFolder, true);
            Directory.CreateDirectory(inPutFolder);
            List<string> allFiles = Directory.GetFiles(dir, "*.*", SearchOption.AllDirectories).Where(s => s.EndsWith(".mp4")).ToList(); ;
            allFiles.ForEach(file =>
            {
                File.Copy(file, Path.Combine(Path.Combine(inPutFolder, Path.GetFileName(file))));
            });
        }
        public void checkCustomFiles(CustomFiles[] customFiles)
        {
            customFiles.ToList().ForEach(item => {
                string[] file = Directory.GetFiles(path, item.name + ".*");
                if (file.Length > 0)
                {
                    try
                    {
                        File.Copy(file[0], Path.Combine(path, item.path, Path.GetFileName(file[0])));
                    }
                    catch {}
                }
            });
        }
    }
}
