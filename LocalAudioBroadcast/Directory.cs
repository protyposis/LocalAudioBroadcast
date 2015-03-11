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
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Text;

namespace LocalAudioBroadcast {
    class Directory {

        private List<Item> items;

        public Directory(string baseUri) {
            Init(baseUri);
        }

        public void Init(string baseUri) {
            items = new List<Item>();

            CaptureDevice defaultDevice = WasapiLoopbackCapture2.GetDefaultLoopbackCaptureDevice();
            List<CaptureDevice> devices = WasapiLoopbackCapture2.GetLoopbackCaptureDevices();

            int itemId = 0;
            items.Add(new Item {
                Uri = baseUri,
                Definition = DidlUtil.GenerateCaptureDeviceItem(++itemId, defaultDevice, baseUri)
            });
            
            int deviceId = 0;
            foreach (CaptureDevice captureDevice in devices) {
                if (captureDevice != defaultDevice) {
                    string uri = baseUri + "?id=" + deviceId;
                    items.Add(new Item {
                        Uri = uri,
                        Definition = DidlUtil.GenerateCaptureDeviceItem(++itemId, captureDevice, uri)
                    });
                }
                deviceId++;
            }
        }

        public uint Count {
            get {  return (uint)items.Count;}
        }

        public string GetDirectoryDidl() {
            string list = "";

            foreach (Item item in items) {
                list += item.Definition;
            }

            return DidlUtil.BeginDidl() + list + DidlUtil.EndDidl();
        }

        public Item GetItem(int index) {
            return items[index];
        }

        public class Item {
            public string Uri { get; set; }
            public string Definition { get; set; }

            public string GetDidl() {
                return DidlUtil.BeginDidl() + Definition + DidlUtil.EndDidl();
            }
        }
    }
}
