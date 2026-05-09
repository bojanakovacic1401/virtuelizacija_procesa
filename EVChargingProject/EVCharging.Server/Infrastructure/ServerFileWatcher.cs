using System;
using System.IO;

namespace EVCharging.Server.Infrastructure
{
    public class ServerFileWatcher : IDisposable
    {
        private FileSystemWatcher _watcher;
        private bool _disposed;

        public ServerFileWatcher(string path)
        {
            Directory.CreateDirectory(path);
            CreateFileSystemWatcher(path);
        }

        ~ServerFileWatcher()
        {
            Dispose(false);
        }

        private void CreateFileSystemWatcher(string path)
        {
            _watcher = new FileSystemWatcher
            {
                Path = path,
                IncludeSubdirectories = true,
                InternalBufferSize = 32768,
                Filter = "*.csv",
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.CreationTime
            };

            _watcher.Created += FileChanged;
            _watcher.Changed += FileChanged;
            _watcher.Renamed += FileRenamed;
            _watcher.Deleted += FileDeleted;
            _watcher.EnableRaisingEvents = true;
        }

        private void FileChanged(object sender, FileSystemEventArgs e)
        {
            Console.WriteLine("[WATCHER] {0}: {1} | {2:HH:mm:ss}", e.ChangeType, e.FullPath, DateTime.Now);
        }

        private void FileRenamed(object sender, RenamedEventArgs e)
        {
            Console.WriteLine("[WATCHER] Renamed: {0} -> {1} | {2:HH:mm:ss}", e.OldName, e.Name, DateTime.Now);
        }

        private void FileDeleted(object sender, FileSystemEventArgs e)
        {
            Console.WriteLine("[WATCHER] Deleted: {0} | {1:HH:mm:ss}", e.FullPath, DateTime.Now);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing && _watcher != null)
            {
                _watcher.EnableRaisingEvents = false;
                _watcher.Dispose();
            }

            _disposed = true;
        }
    }
}
