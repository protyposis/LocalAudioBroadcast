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
using System.Web;

namespace LocalAudioBroadcast {
    class DidlUtil {

        public static string GetContainer(string id, string restricted, string parentID, string title, string _class) {
            string retVal =
            "<container id=\"" + id + "\" restricted=\"" + restricted + "\" parentID=\"" + parentID + "\">" +
            "<dc:title>" + HttpUtility.HtmlEncode(title) + "</dc:title>" +
            "<upnp:class>" + _class + "</upnp:class>" +
            "</container>";
            return retVal;
        }

        public static string GetMusicItem(string id, string parentID, string restricted, string title,
            string artist, string album, string genre,
            string duration, string bitrate, string sampleFrequency, string nrAudioChannels, string bitsPerSample,
            string protocolInfo, string resUri, string _class) {
            string retVal =
            "<item id=\"" + id + "\" parentID=\"" + parentID + "\" restricted=\"" + restricted + "\">" +
            "<dc:title>" + HttpUtility.HtmlEncode(title) + "</dc:title>" +
            "<res bitsPerSample=\"" + bitsPerSample + "\" protocolInfo=\"" + protocolInfo + "\" duration=\"" + duration + "\" nrAudioChannels=\"" + nrAudioChannels + "\" bitrate=\"" + bitrate + "\" sampleFrequency=\"" + sampleFrequency + "\">" + HttpUtility.HtmlEncode(resUri) + "</res>" +
            "<upnp:artist>" + artist + "</upnp:artist>" +
            "<upnp:album>" + album + "</upnp:album>" +
            "<upnp:genre>" + genre + "</upnp:genre>" +
            "<upnp:class>" + _class + "</upnp:class>" +
            "</item>";
            return retVal;
        }

        public static string BeginDidl() {
            return "<DIDL-Lite xmlns:dc=\"http://purl.org/dc/elements/1.1/\" xmlns:upnp=\"urn:schemas-upnp-org:metadata-1-0/upnp/\" xmlns=\"urn:schemas-upnp-org:metadata-1-0/DIDL-Lite/\">";
        }

        public static string EndDidl() {
            return "</DIDL-Lite>";
        }

    }
}
