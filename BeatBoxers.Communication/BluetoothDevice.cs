using System;

namespace BeatBoxers.Communication
{
	public class BluetoothDevice
	{
		public event EventHandler<BeatEventArgs> BeatReceived;

		public string Name { get; private set; }

		public string UUID { get; private set; }

		public void RaiseBeat(string name, double intensity)
		{
			if (BeatReceived != null) {
				BeatReceived (this, new BeatEventArgs (new Beat (name, intensity, DateTime.Now)));
			}
		}

		public BluetoothDevice (string name, string uuid)
		{
			Name = name;
			UUID = uuid;
		}
	}
}

