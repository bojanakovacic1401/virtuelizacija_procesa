using System;
using System.Globalization;
using System.IO;

namespace EVCharging.Client.Logging
{
    public class ClientLogger : IDisposable
    {
        private readonly StreamWriter _writer;
        private bool _disposed;

        public ClientLogger(string logPath)
        {
            string directory = Path.GetDirectoryName(logPath);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            _writer = new StreamWriter(new FileStream(logPath, FileMode.Append, FileAccess.Write, FileShare.Read));
            _writer.AutoFlush = true;
        }

        ~ClientLogger()
        {
            Dispose(false);
        }

        public void Info(string message)
        {
            Write("INFO", message);
        }

        public void Warn(string message)
        {
            Write("WARN", message);
        }

        public void Error(string message)
        {
            Write("ERROR", message);
        }

        private void Write(string level, string message)
        {
            if (_disposed)
            {
                return;
            }

            _writer.WriteLine("{0},{1},{2}",
                DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
                level,
                Escape(message));
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

            if (disposing && _writer != null)
            {
                _writer.Dispose();
            }

            _disposed = true;
        }

        private static string Escape(string value)
        {
            if (value == null)
            {
                return string.Empty;
            }

            return "\"" + value.Replace("\"", "\"\"") + "\"";
        }
    }
}
