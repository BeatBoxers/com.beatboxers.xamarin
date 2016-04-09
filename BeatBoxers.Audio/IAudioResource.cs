using System;

namespace BeatBoxers.Audio
{
	public interface IAudioResource
	{
		float[] GetAudioResource(string name);
	}
}

