using Unity.Services.Core.Internal;

namespace Unity.Services.Authentication.Internal
{
	public interface IAccessToken : IServiceComponent
	{
		string AccessToken { get; }
	}
}
