using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace InspectorSwfTime.Swf
{
    public class SwfBitStream : IDisposable
    {
        private readonly Stream source;
        private byte currentByte;
        private uint currentBitIndex;
        public ulong BytesRead { get; protected set; }

        public SwfBitStream(Stream swfData)
        {
            BytesRead = 8; // the beginning of the header had already been read, but this needs to represent that
            source = swfData;
        }

        public ulong GetUnsignedBitValue(uint numBits)
        {
            if (numBits > 64)
            {
                throw new SwfParseException("Unsigned bit values greater than 64 bits are not supported.");
            }

            var bitsRemaining = numBits;
            ulong result = 0;

            if (currentBitIndex > 0)
            {
                // read the first few bytes from some chunk of a byte that is hanging around
                var bitsToRead = 8 - currentBitIndex;
                bitsToRead = bitsToRead > numBits ? numBits : bitsToRead;
                result = CopyBits(result, currentByte, currentBitIndex, bitsToRead, numBits - bitsToRead);
                currentBitIndex += bitsToRead;
                if (currentBitIndex == 8)
                {
                    currentBitIndex = 0;
                }
                bitsRemaining -= bitsToRead;
            }

            while (bitsRemaining >= 8)
            {
                // read entire bytes into the number
                ++BytesRead;
                result = CopyBits(result, (byte)source.ReadByte(), 0, 8, bitsRemaining - 8);
                bitsRemaining -= 8;
            }

            if (bitsRemaining != 0)
            {
                ++BytesRead;
                currentByte = (byte)source.ReadByte();
                result = CopyBits(result, currentByte, 0, bitsRemaining, 0);
                currentBitIndex = bitsRemaining;
            }

            return result;
        }

        public long GetSignedBitValue(uint numBits)
        {
            if (numBits < 2)
            {
                throw new SwfParseException("Signed bit values must have at least 2 bits.");
            }
            if (numBits > 64)
            {
                throw new SwfParseException("Signed bit values greater than 64 bits are not supported.");
            }

            var unsigned = GetUnsignedBitValue(numBits);
            if (HasBit(unsigned, numBits - 1))
            {
                // do the sign extension
                unsigned = unsigned | ((ulong.MaxValue >> (int)numBits) << (int)numBits);
            }

            return (long)unsigned;
        }

        public byte[] GetBytes(uint numBytes)
        {
            if (numBytes > int.MaxValue)
            {
                throw new SwfParseException(
                    string.Format("Can't get more than {0} bytes at a time. Requested: {1}", int.MaxValue, numBytes));
            }

            BytesRead += numBytes;
            currentBitIndex = 0;
            var bytes = new byte[numBytes];
            uint offset = 0;
            int read;
            do
            {
                read = source.Read(bytes, (int)offset, (int)(numBytes - offset));
                offset += (uint)read;
            } while (read != 0 && offset != numBytes);
            if (read == 0 && offset != numBytes)
            {
                throw new SwfParseException(
                    string.Format("Unable to read enough bytes from stream. Requested: {0} Read: {1}", numBytes, offset));
            }
            return bytes;
        }

        public string GetString()
        {
            ++BytesRead;
            currentBitIndex = 0;
            var chars = new List<byte>();
            var nextChar = (byte)source.ReadByte();
            if (nextChar == 0)
            {
                return string.Empty;
            }
            while (nextChar != 0)
            {
                ++BytesRead;
                chars.Add(nextChar);
                nextChar = (byte)source.ReadByte();
            }
            return Encoding.UTF8.GetString(chars.ToArray());
        }

        public ushort GetUInt16()
        {
            var bytes = GetBytes(2);
            return BitConverter.ToUInt16(bytes, 0);
        }

        public uint GetUInt32()
        {
            var bytes = GetBytes(4);
            return BitConverter.ToUInt32(bytes, 0);
        }

        public ulong GetUInt64()
        {
            var bytes = GetBytes(8);
            return BitConverter.ToUInt64(bytes, 0);
        }

        public short GetSInt16()
        {
            var bytes = GetBytes(2);
            return BitConverter.ToInt16(bytes, 0);
        }

        public int GetSInt32()
        {
            var bytes = GetBytes(4);
            return BitConverter.ToInt32(bytes, 0);
        }

        public long GetSInt64()
        {
            var bytes = GetBytes(8);
            return BitConverter.ToInt64(bytes, 0);
        }

        #region IDisposable

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void Dispose(bool disposing)
        {
            if (disposing && source != null)
            {
                source.Dispose();
            }
        }

        #endregion

        private static ulong CopyBits(ulong initial, byte sourceBits, uint startBit, uint numBits, uint leftShift)
        {
            return initial | (((((ulong)sourceBits) << (int)(56 + startBit)) >> (int)(64 - numBits)) << (int)leftShift);
        }

        private static bool HasBit(ulong number, uint bit)
        {
            var bitmask = (uint)Math.Pow(2, bit);
            return (number & bitmask) == bitmask;
        }
    }
}
