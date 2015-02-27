// Copyright 2014 Mario Guggenberger <mg@protyposis.net>
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
using System;
using System.Collections.Generic;
using System.Text;

namespace LocalAudioBroadcast.Data {
    class CircleBuffer : IDataSource {

        private byte[] buffer;
        private int fillLevel;
        private int startPosition;
        private IInputTransform transform;

        public CircleBuffer(int size, IInputTransform transform) {
            buffer = new byte[size];
            this.transform = transform;
        }

        public int Length {
            get { return buffer.Length; }
        }

        public int FillLevel {
            get { return fillLevel; }
        }

        public bool Empty {
            get { return fillLevel == 0; }
        }

        public void Reset() {
            startPosition = 0;
            fillLevel = 0;
        }

        public void Write(byte[] data, int offset, int count) {
            if (data.Length < offset + count) {
                throw new ArgumentOutOfRangeException("invalid parameters");
            }
            if (count > buffer.Length) {
                throw new ArgumentException("buffer too small - cannot write that many");
            }

            data = transform.Transform(data, offset, count);

            if (startPosition + count > buffer.Length) { // 2 writes necessary
                int firstWriteCount = buffer.Length - startPosition;
                int secondWriteCount = count - firstWriteCount;
                Array.Copy(data, offset, buffer, startPosition, firstWriteCount);
                Array.Copy(data, offset + firstWriteCount, buffer, 0, secondWriteCount);
            }
            else {
                Array.Copy(data, offset, buffer, startPosition, count);
            }

            fillLevel = Math.Min(buffer.Length, fillLevel + count);
            startPosition = (startPosition + count) % buffer.Length;
        }

        public int Read(byte[] data, int offset, int count) {
            if (offset + count > data.Length) {
                throw new ArgumentOutOfRangeException("invalid parameters");
            }

            count = Math.Min(count, fillLevel); // cannot read more than the buffer contains

            int readStartPos = startPosition - fillLevel;
            if (readStartPos < 0) {
                readStartPos = buffer.Length + readStartPos;
            }

            if (readStartPos + count > buffer.Length) { // 2 reads necessary
                int firstReadCount = Math.Min(count, buffer.Length - readStartPos);
                int secondReadCount = count - firstReadCount;
                Array.Copy(buffer, readStartPos, data, offset, firstReadCount);
                Array.Copy(buffer, 0, data, offset + firstReadCount, secondReadCount);
            }
            else {
                Array.Copy(buffer, readStartPos, data, offset, count);
            }

            fillLevel -= count;

            return count;
        }
    }
}
