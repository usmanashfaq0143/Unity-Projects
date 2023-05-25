using System;

namespace Unity.Services.Core.Telemetry.Internal
{
	internal static class CacheExtensions
	{
		public static bool IsEmpty<TPayload>(this CachedPayload<TPayload> self) where TPayload : ITelemetryPayload
		{
			return (self.Payload?.Count ?? 0) <= 0;
		}

		public static void AddRangeFrom(this CachedPayload<DiagnosticsPayload> self, CachedPayload<DiagnosticsPayload> payload)
		{
			bool num = payload.Payload.Diagnostics.Count > 0;
			if (num)
			{
				self.Payload.Diagnostics.AddRange(payload.Payload.Diagnostics);
			}
			if (num && self.TimeOfOccurenceTicks <= 0)
			{
				self.TimeOfOccurenceTicks = payload.TimeOfOccurenceTicks;
			}
		}

		public static void Add<TPayload>(this CachedPayload<TPayload> self, ITelemetryEvent telemetryEvent) where TPayload : ITelemetryPayload
		{
			if (self.TimeOfOccurenceTicks == 0L)
			{
				self.TimeOfOccurenceTicks = DateTime.UtcNow.Ticks;
			}
			self.Payload.Add(telemetryEvent);
		}
	}
}
