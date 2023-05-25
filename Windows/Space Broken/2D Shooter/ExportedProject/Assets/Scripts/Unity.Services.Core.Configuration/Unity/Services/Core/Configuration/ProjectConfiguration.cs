using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Newtonsoft.Json;
using Unity.Services.Core.Configuration.Internal;
using Unity.Services.Core.Internal;

namespace Unity.Services.Core.Configuration
{
	internal class ProjectConfiguration : IProjectConfiguration, IServiceComponent
	{
		private string m_JsonCache;

		private readonly IReadOnlyDictionary<string, ConfigurationEntry> m_ConfigValues;

		public ProjectConfiguration(IReadOnlyDictionary<string, ConfigurationEntry> configValues)
		{
			m_ConfigValues = configValues;
		}

		public bool GetBool(string key, bool defaultValue = false)
		{
			if (bool.TryParse(GetString(key), out var result))
			{
				return result;
			}
			return defaultValue;
		}

		public int GetInt(string key, int defaultValue = 0)
		{
			if (int.TryParse(GetString(key), out var result))
			{
				return result;
			}
			return defaultValue;
		}

		public float GetFloat(string key, float defaultValue = 0f)
		{
			if (float.TryParse(GetString(key), NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
			{
				return result;
			}
			return defaultValue;
		}

		public string GetString(string key, string defaultValue = null)
		{
			if (m_ConfigValues.TryGetValue(key, out var value))
			{
				return value.Value;
			}
			return defaultValue;
		}

		public string ToJson()
		{
			if (m_JsonCache == null)
			{
				Dictionary<string, string> value = m_ConfigValues.ToDictionary((KeyValuePair<string, ConfigurationEntry> pair) => pair.Key, (KeyValuePair<string, ConfigurationEntry> pair) => pair.Value.Value);
				m_JsonCache = JsonConvert.SerializeObject(value);
			}
			return m_JsonCache;
		}
	}
}
