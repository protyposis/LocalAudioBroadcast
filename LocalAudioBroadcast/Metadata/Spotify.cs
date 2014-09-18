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
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Threading;

namespace LocalAudioBroadcast.Metadata {
    /// <summary>
    /// Inspiration taken from: http://spotifyremote.codeplex.com/SourceControl/changeset/view/24942#104454 (GPL)
    /// (This code can also be found in various projects all over the internet)
    /// </summary>
    class Spotify : ITrackInfoProvider {

        private const string SPOTIFY_TITLE = "Spotify";
        private const string SPOTIFY_TITLE_SEPARATOR = " – ";
        private const string SPOTIFY_PLAYBACK_TITLE_PREFIX = SPOTIFY_TITLE + SPOTIFY_TITLE_SEPARATOR;

        private Process spotifyProcess;
        private volatile bool _threadRunning;
        private volatile bool _threadShouldStop;
        private string title;
        private event EventHandler<TrackInfoChangedEventArgs> _TrackInfoChanged;

        public bool UpdateSpotifyProcess() {
            if (spotifyProcess == null || spotifyProcess.HasExited) {
                Process[] processes = Process.GetProcessesByName("spotify");
                if (processes.Length > 0) {
                    spotifyProcess = processes[0];
                    return true;
                }
                else {
                    return false;
                }
            }

            spotifyProcess.Refresh();
            return true;
        }

        public string Title {
            get {
                if (UpdateSpotifyProcess()) {
                    string title = spotifyProcess.MainWindowTitle;

                    // remove Spotify title prefix
                    if (title.Length > SPOTIFY_PLAYBACK_TITLE_PREFIX.Length)
                        title = title.Substring(SPOTIFY_PLAYBACK_TITLE_PREFIX.Length);
                    else if (title.Equals(SPOTIFY_TITLE))
                        title = "N/A";

                    return title;
                }
                else {
                    return "N/A";
                }
            }
        }

        public TrackInfo TrackInfo {
            get {
                return GetTrackInfo(Title);
            }
        }

        private TrackInfo GetTrackInfo(string title) {
            TrackInfo trackInfo = new TrackInfo() {
                FullTitle = title
            };

            // split into Artist and Track Title
            int separator = title.IndexOf(SPOTIFY_TITLE_SEPARATOR);
            if (separator > 0) {
                trackInfo.Artist = title.Substring(0, separator);
                trackInfo.Track = title.Substring(separator + SPOTIFY_TITLE_SEPARATOR.Length);
            }

            return trackInfo;
        }

        public void RequestTrackInfo() {
            if (_TrackInfoChanged != null) {
                _TrackInfoChanged(this, new TrackInfoChangedEventArgs { TrackInfo = TrackInfo });
            }
        }

        public event EventHandler<TrackInfoChangedEventArgs> TrackInfoChanged {
            add {
                if (_TrackInfoChanged == null) {
                    Start();
                }
                _TrackInfoChanged += value;
            }
            remove {
                _TrackInfoChanged -= value;
                if (_TrackInfoChanged == null) {
                    Stop();
                }
            }
        }

        private void Start() {
            if (_threadRunning) {
                return;
            }

            _threadShouldStop = false;
            _threadRunning = true;

            UpdateSpotifyProcess();
            new Thread(Update).Start();
        }

        private void Stop() {
            _threadShouldStop = true;
        }

        private void Update() {
            while (!_threadShouldStop) {
                string newTitle = Title;

                if (title != newTitle) {
                    title = newTitle;
                    RequestTrackInfo();
                }
                Thread.Sleep(2000);
            }
            _threadRunning = false;
        }
    }
}
