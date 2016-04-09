using System;
using BeatBoxers.Communication;
using System.Collections.Generic;

namespace BeatBoxers.Audio
{
	internal class SampleBeat
	{
		public string Name { get; set; }

		public long SamplePos { get; set; }

		public DateTime Time { get; set; }

		public float[] Audio { get; set; }

		public int PlayPos { get; set; }

		public float Intensity { get; set; }

		public SampleBeat (IAudioResource audioResource, Beat beat, Dictionary<string, string> beatMap)
		{
			Name = beat.Name;
			Time = beat.Time;
			Intensity = (float)beat.Intensity; 
			Audio = audioResource.GetAudioResource (beatMap[beat.Name]);
		}
	}
}

