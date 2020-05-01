using System;
using System.IO;

namespace SemiPatch {
    public class SpanStream : Stream, IDisposable {
        public Stream BaseStream;
        public long BaseStreamOffset;
        public long MaxLength;
        private long _Position;

        public SpanStream(Stream stream, long offs, long max_len) {
            BaseStream = stream;
            BaseStreamOffset = offs;
            MaxLength = max_len;
            _Position = 0;
        }

        public override bool CanRead => BaseStream.CanRead;

        public override bool CanSeek => BaseStream.CanSeek;

        public override bool CanWrite => false;

        public override long Length => MaxLength;

        public override long Position {
            get => _Position;
            set => _Position = value;
        }

        public override void Flush() {
            BaseStream.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count) {
            if (offset >= MaxLength) throw new ArgumentOutOfRangeException(nameof(offset));
            count = (int)Math.Min(MaxLength, count);

            lock (BaseStream) {
                var old_pos = BaseStream.Position;
                try {
                    BaseStream.Seek(BaseStreamOffset + Position, SeekOrigin.Begin);
                    var result = BaseStream.Read(buffer, offset, count);
                    _Position += result;
                    return result;
                } finally {
                    BaseStream.Seek(old_pos, SeekOrigin.Begin);
                }
            }
        }

        public override long Seek(long offset, SeekOrigin origin) {
            switch(origin) {
            case SeekOrigin.Begin:
                if (offset >= MaxLength || offset < 0) throw new ArgumentOutOfRangeException(nameof(offset));
                return _Position = offset;
            case SeekOrigin.Current:
                if (Position + offset >= MaxLength || Position + offset < 0) throw new ArgumentOutOfRangeException(nameof(offset));
                return _Position += offset;
            case SeekOrigin.End:
                var endpos = MaxLength - offset;
                if (endpos >= MaxLength || endpos < 0) throw new ArgumentOutOfRangeException(nameof(offset));
                return _Position = endpos;
            default:
                throw new NotImplementedException($"seek origin: {origin}");
            }
        }

        public override void SetLength(long value) {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count) {
            throw new NotImplementedException();
        }
    }
}
