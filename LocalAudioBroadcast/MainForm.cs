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
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using LocalAudioBroadcast.Properties;

namespace LocalAudioBroadcast {
    public partial class MainForm : Form {

        private LocalAudioBroadcast lab;
        private int volumeIconIndex;

        public MainForm() {
            InitializeComponent();

            lab = new LocalAudioBroadcast();
            lab.ControlPoint.OnAddedDevice += ControlPoint_OnAddedDevice;
            lab.ControlPoint.OnRemovedDevice += ControlPoint_OnRemovedDevice;
        }

        private void MainForm_Shown(Object sender, EventArgs e) {
            // start after the form has loaded, else the BeginInvoke methods in the event handlers 
            // won't be executed in cases where a device is found before the form is loaded
            lab.Start();

            StreamingFormat format;
            try {
                // try to get default format from settings
                format = StreamingFormat.GetFormat(Settings.Default.StreamingFormat);
                StreamingFormat.DefaultFormat = format;
            }
            catch {
                // no (valid) default format in settings
                format = StreamingFormat.DefaultFormat;
            }

            foreach(RadioButton rb in new[] { rbFormatLPCM, rbFormatWAV }) {
                if (rb.Tag.ToString() == format.Id) {
                    rb.Checked = true;
                    break;
                }
            }
        }

        private void ControlPoint_OnAddedDevice(OpenSource.UPnP.UPnPSmartControlPoint sender, OpenSource.UPnP.UPnPDevice device) {
            BeginInvoke((MethodInvoker)delegate {
                cbRenderers.Items.Add(new ComboBoxItemWrapper(device));
                if (cbRenderers.SelectedIndex == -1) {
                    cbRenderers.SelectedIndex = 0;
                }
                if (device.UniqueDeviceName == Settings.Default.PreferredRenderer) {
                    cbRenderers.SelectedIndex = cbRenderers.Items.Count - 1;
                }
            });
        }

        private void ControlPoint_OnRemovedDevice(OpenSource.UPnP.UPnPSmartControlPoint sender, OpenSource.UPnP.UPnPDevice device) {
            BeginInvoke((MethodInvoker)delegate {
                foreach (ComboBoxItemWrapper wrapper in cbRenderers.Items) {
                    if (wrapper.Device == device) {
                        cbRenderers.Items.Remove(wrapper);
                        break;
                    }
                }
            });
        }

        private void btnRefreshRenderers_Click(object sender, EventArgs e) {
            lab.ControlPoint.Rescan();
        }

        private void cbRenderers_SelectedIndexChanged(object sender, EventArgs e) {
            if (cbRenderers.SelectedIndex >= 0) {
                // deregister event handlers of previous device
                if (lab.ControlPoint.Device != null) {
                    lab.ControlPoint.EventHandler.OnVolumeChanged -= EventHandler_OnVolumeChanged;
                    lab.ControlPoint.EventHandler.OnMuteChanged -= EventHandler_OnMuteChanged;
                    lab.ControlPoint.EventHandler.OnPlaybackChanged -= EventHandler_OnPlaybackChanged;
                }

                // set new device
                lab.ControlPoint.Device = ((ComboBoxItemWrapper)cbRenderers.SelectedItem).Device;

                // register event handlers for new device
                lab.ControlPoint.EventHandler.OnVolumeChanged += EventHandler_OnVolumeChanged;
                lab.ControlPoint.EventHandler.OnMuteChanged += EventHandler_OnMuteChanged;
                lab.ControlPoint.EventHandler.OnPlaybackChanged += EventHandler_OnPlaybackChanged;

                // update ui
                tbVolume.Value = lab.ControlPoint.GetVolume();
                controlPanel.Enabled = true;
            }
        }

        private void EventHandler_OnVolumeChanged(ServiceEventHandler sender, int volume) {
            if (InvokeRequired) { Invoke((MethodInvoker)(() => this.EventHandler_OnVolumeChanged(sender, volume))); return; }
            tbVolume.Value = volume;
        }

        private void EventHandler_OnMuteChanged(ServiceEventHandler sender, bool muted) {
            if (InvokeRequired) { Invoke((MethodInvoker)(() => this.EventHandler_OnMuteChanged(sender, muted))); return; }
            btnVolume.ImageIndex = muted ? 0 : volumeIconIndex;
        }

        private void EventHandler_OnPlaybackChanged(ServiceEventHandler sender, bool playing) {
            if (InvokeRequired) { Invoke((MethodInvoker)(() => this.EventHandler_OnPlaybackChanged(sender, playing))); return; }
            btnPlay.ImageIndex = playing ? 1 : 0;
        }

        private void btnPlay_Click(object sender, EventArgs e) {
            if (btnPlay.ImageIndex == 0) {
                lab.ControlPoint.Playback();
                btnPlay.ImageIndex = 1;
            }
            else {
                lab.ControlPoint.Stop();
                btnPlay.ImageIndex = 0;
            }
        }

        private void btnVolume_Click(object sender, EventArgs e) {
            if (!lab.ControlPoint.GetMute()) {
                lab.ControlPoint.SetMute(true);
                btnVolume.ImageIndex = 0;
            }
            else {
                lab.ControlPoint.SetMute(false);
                btnVolume.ImageIndex = volumeIconIndex;
                lab.ControlPoint.SetVolume(tbVolume.Value);
            }
        }

        private void tbVolume_ValueChanged(object sender, EventArgs e) {
            if (tbVolume.Value < 30) {
                volumeIconIndex = 1;
            }
            else if (tbVolume.Value > 70) {
                volumeIconIndex = 3;
            }
            else {
                volumeIconIndex = 2;
            }
            if (btnVolume.ImageIndex > 0) {
                btnVolume.ImageIndex = volumeIconIndex;
                lab.ControlPoint.SetVolume(tbVolume.Value);
            }
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e) {
            lab.Stop();
            Application.Exit();
        }

        private class ComboBoxItemWrapper {

            public OpenSource.UPnP.UPnPDevice Device { get; private set; }

            public ComboBoxItemWrapper(OpenSource.UPnP.UPnPDevice device) {
                Device = device;
            }

            public override string ToString() {
                return Device.FriendlyName;
            }
        }

        private void cbRenderers_SelectionChangeCommitted(object sender, EventArgs e) {
            if (cbRenderers.SelectedIndex >= 0) {
                Settings.Default.PreferredRenderer = ((ComboBoxItemWrapper)cbRenderers.SelectedItem).Device.UniqueDeviceName;
                Settings.Default.Save();
            }
        }

        private void btnAbout_Click(object sender, EventArgs e) {
            new AboutBox().ShowDialog(this);
        }

        private void rbFormat_CheckedChanged(object sender, EventArgs e) {
            StreamingFormat format = StreamingFormat.GetFormat(((RadioButton)sender).Tag.ToString());
            StreamingFormat.DefaultFormat = format;
            Settings.Default.StreamingFormat = format.Id;
            Settings.Default.Save();

            // reinitialize directory after format change to reflect the current format in the item XMl definitions
            DirectoryServer.Directory.Init(DirectoryServer.S1);
        }
    }
}
