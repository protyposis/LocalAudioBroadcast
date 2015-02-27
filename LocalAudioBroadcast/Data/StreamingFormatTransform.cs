using System;
using System.Collections.Generic;
using System.Text;

namespace LocalAudioBroadcast.Data {
    /// <summary>
    /// Applies endianness conversion for LPCM output.
    /// </summary>
    class StreamingFormatTransform : IInputTransform {

        private Boolean bigEndian;
        private byte[] transformBuffer;

        public StreamingFormatTransform(StreamingFormat format) {
            bigEndian = format.BigEndian;
        }

        public byte[] Transform(byte[] buffer, int offset, int count) {
            if (bigEndian) {
                if (transformBuffer == null || transformBuffer.Length < buffer.Length) {
                    transformBuffer = new byte[buffer.Length];
                }

                Array.Copy(buffer, offset, transformBuffer, offset, count);

                // DLNA LPCM must be big-endian (WAV is little-endian)
                AudioUtil.L16leToL16be(transformBuffer, 0, count);

                return transformBuffer;
            }

            return buffer;
        }
    }
}
