using System;
using Xamarin.Forms;
using BeatBoxers.Communication;
using BeatBoxers.Audio;

namespace BeatBoxers
{
	public class App : Application
	{
		public IBluetooth Bluetooth { get; private set; }

		public AudioRenderer AudioRenderer { get; private set; }

		public static ViewModelBase MainContent {
			get {
				return Application.Current.MainPage.BindingContext as ViewModelBase;
			}
		}

		public App (IBluetooth bluetooth, IAudio audio, IAudioResource audioResource)
		{
			Bluetooth = bluetooth;
			AudioRenderer = new AudioRenderer (audio, audioResource);

			bluetooth.ConnectedDevices.CollectionChanged += (object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) => {
				if (e.OldItems != null) {
					foreach (BluetoothDevice oldItem in e.OldItems) {
						oldItem.BeatReceived -= BeatReceived;
					}
				}

				if (e.NewItems != null) {
					foreach (BluetoothDevice newItem in e.NewItems) {
						newItem.BeatReceived += BeatReceived;
					}
				}
			};

			// The root page of your application
			MainPage = new ContentPage();

			OpenScanView ();
		}

		private void BeatReceived(object sender, BeatEventArgs e)
		{
			AudioRenderer.PushBeat (e.Beat);
		}

		public void OpenScanView()
		{
			MainPage.BindingContext = new ScanViewViewModel (Bluetooth);
			(MainPage as ContentPage).Content = new ScanView ();
		}

		protected override void OnStart ()
		{
			// Handle when your app starts
		}

		protected override void OnSleep ()
		{
			// Handle when your app sleeps
		}

		protected override void OnResume ()
		{
			// Handle when your app resumes
		}
	}
}

