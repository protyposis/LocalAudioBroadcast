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
using OpenSource.UPnP;
using System.Web;

namespace LocalAudioBroadcast {
    class ControlPoint {

        private const string UPNP_DEVICE_RENDERER = "urn:schemas-upnp-org:device:MediaRenderer:1";
        private const string UPNP_SERVICE_AVTRANSPORT = "urn:schemas-upnp-org:service:AVTransport:1";
        private const string UPNP_SERVICE_CONTROL = "urn:schemas-upnp-org:service:RenderingControl:1";

        private UPnPSmartControlPoint scp;
        private UPnPDevice device;

        public event UPnPSmartControlPoint.DeviceHandler OnAddedDevice;
        public event UPnPSmartControlPoint.DeviceHandler OnRemovedDevice;

        public ControlPoint() {
            scp = new UPnPSmartControlPoint(null, null, UPNP_DEVICE_RENDERER);
            scp.OnAddedDevice += new UPnPSmartControlPoint.DeviceHandler(ControlPoint_OnAddedDevice);
            scp.OnRemovedDevice += new UPnPSmartControlPoint.DeviceHandler(ControlPoint_OnRemovedDevice);
        }

        public void Rescan() {
            scp.Rescan();
        }

        private void ControlPoint_OnAddedDevice(UPnPSmartControlPoint sender, UPnPDevice device) {
            Console.WriteLine("OnAddedDevice(" + device.FriendlyName + ", " + device.RemoteEndPoint + ", " + device.UniqueDeviceName + ")");

            foreach (UPnPService service in device.Services) {
                Console.WriteLine(device.FriendlyName + " service: " + service.ServiceURN);
            }

            // only add device if necessary services are available
            if (device.GetServices(UPNP_SERVICE_CONTROL).Length > 0 && device.GetServices(UPNP_SERVICE_AVTRANSPORT).Length > 0) {
                if (OnAddedDevice != null)
                    OnAddedDevice(sender, device);
            }
        }

        private void ControlPoint_OnRemovedDevice(UPnPSmartControlPoint sender, UPnPDevice device) {
            Console.WriteLine("OnRemovedDevice(" + device.FriendlyName + ", " + device.RemoteEndPoint + ", " + device.UniqueDeviceName + ")");

            if (this.device != null && this.device.UniqueDeviceName == device.UniqueDeviceName) {
                this.device = null;
            }

            if (OnRemovedDevice != null) OnRemovedDevice(sender, device);
        }

        public UPnPDevice Device {
            get { return device; }
            set { device = value; }
        }

        public int GetVolume() {
            if (device == null) return -1;
            var service = device.GetServices(UPNP_SERVICE_CONTROL)[0];

            UPnPArgument[] args = new UPnPArgument[] {
                new UPnPArgument("InstanceID", (uint)0),
                new UPnPArgument("Channel", "Master"),
                new UPnPArgument("CurrentVolume", null)
            };

            service.InvokeSync("GetVolume", args);
            return (int)(ushort)args[2].DataValue;
        }

        public void SetVolume(int volume) {
            if (device == null) return;
            var service = device.GetServices(UPNP_SERVICE_CONTROL)[0];

            UPnPArgument[] args = new UPnPArgument[] {
                new UPnPArgument("InstanceID", (uint)0),
                new UPnPArgument("Channel", "Master"),
                new UPnPArgument("DesiredVolume", (ushort)volume)
            };

            service.InvokeSync("SetVolume", args);
        }

        public void VolumeIncrease() {
            SetVolume(GetVolume() + 1);
        }

        public void VolumeDecrease() {
            SetVolume(GetVolume() - 1);
        }

        public bool GetMute() {
            if (device == null) return false;
            var service = device.GetServices(UPNP_SERVICE_CONTROL)[0];

            UPnPArgument[] args = new UPnPArgument[] {
                new UPnPArgument("InstanceID", (uint)0),
                new UPnPArgument("Channel", "Master"),
                new UPnPArgument("CurrentMute", null)
            };

            service.InvokeSync("GetMute", args);
            return (bool)args[2].DataValue;
        }

        public void SetMute(bool mute) {
            if (device == null) return;
            var service = device.GetServices(UPNP_SERVICE_CONTROL)[0];

            UPnPArgument[] args = new UPnPArgument[] {
                new UPnPArgument("InstanceID", (uint)0),
                new UPnPArgument("Channel", "Master"),
                new UPnPArgument("DesiredMute", mute)
            };

            service.InvokeSync("SetMute", args);
        }

        public void MuteToggle() {
            SetMute(!GetMute());
        }

        public void Play() {
            if (device == null) return;
            var service = device.GetServices(UPNP_SERVICE_AVTRANSPORT)[0];

            UPnPArgument[] args = new UPnPArgument[] {
                new UPnPArgument("InstanceID", (uint)0),
                new UPnPArgument("Speed", "1")
            };

            service.InvokeSync("Play", args);
        }

        public void Pause() {
            if (device == null) return;
            var service = device.GetServices(UPNP_SERVICE_AVTRANSPORT)[0];

            UPnPArgument[] args = new UPnPArgument[] {
                new UPnPArgument("InstanceID", (uint)0)
            };

            service.InvokeSync("Pause", args);
        }

        public void Stop() {
            if (device == null) return;
            var service = device.GetServices(UPNP_SERVICE_AVTRANSPORT)[0];

            UPnPArgument[] args = new UPnPArgument[] {
                new UPnPArgument("InstanceID", (uint)0)
            };

            service.InvokeSync("Stop", args);
        }

        public void SetAVTransportURI() {
            if (device == null) return;
            var service = device.GetServices(UPNP_SERVICE_AVTRANSPORT)[0];

            UPnPArgument[] args = new UPnPArgument[] {
                new UPnPArgument("InstanceID", (uint)0),
                new UPnPArgument("CurrentURI", DirectoryServer.S1),
                new UPnPArgument("CurrentURIMetaData", "&lt;DIDL-Lite&gt;&lt;/DIDL-Lite&gt;")
            };

            service.InvokeSync("SetAVTransportURI", args);
        }

        public void Playback() {
            SetAVTransportURI();
            Play();
        }
    }
}
