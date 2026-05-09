using System;
using System.Globalization;
using System.IO;
using EVCharging.Server.Events;
using EVCharging.Shared.Models;

namespace EVCharging.Server.Infrastructure
{
	public class FileSessionWriter : IDisposable
	{
		private readonly StreamWriter _sessionWriter;
		private readonly StreamWriter _rejectWriter;
		private readonly StreamWriter _warningWriter;
		private bool _disposed;

		public FileSessionWriter(string sessionDirectory)
		{
			SessionDirectory = sessionDirectory;
			Directory.CreateDirectory(SessionDirectory);

			string sessionPath = Path.Combine(SessionDirectory, "session.csv");
			string rejectPath = Path.Combine(SessionDirectory, "rejects.csv");
			string warningPath = Path.Combine(SessionDirectory, "warnings.csv");

			bool newSession = !File.Exists(sessionPath) || new FileInfo(sessionPath).Length == 0;
			bool newRejects = !File.Exists(rejectPath) || new FileInfo(rejectPath).Length == 0;
			bool newWarnings = !File.Exists(warningPath) || new FileInfo(warningPath).Length == 0;

			_sessionWriter = new StreamWriter(new FileStream(sessionPath, FileMode.Append, FileAccess.Write, FileShare.Read));
			_rejectWriter = new StreamWriter(new FileStream(rejectPath, FileMode.Append, FileAccess.Write, FileShare.Read));
			_warningWriter = new StreamWriter(new FileStream(warningPath, FileMode.Append, FileAccess.Write, FileShare.Read));

			_sessionWriter.AutoFlush = true;
			_rejectWriter.AutoFlush = true;
			_warningWriter.AutoFlush = true;

			if (newSession)
			{
				_sessionWriter.WriteLine(ChargingSample.CsvHeader());
			}

			if (newRejects)
			{
				_rejectWriter.WriteLine("Timestamp,VehicleId,RowIndex,Reason");
			}

			if (newWarnings)
			{
				_warningWriter.WriteLine("Timestamp,WarningType,VehicleId,RowIndex,PreviousValue,CurrentValue,Threshold,Message");
			}
		}

		~FileSessionWriter()
		{
			Dispose(false);
		}

		public string SessionDirectory { get; private set; }

		public void WriteSample(ChargingSample sample)
		{
			ThrowIfDisposed();
			_sessionWriter.WriteLine(sample.ToCsvLine());
		}

		public void WriteReject(string vehicleId, int rowIndex, string reason)
		{
			ThrowIfDisposed();
			_rejectWriter.WriteLine(string.Format(CultureInfo.InvariantCulture,
				"{0},{1},{2},{3}",
				DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
				Escape(vehicleId),
				rowIndex,
				Escape(reason)));
		}

		public void WriteWarning(WarningEventArgs warning)
		{
			ThrowIfDisposed();
			_warningWriter.WriteLine(string.Format(CultureInfo.InvariantCulture,
				"{0},{1},{2},{3},{4},{5},{6},{7}",
				warning.Time.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
				Escape(warning.WarningType),
				Escape(warning.VehicleId),
				warning.RowIndex,
				warning.PreviousValue,
				warning.CurrentValue,
				warning.Threshold,
				Escape(warning.Message)));
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

			if (disposing)
			{
				if (_sessionWriter != null)
				{
					_sessionWriter.Dispose();
				}

				if (_rejectWriter != null)
				{
					_rejectWriter.Dispose();
				}

				if (_warningWriter != null)
				{
					_warningWriter.Dispose();
				}
			}

			_disposed = true;
		}

		private void ThrowIfDisposed()
		{
			if (_disposed)
			{
				throw new ObjectDisposedException("FileSessionWriter");
			}
		}

		private static string Escape(string value)
		{
			if (value == null)
			{
				return string.Empty;
			}

			if (value.Contains(",") || value.Contains("\"") || value.Contains("\n"))
			{
				return "\"" + value.Replace("\"", "\"\"") + "\"";
			}

			return value;
		}
	}
}
