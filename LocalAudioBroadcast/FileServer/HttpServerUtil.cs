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
using System.Collections.Specialized;
using System.Reflection;
using HttpServer;
using System.IO;
using System.Net.Sockets;

namespace LocalAudioBroadcast.FileServer {
    class HttpServerUtil {

        public static void DebugPrintRequest(HttpServer.IHttpRequest request) {
            Console.WriteLine("-----------------------------");
            Console.WriteLine(request.Method + " " + request.Uri.ToString() + " " + request.HttpVersion);
            foreach (string headerKey in request.Headers.Keys) {
                Console.WriteLine(headerKey + ": " + request.Headers[headerKey]);
            }
            Console.WriteLine("-----------------------------");
        }

        public static void DebugPrintResponse(HttpServer.IHttpResponse response) {
            // TODO this should be precomputed a single time and reused
            var f = response.GetType().GetField("_headers", BindingFlags.Instance | BindingFlags.NonPublic);
            
            var headers = (NameValueCollection)f.GetValue(response);

            Console.WriteLine(response.ProtocolVersion + " " + response.Status.ToString());
            Console.WriteLine(response.ContentType);
            foreach (string headerKey in headers.Keys) {
                Console.WriteLine(headerKey + ": " + headers[headerKey]);
            }
            Console.WriteLine("-----------------------------");
        }

        public static string GetHeaderRange(HttpServer.IHttpRequest request) {
            foreach (string headerKey in request.Headers.Keys) {
                if (headerKey.Equals("Range"))
                    return request.Headers[headerKey];
            }
            return "";
        }

        public static IHttpClientContext GetContext(HttpServer.IHttpResponse response) {
            var f = response.GetType().GetField("_context", BindingFlags.Instance | BindingFlags.NonPublic);
            return (IHttpClientContext)f.GetValue(response);
        }

        public static NetworkStream GetNetworkStream(HttpServer.IHttpResponse response) {
            HttpClientContext context = GetContext(response) as HttpClientContext;
            if (context != null) {
                var f = context.GetType().GetField("_stream", BindingFlags.Instance | BindingFlags.NonPublic);
                return f.GetValue(context) as NetworkStream;
            }
            return null;
        }

        public static Socket GetNetworkSocket(HttpServer.IHttpResponse response) {
            NetworkStream stream = GetNetworkStream(response); // internal ReusableSocketNetworkStream
            if (stream != null) {
                var f = stream.GetType().GetProperty("Socket", BindingFlags.Instance | BindingFlags.NonPublic);
                return f.GetValue(stream, null) as Socket;
            }
            return null;
        }

    }
}
