using System;
using BeatBoxers.Communication;
using CoreBluetooth;
using System.Collections.ObjectModel;
using Foundation;
using System.Linq;
using System.Collections.Generic;

namespace BeatBoxers.iOS
{
	public class Bluetooth : BeatBoxers.Communication.IBluetooth
	{
		private readonly NSString UUIDField = new NSString("kCBAdvDataServiceUUIDs");

		CBCentralManager _manager;

		private bool _startScann;

		private readonly Dictionary<BluetoothDevice, CBPeripheral> _peripherialMap;

		private readonly Dictionary<CBPeripheral, BluetoothDevice> _deviceMap;

		public Bluetooth ()
		{
			Devices = new ObservableCollection<BluetoothDevice> ();
			ConnectedDevices = new ObservableCollection<BluetoothDevice> ();
			_peripherialMap = new Dictionary<BluetoothDevice, CBPeripheral> ();
			_deviceMap = new Dictionary<CBPeripheral, BluetoothDevice> ();

			_manager = new CBCentralManager (CoreFoundation.DispatchQueue.CurrentQueue);
			_manager.DiscoveredPeripheral += (object sender, CBDiscoveredPeripheralEventArgs e) => {
				try {
					var name = e.Peripheral.Name;
					var uuid = e.AdvertisementData.ContainsKey(UUIDField) ? (e.AdvertisementData[UUIDField] as NSArray).GetItem<NSObject>(0).Description : null;

					if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(uuid)) {
						name = name.Trim(new char[] { '\r', '\n' });
						var device = _peripherialMap.Any(i => i.Key.Name == name && i.Key.UUID == uuid) ? _peripherialMap.FirstOrDefault(i => i.Key.Name == name && i.Key.UUID == uuid).Key : new BluetoothDevice(name, uuid);
						_peripherialMap[device] = e.Peripheral;

						if (!Devices.Contains(device)) {
							Devices.Add(device);
						}
					}
				} catch (Exception) {
				}
			};
			_manager.DisconnectedPeripheral += (object sender, CBPeripheralErrorEventArgs e) => {
				try {
					if (_peripherialMap.ContainsValue(e.Peripheral)) {
						var device = _peripherialMap.First(i => i.Value == e.Peripheral).Key;
						_peripherialMap.Remove(device);
						if (_deviceMap.ContainsKey(e.Peripheral)) {
							_deviceMap.Remove(e.Peripheral);
						}
						if (Devices.Contains(device)) {
							Devices.Remove(device);
						}
						if (ConnectedDevices.Contains(device)) {
							ConnectedDevices.Remove(device);
						}
					}
				} catch (Exception) {
				}
			};

			_manager.UpdatedState += (object sender, EventArgs e) => {
				if (_startScann && _manager.State == CBCentralManagerState.PoweredOn) {
					StartScan();
				}
			};
			_manager.ConnectedPeripheral += (object sender, CBPeripheralEventArgs e) => {
				if (_peripherialMap.ContainsValue(e.Peripheral)) {
					_deviceMap[e.Peripheral] = _peripherialMap.FirstOrDefault(i => i.Value == e.Peripheral).Key;
					var device = _deviceMap[e.Peripheral];

					if (!ConnectedDevices.Contains(device)) {
						ConnectedDevices.Add(device);
					}

					e.Peripheral.DiscoveredService += (object sender2, NSErrorEventArgs e2) => {
						//System.Console.WriteLine ("Discovered a service");
						foreach (var service in e.Peripheral.Services) {
							Console.WriteLine (service.ToString ()); 
							e.Peripheral.DiscoverCharacteristics (service);
						}
					};

					e.Peripheral.DiscoveredCharacteristic += (object sender2, CBServiceEventArgs e2) => {
						//System.Console.WriteLine ("Discovered characteristics of " + e.Peripheral);
						foreach (var c in e2.Service.Characteristics) {
							Console.WriteLine (string.Format("{0} - isNotifying = {1}", c.ToString (), c.IsNotifying));
							if (device.UUID == "2220" && !c.IsNotifying && c.UUID == CBUUID.FromString("2221")) {
								e.Peripheral.SetNotifyValue(true, c);
								e.Peripheral.ReadValue (c);
							}
							if (device.UUID == "FFF0" && !c.IsNotifying && c.UUID == CBUUID.FromString("FFF6")) {
								e.Peripheral.SetNotifyValue(true, c);
								e.Peripheral.ReadValue (c);
							}
						}
					};

					e.Peripheral.UpdatedCharacterteristicValue += (object sender2, CBCharacteristicEventArgs e2) => {
						//Console.WriteLine ("Value of characteristic " + e2.Characteristic + " is " + e2.Characteristic.Value);

						if (device.UUID == "2220" && e2.Characteristic.UUID == CBUUID.FromString("2221")) {
							_deviceMap[e.Peripheral].RaiseBeat("foot", 0.5);
						}

						if (device.UUID == "FFF0" && e2.Characteristic.UUID == CBUUID.FromString("FFF6")) {
							var data = e2.Characteristic.Value;
							for (int i = 0; i < Math.Min(8, (int)data.Length); i++) {
								if (data[i] != 48) {
									_deviceMap[e.Peripheral].RaiseBeat(string.Format("pad{0}", i + 1), 0.5);
								}
							}
						}

						if (e2.Characteristic.Descriptors != null) {
							foreach (var d in e2.Characteristic.Descriptors) {
								Console.WriteLine (d.ToString ());
								e.Peripheral.ReadValue (d);
							}
						}
					};

					e.Peripheral.UpdatedValue += (object sender2, CBDescriptorEventArgs e2) => {
						//Console.WriteLine ("Value of descriptor " + e2.Descriptor + " is " + e2.Descriptor.Value);
					};

					if (device.Name == "BeatS" && device.UUID == "2220") {
						e.Peripheral.DiscoverServices(new CBUUID[] { CBUUID.FromString("00002220-0000-1000-8000-00805F9B34FB"), CBUUID.FromString("00002221-0000-1000-8000-00805F9B34FB"), CBUUID.FromString("00002902-0000-1000-8000-00805F9B34FB") } );
					} 
					else if (device.Name == "BeatBoxers" && device.UUID == "FFF0")
					{
						e.Peripheral.DiscoverServices(new CBUUID[] { CBUUID.FromString("0000fff0-0000-1000-8000-00805F9B34FB"), CBUUID.FromString("0000fff6-0000-1000-8000-00805F9B34FB"), CBUUID.FromString("00002902-0000-1000-8000-00805F9B34FB") } );
					} 
					else 
					{
						_manager.CancelPeripheralConnection(e.Peripheral);
					}
				}
			};
		}

		#region IBluetooth implementation

		public System.Collections.ObjectModel.ObservableCollection<BluetoothDevice> Devices {
			get;
			private set;
		}

		public ObservableCollection<BluetoothDevice> ConnectedDevices {
			get;
			private set;
		}

		public void StartScan ()
		{
			if (_manager.State != CBCentralManagerState.PoweredOn) {
				_startScann = true;
			} else {
				_startScann = false;
				_manager.ScanForPeripherals ((CBUUID[])null);
			}
		}

		public void StopScan ()
		{
			_manager.StopScan ();
		}

		public bool IsScanning {
			get {
				return _manager.IsScanning;
			}
		}

		public void Connect (BluetoothDevice device)
		{
			if (_peripherialMap.ContainsKey (device)) {
				_manager.ConnectPeripheral (_peripherialMap [device]);
			}
		}
		#endregion
	}
}

