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

namespace LocalAudioBroadcast {
    abstract class StreamingFormat {

        public static StreamingFormat LPCM = new LPCMFormat();
        public static StreamingFormat WAV = new WAVFormat();

        private static StreamingFormat[] formats = { LPCM, WAV };
        private static StreamingFormat defaultFormat = LPCM;

        public abstract string Id { get; }

        public abstract string Name { get; }

        public abstract string GetFormatDescriptor(int sampleRate, int channels);

        public abstract string GetNetworkFormatDescriptor(int sampleRate, int channels);

        public abstract bool BigEndian { get; }

        public static StreamingFormat[] Formats {
            get {
                return formats;
            }
        }

        public static StreamingFormat GetFormat(string id) {
            foreach(StreamingFormat format in formats) {
                if(format.Id == id) {
                    return format;
                }
            }
            throw new KeyNotFoundException("not format with id " + id);
        }

        public static StreamingFormat DefaultFormat {
            get { return defaultFormat; }
            set { defaultFormat = value; }
        }

        class LPCMFormat : StreamingFormat {

            public override string Id {
                get { return "lpcm"; }
            }

            public override string Name {
                get { return "LPCM"; }
            }

            public override string GetFormatDescriptor(int sampleRate, int channels) {
                return "audio/L16;rate=" + sampleRate + ";channels=" + channels;
            }

            public override string GetNetworkFormatDescriptor(int sampleRate, int channels) {
                return "http-get:*:" + GetFormatDescriptor(sampleRate, channels) + ":DLNA.ORG_PN=LPCM";
                
            }

            public override bool BigEndian { 
                get { return true; }
            }
        }

        class WAVFormat : StreamingFormat {

            public override string Id {
                get { return "wav"; }
            }

            public override string Name {
                get { return "WAV"; }
            }

            public override string GetFormatDescriptor(int sampleRate, int channels) {
                return "audio/wav";
            }

            public override string GetNetworkFormatDescriptor(int sampleRate, int channels) {
                return "http-get:*:" + GetFormatDescriptor(sampleRate, channels) + ":*";
            }

            public override bool BigEndian { 
                get { return false; }
            }
        }
    }
}
