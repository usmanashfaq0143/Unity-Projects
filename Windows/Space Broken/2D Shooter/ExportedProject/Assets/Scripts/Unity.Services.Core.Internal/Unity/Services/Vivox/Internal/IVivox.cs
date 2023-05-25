using Unity.Services.Core.Internal;

namespace Unity.Services.Vivox.Internal
{
	public interface IVivox : IServiceComponent
	{
		void RegisterTokenProvider(IVivoxTokenProviderInternal tokenProvider);
	}
}
