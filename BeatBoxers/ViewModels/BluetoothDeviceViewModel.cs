using System;
using BeatBoxers.Communication;
using System.Windows.Input;
using Xamarin.Forms;

namespace BeatBoxers
{
	public class BluetoothDeviceViewModel : ViewModelBase
	{
		private readonly BluetoothDevice _device;

		public BluetoothDevice Device { get { return _device; } }

		public string Name { get { return _device.Name; } }

		public string UUID { get { return _device.UUID; } }

		public bool IsConnected {
			get {
				return _IsConnected;
			}
			set {
				if (_IsConnected != value) {
					_IsConnected = value;
					OnPropertyChanged (IsConnectedPropertyName);
				}
			}
		}
		private bool _IsConnected;
		public const string IsConnectedPropertyName = "IsConnected";

		public ICommand ConnectCommand { get; private set; }

		private ScanViewViewModel ScanViewViewModel {
			get {
				return App.MainContent as ScanViewViewModel;
			}
		}

		public BluetoothDeviceViewModel (BluetoothDevice device)
		{
			_device = device;
			ConnectCommand = new Command (() => {
				if (ScanViewViewModel != null) {
					ScanViewViewModel.Bluetooth.Connect(_device);
				}
			}, () => {
				return !IsConnected;
			});
		}
	}
}

