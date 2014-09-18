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
using System.IO;
using System.Net.Sockets;
using System.Text;

namespace LocalAudioBroadcast.FileServer {
    class FileModule : ServerModule {

        private const int BUFFER_SIZE = 8192 * 128;

        private FileInfo fileInfo;
        private string path;

        public FileModule(FileInfo fileInfo, string path) {
            this.fileInfo = fileInfo;
            this.path = path;
        }

        public override void Start() {
            if (!fileInfo.Exists) {
                Console.WriteLine("FileModule: files does not exist: " + fileInfo.FullName);
            }
        }

        public override void Stop() {
            // nop
        }

        public override bool Process(HttpServer.IHttpRequest request, HttpServer.IHttpResponse response, HttpServer.Sessions.IHttpSession session) {
            if (!request.Uri.AbsolutePath.StartsWith(path)) {
                return false;
            }
            HttpServerUtil.DebugPrintRequest(request);

            Socket socket = HttpServerUtil.GetNetworkSocket(response);

            //response.ContentLength = long.MaxValue;
            response.ContentType = "audio/" + fileInfo.Extension.Substring(1);
            response.AddHeader("TransferMode.DLNA.ORG", "Streaming");
            response.AddHeader("Server", "UPnP/1.0 DLNADOC/1.50 LAB/1.0");

            FileStream stream = fileInfo.OpenRead();

            // create local output buffers
            byte[] buffer = new byte[BUFFER_SIZE];

            HttpServerUtil.DebugPrintResponse(response);
            response.SendHeaders();

            int bytesRead = 1;
            while (socket.Connected && bytesRead > 0) {
                // file stream -> byte array buffer -> circlebuffer -> add metadata -> byte array buffer -> response stream
                bytesRead = stream.Read(buffer, 0, buffer.Length);
                response.SendBody(buffer, 0, bytesRead);
            }

            // remove local output buffer
            stream.Close();

            Console.WriteLine("request processing finished");

            return true;
        }
    }
}
