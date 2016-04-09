using System;
using AVFoundation;
using UIKit;
using AudioUnit;
using Foundation;
using AudioToolbox;
using Xamarin.Forms;
using ObjCRuntime;
using BeatBoxers.Audio;

namespace BeatBoxers.iOS
{
	public class AudioIO : NSObject, IAudio , IDisposable
	{

		public event EventHandler<ProcessBlockEventArgs> ProcessBlockEventHandler
		{
			add
			{
				_processBlockEventHandler += value;
			}
			remove
			{
				_processBlockEventHandler -= value;
			}
		}
		private EventHandler<ProcessBlockEventArgs> _processBlockEventHandler;

		public bool IsRunning {
			get{
				return _audioUnit != null && _audioUnit.IsPlaying;
			}
		}

		public double SampleRate { 
			get{
				return _sampleRate;
			}
		}
		private double _sampleRate = 44100;

		private AudioComponent _audioComponent;
		private AudioUnit.AudioUnit _audioUnit;
		private AudioStreamBasicDescription _audioFormat;
		private NativeProcessBlockEventArgs _processBlockArgs;
		private NSObject notification;

		public AudioIO() {
			InitAudio ();
		}

		public void InitAudio(){ 
			var session = AVAudioSession.SharedInstance();
			NSError error;

			if (session == null)
			{
				var alert = new UIAlertView("Session error", "Unable to create audio session", null, "Cancel");
				alert.Show();
				alert.Clicked += delegate
				{
					alert.DismissWithClickedButtonIndex(0, true);
					return;
				};
			}
			session.SetActive(false);
			session.SetCategory(AVAudioSessionCategory.Playback,AVAudioSessionCategoryOptions.AllowBluetooth | AVAudioSessionCategoryOptions.DefaultToSpeaker | AVAudioSessionCategoryOptions.DuckOthers); //Neded so we can listen to remote events

			notification = AVAudioSession.Notifications.ObserveInterruption((sender, args) => {
				/* Handling audio interuption here */

				if(args.InterruptionType==AVAudioSessionInterruptionType.Began){
					if(_audioUnit!=null && _audioUnit.IsPlaying){
						_audioUnit.Stop();
					}
				}

				System.Diagnostics.Debug.WriteLine ("Notification: {0}", args.Notification);

				System.Diagnostics.Debug.WriteLine ("InterruptionType: {0}", args.InterruptionType);
				System.Diagnostics.Debug.WriteLine ("Option: {0}", args.Option);
			});

			var opts = session.CategoryOptions;

			session.SetPreferredIOBufferDuration(0.01, out error);

			session.SetActive(true);

			_audioFormat = AudioStreamBasicDescription.CreateLinearPCM(_sampleRate, bitsPerChannel: 32);

			_audioFormat.FormatFlags|= AudioFormatFlags.IsNonInterleaved | AudioFormatFlags.IsFloat;

			_audioComponent = AudioComponent.FindComponent(AudioTypeOutput.Remote);

			// creating an audio unit instance
			_audioUnit = new AudioUnit.AudioUnit(_audioComponent);



			// setting audio format
			_audioUnit.SetAudioFormat(_audioFormat,
				AudioUnitScopeType.Input,
				0 // Remote Output
			);
				
			//_audioFormat.FormatFlags = AudioStreamBasicDescription.AudioFormatFlagsNativeFloat;

			_audioUnit.SetAudioFormat(_audioFormat, AudioUnitScopeType.Output, 1);
			// setting callback method
			_audioUnit.SetRenderCallback(_audioUnit_RenderCallback, AudioUnitScopeType.Global);

			_audioUnit.Initialize();
			_audioUnit.Stop(); 
		}

		public void Start(){
			if (_audioUnit != null) {
				_audioUnit.Start ();
			}
		}

		public void Pause(){
			if (_audioUnit != null && _audioUnit.IsPlaying)
				_audioUnit.Stop ();
		}

		public void Stop(){
			if (_audioUnit != null) {
				_audioUnit.Stop();
				_audioUnit.Dispose();

				notification.Dispose ();
			}
		}

		private void UpdateSampleRates (){
			if(_audioUnit!=null && !_audioUnit.IsPlaying){
				_audioFormat = AudioStreamBasicDescription.CreateLinearPCM(_sampleRate, bitsPerChannel: 32);
				_audioFormat.SampleRate = _sampleRate;
				_audioFormat.FormatFlags = AudioStreamBasicDescription.AudioFormatFlagsNativeFloat;

				// setting audio format
				_audioUnit.SetAudioFormat(_audioFormat,
					AudioUnitScopeType.Input,
					0 // Remote Output
				);

				_audioUnit.SetAudioFormat(_audioFormat, AudioUnitScopeType.Output, 1);
			}
		}

		AudioUnitStatus _audioUnit_RenderCallback(AudioUnitRenderActionFlags actionFlags, AudioTimeStamp timeStamp, uint busNumber, uint numberFrames, AudioBuffers data)
		{
			if (_processBlockEventHandler != null) 
			{
				if (_processBlockArgs == null || 
					_processBlockArgs.ChannelCount != _audioFormat.ChannelsPerFrame || 
					_processBlockArgs.NumberOfFrames != numberFrames) 
				{
					_processBlockArgs = new NativeProcessBlockEventArgs ((int)numberFrames, _audioFormat.ChannelsPerFrame);
				}

				_processBlockArgs.AudioBuffers = data;
				_processBlockEventHandler (this, _processBlockArgs);

				//_processBlockArgs.ReturnInterleavedBuffer ();
				_processBlockArgs.ReturnNonInterleavedBuffer ();
			}

			return AudioUnitStatus.NoError;
		}

		protected override void Dispose (bool disposing)
		{
			base.Dispose (disposing);
			Stop ();
		}
	}
}

