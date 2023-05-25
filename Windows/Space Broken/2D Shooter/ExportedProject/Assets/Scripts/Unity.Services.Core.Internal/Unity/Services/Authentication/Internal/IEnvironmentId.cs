using Unity.Services.Core.Internal;

namespace Unity.Services.Authentication.Internal
{
	public interface IEnvironmentId : IServiceComponent
	{
		string EnvironmentId { get; }
	}
}
