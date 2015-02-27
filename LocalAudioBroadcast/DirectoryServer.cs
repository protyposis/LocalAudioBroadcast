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

// UPnP .NET Framework Device Stack, Device Module
// Device Builder Build#1.0.4561.18413

using System;
using OpenSource.UPnP;
using System.Web;
using System.Net;
using NAudio.Wave;
using NAudio.CoreAudioApi;
using System.Collections.Generic;
using System.Net.Sockets;

namespace LocalAudioBroadcast
{
	/// <summary>
	/// Summary description for SampleDevice.
	/// </summary>
	class DirectoryServer
	{
		private UPnPDevice device;
        private IPEndPoint ipEndPoint;
		
		public DirectoryServer()
		{
			device = UPnPDevice.CreateRootDevice(1800,1.0,"\\");

            device.FriendlyName = "Local Audio Broadcast @ " + System.Environment.MachineName;
			device.Manufacturer = "Mario Guggenberger / Protyposis";
			device.ManufacturerURL = "http://protyposis.net";
			device.ModelName = "LAB";
			device.ModelDescription = "LAB";
			device.ModelNumber = "1.0";
			device.HasPresentation = false;
			device.DeviceURN = "urn:schemas-upnp-org:device:MediaServer:1";


            DvX_MS_MediaReceiverRegistrar X_MS_MediaReceiverRegistrar = new DvX_MS_MediaReceiverRegistrar();
            X_MS_MediaReceiverRegistrar.External_IsAuthorized = new DvX_MS_MediaReceiverRegistrar.Delegate_IsAuthorized(X_MS_MediaReceiverRegistrar_IsAuthorized);
            X_MS_MediaReceiverRegistrar.External_IsValidated = new DvX_MS_MediaReceiverRegistrar.Delegate_IsValidated(X_MS_MediaReceiverRegistrar_IsValidated);
            X_MS_MediaReceiverRegistrar.External_RegisterDevice = new DvX_MS_MediaReceiverRegistrar.Delegate_RegisterDevice(X_MS_MediaReceiverRegistrar_RegisterDevice);
            device.AddService(X_MS_MediaReceiverRegistrar);
            DvConnectionManager ConnectionManager = new DvConnectionManager();
            ConnectionManager.External_GetCurrentConnectionIDs = new DvConnectionManager.Delegate_GetCurrentConnectionIDs(ConnectionManager_GetCurrentConnectionIDs);
            ConnectionManager.External_GetCurrentConnectionInfo = new DvConnectionManager.Delegate_GetCurrentConnectionInfo(ConnectionManager_GetCurrentConnectionInfo);
            ConnectionManager.External_GetProtocolInfo = new DvConnectionManager.Delegate_GetProtocolInfo(ConnectionManager_GetProtocolInfo);
            device.AddService(ConnectionManager);
			DvContentDirectory ContentDirectory = new DvContentDirectory();
            ContentDirectory.External_Browse = new DvContentDirectory.Delegate_Browse(ContentDirectory_Browse);
			ContentDirectory.External_GetSearchCapabilities = new DvContentDirectory.Delegate_GetSearchCapabilities(ContentDirectory_GetSearchCapabilities);
			ContentDirectory.External_GetSortCapabilities = new DvContentDirectory.Delegate_GetSortCapabilities(ContentDirectory_GetSortCapabilities);
			ContentDirectory.External_GetSystemUpdateID = new DvContentDirectory.Delegate_GetSystemUpdateID(ContentDirectory_GetSystemUpdateID);
			ContentDirectory.External_Search = new DvContentDirectory.Delegate_Search(ContentDirectory_Search);
			device.AddService(ContentDirectory);
			
			// Setting the initial value of evented variables
            X_MS_MediaReceiverRegistrar.Evented_AuthorizationGrantedUpdateID = 0;
            X_MS_MediaReceiverRegistrar.Evented_ValidationRevokedUpdateID = 0;
            X_MS_MediaReceiverRegistrar.Evented_ValidationSucceededUpdateID = 0;
            X_MS_MediaReceiverRegistrar.Evented_AuthorizationDeniedUpdateID = 0;
            ConnectionManager.Evented_SourceProtocolInfo = "Sample String";
            ConnectionManager.Evented_SinkProtocolInfo = "Sample String";
            ConnectionManager.Evented_CurrentConnectionIDs = "Sample String";
			ContentDirectory.Evented_ContainerUpdateIDs = "Sample String";
			ContentDirectory.Evented_SystemUpdateID = 0;
		}

        public static string S1, S2;
        public static uint S2count;
		
		public void Start()
		{
			device.StartDevice();

            IPAddress ipAddress = null; 

            foreach (IPEndPoint ipep in device.LocalIPEndPoints) {
                if (ipep.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork) {
                    Console.WriteLine("DLNA server STARTED listening @ " + ipep.ToString());
                    // create HTTP resource server endpoint
                    ipAddress = ipep.Address;
                    break;
                }
            }

            // find a free port for the HTTP server: http://stackoverflow.com/a/9895416
            Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            sock.Bind(new IPEndPoint(IPAddress.Any, 0)); // Pass 0 here.
            ipEndPoint = new IPEndPoint(ipAddress, ((IPEndPoint)sock.LocalEndPoint).Port);
            sock.Close();

            S1 = HttpBaseURL + "capture";

            S2 = DidlUtil.BeginDidl();

            CaptureDevice defaultDevice = WasapiLoopbackCapture2.GetDefaultLoopbackCaptureDevice();
            List<CaptureDevice> devices = WasapiLoopbackCapture2.GetLoopbackCaptureDevices();

            int itemId = 0;

            foreach (StreamingFormat format in StreamingFormat.Formats) {
                S2 += GenerateCaptureDeviceEntry(++itemId, defaultDevice, format, S1 + "?format=" + format.Id);
            }
            
            int deviceId = 0;
            foreach (CaptureDevice captureDevice in devices) {
                if (captureDevice != defaultDevice) {
                    foreach (StreamingFormat format in StreamingFormat.Formats) {
                        S2 += GenerateCaptureDeviceEntry(++itemId, captureDevice, format, S1 + "?id=" + deviceId + "&format=" + format.Id);
                    }
                }
                deviceId++;
            }

            S2 += DidlUtil.EndDidl();
            S2count = (uint)itemId;
		}

        private String GenerateCaptureDeviceEntry(int itemId, CaptureDevice captureDevice, StreamingFormat format, String url) {
            int sampleRate = captureDevice.MMDevice.AudioClient.MixFormat.SampleRate;
            /* The channels and bitDepth are fixed, as requested from the WasapiLoopbackCapture2
             * in LoopbackModule.Start(). */
            int channels = 2;
            int bitDepth = 16;
            int bitRate = sampleRate * channels * (bitDepth / 2) * 8;

            return DidlUtil.GetMusicItem(itemId+"", "0", "1",
                captureDevice.Name + " (" + format.Name + ")", "N/A", "N/A", "N/A", "0",
                bitRate + "", sampleRate + "", channels + "", bitDepth + "",
                format.GetNetworkFormatDescriptor(sampleRate, channels),
                url, "object.item.audioItem.musicTrack");
        }
		
		public void Stop()
		{
			device.StopDevice();
		}

        public string HttpBaseURL {
            get { return "http://" + ipEndPoint.ToString() + "/"; }
        }

        public IPEndPoint IPEndPoint {
            get { return ipEndPoint; }
        }
		
		public void X_MS_MediaReceiverRegistrar_IsAuthorized(System.String DeviceID, out System.Int32 Result)
		{
			Result = 0;
			Console.WriteLine("X_MS_MediaReceiverRegistrar_IsAuthorized(" + DeviceID.ToString() + ")");
		}
		
		public void X_MS_MediaReceiverRegistrar_IsValidated(System.String DeviceID, out System.Int32 Result)
		{
			Result = 0;
			Console.WriteLine("X_MS_MediaReceiverRegistrar_IsValidated(" + DeviceID.ToString() + ")");
		}
		
		public void X_MS_MediaReceiverRegistrar_RegisterDevice(System.Byte[] RegistrationReqMsg, out System.Byte[] RegistrationRespMsg)
		{
            RegistrationRespMsg = null; // "Sample String";
			Console.WriteLine("X_MS_MediaReceiverRegistrar_RegisterDevice(" + RegistrationReqMsg.ToString() + ")");
		}
		
		public void ConnectionManager_GetCurrentConnectionIDs(out System.String ConnectionIDs)
		{
			ConnectionIDs = "Sample String";
			Console.WriteLine("ConnectionManager_GetCurrentConnectionIDs(" + ")");
		}
		
		public void ConnectionManager_GetCurrentConnectionInfo(System.Int32 ConnectionID, out System.Int32 RcsID, out System.Int32 AVTransportID, out System.String ProtocolInfo, out System.String PeerConnectionManager, out System.Int32 PeerConnectionID, out DvConnectionManager.Enum_A_ARG_TYPE_Direction Direction, out DvConnectionManager.Enum_A_ARG_TYPE_ConnectionStatus Status)
		{
			RcsID = 0;
			AVTransportID = 0;
			ProtocolInfo = "Sample String";
			PeerConnectionManager = "Sample String";
			PeerConnectionID = 0;
			Direction = DvConnectionManager.Enum_A_ARG_TYPE_Direction.INPUT;
			Status = DvConnectionManager.Enum_A_ARG_TYPE_ConnectionStatus.OK;
			Console.WriteLine("ConnectionManager_GetCurrentConnectionInfo(" + ConnectionID.ToString() + ")");
		}
		
		public void ConnectionManager_GetProtocolInfo(out System.String Source, out System.String Sink)
		{
			Source = "Sample String";
			Sink = "Sample String";
			Console.WriteLine("ConnectionManager_GetProtocolInfo(" + ")");
		}
		
		public void ContentDirectory_Browse(System.String ObjectID, DvContentDirectory.Enum_A_ARG_TYPE_BrowseFlag BrowseFlag, System.String Filter, System.UInt32 StartingIndex, System.UInt32 RequestedCount, System.String SortCriteria, out System.String Result, out System.UInt32 NumberReturned, out System.UInt32 TotalMatches, out System.UInt32 UpdateID)
		{
			Console.WriteLine("DEVICE ContentDirectory_Browse(" + ObjectID.ToString() + BrowseFlag.ToString() + Filter.ToString() + StartingIndex.ToString() + RequestedCount.ToString() + SortCriteria.ToString() + ")");

            Result = "Sample String";
            NumberReturned = 0;
            TotalMatches = 0;
            UpdateID = 0;

            if (BrowseFlag == DvContentDirectory.Enum_A_ARG_TYPE_BrowseFlag.BROWSEDIRECTCHILDREN) {
                switch (ObjectID) {
                    case "0": // root
                        NumberReturned = S2count;
                        TotalMatches = S2count;
                        Result = S2;
                        break;
                }
            }
		}
		
		public void ContentDirectory_GetSearchCapabilities(out System.String SearchCaps)
		{
			SearchCaps = "Sample String";
			Console.WriteLine("ContentDirectory_GetSearchCapabilities(" + ")");
		}
		
		public void ContentDirectory_GetSortCapabilities(out System.String SortCaps)
		{
			SortCaps = "Sample String";
			Console.WriteLine("ContentDirectory_GetSortCapabilities(" + ")");
		}
		
		public void ContentDirectory_GetSystemUpdateID(out System.UInt32 Id)
		{
			Id = 0;
			Console.WriteLine("ContentDirectory_GetSystemUpdateID(" + ")");
		}
		
		public void ContentDirectory_Search(System.String ContainerID, System.String SearchCriteria, System.String Filter, System.UInt32 StartingIndex, System.UInt32 RequestedCount, System.String SortCriteria, out System.String Result, out System.UInt32 NumberReturned, out System.UInt32 TotalMatches, out System.UInt32 UpdateID)
		{
			Result = "Sample String";
			NumberReturned = 0;
			TotalMatches = 0;
			UpdateID = 0;
			Console.WriteLine("ContentDirectory_Search(" + ContainerID.ToString() + SearchCriteria.ToString() + Filter.ToString() + StartingIndex.ToString() + RequestedCount.ToString() + SortCriteria.ToString() + ")");
		}
		
	}
}

