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
            scp = new UPnPSmartControlPoint(ControlPoint_OnAddedDevice, null, UPNP_DEVICE_RENDERER);
            scp.OnRemovedDevice += ControlPoint_OnRemovedDevice;
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

            /*
             * Why does an empty/null CurrentURIMetaData not work with all renderers?
             * 
             * ----------------------------------------------------------------------
             * 
             * UPnP AVTransport:3 Service (2013, for UPnP 1.0)
             * http://upnp.org/specs/av/av4/
             * 
             * 2.4.1 SetAVTransportURI
             * 
             * A control point can supply metadata associated with the specified resource, using 
             * a DIDL-Lite XML Fragment (defined in the ContentDirectory service specification), 
             * in argument CurrentURIMetaData.
             * If a control point does not want to use this feature it can supply the empty string 
             * for the CurrentURIMetaData argument.
             * 
             * ----------------------------------------------------------------------
             * 
             * DLNA Part 1-1 (March 2014)
             * http://www.dlna.org/dlna-for-industry/guidelines
             * 
             * 7.4.1.6.8.3
             * 
             * This is a mandate over what UPnP normally allows as optional behavior.
             * 
             * For a Push Controller (+PU+) that does not contain a CDS, the DIDL-Lite metadata 
             * can typically be created from a DIDL-Lite XML fragment template containing only 
             * the minimal properties as described in 7.4.1.6.14.9 or example, since the @id 
             * property value only needs to be unique within the scope of the DIDL-Lite XML 
             * fragment, the @id property value can be any value chosen by the Push Controller; 
             * the @parent property can have a value of −1, and the @restricted property value 
             * can be either 0 or 1.
             * 
             * 7.4.1.6.14.9
             * 
             * If a UPnP AV MediaRenderer control point specifies a value for the CurrentURIMetaData 
             * argument of an AVT:SetAVTransportURI request, then the control point shall follow 
             * these restrictions for the value of the CurrentURIMetaData argument, as follows:
             * 
             * - compliant with the DIDL-Lite schema;
             * - exactly one <DIDL-Lite> element;
             * - exactly one <item> or <container> element;
             * - exactly one <dc:title> element and value;
             * - a minimum of zero and a maximum of one <dc:creator> element and value;
             * - exactly one <upnp:class> element and value;
             * - a minimum of one <res> element.
             * 
             * All other XML elements are permitted as long as they are properly declared with 
             * their namespaces.
             * 
             * The provided metadata shall represent the metadata of the content indicated by the 
             * CurrentURI input argument. One of the <res> elements shall be the <res> element that 
             * contains the URI specified in the CurrentURI input argument.
             * 
             * ----------------------------------------------------------------------
             * 
             * Conclusion: 
             * Some renderers are UPnP certified and accept empty metadata, others are 
             * DLNA specified and demand the metadata.
             */

            var defaultItem = DirectoryServer.Directory.GetItem(0);

            UPnPArgument[] args = new UPnPArgument[] {
                new UPnPArgument("InstanceID", (uint)0),
                new UPnPArgument("CurrentURI", defaultItem.Uri),
                new UPnPArgument("CurrentURIMetaData", defaultItem.GetDidl())
            };

            service.InvokeSync("SetAVTransportURI", args);
        }

        public void Playback() {
            SetAVTransportURI();
            Play();
        }
    }
}
