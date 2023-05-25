using System;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Unity.Services.Core.Scheduler.Internal;
using UnityEngine.Networking;

namespace Unity.Services.Core.Telemetry.Internal
{
	internal class TelemetrySender
	{
		private readonly ExponentialBackOffRetryPolicy m_RetryPolicy;

		private readonly IActionScheduler m_Scheduler;

		private readonly IUnityWebRequestSender m_RequestSender;

		public string TargetUrl { get; }

		public TelemetrySender([NotNull] string targetUrl, [NotNull] string servicePath, [NotNull] IActionScheduler scheduler, [NotNull] ExponentialBackOffRetryPolicy retryPolicy, [NotNull] IUnityWebRequestSender requestSender)
		{
			TargetUrl = targetUrl + "/" + servicePath;
			m_RetryPolicy = retryPolicy;
			m_Scheduler = scheduler;
			m_RequestSender = requestSender;
		}

		public Task SendAsync<TPayload>(TPayload payload) where TPayload : ITelemetryPayload
		{
			TaskCompletionSource<object> completionSource = new TaskCompletionSource<object>();
			int sendCount = 0;
			byte[] serializedPayload;
			try
			{
				serializedPayload = SerializePayload(payload);
				SendWebRequest();
			}
			catch (Exception exception)
			{
				completionSource.TrySetException(exception);
			}
			return completionSource.Task;
			void OnRequestCompleted(WebRequest webRequest)
			{
				if (webRequest.IsSuccess)
				{
					completionSource.SetResult(null);
				}
				else if (m_RetryPolicy.CanRetry(webRequest, sendCount))
				{
					float delayBeforeSendingSeconds = m_RetryPolicy.GetDelayBeforeSendingSeconds(sendCount);
					m_Scheduler.ScheduleAction(SendWebRequest, delayBeforeSendingSeconds);
				}
				else
				{
					string message = "Error: " + webRequest.ErrorMessage + "\nBody: " + webRequest.ErrorBody;
					completionSource.TrySetException(new Exception(message));
				}
			}
			void SendWebRequest()
			{
				UnityWebRequest request = CreateRequest(serializedPayload);
				sendCount++;
				m_RequestSender.SendRequest(request, OnRequestCompleted);
			}
		}

		internal static byte[] SerializePayload<TPayload>(TPayload payload) where TPayload : ITelemetryPayload
		{
			string s = JsonConvert.SerializeObject(payload);
			return Encoding.UTF8.GetBytes(s);
		}

		internal UnityWebRequest CreateRequest(byte[] serializedPayload)
		{
			UnityWebRequest unityWebRequest = new UnityWebRequest(TargetUrl, "POST");
			unityWebRequest.uploadHandler = new UploadHandlerRaw(serializedPayload)
			{
				contentType = "application/json"
			};
			unityWebRequest.downloadHandler = new DownloadHandlerBuffer();
			unityWebRequest.SetRequestHeader("Content-Type", "application/json");
			return unityWebRequest;
		}
	}
}
