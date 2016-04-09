using System;

namespace BeatBoxers.Communication
{
	public class BeatEventArgs : EventArgs
	{
		public Beat Beat { get; private set; }

		public BeatEventArgs (Beat beat)
		{
			Beat = beat;
		}
	}
}

