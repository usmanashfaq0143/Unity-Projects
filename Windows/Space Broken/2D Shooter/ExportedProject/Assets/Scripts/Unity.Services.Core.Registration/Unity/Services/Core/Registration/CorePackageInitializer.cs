using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Unity.Services.Core.Configuration;
using Unity.Services.Core.Configuration.Internal;
using Unity.Services.Core.Device;
using Unity.Services.Core.Device.Internal;
using Unity.Services.Core.Environments.Internal;
using Unity.Services.Core.Internal;
using Unity.Services.Core.Scheduler.Internal;
using Unity.Services.Core.Telemetry.Internal;
using Unity.Services.Core.Threading.Internal;
using UnityEngine;

namespace Unity.Services.Core.Registration
{
	internal class CorePackageInitializer : IInitializablePackage, IDiagnosticsComponentProvider
	{
		internal const string CorePackageName = "com.unity.services.core";

		private InitializationOptions m_CurrentInitializationOptions;

		internal ActionScheduler ActionScheduler { get; private set; }

		internal InstallationId InstallationId { get; private set; }

		internal ProjectConfiguration ProjectConfig { get; private set; }

		internal Unity.Services.Core.Environments.Internal.Environments Environments { get; private set; }

		internal CloudProjectId CloudProjectId { get; private set; }

		internal IDiagnosticsFactory DiagnosticsFactory { get; private set; }

		internal IMetricsFactory MetricsFactory { get; private set; }

		internal UnityThreadUtilsInternal UnityThreadUtils { get; private set; }

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		private static void Register()
		{
			CorePackageInitializer corePackageInitializer = new CorePackageInitializer();
			CoreDiagnostics.Instance.DiagnosticsComponentProvider = corePackageInitializer;
			CoreRegistry.Instance.RegisterPackage(corePackageInitializer).ProvidesComponent<IInstallationId>().ProvidesComponent<ICloudProjectId>()
				.ProvidesComponent<IActionScheduler>()
				.ProvidesComponent<IEnvironments>()
				.ProvidesComponent<IProjectConfiguration>()
				.ProvidesComponent<IMetricsFactory>()
				.ProvidesComponent<IDiagnosticsFactory>()
				.ProvidesComponent<IUnityThreadUtils>();
		}

		public async Task Initialize(CoreRegistry registry)
		{
			Stopwatch stopwatch = new Stopwatch();
			stopwatch.Start();
			try
			{
				if (HaveInitOptionsChanged())
				{
					FreeOptionsDependantComponents();
				}
				InitializeInstallationId();
				InitializeActionScheduler();
				await InitializeProjectConfigAsync(UnityServices.Instance.Options);
				InitializeEnvironments(ProjectConfig);
				InitializeCloudProjectId();
				if (string.IsNullOrEmpty(CloudProjectId.GetCloudProjectId()))
				{
					UnityProjectNotLinkedException ex = new UnityProjectNotLinkedException("To use Unity's dashboard services, you need to link your Unity project to a project ID. To do this, go to Project Settings to select your organization, select your project and then link a project ID. You also need to make sure your organization has access to the required products. Visit https://dashboard.unity3d.com to sign up. This will throw an exception in future release.");
					CoreDiagnostics.Instance.SendCorePackageInitDiagnostics(ex);
					CoreLogger.LogError(ex.Message);
				}
				InitializeDiagnostics(ActionScheduler, ProjectConfig, CloudProjectId, Environments);
				CoreDiagnostics.Instance.Diagnostics = DiagnosticsFactory.Create("com.unity.services.core");
				CoreDiagnostics.Instance.SetProjectConfiguration(ProjectConfig.ToJson());
				InitializeMetrics(ActionScheduler, ProjectConfig, CloudProjectId, Environments);
				CoreMetrics.Instance.Metrics = MetricsFactory.Create("com.unity.services.core");
				InitializeUnityThreadUtils();
				RegisterProvidedComponents();
			}
			catch (Exception exception)
			{
				CoreDiagnostics.Instance.SendCorePackageInitDiagnostics(exception);
				throw;
			}
			stopwatch.Stop();
			CoreMetrics.Instance.SendCorePackageInitTimeMetric(stopwatch.Elapsed.TotalSeconds);
			void RegisterProvidedComponents()
			{
				registry.RegisterServiceComponent((IInstallationId)InstallationId);
				registry.RegisterServiceComponent((IActionScheduler)ActionScheduler);
				registry.RegisterServiceComponent((IProjectConfiguration)ProjectConfig);
				registry.RegisterServiceComponent((IEnvironments)Environments);
				registry.RegisterServiceComponent((ICloudProjectId)CloudProjectId);
				registry.RegisterServiceComponent(DiagnosticsFactory);
				registry.RegisterServiceComponent(MetricsFactory);
				registry.RegisterServiceComponent((IUnityThreadUtils)UnityThreadUtils);
			}
		}

		private bool HaveInitOptionsChanged()
		{
			if (m_CurrentInitializationOptions != null)
			{
				return !m_CurrentInitializationOptions.Values.ValueEquals(UnityServices.Instance.Options.Values);
			}
			return false;
		}

		private void FreeOptionsDependantComponents()
		{
			ProjectConfig = null;
			Environments = null;
			DiagnosticsFactory = null;
			MetricsFactory = null;
		}

		internal void InitializeInstallationId()
		{
			if (InstallationId == null)
			{
				InstallationId installationId = new InstallationId();
				installationId.CreateIdentifier();
				InstallationId = installationId;
			}
		}

		internal void InitializeActionScheduler()
		{
			if (ActionScheduler == null)
			{
				ActionScheduler actionScheduler = new ActionScheduler();
				actionScheduler.JoinPlayerLoopSystem();
				ActionScheduler = actionScheduler;
			}
		}

		internal async Task InitializeProjectConfigAsync([NotNull] InitializationOptions options)
		{
			if (ProjectConfig == null)
			{
				ProjectConfig = await GenerateProjectConfigurationAsync(options);
				m_CurrentInitializationOptions = new InitializationOptions(options);
			}
		}

		internal static async Task<ProjectConfiguration> GenerateProjectConfigurationAsync([NotNull] InitializationOptions options)
		{
			SerializableProjectConfiguration config = await GetSerializedConfigOrEmptyAsync();
			if (config.Keys == null || config.Values == null)
			{
				config = SerializableProjectConfiguration.Empty;
			}
			Dictionary<string, ConfigurationEntry> dictionary = new Dictionary<string, ConfigurationEntry>(config.Keys.Length);
			dictionary.FillWith(config);
			dictionary.FillWith(options);
			return new ProjectConfiguration(dictionary);
		}

		internal static async Task<SerializableProjectConfiguration> GetSerializedConfigOrEmptyAsync()
		{
			try
			{
				return await ConfigurationUtils.ConfigurationLoader.GetConfigAsync();
			}
			catch (Exception ex)
			{
				CoreLogger.LogError("En error occured while trying to get the project configuration for services.\n" + ex.Message + "\n" + ex.StackTrace);
				return SerializableProjectConfiguration.Empty;
			}
		}

		internal void InitializeEnvironments(IProjectConfiguration projectConfiguration)
		{
			if (Environments == null)
			{
				string @string = projectConfiguration.GetString("com.unity.services.core.environment-name", "production");
				Environments = new Unity.Services.Core.Environments.Internal.Environments
				{
					Current = @string
				};
			}
		}

		internal void InitializeCloudProjectId()
		{
			if (CloudProjectId == null)
			{
				CloudProjectId = new CloudProjectId();
			}
		}

		internal void InitializeDiagnostics(IActionScheduler scheduler, IProjectConfiguration projectConfiguration, ICloudProjectId cloudProjectId, IEnvironments environments)
		{
			if (DiagnosticsFactory == null)
			{
				DiagnosticsFactory = TelemetryUtils.CreateDiagnosticsFactory(scheduler, projectConfiguration, cloudProjectId, environments);
			}
		}

		internal void InitializeMetrics(IActionScheduler scheduler, IProjectConfiguration projectConfiguration, ICloudProjectId cloudProjectId, IEnvironments environments)
		{
			if (MetricsFactory == null)
			{
				MetricsFactory = TelemetryUtils.CreateMetricsFactory(scheduler, projectConfiguration, cloudProjectId, environments);
			}
		}

		internal void InitializeUnityThreadUtils()
		{
			if (UnityThreadUtils == null)
			{
				UnityThreadUtils = new UnityThreadUtilsInternal();
			}
		}

		public async Task<IDiagnosticsFactory> CreateDiagnosticsComponents()
		{
			if (HaveInitOptionsChanged())
			{
				FreeOptionsDependantComponents();
			}
			InitializeActionScheduler();
			await InitializeProjectConfigAsync(UnityServices.Instance.Options);
			InitializeEnvironments(ProjectConfig);
			InitializeCloudProjectId();
			InitializeDiagnostics(ActionScheduler, ProjectConfig, CloudProjectId, Environments);
			return DiagnosticsFactory;
		}

		[Conditional("ENABLE_UNITY_SERVICES_CORE_VERBOSE_LOGGING")]
		private void LogInitializationInfoJson()
		{
			JObject jObject = new JObject();
			JObject jObject2 = JObject.Parse(JsonConvert.SerializeObject(DiagnosticsFactory.CommonTags));
			JObject value = JObject.Parse(ProjectConfig.ToJson());
			JObject content = JObject.Parse("{\"installation_id\": \"" + InstallationId.Identifier + "\"}");
			jObject2.Merge(content);
			jObject.Add("CommonSettings", jObject2);
			jObject.Add("ServicesRuntimeSettings", value);
		}

		public async Task<string> GetSerializedProjectConfigurationAsync()
		{
			await InitializeProjectConfigAsync(UnityServices.Instance.Options);
			return ProjectConfig.ToJson();
		}
	}
}
