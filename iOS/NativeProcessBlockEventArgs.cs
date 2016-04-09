using System;
using AudioToolbox;
using BeatBoxers.Audio;

namespace BeatBoxers.iOS
{
	public class NativeProcessBlockEventArgs : ProcessBlockEventArgs
	{
		public AudioBuffers AudioBuffers { 
			get { return _AudioBuffers; } 
			set {
				_AudioBuffers = value;
				InterleavedBuffer = null;
				NonInterleavedBuffer = null;
			} 
		}
		protected AudioBuffers _AudioBuffers;

		public override bool GetInterleavedBuffer()
		{
			if (InterleavedBuffer != null) {
				return true;
			}

			if (AudioBuffers == null) {
				return false;
			}

			_interleavedBuffer = _interleavedBuffer ?? new float[NumberOfFrames * ChannelCount];

			unsafe {
				fixed (float * b = _interleavedBuffer) {
					var inL = (float*)AudioBuffers [0].Data;
					var inR = (float*)AudioBuffers [1].Data;
					var outL = b;
					var outR = b + 1; 

					for (int i = 0; i < NumberOfFrames; i++) 
					{
						outL [i * 2] = inL [i];
						outR [i * 2] = inR [i];
					}					
				}
			}

			InterleavedBuffer = _interleavedBuffer;
			return true;
		}

		public override bool ReturnInterleavedBuffer()
		{
			if (InterleavedBuffer == null) {
				return false;
			}

			if (AudioBuffers == null) {
				return false;
			}

			unsafe {
				fixed (float * b = _interleavedBuffer) {
					var outL = (float*)AudioBuffers [0].Data;
					var outR = (float*)AudioBuffers [1].Data;
					var inL = b;
					var inR = b + 1; 

					for (int i = 0; i < NumberOfFrames; i++) 
					{
						outL [i] = inL [i * 2];
						outR [i] = inR [i * 2];
					}					
				}
			}

			InterleavedBuffer = null;
			return true;
		}

		public override bool GetNonInterleavedBuffer()
		{
			if (NonInterleavedBuffer != null) {
				return true;
			}

			if (AudioBuffers == null) {
				return false;
			}

			_nonInterleavedBuffer = _nonInterleavedBuffer ?? new float[ChannelCount][];
			for (int i = 0; i < ChannelCount; i++) {
				_nonInterleavedBuffer [i] = new float[NumberOfFrames];
			}

			for (int i = 0; i < ChannelCount; i++) {
				System.Runtime.InteropServices.Marshal.Copy (AudioBuffers [i].Data, _nonInterleavedBuffer [i], 0, NumberOfFrames); 
			}

			NonInterleavedBuffer = _nonInterleavedBuffer;
			return true;
		}

		public override bool ReturnNonInterleavedBuffer()
		{
			if (NonInterleavedBuffer == null) {
				return false;
			}

			if (AudioBuffers == null) {
				return false;
			}

			for (int i = 0; i < ChannelCount; i++) {
				System.Runtime.InteropServices.Marshal.Copy (_nonInterleavedBuffer [i], 0, AudioBuffers [i].Data, NumberOfFrames); 
			}

			NonInterleavedBuffer = null;
			return true;
		}

		private float[][] _nonInterleavedBuffer;
		private float[] _interleavedBuffer;

		public NativeProcessBlockEventArgs (int numberOfFrames, int channelCount) : base (numberOfFrames, channelCount) 
		{
			NativeObject = this;
		}
	}
}

