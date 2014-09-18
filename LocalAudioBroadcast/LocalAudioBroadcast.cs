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
using LocalAudioBroadcast.FileServer;
using System.IO;

namespace LocalAudioBroadcast {
    class LocalAudioBroadcast {

        private DirectoryServer device;
        private Server fileServer;
        private ControlPoint controlPoint;

        public LocalAudioBroadcast() {
            device = new DirectoryServer();
            fileServer = new Server();
            fileServer.Add(new LoopbackModule());
            //fileServer.Add(new FileModule(new FileInfo(@"C:\metallica\nothing else matters.mp3"), "/testfile"));
            controlPoint = new ControlPoint();
        }

        public DirectoryServer Device {
            get { return device; }
        }

        public Server FileServer {
            get { return fileServer; }
        }

        public ControlPoint ControlPoint {
            get { return controlPoint; }
        }

        public void Start() {
            Console.WriteLine("starting local audio broadcast...");
            device.Start();
            fileServer.Start(device.IPEndPoint);
            controlPoint.Rescan();
        }

        public void Stop() {
            Console.WriteLine("stopping local audio broadcast...");
            fileServer.Stop();
            device.Stop();
        }
    }
}
