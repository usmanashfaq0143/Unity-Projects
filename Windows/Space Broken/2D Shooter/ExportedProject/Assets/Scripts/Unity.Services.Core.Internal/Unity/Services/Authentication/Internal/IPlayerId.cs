using System;
using Unity.Services.Core.Internal;

namespace Unity.Services.Authentication.Internal
{
	public interface IPlayerId : IServiceComponent
	{
		string PlayerId { get; }

		event Action<string> PlayerIdChanged;
	}
}
