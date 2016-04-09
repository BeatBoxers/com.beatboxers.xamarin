using System;
using System.Collections.ObjectModel;

namespace BeatBoxers.Communication
{
	public interface IBluetooth
	{
		ObservableCollection<BluetoothDevice> Devices { get; }

		ObservableCollection<BluetoothDevice> ConnectedDevices { get; }

		bool IsScanning { get; }

		void StartScan();

		void StopScan();

		void Connect(BluetoothDevice device);
			
	}
}

