using Unity.Services.Core.Internal;

namespace Unity.Services.Wire.Internal
{
	public interface IWire : IServiceComponent
	{
		IChannel CreateChannel(IChannelTokenProvider tokenProvider);
	}
}
