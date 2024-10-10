using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net.Http.Headers;
using System.Net.Http;
using System.ServiceProcess;
using System.Threading.Tasks;
using System.Configuration;

namespace RawDataService {

    public partial class Service1 : ServiceBase {

        private readonly static string apiHost = ConfigurationManager.AppSettings["API_HOST"];
        private readonly FileSystemObserveService observeService = new FileSystemObserveService();

        public Service1() {
            InitializeComponent();

            Console.WriteLine("API_HOST=" + apiHost);
        }

        protected override void OnStart(string[] args) {
            observeService.AddObserver(5, @"C:\helloworld", (string targetPath, List<string> filePaths) => {
                string tempFolder = Path.GetTempPath();
                string zipFileName = $"{Environment.UserName}_{DateTime.Now:yyyyMMddHHmmss}.zip";
                string zipFilePath = Path.Combine(tempFolder, zipFileName);
                string systemName = Environment.MachineName;
                string logonName = Environment.UserName;
                string createdAt = DateTime.Now.ToString("yyyy-MM-dd HH시 mm분 ss초");

                using (var zipArchive = ZipFile.Open(zipFilePath, ZipArchiveMode.Create)) {
                    foreach (string filePath in filePaths) {
                        string relativePath = GetRelativePath(targetPath, filePath);
                        zipArchive.CreateEntryFromFile(filePath, relativePath);
                    }
                }

                Console.WriteLine("called BufferedSendRawFile");
                BufferedSendRawFile(systemName, logonName, zipFilePath, createdAt);
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

        private async Task BufferedSendRawFile(string systemName, string systemUserName, string sendFilePath, string createdAt) {
            try {
                using (var client = new HttpClient())
                using (var form = new MultipartFormDataContent())
                using (var fileStream = new FileStream(sendFilePath, FileMode.Open, FileAccess.Read)) {
                    var fileContent = new StreamContent(fileStream);

                    form.Add(fileContent, "file", Path.GetFileName(sendFilePath));
                    form.Add(new StringContent(systemName), "systemName");
                    form.Add(new StringContent(systemUserName), "systemUserName");
                    form.Add(new StringContent("DEVICE01\\Repository01"), "savePath");
                    form.Add(new StringContent(createdAt), "createdAt");

                    var requestUri = apiHost + "/api/upload";
                    Console.WriteLine($"Sending request to: {requestUri}");

                    HttpResponseMessage response = await client.PostAsync(requestUri, form);

                    Console.WriteLine($"Response Status Code: {response.StatusCode}");
                    string responseContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Response Content: {responseContent}");

                    if (response.IsSuccessStatusCode) {
                        Console.WriteLine("Successfully sent raw file");
                    } 
                    else {
                        Console.WriteLine($"Failed to send raw file. Status code: {response.StatusCode}");
                    }
                }
            } 
            catch (Exception ex) {
                Console.WriteLine($"Exception occurred: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
            }
        }
    }
}

