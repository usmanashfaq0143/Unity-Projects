using System.IO;
using UnityEngine;

namespace Unity.Services.Core.Configuration
{
	internal static class ConfigurationUtils
	{
		public const string ConfigFileName = "UnityServicesProjectConfiguration.json";

		public static string RuntimeConfigFullPath { get; } = Path.Combine(Application.streamingAssetsPath, "UnityServicesProjectConfiguration.json");


		public static IConfigurationLoader ConfigurationLoader { get; internal set; } = new StreamingAssetsConfigurationLoader();

	}
}
