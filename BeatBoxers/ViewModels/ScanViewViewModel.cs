using System;
using BeatBoxers.Communication;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Xamarin.Forms;

namespace BeatBoxers
{
	public class ScanViewViewModel : ViewModelBase
	{
		private readonly IBluetooth _bluetooth;
		public IBluetooth Bluetooth { get { return _bluetooth; } }

		public ObservableCollection<BluetoothDeviceViewModel> Devices { get; private set; }

		public ICommand StartScanCommand { get; private set; }

		public ICommand StopScanCommand { get; private set; }

		public bool IsScanning {
			get { return _IsScanning; }
			set {
				if (_IsScanning != value) {
					_IsScanning = value;
					OnPropertyChanged (IsScanningPropertyName);
				}
			}
		}
		private bool _IsScanning;
		public const string IsScanningPropertyName = "IsScanning";

		public ScanViewViewModel (IBluetooth bluetooth)
		{
			_bluetooth = bluetooth;
			_IsScanning = _bluetooth.IsScanning;
			Devices = new ObservableCollection<BluetoothDeviceViewModel> ();

			_bluetooth.Devices.CollectionChanged += (object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) => {
				foreach (var device in _bluetooth.Devices.Where(i => !Devices.Any(d => d.Device == i))) {
					Devices.Add(new BluetoothDeviceViewModel(device) {
						IsConnected = _bluetooth.ConnectedDevices.Contains(device)
					});
				}
				foreach (var device in Devices.Where(i => !bluetooth.Devices.Contains(i.Device)).ToList()) {
					Devices.Remove(device);
				}
			};

			_bluetooth.ConnectedDevices.CollectionChanged += (object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) => {
				foreach (var device in Devices) {
					device.IsConnected = _bluetooth.ConnectedDevices.Contains(device.Device);
				}
			};

			StartScanCommand = new Command (() => {
				IsScanning = true;
				_bluetooth.StartScan();
				(StartScanCommand as Command).ChangeCanExecute();
				(StopScanCommand as Command).ChangeCanExecute();
			}, () => {
				return !IsScanning;
			});

			StopScanCommand = new Command (() => {
				IsScanning = false;
				_bluetooth.StopScan();
				(StartScanCommand as Command).ChangeCanExecute();
				(StopScanCommand as Command).ChangeCanExecute();
			}, () => {
				return IsScanning;
			});
		}
	}
}

