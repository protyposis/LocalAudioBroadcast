using System;
using System.Collections.Generic;
using System.Text;

namespace LocalAudioBroadcast.Data {
    /// <summary>
    /// Transforms captured source audio data into network output audio data.
    /// </summary>
    interface IInputTransform {
        byte[] Transform(byte[] buffer, int offset, int count);
    }
}
