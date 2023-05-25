using Unity.Services.Core.Telemetry.Internal;

namespace Unity.Services.Core.Internal
{
	internal class CoreMetrics
	{
		internal const string CorePackageInitTimeMetricName = "package_init_time";

		internal const string AllPackagesInitSuccessMetricName = "all_packages_init_success";

		internal const string AllPackagesInitTimeMetricName = "all_packages_init_time";

		public static CoreMetrics Instance { get; internal set; }

		internal IMetrics Metrics { get; set; }

		public void SendAllPackagesInitSuccessMetric()
		{
			Metrics.SendSumMetric("all_packages_init_success");
		}

		public void SendAllPackagesInitTimeMetric(double initTimeSeconds)
		{
			Metrics.SendHistogramMetric("all_packages_init_time", initTimeSeconds);
		}

		public void SendCorePackageInitTimeMetric(double initTimeSeconds)
		{
			Metrics.SendHistogramMetric("package_init_time", initTimeSeconds);
		}
	}
}
