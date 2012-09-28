using System;

namespace LibuvSharp
{
	public class ByteBuffer
	{
		public byte[] Buffer { get; protected set; }

		public int Start { get; protected set; }
		public int Length { get; protected set; }
		public int End {
			get {
				return Start + Length;
			}
		}

		public ByteBuffer(byte[] buffer)
			: this(buffer, 0)
		{
		}

		public ByteBuffer(byte[] buffer, int start)
			: this(buffer, start, buffer.Length - start)
		{
		}

		public ByteBuffer(byte[] buffer, int start, int length)
		{
			Buffer = buffer;
			Start = start;
			Length = length;
		}

		public ByteBuffer Slice(int start, int count)
		{
			return new ByteBuffer(Buffer, Start + start, count);
		}

		public byte CurrentByte {
			get {
				return Buffer[Start];
			}
		}

		public byte ReadByte()
		{
			if (Length <= 0) {
				throw new Exception("Reading past the bytebuffer end");
			}
			Length--;
			return Buffer[Start++];
		}

		public void Skip(int length)
		{
			if (length < 0) {
				throw new ArgumentException("length has te be a positive number", "length");
			} else if (length > Length) {
				throw new ArgumentException("Can't skip more than the length of the buffer", "length");
			}
			Start += length;
			Length -= length;
		}

		public ByteBuffer SkipSlice(int length)
		{
			int start = Start;
			Skip(length);
			return new ByteBuffer(Buffer, start, length);
		}

		public byte[] CopyBuffer()
		{
			byte[] bytes = new byte[Length];
			System.Buffer.BlockCopy(Buffer, Start, bytes, 0, Length);
			return bytes;
		}

		public ByteBuffer Copy()
		{
			return new ByteBuffer(CopyBuffer());
		}

		public ByteBuffer Clone()
		{
			return new ByteBuffer(Buffer, Start, Length);
		}
	}
}

