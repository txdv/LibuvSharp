using System;
using System.Collections.Generic;

namespace LibuvSharp
{
	struct QueueElement
	{
		public QueueElement(byte[] data, int index, int count, Action<bool> callback)
		{
			this.data = data;
			this.index = index;
			this.count = count;
			this.callback = callback;
		}

		public byte[] data;
		public int index;
		public int count;
		public Action<bool> callback;
	}

	public class UVFileStream : IUVStream
	{
		public Loop Loop { get; private set; }

		public UVFileStream()
			: this(Loop.Default)
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
			uvfile.Read(buffer, HandleRead, readposition);
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
		Queue<QueueElement> queue = new Queue<QueueElement>();

		void HandleWrite(Exception ex, int size)
		{
			var tuple = queue.Dequeue();

			WriteQueueSize -= tuple.count;

			var cb = tuple.callback;
			if (cb != null) {
				cb(ex == null);
			}

			writeoffset += size;
			WorkWrite();
		}

		void WorkWrite()
		{
			if (queue.Count == 0) {
				OnDrain();
				if (shutdown) {
					uvfile.Truncate(writeoffset, Finish);
				}
				return;
			}
			var item = queue.Peek();
			uvfile.Write(item.data, item.index, item.count, HandleWrite, writeoffset);
		}

		void Finish(Exception ex)
		{
			uvfile.Close((ex2) => {
				if (shutdownCallback != null) {
					shutdownCallback();
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

		public void Write(byte[] data, int index, int count, Action<bool> callback)
		{
			queue.Enqueue(new QueueElement(data, index, count, callback));
			WriteQueueSize += count;
			if (queue.Count == 1) {
				WorkWrite();
			}
		}

		bool shutdown = false;
		Action shutdownCallback = null;
		public void Shutdown(Action callback)
		{
			shutdown = true;
			shutdownCallback = callback;
			if (queue.Count == 0) {
				uvfile.Truncate(writeoffset, Finish);
			}
		}

		void Close(Action<Exception> callback)
		{
			uvfile.Close(callback);
		}

		void Close(Action callback)
		{
			Close((ex) => {
				if (callback != null) {
					callback();
				}
			});
		}

		void Close()
		{
			Close((ex) => { });
		}
	}
}

