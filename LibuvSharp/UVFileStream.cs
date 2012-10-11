using System;
using System.Collections.Generic;

namespace LibuvSharp
{
	public class UVFileStream : IUVStream
	{
		public Loop Loop { get; private set; }

		public UVFileStream(Loop loop)
		{
			Loop = loop;
		}

		UVFile uvfile;

		public void Open(string path, UVFileAccess access, Action<Exception> callback)
		{
			Ensure.ArgumentNotNull(callback, "path");

			UVFile.Open(Loop, path, access, (ex, file) => {
				uvfile = file;
				callback(ex);
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
			if (readCallback != null) {
				readCallback(new ByteBuffer(buffer, 0, size).Copy());
			}

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

		Action<ByteBuffer> readCallback;

		public void Read(Action<ByteBuffer> callback)
		{
			readCallback = callback;
		}

		int writeoffset = 0;
		Queue<Tuple<byte[], int, int, Action<bool>>> queue = new Queue<Tuple<byte[], int, int, Action<bool>>>();

		void HandleWrite(Exception ex, int size)
		{
			var cb = queue.Dequeue().Item4;
			if (cb != null) {
				cb(ex == null);
			}

			writeoffset += size;
			WorkWrite();
		}

		void WorkWrite()
		{
			if (queue.Count == 0) {
				if (shutdown) {
					uvfile.Truncate(writeoffset, Finish);
				}
				return;
			}
			var item = queue.Peek();
			uvfile.Write(item.Item1, item.Item2, item.Item3, HandleWrite, writeoffset);
		}

		void Finish(Exception ex)
		{
			uvfile.Close((ex2) => {
				if (shutdownCallback != null) {
					shutdownCallback();
				}
			});
		}

		public void Write(byte[] data, int index, int count, Action<bool> callback)
		{
			queue.Enqueue(Tuple.Create(data, index, count, callback));
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

