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
using HttpServer;
using LocalAudioBroadcast.Data;
using LocalAudioBroadcast.Metadata;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace LocalAudioBroadcast.FileServer
{
    class LoopbackModule : ServerModule {

        private const int BUFFER_SIZE = 8192 * 128;

        private Dictionary<CaptureDevice, CaptureDeviceHandler> captureDevices;
        private ITrackInfoProvider trackInfoProvider;

        public override void Start() {
            captureDevices = new Dictionary<CaptureDevice, CaptureDeviceHandler>();
            trackInfoProvider = new Spotify();
        }

        public override void Stop() {
            // nop
        }

        public override bool Process(HttpServer.IHttpRequest request, HttpServer.IHttpResponse response, HttpServer.Sessions.IHttpSession session) {
            if (!request.Uri.AbsolutePath.StartsWith("/capture")) {
                return false;
            }
            HttpServerUtil.DebugPrintRequest(request);

            HttpInputItem deviceIdParam = request.Param["id"];
            CaptureDevice device;
            CaptureDeviceHandler captureDevice;

            // First, get the specified lowlevel capture device
            if(deviceIdParam.Count == 1) {
                device = WasapiLoopbackCapture2.GetLoopbackCaptureDevices()[int.Parse(deviceIdParam.Value)];
            } else {
                device = WasapiLoopbackCapture2.GetDefaultLoopbackCaptureDevice();
            }
            
            // Then, get the capture device handler
            if (captureDevices.ContainsKey(device)) {
                captureDevice = captureDevices[device];
            }
            else {
                captureDevice = new CaptureDeviceHandler(device);
                captureDevices.Add(device, captureDevice);
            }

            HttpInputItem formatParam = request.Param["format"];
            StreamingFormat format;
            if (formatParam.Count == 1) {
                format = StreamingFormat.GetFormat(formatParam.Value);
            }
            else {
                format = StreamingFormat.DefaultFormat;
            }

            response.ContentLength = long.MaxValue;
            response.ContentType = format.GetFormatDescriptor(
                captureDevice.WaveFormat.SampleRate, 
                captureDevice.WaveFormat.Channels);
            response.AddHeader("TransferMode.DLNA.ORG", "Streaming");
            response.AddHeader("Server", "UPnP/1.0 DLNADOC/1.50 LAB/1.0");
            response.AddHeader("icy-name", "Local Audio Broadcast");

            // create local output buffers
            CircleBuffer captureBuffer = new CircleBuffer(BUFFER_SIZE, new StreamingFormatTransform(format));
            byte[] buffer = new byte[BUFFER_SIZE];
            byte[] emptiness100ms = new byte[captureDevice.WaveFormat.SampleRate / 10
                * captureDevice.WaveFormat.Channels 
                * (captureDevice.WaveFormat.BitsPerSample / 8)];

            // register buffer for being filled with loopback samples
            captureDevice.Add(captureBuffer);

            IDataSource data = captureBuffer;

            EventHandler<TrackInfoChangedEventArgs> trackInfoHandler = null;

            if (request.Headers["Icy-MetaData"] == "1") {
                ShoutcastMetadataEmbedder me = new ShoutcastMetadataEmbedder(
                    captureDevice.WaveFormat.SampleRate * 2, // 1 second interval
                    captureBuffer);
                response.ProtocolVersion = "ICY";
                response.AddHeader("icy-metaint", me.Interval + "");
                data = me;
                me.SetTrackInfo(trackInfoProvider.TrackInfo);
                trackInfoHandler = new EventHandler<TrackInfoChangedEventArgs>(delegate(object sender, TrackInfoChangedEventArgs e) {
                    me.SetTrackInfo(e.TrackInfo);
                });
                trackInfoProvider.TrackInfoChanged += trackInfoHandler;
            }

            HttpServerUtil.DebugPrintResponse(response);
            Socket socket = HttpServerUtil.GetNetworkSocket(response);
            response.SendHeaders();

            if (format == StreamingFormat.WAV) {
                // build wav header
                byte[] wavHeader = new byte[44];
                MemoryStream header = new MemoryStream(wavHeader);
                using (BinaryWriter headerWriter = new BinaryWriter(header)) {
                    headerWriter.Write(Encoding.ASCII.GetBytes("RIFF"));
                    headerWriter.Write(uint.MaxValue - 8);
                    headerWriter.Write(Encoding.ASCII.GetBytes("WAVE"));
                    headerWriter.Write(Encoding.ASCII.GetBytes("fmt "));
                    headerWriter.Write(16); // fmt chunk data size
                    headerWriter.Write((short)1); // format: 1 == PCM, 3 == PCM float
                    headerWriter.Write((short)captureDevice.WaveFormat.Channels);
                    headerWriter.Write(captureDevice.WaveFormat.SampleRate);
                    headerWriter.Write(captureDevice.WaveFormat.AverageBytesPerSecond);
                    headerWriter.Write((short)captureDevice.WaveFormat.BlockAlign);
                    headerWriter.Write((short)captureDevice.WaveFormat.BitsPerSample);
                    headerWriter.Write(Encoding.ASCII.GetBytes("data"));
                    headerWriter.Write(uint.MaxValue - 44);
                }

                // send header
                // To retain the correct Shoutcast metadata interval bytecount, the header must be written 
                // to the intermediary captureBuffer instead of directly to the response.
                captureBuffer.Write(wavHeader, 0, wavHeader.Length);
            }

            // send audio data
            int bytesRead = 0;
            while (socket.Connected) {
                Thread.Sleep(100);
                while (captureBuffer.Empty) {
                    //Thread.Sleep(200);
                    captureBuffer.Write(emptiness100ms, 0, emptiness100ms.Length);
                }
                lock (captureDevice.lockObject) {
                    bytesRead = data.Read(buffer, 0, buffer.Length);
                }
                //Console.WriteLine("buffer-{3} r {0} - {1} = {2}%", loopbackBuffer.FillLevel + bytesRead, bytesRead,
                //    (float)loopbackBuffer.FillLevel / loopbackBuffer.Length * 100, loopbackBuffer.GetHashCode());
                response.SendBody(buffer, 0, bytesRead);

                Console.WriteLine("sending {0} bytes = {1:0.00} secs", bytesRead, bytesRead / 
                    (double)captureDevice.loopbackCapture.WaveFormat.AverageBytesPerSecond);
            }

            if (trackInfoHandler != null) {
                trackInfoProvider.TrackInfoChanged -= trackInfoHandler;
            }

            // remove local output buffer
            captureDevice.Remove(captureBuffer);

            Console.WriteLine("request processing finished");

            return true;
        }

        private class CaptureDeviceHandler {
            public object lockObject = new object();
            public WasapiLoopbackCapture2 loopbackCapture;
            public List<CircleBuffer> loopbackBuffers;

            public CaptureDeviceHandler(CaptureDevice device) {
                loopbackBuffers = new List<CircleBuffer>();

                loopbackCapture = new WasapiLoopbackCapture2(device);
                loopbackCapture.WaveFormat = new WaveFormat(loopbackCapture.WaveFormat.SampleRate, 16, 2);

                Console.WriteLine(device.Name + " loopback capture source format: " + WaveFormat +
                    " (" + Math.Round(WaveFormat.AverageBytesPerSecond * 8f / 1024 / 1024, 1) + " Mb/s)");
            }

            public void Add(CircleBuffer buffer) {
                if (loopbackBuffers.Count == 0) {
                    Start();
                }
                loopbackBuffers.Add(buffer);
            }

            public void Remove(CircleBuffer buffer) {
                loopbackBuffers.Remove(buffer);
                if (loopbackBuffers.Count == 0) {
                    Stop();
                }
            }

            public WaveFormat WaveFormat {
                get { return loopbackCapture.WaveFormat; }
            }

            private void Start() {
                loopbackCapture.DataAvailable += delegate(object sender, WaveInEventArgs e) {
                    if (e.BytesRecorded % 2 != 0)
                        throw new Exception("illegal state");

                    foreach (CircleBuffer cb in loopbackBuffers) {
                        lock (lockObject) {
                            cb.Write(e.Buffer, 0, e.BytesRecorded);
                        }
                    }
                };

                loopbackCapture.StartRecording();
            }

            private void Stop() {
                loopbackCapture.StopRecording();
            }
        }
    }
}
