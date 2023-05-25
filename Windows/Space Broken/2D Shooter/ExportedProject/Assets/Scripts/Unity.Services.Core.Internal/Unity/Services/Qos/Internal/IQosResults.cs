using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Core.Internal;

namespace Unity.Services.Qos.Internal
{
	public interface IQosResults : IServiceComponent
	{
		Task<IList<QosResult>> GetSortedQosResultsAsync(string service, IList<string> regions);
	}
}
