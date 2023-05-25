namespace Unity.Services.Core.Device
{
	internal class UnityAdsIdentifier : IUserIdentifierProvider
	{
		private const string k_AndroidSettingsFile = "unityads-installinfo";

		private const string k_IdfiKey = "unityads-idfi";

		public string UserId
		{
			get
			{
				return null;
			}
			set
			{
			}
		}
	}
}
