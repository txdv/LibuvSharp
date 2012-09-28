using System;
using System.Collections.Generic;
using System.Linq;

namespace LibuvSharp
{
	public class ByteBufferCollection
	{
		List<ByteBuffer> buffers = new List<ByteBuffer>();

		public ByteBufferCollection()
		{
		}

		public void Add(ByteBuffer buffer)
		{
			buffers.Add(buffer);
		}

		List<ByteBuffer> Clone()
		{
			List<ByteBuffer> r = new List<ByteBuffer>();
			foreach (var buffer in buffers) {
				r.Add(buffer);
			}
			return r;
		}

		public void Skip(int restLength)
		{
			foreach (var buffer in Clone()) {
				int r = restLength - buffer.Length;
				if (r >= 0) {
					buffer.Skip(buffer.Length);
					buffers.Remove(buffer);
					restLength = r;
				} else {
					// it is the last buffer we need to skip
					// break afterwards
					buffer.Skip(restLength);
					return;
				}
			}
		}

		public bool HasLength(int length)
		{
			foreach (var buffer in buffers) {
				if (length < buffer.Length) {
					return true;
				}
				length -= buffer.Length;
			}
			return false;
		}

		public int Length {
			get {
				int length = 0;
				foreach (var buffer in buffers) {
					length += buffer.Length;
				}
				return length;
			}
		}

		public byte this[int index] {
			get {
				int position = index;
				foreach (var buffer in buffers) {
					if (position < buffer.Length) {
						return buffer.Buffer[buffer.Start + position];
					} else {
						position -= buffer.Length;
					}
				}
				throw new Exception();
			}
		}

		public void CopyTo(byte[] destination, int length)
		{
			int startPos = 0;
			foreach (var buffer in buffers) {
				int rest = length - buffer.Length;
				if (rest <= 0) {
					Buffer.BlockCopy(buffer.Buffer, buffer.Start, destination, startPos, length);
					break;
				} else {
					Buffer.BlockCopy(buffer.Buffer, buffer.Start, destination, startPos, buffer.Length);
					startPos += buffer.Length;
					length = rest;
				}
			}
		}

		public int FirstByte(byte val)
		{
			int pos = 0;
			foreach (var buffer in buffers) {
				for (int i = 0; i < buffer.Length; i++) {
					if (buffer.Buffer[i + buffer.Start] == val) {
						return pos;
					} else {
						pos++;
					}
				}
			}
			return -1;
		}

		public byte CurrentByte {
			get {
				var buffer = buffers.First();
				return buffer.CurrentByte;
			}
		}

		public byte[] CopyBuffer()
		{
			int length = Length;
			byte[] data = new byte[length];
			CopyTo(data, length);
			return data;
		}

		public bool ReadULong(int size, out ulong result)
		{
			if (size > sizeof(ulong) || !HasLength(size)) {
				result = 0;
				return false;
			}

			result = this[size - 1];

			for (int i = size - 2; i >= 0; i--) {
				result <<= 8;
				result |= this[i];
			}

			return true;
		}

		public bool ReadULong(out ulong result)
		{
			return ReadULong(sizeof(ulong), out result);
		}

		public bool ReadLong(out long result)
		{
			ulong tmp;
			var ret = ReadULong(sizeof(long), out tmp);
			result = (long)tmp;
			return ret;
		}

		public bool ReadUInt(out uint result)
		{
			ulong tmp;
			var ret = ReadULong(sizeof(uint), out tmp);
			result = (uint)tmp;
			return ret;
		}

		public bool ReadInt(out int result)
		{
			uint tmp;
			var ret = ReadUInt(out tmp);
			result = (int)tmp;
			return ret;
		}

		public bool ReadUShort(out ushort result)
		{
			ulong tmp;
			var ret = ReadULong(sizeof(ushort), out tmp);
			result = (ushort)tmp;
			return ret;
		}

		public bool ReadShort(out short result)
		{
			ushort tmp;
			var ret = ReadUShort(out tmp);
			result = (short)tmp;
			return ret;
		}

		public bool ReadByte(out byte result)
		{
			try {
				result = this[0];
				return true;
			} catch {
				result = 0;
				return false;
			}
		}
	}
}
