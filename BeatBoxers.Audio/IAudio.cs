using System;

namespace BeatBoxers.Audio
{		
	public interface IAudio
	{
		event EventHandler<ProcessBlockEventArgs> ProcessBlockEventHandler;

		bool IsRunning { get; }
		double SampleRate{ get; }

		void Start();

		void Pause();

		void Stop();
	}
}

