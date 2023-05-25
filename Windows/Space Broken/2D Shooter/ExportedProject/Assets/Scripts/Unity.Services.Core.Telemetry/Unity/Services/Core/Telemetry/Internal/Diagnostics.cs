using System.Collections.Generic;
using Unity.Services.Core.Internal;

namespace Unity.Services.Core.Telemetry.Internal
{
	internal class Diagnostics : IDiagnostics
	{
		internal DiagnosticsHandler Handler { get; }

		internal IDictionary<string, string> PackageTags { get; }

		public Diagnostics(DiagnosticsHandler handler, IDictionary<string, string> packageTags)
		{
			Handler = handler;
			PackageTags = packageTags;
		}

		public void SendDiagnostic(string name, string message, IDictionary<string, string> tags = null)
		{
			Diagnostic diagnostic = default(Diagnostic);
			diagnostic.Content = ((tags == null) ? new Dictionary<string, string>(PackageTags) : new Dictionary<string, string>(tags).MergeAllowOverride(PackageTags));
			Diagnostic telemetryEvent = diagnostic;
			telemetryEvent.Content.Add("name", name);
			telemetryEvent.Content.Add("message", message);
			Handler.Register(telemetryEvent);
		}
	}
}
