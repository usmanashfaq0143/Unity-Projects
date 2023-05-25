using System;
using System.IO;
using Newtonsoft.Json;
using Unity.Services.Core.Internal;
using UnityEngine;

namespace Unity.Services.Core.Telemetry.Internal
{
	internal abstract class FileCachePersister
	{
		internal static bool IsAvailableFor(RuntimePlatform platform)
		{
			return !string.IsNullOrEmpty(GetPersistentDataPathFor(platform));
		}

		internal static string GetPersistentDataPathFor(RuntimePlatform platform)
		{
			if (platform == RuntimePlatform.Switch)
			{
				return string.Empty;
			}
			return Application.persistentDataPath;
		}
	}
	internal class FileCachePersister<TPayload> : FileCachePersister, ICachePersister<TPayload> where TPayload : ITelemetryPayload
	{
		public string FilePath { get; }

		public bool CanPersist { get; } = FileCachePersister.IsAvailableFor(Application.platform);


		public FileCachePersister(string fileName)
		{
			FilePath = Path.Combine(FileCachePersister.GetPersistentDataPathFor(Application.platform), fileName);
		}

		public void Persist(CachedPayload<TPayload> cache)
		{
			if (!cache.IsEmpty())
			{
				string contents = JsonConvert.SerializeObject(cache);
				File.WriteAllText(FilePath, contents);
			}
		}

		public bool TryFetch(out CachedPayload<TPayload> persistedCache)
		{
			if (!File.Exists(FilePath))
			{
				persistedCache = null;
				return false;
			}
			try
			{
				string value = File.ReadAllText(FilePath);
				persistedCache = JsonConvert.DeserializeObject<CachedPayload<TPayload>>(value);
				return persistedCache != null;
			}
			catch (Exception exception)
			{
				CoreLogger.LogException(exception);
				persistedCache = null;
				return false;
			}
		}

		public void Delete()
		{
			if (File.Exists(FilePath))
			{
				File.Delete(FilePath);
			}
		}
	}
}
