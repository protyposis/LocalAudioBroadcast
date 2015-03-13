using OpenSource.UPnP;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace LocalAudioBroadcast {
    class ServiceEventHandler {

        // RenderingControl LastChange: Volume, Mute
        private UPnPService renderingControlService;
        private static readonly Regex rcVolumeRegex = new Regex("<Volume\\s+Channel=\"Master\"\\s+val=\"(?<volume>\\d+)\"");
        private static readonly Regex rcMuteRegex = new Regex("<Mute\\s+Channel=\"Master\"\\s+val=\"(?<mute>\\d+)\"");

        // AVTransport LastChange: AVTransportURI, TransportState, CurrentTrackDuration, RelativeTimePosition, 
        // NumberOfTracks, CurrentTrack, CurrentTrackURI, TransportStatus, AVTransportURIMetaData, CurrentPlayMode
        private UPnPService avTransportService;
        private static readonly Regex avTransportStateRegex = new Regex("<TransportState\\s+val=\"(?<state>[A-Z_]+)\"");

        public delegate void ValueChangeEventHandler<T>(ServiceEventHandler sender, T value);

        public event ValueChangeEventHandler<int> OnVolumeChanged;
        public event ValueChangeEventHandler<bool> OnMuteChanged;
        public event ValueChangeEventHandler<bool> OnPlaybackChanged;

        public ServiceEventHandler(UPnPDevice device) {
            renderingControlService = device.GetServices(ControlPoint.UPNP_SERVICE_CONTROL)[0];
            renderingControlService.OnUPnPEvent += RenderingControl_OnUPnPEvent;
            renderingControlService.Subscribe(3000, delegate(UPnPService sender, bool SubscribeOK) {
                Console.WriteLine("Subscription " + (SubscribeOK ? "successful" : "FAILED") + ": " + sender.ServiceID);
            });

            avTransportService = device.GetServices(ControlPoint.UPNP_SERVICE_AVTRANSPORT)[0];
            avTransportService.OnUPnPEvent += AVTransport_OnUPnPEvent;
            avTransportService.Subscribe(3000, delegate(UPnPService sender, bool SubscribeOK) {
                Console.WriteLine("Subscription " + (SubscribeOK ? "successful" : "FAILED") + ": " + sender.ServiceID);
            });
        }

        private void RenderingControl_OnUPnPEvent(UPnPService sender, long SEQ) {
            Match match;
            string lastChange = (string)sender.GetStateVariable("LastChange");

            // Volume change
            // <Event xmlns="urn:schemas-upnp-org:metadata-1-0/RCS/"><InstanceID val="0"><Volume Channel="Master" val="61"/><VolumeDB Channel="Master" val="-1829"/></InstanceID></Event>
            if ((match = rcVolumeRegex.Match(lastChange)).Success) {
                string volume = match.Groups["volume"].Value;
                Console.WriteLine("Volume Change: " + volume);
                if (OnVolumeChanged != null) {
                    OnVolumeChanged(this, int.Parse(volume));
                }
            }
            // Mute change
            // <Event xmlns="urn:schemas-upnp-org:metadata-1-0/RCS/"><InstanceID val="0"><Mute Channel="Master" val="1"/></InstanceID></Event>
            else if ((match = rcMuteRegex.Match(lastChange)).Success) {
                string mute = match.Groups["mute"].Value;
                Console.WriteLine("Mute Change: " + mute);
                if (OnMuteChanged != null) {
                    OnMuteChanged(this, int.Parse(mute) == 1);
                }
            }
            // other events...
            else {
                Console.WriteLine("RenderingControl Event " + SEQ + " " + sender.ServiceID + " " + lastChange);
            }
        }

        private void AVTransport_OnUPnPEvent(UPnPService sender, long SEQ) {
            Match match;
            string lastChange = (string)sender.GetStateVariable("LastChange");

            // TransportState change
            // <Event xmlns="urn:schemas-upnp-org:metadata-1-0/AVT/"><InstanceID val="0"><TransportState val="PLAYING"/></InstanceID></Event>
            // <Event xmlns="urn:schemas-upnp-org:metadata-1-0/AVT/"><InstanceID val="0"><TransportState val="PAUSED_PLAYBACK"/></InstanceID></Event>
            // <Event xmlns="urn:schemas-upnp-org:metadata-1-0/AVT/"><InstanceID val="0"><TransportState val="STOPPED"/></InstanceID></Event>
            if ((match = avTransportStateRegex.Match(lastChange)).Success) {
                string state = match.Groups["state"].Value;
                Console.WriteLine("TransportState Change: " + state);
                if (OnPlaybackChanged != null) {
                    OnPlaybackChanged(this, (state == "PLAYING" || state == "PAUSED_PLAYBACK"));
                }
            }
            // other events...
            else {
                Console.WriteLine("AVTransport Event " + SEQ + " " + sender.ServiceID + " " + lastChange);
            }
        }

        public void Unregister() {
            renderingControlService.OnUPnPEvent -= RenderingControl_OnUPnPEvent;
            renderingControlService.UnSubscribe(delegate(UPnPService sender, long SEQ) {
                Console.WriteLine("Unsubscribed: " + sender.ServiceID);
            });

            avTransportService.OnUPnPEvent -= AVTransport_OnUPnPEvent;
            avTransportService.UnSubscribe(delegate(UPnPService sender, long SEQ) {
                Console.WriteLine("Unsubscribed: " + sender.ServiceID);
            });
        }
    }
}
