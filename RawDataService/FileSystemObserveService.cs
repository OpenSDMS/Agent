using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Timers;

public delegate void ObserverHandler(string observerId, List<string> filePaths);

namespace RawDataService {


    public class FileSystemObserveService {

        private Dictionary<string, ObserverInfo> observers = new Dictionary<string, ObserverInfo>();


        public string AddObserver (int seconds, string targetPath, ObserverHandler handler) {
            string observerId = Guid.NewGuid().ToString();
            var observerInfo  = new ObserverInfo(seconds, targetPath, handler, observerId);
            observers[observerId] = observerInfo;
            return observerId;
        }


        public void RemoveObserver (string observerId) {
            if (observers.TryGetValue(observerId, out var observerInfo)) {
                observerInfo.Dispose();
                observers.Remove(observerId);
            }
        }


        private class ObserverInfo : IDisposable {
            public string TargetPath { get; }
            private ObserverHandler Handler { get; }
            private FileSystemWatcher FileWatcher { get; }
            private List<string> FileGroup { get; } = new List<string>();
            private Timer GroupingTimer { get; }
            private readonly object Lock = new object();
            private string ObserverId { get; }


            public ObserverInfo (int seconds, string targetPath, ObserverHandler handler, string observerId) {
                TargetPath = targetPath;
                Handler = handler;
                ObserverId = observerId;

                FileWatcher = new FileSystemWatcher {
                    Path = targetPath,
                    NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.DirectoryName,
                    Filter = "*.*",
                    IncludeSubdirectories = true
                };
                FileWatcher.Created += OnCreated;
                FileWatcher.EnableRaisingEvents = true;

                GroupingTimer = new Timer(seconds * 1000);
                GroupingTimer.Elapsed += OnGroupingTimeElapsed;
                GroupingTimer.AutoReset = false;
            }


            private void OnCreated(object sender, FileSystemEventArgs e) {
                lock (Lock) {
                    if (Directory.Exists(e.FullPath)) {
                        AddFilesFromNewFolder(e.FullPath);
                    } 
                    else {
                        FileGroup.Add(e.FullPath);
                    }

                    if (!GroupingTimer.Enabled) {
                        GroupingTimer.Start();
                    } 
                    else {
                        GroupingTimer.Stop();
                        GroupingTimer.Start();
                    }
                }
            }


            private void AddFilesFromNewFolder (string folderPath) {
                try {
                    var files = Directory.GetFiles(folderPath, "*.*", SearchOption.AllDirectories);
                    FileGroup.AddRange(files);

                    foreach (var file in files) {
                        Console.WriteLine($"File added from new folder: {file}");
                    }
                } 
                catch (Exception ex) {
                    Console.WriteLine($"Error adding files from new folder: {ex.Message}");
                }
            }


            private void OnGroupingTimeElapsed (object sender, ElapsedEventArgs e) {
                lock (Lock) {
                    if (FileGroup.Any()) {
                        Handler(ObserverId, new List<string>(FileGroup));
                        FileGroup.Clear();
                    }
                }
            }


            public void Dispose() {
                FileWatcher.EnableRaisingEvents = false;
                FileWatcher.Dispose();
                GroupingTimer.Dispose();
            }
        }
    }
}
