using System;
using System.Collections.Generic;
using System.Linq;

namespace LibuvSharp
{
	public class ByteBufferCollection : List<ArraySegment<byte>>
	{
		public ByteBufferCollection()
		{
		}

		List<ArraySegment<byte>> Clone()
		{
			List<ArraySegment<byte>> r = new List<ArraySegment<byte>>();
			foreach (var buffer in this) {
				r.Add(buffer);
			}
			return r;
		}

		public void Skip(int restLength)
		{
			foreach (var buffer in Clone()) {
				int r = restLength - buffer.Count;
				if (r >= 0) {
					buffer.Skip(buffer.Count);
					Remove(buffer);
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
			foreach (var buffer in this) {
				if (length < buffer.Count) {
					return true;
				}
				length -= buffer.Count;
			}
			return false;
		}

		public int Length {
			get {
				int length = 0;
				foreach (var buffer in this) {
					length += buffer.Count;
				}
				return length;
			}
		}

		public byte Get(int index) {
			int position = index;
			foreach (var buffer in this) {
				if (position < buffer.Count) {
					return buffer.Array[buffer.Offset + position];
				} else {
					position -= buffer.Count;
				}
			}
			throw new Exception();
		}

		public void CopyTo(byte[] destination, int length)
		{
			int startPos = 0;
			foreach (var buffer in this) {
				int rest = length - buffer.Count;
				if (rest <= 0) {
					Buffer.BlockCopy(buffer.Array, buffer.Offset, destination, startPos, length);
					break;
				} else {
					Buffer.BlockCopy(buffer.Array, buffer.Offset, destination, startPos, buffer.Count);
					startPos += buffer.Count;
					length = rest;
				}
			}
		}

		public int FirstByte(byte val)
		{
			int pos = 0;
			foreach (var buffer in this) {
				for (int i = 0; i < buffer.Count; i++) {
					if (buffer.Array[i + buffer.Offset] == val) {
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
				var buffer = this.First();
				return buffer.Array[buffer.Offset];
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

			result = Get(size - 1);

			for (int i = size - 2; i >= 0; i--) {
				result <<= 8;
				result |= Get(i);
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
				result = Get(0);
				return true;
			} catch {
				result = 0;
				return false;
			}
		}
	}
}
