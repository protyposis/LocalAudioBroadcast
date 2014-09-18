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
using System.IO;
using LocalAudioBroadcast.Data;

namespace LocalAudioBroadcast.Metadata {
    /// <summary>
    ///  Embeds Shoutcast metadata into a stream.
    ///  http://www.smackfu.com/stuff/programming/shoutcast.html
    /// </summary>
    class ShoutcastMetadataEmbedder : IDataSource {

        private int interval;
        private CircleBuffer dataSource;
        private int readCount;
        private byte[] metadata;
        private int metadataLength;
        private int metadataWritePosition;

        public ShoutcastMetadataEmbedder(int interval, CircleBuffer dataSource) {
            this.interval = interval;
            this.dataSource = dataSource;
            this.readCount = 0;

            metadata = new byte[4080]; // max size = 16 * 255 (max byte value)
            metadata[0] = 0;
            metadataLength = 1;
            metadataWritePosition = 0;
        }

        public ShoutcastMetadataEmbedder(CircleBuffer dataSource)
            : this(1024 * 512, dataSource) {
            // nothing to do here
        }

        public int Interval {
            get { return interval; }
        }

        public void SetTrackInfo(TrackInfo trackInfo) {
            // http://www.smackfu.com/stuff/programming/shoutcast.html
            Array.Clear(metadata, 0, metadata.Length);
            // write track title
            string title = "StreamTitle='" + trackInfo.FullTitle + "';";
            // should throw an exception if the title is too long to fit into the metadata block
            int dataLength = System.Text.Encoding.UTF8.GetBytes(title, 0, title.Length, metadata, 1);
            // write metadata block size
            int blockSize = dataLength + (16 - (dataLength % 16)); // may result in 16 padding bytes - could be avoided
            metadata[0] = (byte)(blockSize / 16); // first byte contains the block size
            metadataLength = blockSize + 1; // block size plus header byte
            Console.WriteLine("MetadataEmbedder:SetTrackInfo Metadata set: " + title);
        }

        private bool metadataWriting = false; // true == write metadata on first read

        public int Read(byte[] data, int offset, int count) {
            int totalBytesRead = 0;
            int bytesRead = 0;
            bool reading = true;

            while (reading) {
                bytesRead = ReadInternal(data, offset, count);
                if (bytesRead == 0)
                    reading = false;

                totalBytesRead += bytesRead;
                offset += bytesRead;
                count -= bytesRead;
            }

            return totalBytesRead;
        }

        private int ReadInternal(byte[] data, int offset, int count) {
            int bytesRead = 0;

            if (metadataWriting) {
                // read metadata
                //Console.WriteLine("MetadataEmbedder:ReadInternal writing metadata: " + (metadataLength > 1) + " (" + System.Text.Encoding.UTF8.GetString(metadata).Replace("\0", "") + ")");
                int bytesToRead = Math.Min(metadataLength - metadataWritePosition, count);
                Array.Copy(metadata, metadataWritePosition, data, offset, bytesToRead);
                bytesRead = bytesToRead;
                metadataWritePosition += bytesToRead;

                if (metadataWritePosition == metadataLength) { // check if all metadata has been read
                    // clear/reset metadata
                    metadataWriting = false;
                    readCount = 0; // just count up to interval, then reset
                    metadata[0] = 0; // length 0 = no metadata
                    metadataLength = 1; // 1 header byte
                    metadataWritePosition = 0;
                }
            }
            else if (readCount + count > interval) {
                // read data until interval length
                int bytesToRead = interval - readCount; // to reach interval size
                bytesRead = dataSource.Read(data, offset, bytesToRead);
                readCount += bytesRead;
                if (bytesRead == bytesToRead) {
                    metadataWriting = true;
                }
            }
            else {
                // read data between interval borders
                bytesRead = dataSource.Read(data, offset, count);
                readCount += bytesRead;
            }

            return bytesRead;
        }
    }
}
