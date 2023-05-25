using System;

namespace UnityEngine.U2D
{
	[Serializable]
	internal class PixelPerfectCameraInternal : ISerializationCallbackReceiver
	{
		[NonSerialized]
		private IPixelPerfectCamera m_Component;

		private PixelPerfectCamera m_SerializableComponent;

		internal float originalOrthoSize;

		internal bool hasPostProcessLayer;

		internal bool cropFrameXAndY;

		internal bool cropFrameXOrY;

		internal bool useStretchFill;

		internal int zoom = 1;

		internal bool useOffscreenRT;

		internal int offscreenRTWidth;

		internal int offscreenRTHeight;

		internal Rect pixelRect = Rect.zero;

		internal float orthoSize = 1f;

		internal float unitsPerPixel;

		internal int cinemachineVCamZoom = 1;

		internal PixelPerfectCameraInternal(IPixelPerfectCamera component)
		{
			m_Component = component;
		}

		public void OnBeforeSerialize()
		{
			m_SerializableComponent = m_Component as PixelPerfectCamera;
		}

		public void OnAfterDeserialize()
		{
			if (m_SerializableComponent != null)
			{
				m_Component = m_SerializableComponent;
			}
		}

		internal void CalculateCameraProperties(int screenWidth, int screenHeight)
		{
			int assetsPPU = m_Component.assetsPPU;
			int refResolutionX = m_Component.refResolutionX;
			int refResolutionY = m_Component.refResolutionY;
			bool upscaleRT = m_Component.upscaleRT;
			bool pixelSnapping = m_Component.pixelSnapping;
			bool cropFrameX = m_Component.cropFrameX;
			bool cropFrameY = m_Component.cropFrameY;
			bool stretchFill = m_Component.stretchFill;
			cropFrameXAndY = cropFrameY && cropFrameX;
			cropFrameXOrY = cropFrameY || cropFrameX;
			useStretchFill = cropFrameXAndY && stretchFill;
			int val = screenHeight / refResolutionY;
			int val2 = screenWidth / refResolutionX;
			zoom = Math.Max(1, Math.Min(val, val2));
			useOffscreenRT = false;
			offscreenRTWidth = 0;
			offscreenRTHeight = 0;
			if (cropFrameXOrY)
			{
				if (!upscaleRT)
				{
					if (useStretchFill)
					{
						useOffscreenRT = true;
						offscreenRTWidth = zoom * refResolutionX;
						offscreenRTHeight = zoom * refResolutionY;
					}
				}
				else
				{
					useOffscreenRT = true;
					if (cropFrameXAndY)
					{
						offscreenRTWidth = refResolutionX;
						offscreenRTHeight = refResolutionY;
					}
					else if (cropFrameY)
					{
						offscreenRTWidth = screenWidth / zoom / 2 * 2;
						offscreenRTHeight = refResolutionY;
					}
					else
					{
						offscreenRTWidth = refResolutionX;
						offscreenRTHeight = screenHeight / zoom / 2 * 2;
					}
				}
			}
			else if (upscaleRT && zoom > 1)
			{
				useOffscreenRT = true;
				offscreenRTWidth = screenWidth / zoom / 2 * 2;
				offscreenRTHeight = screenHeight / zoom / 2 * 2;
			}
			pixelRect = Rect.zero;
			if (cropFrameXOrY && !upscaleRT && !useStretchFill)
			{
				if (cropFrameXAndY)
				{
					pixelRect.width = zoom * refResolutionX;
					pixelRect.height = zoom * refResolutionY;
				}
				else if (cropFrameY)
				{
					pixelRect.width = screenWidth;
					pixelRect.height = zoom * refResolutionY;
				}
				else
				{
					pixelRect.width = zoom * refResolutionX;
					pixelRect.height = screenHeight;
				}
				pixelRect.x = (screenWidth - (int)pixelRect.width) / 2;
				pixelRect.y = (screenHeight - (int)pixelRect.height) / 2;
			}
			else if (useOffscreenRT)
			{
				pixelRect = new Rect(0f, 0f, offscreenRTWidth, offscreenRTHeight);
			}
			if (cropFrameY)
			{
				orthoSize = (float)refResolutionY * 0.5f / (float)assetsPPU;
			}
			else if (cropFrameX)
			{
				float num = ((pixelRect == Rect.zero) ? ((float)screenWidth / (float)screenHeight) : (pixelRect.width / pixelRect.height));
				orthoSize = (float)refResolutionX / num * 0.5f / (float)assetsPPU;
			}
			else if (upscaleRT && zoom > 1)
			{
				orthoSize = (float)offscreenRTHeight * 0.5f / (float)assetsPPU;
			}
			else
			{
				float num2 = ((pixelRect == Rect.zero) ? ((float)screenHeight) : pixelRect.height);
				orthoSize = num2 * 0.5f / (float)(zoom * assetsPPU);
			}
			if (upscaleRT || (!upscaleRT && pixelSnapping))
			{
				unitsPerPixel = 1f / (float)assetsPPU;
			}
			else
			{
				unitsPerPixel = 1f / (float)(zoom * assetsPPU);
			}
		}

		internal Rect CalculatePostRenderPixelRect(float cameraAspect, int screenWidth, int screenHeight)
		{
			Rect result = default(Rect);
			if (useStretchFill)
			{
				if ((float)screenWidth / (float)screenHeight > cameraAspect)
				{
					result.height = screenHeight;
					result.width = (float)screenHeight * cameraAspect;
					result.x = (screenWidth - (int)result.width) / 2;
					result.y = 0f;
				}
				else
				{
					result.width = screenWidth;
					result.height = (float)screenWidth / cameraAspect;
					result.y = (screenHeight - (int)result.height) / 2;
					result.x = 0f;
				}
			}
			else
			{
				result.height = zoom * offscreenRTHeight;
				result.width = zoom * offscreenRTWidth;
				result.x = (screenWidth - (int)result.width) / 2;
				result.y = (screenHeight - (int)result.height) / 2;
			}
			return result;
		}

		internal float CorrectCinemachineOrthoSize(float targetOrthoSize)
		{
			float result;
			if (m_Component.upscaleRT)
			{
				cinemachineVCamZoom = Math.Max(1, Mathf.RoundToInt(orthoSize / targetOrthoSize));
				result = orthoSize / (float)cinemachineVCamZoom;
			}
			else
			{
				cinemachineVCamZoom = Math.Max(1, Mathf.RoundToInt((float)zoom * orthoSize / targetOrthoSize));
				result = (float)zoom * orthoSize / (float)cinemachineVCamZoom;
			}
			if (!m_Component.upscaleRT && !m_Component.pixelSnapping)
			{
				unitsPerPixel = 1f / (float)(cinemachineVCamZoom * m_Component.assetsPPU);
			}
			return result;
		}
	}
}
