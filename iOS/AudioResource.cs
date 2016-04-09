using System;
using BeatBoxers.Audio;
using System.IO;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;

namespace BeatBoxers.iOS
{
	public class AudioResource : IAudioResource
	{
		private readonly Dictionary<string, float[]> _valueMap;

		#region IAudioResource implementation

		public float[] GetAudioResource (string resourceFileName)
		{
			if (_valueMap.ContainsKey (resourceFileName)) {
				return _valueMap [resourceFileName];
			}

			var stream = GetEmbeddedResourceStream(Assembly.GetAssembly(typeof(AudioResource)), resourceFileName);

			var retVal = new float[stream.Length / 4];

			using (var reader = new BinaryReader (stream)) {
				for (int i = 0; i < retVal.Length; i++) {
					retVal [i] = reader.ReadSingle ();
				}
			}

			_valueMap [resourceFileName] = retVal;

			return retVal;
		}

		#endregion

		public static Stream GetEmbeddedResourceStream(Assembly assembly, string resourceFileName)
		{
			var resourceNames = assembly.GetManifestResourceNames();

			var resourcePaths = resourceNames
				.Where(x => x.EndsWith(resourceFileName, StringComparison.CurrentCultureIgnoreCase))
				.ToArray();

			if (!resourcePaths.Any())
			{
				throw new Exception(string.Format("Resource ending with {0} not found.", resourceFileName));
			}

			if (resourcePaths.Count() > 1)
			{
				throw new Exception(string.Format("Multiple resources ending with {0} found: {1}{2}", resourceFileName, Environment.NewLine, string.Join(Environment.NewLine, resourcePaths)));
			}

			return assembly.GetManifestResourceStream(resourcePaths.Single());
		}

		public AudioResource ()
		{
			_valueMap = new Dictionary<string, float[]> ();
		}
	}
}

