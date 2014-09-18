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

// UPnP .NET Framework Device Stack, Core Module
// Device Builder Build#1.0.4561.18413

using System;
using OpenSource.UPnP;

namespace LocalAudioBroadcast
{
	/// <summary>
	/// Summary description for Main.
	/// </summary>
	class MainConsole
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main(string[] args)
		{
			// Starting UPnP Device
			//System.Console.WriteLine("UPnP .NET Framework Stack");
			//System.Console.WriteLine("Device Builder Build#1.0.4561.18413");

            LocalAudioBroadcast lab = new LocalAudioBroadcast();
            lab.Start();

			System.Console.WriteLine(Environment.NewLine + "Control:" + Environment.NewLine
                + "RETURN ... exit" + Environment.NewLine
                + "P ........ play to default renderer" + Environment.NewLine
                + "S ........ stop playback" + Environment.NewLine
                + "R ........ rescan renderers" + Environment.NewLine
                + "+ ........ increase volume" + Environment.NewLine
                + "- ........ decrease volume" + Environment.NewLine
                + "M ........ mute" + Environment.NewLine
                );

            while(true) {
                ConsoleKeyInfo key = System.Console.ReadKey(true);
                if (key.Key == ConsoleKey.Enter) {
                    break;
                }
                else if (key.Key == ConsoleKey.Add || key.Key == ConsoleKey.OemPlus) {
                    lab.ControlPoint.VolumeIncrease();
                    Console.WriteLine("Vol+");
                }
                else if (key.Key == ConsoleKey.Subtract || key.Key == ConsoleKey.OemMinus) {
                    lab.ControlPoint.VolumeDecrease();
                    Console.WriteLine("Vol-");
                }
                else if (key.Key == ConsoleKey.M) {
                    lab.ControlPoint.MuteToggle();
                    Console.WriteLine("Mute");
                }
                else if (key.Key == ConsoleKey.P) {
                    lab.ControlPoint.Playback();
                    Console.WriteLine("Playback");
                }
                else if (key.Key == ConsoleKey.R) {
                    lab.ControlPoint.Rescan();
                    Console.WriteLine("Rescan");
                }
                else if (key.Key == ConsoleKey.S) {
                    lab.ControlPoint.Stop();
                    Console.WriteLine("Stop");
                }
            }

            lab.Stop();
		}
		
	}
}

