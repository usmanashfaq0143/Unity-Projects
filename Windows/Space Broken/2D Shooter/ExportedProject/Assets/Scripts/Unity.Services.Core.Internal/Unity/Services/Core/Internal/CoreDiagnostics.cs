using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Core.Telemetry.Internal;

namespace Unity.Services.Core.Internal
{
	internal class CoreDiagnostics
	{
		internal const string CorePackageName = "com.unity.services.core";

		internal const string CircularDependencyDiagnosticName = "circular_dependency";

		internal const string CorePackageInitDiagnosticName = "core_package_init";

		internal const string OperateServicesInitDiagnosticName = "operate_services_init";

		internal const string ProjectConfigTagName = "project_config";

		public IDictionary<string, string> CoreTags { get; internal set; }

		public static CoreDiagnostics Instance { get; internal set; }

		internal IDiagnosticsComponentProvider DiagnosticsComponentProvider { get; set; }

		internal IDiagnostics Diagnostics { get; set; }

		private async Task<IDiagnostics> GetOrCreateDiagnostics()
		{
			if (Diagnostics == null)
			{
				Diagnostics = (await DiagnosticsComponentProvider.CreateDiagnosticsComponents()).Create("com.unity.services.core");
				SetProjectConfiguration(await DiagnosticsComponentProvider.GetSerializedProjectConfigurationAsync());
			}
			return Diagnostics;
		}

		public void SetProjectConfiguration(string serializedProjectConfig)
		{
			CoreTags = new Dictionary<string, string>();
			CoreTags["project_config"] = serializedProjectConfig;
		}

		public void SendCircularDependencyDiagnostics(Exception exception)
		{
			SendCoreDiagnostics("circular_dependency", exception).ContinueWith(OnSendFailed, TaskContinuationOptions.OnlyOnFaulted);
		}

		public void SendCorePackageInitDiagnostics(Exception exception)
		{
			SendCoreDiagnostics("core_package_init", exception).ContinueWith(OnSendFailed, TaskContinuationOptions.OnlyOnFaulted);
		}

		public void SendOperateServicesInitDiagnostics(Exception exception)
		{
			SendCoreDiagnostics("operate_services_init", exception).ContinueWith(OnSendFailed, TaskContinuationOptions.OnlyOnFaulted);
		}

		private static void OnSendFailed(Task failedSendTask)
		{
			CoreLogger.LogException(failedSendTask.Exception);
		}

		private async Task SendCoreDiagnostics(string diagnosticName, Exception exception)
		{
			(await GetOrCreateDiagnostics()).SendDiagnostic(diagnosticName, exception.ToString(), CoreTags);
		}
	}
}
