using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.ServiceProcess;
using System.Threading.Tasks;

namespace RawDataService {

    public partial class Service1 : ServiceBase {

        private readonly FileSystemObserveService observeService = new FileSystemObserveService();

        public Service1() {
            InitializeComponent();
        }

        protected override void OnStart(string[] args) {
            Task.Run(async () => {
                // await SocketService.Start();

                observeService.AddObserver(5, @"C:\helloworld", (string targetPath, List<string> filePaths) => {
                    string tempFolder  = Path.GetTempPath();
                    string zipFileName = $"{Environment.UserName}_{DateTime.Now:yyyyMMddHHmmss}.zip";
                    string zipFilePath = Path.Combine(tempFolder, zipFileName);
                    string systemName  = Environment.MachineName;
                    string logonName   = Environment.UserName;
                    string createdAt   = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                    using (var zipArchive = ZipFile.Open(zipFilePath, ZipArchiveMode.Create)) {
                        foreach (string filePath in filePaths) {
                            string relativePath = GetRelativePath(targetPath, filePath);
                            zipArchive.CreateEntryFromFile(filePath, relativePath);
                        }
                    }
                   
                    
                });
            });
        }

        protected override void OnStop() {

        }

        internal void TestStartup(string[] args) {
            this.OnStart(args);
            Console.ReadKey();
            this.OnStop();
        }

        private string GetRelativePath(string basePath, string fullPath) {
            basePath = Path.GetFullPath(basePath).TrimEnd(Path.DirectorySeparatorChar);
            fullPath = Path.GetFullPath(fullPath);

            string[] baseParts = basePath.Split(Path.DirectorySeparatorChar);
            string[] fullParts = fullPath.Split(Path.DirectorySeparatorChar);

            int commonDepth = 0;
            while (commonDepth < baseParts.Length && commonDepth < fullParts.Length &&
                   string.Equals(baseParts[commonDepth], fullParts[commonDepth], StringComparison.OrdinalIgnoreCase)) {
                commonDepth++;
            }

            if (commonDepth == 0) {
                return fullPath;
            }

            var relativeParts = new List<string>();
            for (int i = commonDepth; i < fullParts.Length; i++) {
                relativeParts.Add(fullParts[i]);
            }

            return string.Join(Path.DirectorySeparatorChar.ToString(), relativeParts);
        }

        private async Task BufferedSendRawFile (string url, string sendFilePath, object metadata) {

        }
    }
}

