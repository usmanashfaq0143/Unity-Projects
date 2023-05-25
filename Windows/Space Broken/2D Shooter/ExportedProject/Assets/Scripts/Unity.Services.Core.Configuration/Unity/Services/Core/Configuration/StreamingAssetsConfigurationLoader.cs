using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Unity.Services.Core.Configuration
{
	internal class StreamingAssetsConfigurationLoader : IConfigurationLoader
	{
		public async Task<SerializableProjectConfiguration> GetConfigAsync()
		{
			return JsonConvert.DeserializeObject<SerializableProjectConfiguration>(await StreamingAssetsUtils.GetFileTextFromStreamingAssetsAsync("UnityServicesProjectConfiguration.json"));
		}
	}
}
