using System;

namespace BeatBoxers.Audio
{
	public class ProcessBlockEventArgs : EventArgs{
		public int NumberOfFrames {
			get;
			private set;
		}

		public int ChannelCount {
			get;
			private set;
		}

		public float[] InterleavedBuffer {
			get;
			protected set;
		}

		public float[][] NonInterleavedBuffer {
			get;
			protected set;
		}

		public object NativeObject {
			get;
			protected set;
		}

		public virtual bool GetInterleavedBuffer()
		{
			return false;
		}

		public virtual bool ReturnInterleavedBuffer()
		{
			return false;
		}

		public virtual bool GetNonInterleavedBuffer()
		{
			return false;
		}

		public virtual bool ReturnNonInterleavedBuffer()
		{
			return false;
		}

		public ProcessBlockEventArgs(int numberOfFrames, int channelCount) {
			ChannelCount = channelCount;
			NumberOfFrames = numberOfFrames;
		}
	}
}

