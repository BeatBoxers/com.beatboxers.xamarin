using System;

namespace BeatBoxers.Communication
{
	public class Beat
	{
		public DateTime Time { get; private set; }

		public string Name { get; private set; }

		public double Intensity { get; private set; }

		public Beat (string name, double intensity, DateTime time)
		{
			Name = name;
			Intensity = intensity;
			Time = time;
		}
	}
}

