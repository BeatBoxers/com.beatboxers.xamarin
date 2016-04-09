using System;
using BeatBoxers.Communication;
using System.Collections.Generic;
using System.Linq;

namespace BeatBoxers.Audio
{
	public class AudioRenderer
	{
		private readonly IAudio _audio;
		private readonly IAudioResource _audioResource;

		private readonly LinkedList<SampleBeat> _beatList;

		private long _samplePos;
		private DateTime _lastProcessTime;

		private readonly Dictionary<string, DateTime> _lastBeatTime;

		public readonly Dictionary<string, string> BeatMap;

		public void PushBeat(Beat beat)
		{
			var sampleBeat = new SampleBeat (_audioResource, beat, BeatMap);

			if (!_lastBeatTime.ContainsKey (beat.Name) || (beat.Time - _lastBeatTime [beat.Name]).TotalMilliseconds > 100) {
				_lastBeatTime [beat.Name] = beat.Time;

				lock (this) {
					sampleBeat.SamplePos = _samplePos;
					_beatList.AddLast (sampleBeat);
				}
			}
		}

		public AudioRenderer (IAudio audio, IAudioResource audioResource)
		{
			_audio = audio;
			_audioResource = audioResource;

			_beatList = new LinkedList<SampleBeat> ();
			_lastBeatTime = new Dictionary<string, DateTime> ();

			BeatMap = new Dictionary<string, string> () {
				{"foot", "psycho_boomer.raw"},
				{"pad1", "drum.raw"},
				{"pad2", "hi_hat.raw"},
				{"pad3", "hi_hat_2.raw"},
				{"pad4", "kick.raw"},
				{"pad5", "plunk.raw"},
				{"pad6", "snare.raw"},
				{"pad7", "stick.raw"},
				{"pad8", "inception.raw"},
			};

			audio.ProcessBlockEventHandler += ProcessBlock;
			audio.Start ();
		}

		private void ProcessBlock(object sender, ProcessBlockEventArgs e)
		{
			var dateTime = DateTime.Now;

			List<SampleBeat> sampleBeats;
			List<SampleBeat> consumedBeats = new List<SampleBeat> ();
			long samplePos;

			lock (this) {
				sampleBeats = _beatList.ToList ();
				samplePos = _samplePos;
			}
				
			if (e.GetNonInterleavedBuffer ()) {
				foreach (var channel in e.NonInterleavedBuffer) {
					for (int i = 0; i < channel.Length; i++) {
						channel [i] = 0.0f;
					}
				}

				foreach (var beat in sampleBeats) {
					if (beat.PlayPos >= beat.Audio.Length || // beat ended
						beat.SamplePos >= samplePos + e.NumberOfFrames || // beat in future
						beat.SamplePos + beat.Audio.Length < samplePos // beat is history
						)
					{
						consumedBeats.Add (beat);
						continue;
					}

					int framesToCopy = Math.Min (e.NumberOfFrames, beat.Audio.Length - beat.PlayPos);

					foreach (var channel in e.NonInterleavedBuffer) {
						for (int i = 0; i < framesToCopy; i++) {
							channel [i] = channel [i] + beat.Intensity * beat.Audio [beat.PlayPos + i];
						}
					}

					beat.PlayPos += framesToCopy;
				}

			}

			lock (this) {
				_samplePos += e.NumberOfFrames;
				_lastProcessTime = dateTime;
				foreach (var beat in consumedBeats) {
					_beatList.Remove (beat);
				}
			}
		}
	}
}

