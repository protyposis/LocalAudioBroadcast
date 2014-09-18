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
using NAudio.CoreAudioApi;

namespace NAudio.Wave {
    /// <summary>
    /// WASAPI Loopback Capture
    /// based on a contribution from "Pygmy" - http://naudio.codeplex.com/discussions/203605
    /// </summary>
    public class WasapiLoopbackCapture2 : WasapiCapture {
        /// <summary>
        /// Initialises a new instance of the WASAPI capture class
        /// </summary>
        public WasapiLoopbackCapture2() :
            this(GetDefaultLoopbackCaptureDevice()) {
        }

        /// <summary>
        /// Initialises a new instance of the WASAPI capture class
        /// </summary>
        /// <param name="captureDevice">Capture device to use</param>
        public WasapiLoopbackCapture2(MMDevice captureDevice) :
            base(captureDevice) {
        }

        /// <summary>
        /// Initialises a new instance of the WASAPI capture class
        /// </summary>
        /// <param name="captureDevice">Capture device to use</param>
        public WasapiLoopbackCapture2(CaptureDevice captureDevice) :
            base(captureDevice.MMDevice) {
        }

        /// <summary>
        /// Gets the default audio loopback capture device
        /// </summary>
        /// <returns>The default audio loopback capture device</returns>
        public static CaptureDevice GetDefaultLoopbackCaptureDevice() {
            MMDeviceEnumerator devices = new MMDeviceEnumerator();
            return new CaptureDevice(devices.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia), true);
        }

        public static List<CaptureDevice> GetLoopbackCaptureDevices() {
            CaptureDevice defaultDevice = GetDefaultLoopbackCaptureDevice();
            List<CaptureDevice> list = new List<CaptureDevice>();
            MMDeviceEnumerator devices = new MMDeviceEnumerator();
            foreach (MMDevice mmd in devices.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active)) {
                list.Add(new CaptureDevice(mmd, mmd == defaultDevice.MMDevice));
            }
            return list;
        }

        ///// <summary>
        ///// Recording wave format
        ///// </summary>
        //public override WaveFormat WaveFormat
        //{
        //    get { return base.WaveFormat; }
        //    set { throw new InvalidOperationException("WaveFormat cannot be set for WASAPI Loopback Capture"); }
        //}

        /// <summary>
        /// Specify loopback
        /// </summary>
        protected override AudioClientStreamFlags GetAudioClientStreamFlags() {
            return AudioClientStreamFlags.Loopback;
        }
    }

    public class CaptureDevice {

        public CaptureDevice(MMDevice device, bool isDefault) {
            MMDevice = device;
            IsDefault = isDefault;
        }

        public MMDevice MMDevice { get; private set; }

        public bool IsDefault { get; private set; }

        public String Name {
            get {
                return (IsDefault ? "Default Playback Capture: " : "") 
                    + MMDevice.FriendlyName + " (" + MMDevice.DeviceFriendlyName + ")";
            }
        }

        public override bool Equals(Object obj) {
            if (obj == null || GetType() != obj.GetType())
                return false;

            return MMDevice.ID == ((CaptureDevice)obj).MMDevice.ID;
        }

        public override int GetHashCode() {
            return MMDevice.GetHashCode();
        }

        public static bool operator ==(CaptureDevice a, CaptureDevice b) {
            if (((object)a == null) || ((object)b == null)) {
                return false;
            }

            return a.Equals(b);
        }

        public static bool operator !=(CaptureDevice a, CaptureDevice b) {
            return !(a == b);
        }
    }
}
