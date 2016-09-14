using System;
using System.Collections.Generic;

namespace LibuvSharp
{
	public class UVFileStream : IUVStream<ArraySegment<byte>>, IDisposable, IHandle
	{
		public void Ref()
		{
		}

		public void Unref()
		{
		}

		public void Close(Action callback)
		{
			Close((ex) => {
				if (callback != null) {
					callback();
				}
			});
		}

		public bool HasRef {
			get {
				return true;
			}
		}

		public Loop Loop { get; private set; }

		public bool IsClosed {
			get {
				return uvfile == null;
			}
		}

		public bool IsClosing { get; private set; }

		public UVFileStream()
			: this(Loop.Constructor)
		{
		}

		public UVFileStream(Loop loop)
		{
			Loop = loop;
		}

		UVFile uvfile;

		public void OpenRead(string path, Action<Exception> callback)
		{
			Open(path, UVFileAccess.Read, callback);
		}

		public void OpenWrite(string path, Action<Exception> callback)
		{
			Open(path, UVFileAccess.Write, callback);
		}

		public void Open(string path, UVFileAccess access, Action<Exception> callback)
		{
			Ensure.ArgumentNotNull(callback, "path");

			switch (access) {
			case UVFileAccess.Read:
				Readable = true;
				break;
			case UVFileAccess.Write:
				Writeable = true;
				break;
			case UVFileAccess.ReadWrite:
				Writeable = true;
				Readable = true;
				break;
			default:
				throw new ArgumentException("access not supported");
			}

			UVFile.Open(Loop, path, access, (ex, file) => {
				uvfile = file;
				if (callback != null) {
					callback(ex);
				}
			});
		}

		protected void OnComplete()
		{
			if (Complete != null) {
				Complete();
			}
		}

		public event Action Complete;

		protected void OnError(Exception ex)
		{
			if (Error != null) {
				Error(ex);
			}
		}

		public event Action<Exception> Error;

		public bool Readable { get; private set; }

		byte[] buffer = new byte[0x1000];
		bool reading = false;
		int readposition = 0;

		void HandleRead(Exception ex, int size)
		{
			if (!reading) {
				return;
			}

			if (ex != null) {
				OnError(ex);
				return;
			}

			if (size == 0) {
				uvfile.Close((ex2) => {
					OnComplete();
				});
				return;
			}

			readposition += size;
			OnData(new ArraySegment<byte>(buffer, 0, size));

			if (reading) {
				WorkRead();
			}
		}

		void WorkRead()
		{
			uvfile.Read(Loop, readposition, new ArraySegment<byte>(buffer, 0, buffer.Length), HandleRead);
		}

		public void Resume()
		{
			reading = true;
			WorkRead();
		}

		public void Pause()
		{
			reading = false;
		}

		void OnData(ArraySegment<byte> data)
		{
			if (Data != null) {
				Data(data);
			}
		}
		public event Action<ArraySegment<byte>> Data;

		int writeoffset = 0;
		Queue<Tuple<ArraySegment<byte>, Action<Exception>>> queue = new Queue<Tuple<ArraySegment<byte>, Action<Exception>>>();

		void HandleWrite(Exception ex, int size)
		{
			var tuple = queue.Dequeue();

			WriteQueueSize -= tuple.Item1.Count;

			var cb = tuple.Item2;
			if (cb != null) {
				cb(ex);
			}

			writeoffset += size;
			WorkWrite();
		}

		void WorkWrite()
		{
			if (queue.Count == 0) {
				if (shutdown) {
					uvfile.Truncate(writeoffset, Finish);
					//uvfile.Close(shutdownCallback);
				}
				OnDrain();
			} else {
				// handle next write
				var item = queue.Peek();
				uvfile.Write(Loop, writeoffset, item.Item1, HandleWrite);
			}
		}

		void Finish(Exception ex)
		{
			uvfile.Close((ex2) => {
				uvfile = null;
				IsClosing = false;
				if (shutdownCallback != null) {
					shutdownCallback(ex ?? ex2);
				}
			});
		}

		void OnDrain()
		{
			if (Drain != null) {
				Drain();
			}
		}

		public event Action Drain;

		public long WriteQueueSize { get; private set; }

		public bool Writeable { private set; get; }

		public void Write(ArraySegment<byte> data, Action<Exception> callback)
		{
			queue.Enqueue(Tuple.Create(data, callback));
			WriteQueueSize += data.Count;
			if (queue.Count == 1) {
				WorkWrite();
			}
		}

		bool shutdown = false;
		Action<Exception> shutdownCallback = null;
		public void Shutdown(Action<Exception> callback)
		{
			shutdown = true;
			shutdownCallback = callback;
			if (queue.Count == 0) {
				uvfile.Truncate(writeoffset, Finish);
			}
		}

		void Close(Action<Exception> callback)
		{
			if (!IsClosed && !IsClosing) {
				IsClosing = true;
				uvfile.Close(callback);
			}
		}

		void Close()
		{
			Close((ex) => { });
		}

		public void Dispose()
		{
			Close();
		}
	}
}

