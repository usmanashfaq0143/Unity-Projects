using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Profiling;
using UnityEngine.Rendering;

namespace UnityEngine.U2D
{
	[ExecuteInEditMode]
	[RequireComponent(typeof(SpriteShapeRenderer))]
	[DisallowMultipleComponent]
	[HelpURL("https://docs.unity3d.com/Packages/com.unity.2d.spriteshape@latest/index.html?subfolder=/manual/SSController.html")]
	public class SpriteShapeController : MonoBehaviour
	{
		private const float s_DistanceTolerance = 0.001f;

		private SpriteShape m_ActiveSpriteShape;

		private EdgeCollider2D m_EdgeCollider2D;

		private PolygonCollider2D m_PolygonCollider2D;

		private SpriteShapeRenderer m_SpriteShapeRenderer;

		private SpriteShapeGeometryCache m_SpriteShapeGeometryCache;

		private Sprite[] m_SpriteArray = new Sprite[0];

		private Sprite[] m_EdgeSpriteArray = new Sprite[0];

		private Sprite[] m_CornerSpriteArray = new Sprite[0];

		private AngleRangeInfo[] m_AngleRangeInfoArray = new AngleRangeInfo[0];

		private NativeArray<float2> m_ColliderData;

		private NativeArray<Vector4> m_TangentData;

		private NativeArray<SpriteShapeGeneratorStats> m_Statistics;

		private bool m_DynamicOcclusionLocal;

		private bool m_DynamicOcclusionOverriden;

		private int m_ActiveSplineHash;

		private int m_ActiveSpriteShapeHash;

		private int m_MaxArrayCount;

		private JobHandle m_JobHandle;

		private SpriteShapeParameters m_ActiveShapeParameters;

		[SerializeField]
		private Spline m_Spline = new Spline();

		[SerializeField]
		private SpriteShape m_SpriteShape;

		[SerializeField]
		private float m_FillPixelPerUnit = 100f;

		[SerializeField]
		private float m_StretchTiling = 1f;

		[SerializeField]
		private int m_SplineDetail;

		[SerializeField]
		private bool m_AdaptiveUV;

		[SerializeField]
		private bool m_StretchUV;

		[SerializeField]
		private bool m_WorldSpaceUV;

		[SerializeField]
		private float m_CornerAngleThreshold = 30f;

		[SerializeField]
		private int m_ColliderDetail;

		[SerializeField]
		[Range(-0.5f, 0.5f)]
		private float m_ColliderOffset;

		[SerializeField]
		private bool m_UpdateCollider = true;

		[SerializeField]
		private bool m_OptimizeCollider = true;

		[SerializeField]
		private bool m_OptimizeGeometry = true;

		[SerializeField]
		private bool m_EnableTangents;

		[SerializeField]
		[HideInInspector]
		private bool m_GeometryCached;

		[SerializeField]
		private bool m_UTess2D;

		[SerializeField]
		private SpriteShapeGeometryCreator m_Creator;

		[SerializeField]
		private List<SpriteShapeGeometryModifier> m_Modifiers = new List<SpriteShapeGeometryModifier>();

		internal static readonly ProfilerMarker generateGeometry = new ProfilerMarker("SpriteShape.GenerateGeometry");

		internal static readonly ProfilerMarker generateCollider = new ProfilerMarker("SpriteShape.GenerateCollider");

		internal int maxArrayCount
		{
			get
			{
				return m_MaxArrayCount;
			}
			set
			{
				m_MaxArrayCount = value;
			}
		}

		internal bool geometryCached
		{
			get
			{
				return m_GeometryCached;
			}
			set
			{
				m_GeometryCached = value;
			}
		}

		internal int splineHashCode => m_ActiveSplineHash;

		internal Sprite[] spriteArray => m_SpriteArray;

		internal SpriteShapeParameters spriteShapeParameters => m_ActiveShapeParameters;

		internal SpriteShapeGeometryCache spriteShapeGeometryCache
		{
			get
			{
				if (!m_SpriteShapeGeometryCache)
				{
					m_SpriteShapeGeometryCache = GetComponent<SpriteShapeGeometryCache>();
				}
				return m_SpriteShapeGeometryCache;
			}
		}

		internal bool optimizeColliderInternal
		{
			set
			{
				m_OptimizeCollider = value;
			}
		}

		internal Sprite[] cornerSpriteArray => m_CornerSpriteArray;

		internal Sprite[] edgeSpriteArray => m_EdgeSpriteArray;

		public AngleRangeInfo[] angleRangeInfoArray => m_AngleRangeInfoArray;

		public SpriteShapeGeometryCreator spriteShapeCreator
		{
			get
			{
				if (m_Creator == null)
				{
					m_Creator = SpriteShapeDefaultCreator.defaultInstance;
				}
				return m_Creator;
			}
			set
			{
				if (value != null)
				{
					m_Creator = value;
				}
			}
		}

		public List<SpriteShapeGeometryModifier> modifiers => m_Modifiers;

		public int spriteShapeHashCode => m_ActiveSpriteShapeHash;

		public bool worldSpaceUVs
		{
			get
			{
				return m_WorldSpaceUV;
			}
			set
			{
				m_WorldSpaceUV = value;
			}
		}

		public float fillPixelsPerUnit
		{
			get
			{
				return m_FillPixelPerUnit;
			}
			set
			{
				m_FillPixelPerUnit = value;
			}
		}

		public bool enableTangents
		{
			get
			{
				return m_EnableTangents;
			}
			set
			{
				m_EnableTangents = value;
			}
		}

		public float stretchTiling
		{
			get
			{
				return m_StretchTiling;
			}
			set
			{
				m_StretchTiling = value;
			}
		}

		public int splineDetail
		{
			get
			{
				return m_SplineDetail;
			}
			set
			{
				m_SplineDetail = Mathf.Max(0, value);
			}
		}

		public int colliderDetail
		{
			get
			{
				return m_ColliderDetail;
			}
			set
			{
				m_ColliderDetail = Mathf.Max(0, value);
			}
		}

		public float colliderOffset
		{
			get
			{
				return m_ColliderOffset;
			}
			set
			{
				m_ColliderOffset = value;
			}
		}

		public float cornerAngleThreshold
		{
			get
			{
				return m_CornerAngleThreshold;
			}
			set
			{
				m_CornerAngleThreshold = value;
			}
		}

		public bool autoUpdateCollider
		{
			get
			{
				return m_UpdateCollider;
			}
			set
			{
				m_UpdateCollider = value;
			}
		}

		public bool optimizeCollider => m_OptimizeCollider;

		public bool optimizeGeometry => m_OptimizeGeometry;

		public bool hasCollider
		{
			get
			{
				if (!(edgeCollider != null))
				{
					return polygonCollider != null;
				}
				return true;
			}
		}

		public Spline spline => m_Spline;

		public SpriteShape spriteShape
		{
			get
			{
				return m_SpriteShape;
			}
			set
			{
				m_SpriteShape = value;
			}
		}

		public EdgeCollider2D edgeCollider
		{
			get
			{
				if (!m_EdgeCollider2D)
				{
					m_EdgeCollider2D = GetComponent<EdgeCollider2D>();
				}
				return m_EdgeCollider2D;
			}
		}

		public PolygonCollider2D polygonCollider
		{
			get
			{
				if (!m_PolygonCollider2D)
				{
					m_PolygonCollider2D = GetComponent<PolygonCollider2D>();
				}
				return m_PolygonCollider2D;
			}
		}

		public SpriteShapeRenderer spriteShapeRenderer
		{
			get
			{
				if (!m_SpriteShapeRenderer)
				{
					m_SpriteShapeRenderer = GetComponent<SpriteShapeRenderer>();
				}
				return m_SpriteShapeRenderer;
			}
		}

		internal NativeArray<SpriteShapeGeneratorStats> stats
		{
			get
			{
				if (!m_Statistics.IsCreated)
				{
					m_Statistics = new NativeArray<SpriteShapeGeneratorStats>(1, Allocator.Persistent);
				}
				return m_Statistics;
			}
		}

		private void DisposeInternal()
		{
			if (m_ColliderData.IsCreated)
			{
				m_ColliderData.Dispose();
			}
			if (m_TangentData.IsCreated)
			{
				m_TangentData.Dispose();
			}
			if (m_Statistics.IsCreated)
			{
				m_Statistics.Dispose();
			}
		}

		private void OnApplicationQuit()
		{
			DisposeInternal();
		}

		private void OnEnable()
		{
			m_DynamicOcclusionOverriden = true;
			m_DynamicOcclusionLocal = spriteShapeRenderer.allowOcclusionWhenDynamic;
			spriteShapeRenderer.allowOcclusionWhenDynamic = false;
			InitBounds();
			UpdateSpriteData();
		}

		private void OnDisable()
		{
			DisposeInternal();
		}

		private void OnDestroy()
		{
		}

		private void Reset()
		{
			m_SplineDetail = 16;
			m_AdaptiveUV = true;
			m_StretchUV = false;
			m_FillPixelPerUnit = 100f;
			m_ColliderDetail = 16;
			m_StretchTiling = 1f;
			m_WorldSpaceUV = false;
			m_CornerAngleThreshold = 30f;
			m_ColliderOffset = 0f;
			m_UpdateCollider = true;
			m_OptimizeCollider = true;
			m_OptimizeGeometry = true;
			m_EnableTangents = false;
			spline.Clear();
			spline.InsertPointAt(0, Vector2.left + Vector2.down);
			spline.InsertPointAt(1, Vector2.left + Vector2.up);
			spline.InsertPointAt(2, Vector2.right + Vector2.up);
			spline.InsertPointAt(3, Vector2.right + Vector2.down);
		}

		private static void SmartDestroy(Object o)
		{
			if (!(o == null))
			{
				Object.Destroy(o);
			}
		}

		internal Bounds InitBounds()
		{
			int pointCount = spline.GetPointCount();
			if (pointCount > 1)
			{
				Bounds bounds = new Bounds(spline.GetPosition(0), Vector3.zero);
				for (int i = 1; i < pointCount; i++)
				{
					bounds.Encapsulate(spline.GetPosition(i));
				}
				bounds.Encapsulate(spriteShapeRenderer.localBounds);
				spriteShapeRenderer.SetLocalAABB(bounds);
				return bounds;
			}
			return default(Bounds);
		}

		public void RefreshSpriteShape()
		{
			m_ActiveSplineHash = 0;
		}

		private bool ValidateSpline()
		{
			int pointCount = spline.GetPointCount();
			if (pointCount < 2)
			{
				return false;
			}
			for (int i = 0; i < pointCount - 1; i++)
			{
				if ((spline.GetPosition(i) - spline.GetPosition(i + 1)).sqrMagnitude < 0.001f)
				{
					Debug.LogWarningFormat(base.gameObject, "[SpriteShape] Control points {0} & {1} are too close. SpriteShape will not be generated for < {2} >.", i, i + 1, base.gameObject.name);
					return false;
				}
			}
			return true;
		}

		private bool ValidateSpriteShapeTexture()
		{
			bool result = false;
			if (spriteShape != null)
			{
				if (!spline.isOpenEnded)
				{
					result = spriteShape.fillTexture != null;
				}
			}
			else
			{
				Debug.LogWarningFormat(base.gameObject, "[SpriteShape] A valid SpriteShape profile has not been set for gameObject < {0} >.", base.gameObject.name);
			}
			return result;
		}

		internal bool ValidateUTess2D()
		{
			bool result = m_UTess2D;
			if (m_UTess2D && null != spriteShape)
			{
				result = spriteShape.fillOffset == 0f;
			}
			return result;
		}

		private bool HasSpriteShapeChanged()
		{
			bool num = m_ActiveSpriteShape != spriteShape;
			if (num)
			{
				m_ActiveSpriteShape = spriteShape;
			}
			return num;
		}

		private bool HasSpriteShapeDataChanged()
		{
			bool result = HasSpriteShapeChanged();
			if ((bool)spriteShape)
			{
				int num = SpriteShape.GetSpriteShapeHashCode(spriteShape);
				if (spriteShapeHashCode != num)
				{
					m_ActiveSpriteShapeHash = num;
					result = true;
				}
			}
			return result;
		}

		private bool HasSplineDataChanged()
		{
			int num = -2128831035 ^ spline.GetHashCode();
			num = (num * 16777619) ^ (m_UTess2D ? 1 : 0);
			num = (num * 16777619) ^ (m_WorldSpaceUV ? 1 : 0);
			num = (num * 16777619) ^ (m_EnableTangents ? 1 : 0);
			num = (num * 16777619) ^ (m_GeometryCached ? 1 : 0);
			num = (num * 16777619) ^ (m_OptimizeGeometry ? 1 : 0);
			num = (num * 16777619) ^ m_StretchTiling.GetHashCode();
			num = (num * 16777619) ^ m_ColliderOffset.GetHashCode();
			num = (num * 16777619) ^ m_ColliderDetail.GetHashCode();
			if (splineHashCode != num)
			{
				m_ActiveSplineHash = num;
				return true;
			}
			return false;
		}

		private void LateUpdate()
		{
			BakeCollider();
		}

		private void OnWillRenderObject()
		{
			BakeMesh();
		}

		public JobHandle BakeMesh()
		{
			JobHandle result = default(JobHandle);
			if ((bool)spriteShapeGeometryCache && m_ActiveSplineHash != 0 && spriteShapeGeometryCache.maxArrayCount != 0)
			{
				return result;
			}
			if (ValidateSpline())
			{
				bool num = HasSplineDataChanged();
				bool flag = HasSpriteShapeDataChanged();
				bool flag2 = UpdateSpriteShapeParameters();
				if (flag)
				{
					UpdateSpriteData();
				}
				if (num || flag || flag2)
				{
					return ScheduleBake();
				}
			}
			return result;
		}

		public bool UpdateSpriteShapeParameters()
		{
			bool flag = !spline.isOpenEnded;
			bool flag2 = true;
			bool adaptiveUV = m_AdaptiveUV;
			bool stretchUV = m_StretchUV;
			bool flag3 = false;
			uint num = 0u;
			uint num2 = (uint)m_SplineDetail;
			float num3 = 0f;
			float num4 = ((m_CornerAngleThreshold >= 0f && m_CornerAngleThreshold < 90f) ? m_CornerAngleThreshold : 89.9999f);
			Texture2D texture2D = null;
			Matrix4x4 matrix4x = Matrix4x4.identity;
			if ((bool)spriteShape)
			{
				if (worldSpaceUVs)
				{
					matrix4x = base.transform.localToWorldMatrix;
				}
				texture2D = spriteShape.fillTexture;
				num = (stretchUV ? ((uint)stretchTiling) : ((uint)fillPixelsPerUnit));
				num3 = spriteShape.fillOffset;
				flag3 = spriteShape.useSpriteBorders;
				if (spriteShape.cornerSprites.Count > 0)
				{
					flag2 = false;
				}
			}
			bool result = m_ActiveShapeParameters.adaptiveUV != adaptiveUV || m_ActiveShapeParameters.angleThreshold != num4 || m_ActiveShapeParameters.borderPivot != num3 || m_ActiveShapeParameters.carpet != flag || m_ActiveShapeParameters.fillScale != num || m_ActiveShapeParameters.fillTexture != texture2D || m_ActiveShapeParameters.smartSprite != flag2 || m_ActiveShapeParameters.splineDetail != num2 || m_ActiveShapeParameters.spriteBorders != flag3 || m_ActiveShapeParameters.transform != matrix4x || m_ActiveShapeParameters.stretchUV != stretchUV;
			m_ActiveShapeParameters.adaptiveUV = adaptiveUV;
			m_ActiveShapeParameters.stretchUV = stretchUV;
			m_ActiveShapeParameters.angleThreshold = num4;
			m_ActiveShapeParameters.borderPivot = num3;
			m_ActiveShapeParameters.carpet = flag;
			m_ActiveShapeParameters.fillScale = num;
			m_ActiveShapeParameters.fillTexture = texture2D;
			m_ActiveShapeParameters.smartSprite = flag2;
			m_ActiveShapeParameters.splineDetail = num2;
			m_ActiveShapeParameters.spriteBorders = flag3;
			m_ActiveShapeParameters.transform = matrix4x;
			return result;
		}

		private void UpdateSpriteData()
		{
			if ((bool)spriteShape)
			{
				List<Sprite> list = new List<Sprite>();
				List<Sprite> list2 = new List<Sprite>();
				List<AngleRangeInfo> list3 = new List<AngleRangeInfo>();
				List<AngleRange> list4 = new List<AngleRange>(spriteShape.angleRanges);
				list4.Sort((AngleRange a, AngleRange b) => a.order.CompareTo(b.order));
				for (int i = 0; i < list4.Count; i++)
				{
					bool flag = false;
					AngleRange angleRange = list4[i];
					foreach (Sprite sprite in angleRange.sprites)
					{
						if (sprite != null)
						{
							flag = true;
							break;
						}
					}
					if (!flag)
					{
						continue;
					}
					AngleRangeInfo item = default(AngleRangeInfo);
					item.start = angleRange.start;
					item.end = angleRange.end;
					item.order = (uint)angleRange.order;
					List<int> list5 = new List<int>();
					foreach (Sprite sprite2 in angleRange.sprites)
					{
						list.Add(sprite2);
						list5.Add(list.Count - 1);
					}
					item.sprites = list5.ToArray();
					list3.Add(item);
				}
				bool flag2 = false;
				foreach (CornerSprite cornerSprite2 in spriteShape.cornerSprites)
				{
					if (cornerSprite2.sprites[0] != null)
					{
						flag2 = true;
						break;
					}
				}
				if (flag2)
				{
					for (int j = 0; j < spriteShape.cornerSprites.Count; j++)
					{
						CornerSprite cornerSprite = spriteShape.cornerSprites[j];
						list2.Add(cornerSprite.sprites[0]);
					}
				}
				m_EdgeSpriteArray = list.ToArray();
				m_CornerSpriteArray = list2.ToArray();
				m_AngleRangeInfoArray = list3.ToArray();
				List<Sprite> list6 = new List<Sprite>();
				list6.AddRange(m_EdgeSpriteArray);
				list6.AddRange(m_CornerSpriteArray);
				m_SpriteArray = list6.ToArray();
			}
			else
			{
				m_SpriteArray = new Sprite[0];
				m_EdgeSpriteArray = new Sprite[0];
				m_CornerSpriteArray = new Sprite[0];
				m_AngleRangeInfoArray = new AngleRangeInfo[0];
			}
		}

		internal NativeArray<ShapeControlPoint> GetShapeControlPoints()
		{
			int pointCount = spline.GetPointCount();
			NativeArray<ShapeControlPoint> result = new NativeArray<ShapeControlPoint>(pointCount, Allocator.Temp);
			ShapeControlPoint value = default(ShapeControlPoint);
			for (int i = 0; i < pointCount; i++)
			{
				value.position = spline.GetPosition(i);
				value.leftTangent = spline.GetLeftTangent(i);
				value.rightTangent = spline.GetRightTangent(i);
				value.mode = (int)spline.GetTangentMode(i);
				result[i] = value;
			}
			return result;
		}

		internal NativeArray<SplinePointMetaData> GetSplinePointMetaData()
		{
			int pointCount = spline.GetPointCount();
			NativeArray<SplinePointMetaData> result = new NativeArray<SplinePointMetaData>(pointCount, Allocator.Temp);
			SplinePointMetaData value = default(SplinePointMetaData);
			for (int i = 0; i < pointCount; i++)
			{
				value.height = m_Spline.GetHeight(i);
				value.spriteIndex = (uint)m_Spline.GetSpriteIndex(i);
				value.cornerMode = (int)m_Spline.GetCornerMode(i);
				result[i] = value;
			}
			return result;
		}

		internal int CalculateMaxArrayCount(NativeArray<ShapeControlPoint> shapePoints)
		{
			int y = 65536;
			bool flag = false;
			float num = 99999f;
			if (spriteArray != null)
			{
				Sprite[] array = m_SpriteArray;
				foreach (Sprite sprite in array)
				{
					if (sprite != null)
					{
						flag = true;
						float spritePixelWidth = BezierUtility.GetSpritePixelWidth(sprite);
						num = ((num > spritePixelWidth) ? spritePixelWidth : num);
					}
				}
			}
			float smallestSegment = num;
			float num2 = BezierUtility.BezierLength(shapePoints, splineDetail, ref smallestSegment) * 4f;
			int num3 = shapePoints.Length * 4 * splineDetail;
			int num4 = (flag ? ((int)(num2 / smallestSegment) * splineDetail + num3) : 0);
			num3 = (optimizeGeometry ? num3 : (num3 * 2));
			num3 = (ValidateSpriteShapeTexture() ? num3 : 0);
			maxArrayCount = num3 + num4;
			maxArrayCount = math.min(maxArrayCount, y);
			return maxArrayCount;
		}

		private JobHandle ScheduleBake()
		{
			JobHandle result = default(JobHandle);
			_ = Application.isPlaying;
			if (true && geometryCached && (bool)spriteShapeGeometryCache && spriteShapeGeometryCache.maxArrayCount != 0)
			{
				return spriteShapeGeometryCache.Upload(spriteShapeRenderer, this);
			}
			maxArrayCount = spriteShapeCreator.GetVertexArrayCount(this);
			if (maxArrayCount > 0 && base.enabled)
			{
				m_JobHandle.Complete();
				if (m_ColliderData.IsCreated)
				{
					m_ColliderData.Dispose();
				}
				m_ColliderData = new NativeArray<float2>(maxArrayCount, Allocator.Persistent);
				if (!m_TangentData.IsCreated)
				{
					m_TangentData = new NativeArray<Vector4>(1, Allocator.Persistent);
				}
				NativeArray<SpriteShapeSegment> segments = spriteShapeRenderer.GetSegments(spline.GetPointCount() * 8);
				NativeSlice<Vector4> tangents = new NativeSlice<Vector4>(m_TangentData);
				NativeArray<ushort> indices;
				NativeSlice<Vector3> vertices;
				NativeSlice<Vector2> texcoords;
				if (m_EnableTangents)
				{
					spriteShapeRenderer.GetChannels(maxArrayCount, out indices, out vertices, out texcoords, out tangents);
				}
				else
				{
					spriteShapeRenderer.GetChannels(maxArrayCount, out indices, out vertices, out texcoords);
				}
				result = (m_JobHandle = spriteShapeCreator.MakeCreatorJob(this, indices, vertices, texcoords, tangents, segments, m_ColliderData));
				foreach (SpriteShapeGeometryModifier modifier in m_Modifiers)
				{
					m_JobHandle = modifier.MakeModifierJob(m_JobHandle, this, indices, vertices, texcoords, tangents, segments, m_ColliderData);
				}
				spriteShapeRenderer.Prepare(m_JobHandle, m_ActiveShapeParameters, m_SpriteArray);
				result = m_JobHandle;
				JobHandle.ScheduleBatchedJobs();
			}
			if (m_DynamicOcclusionOverriden)
			{
				spriteShapeRenderer.allowOcclusionWhenDynamic = m_DynamicOcclusionLocal;
				m_DynamicOcclusionOverriden = false;
			}
			return result;
		}

		public void BakeCollider()
		{
			m_JobHandle.Complete();
			if (!m_ColliderData.IsCreated)
			{
				return;
			}
			if (autoUpdateCollider && hasCollider)
			{
				int num = 32766;
				float2 x = 0;
				List<Vector2> list = new List<Vector2>();
				for (int i = 0; i < num; i++)
				{
					float2 x2 = m_ColliderData[i];
					if (!math.any(x) && !math.any(x2))
					{
						break;
					}
					list.Add(new Vector2(x2.x, x2.y));
				}
				if (edgeCollider != null)
				{
					edgeCollider.points = list.ToArray();
				}
				if (polygonCollider != null)
				{
					polygonCollider.points = list.ToArray();
				}
			}
			m_ColliderData.Dispose();
			if (m_Statistics.IsCreated)
			{
				switch (m_Statistics[0].status)
				{
				case SpriteShapeGeneratorResult.ErrorNativeDataOverflow:
					Debug.LogWarningFormat(base.gameObject, "NativeArray access not within range. Please submit a bug report.");
					break;
				case SpriteShapeGeneratorResult.ErrorSpritesTightPacked:
					Debug.LogWarningFormat(base.gameObject, "Sprites used in SpriteShape profile must use FullRect.");
					break;
				case SpriteShapeGeneratorResult.ErrorSpritesWrongBorder:
					Debug.LogWarningFormat(base.gameObject, "Sprites used in SpriteShape profile have invalid borders. Please check SpriteShape profile.");
					break;
				case SpriteShapeGeneratorResult.ErrorVertexLimitReached:
					Debug.LogWarningFormat(base.gameObject, "Mesh data has reached Limits. Please try dividing shape into smaller blocks.");
					break;
				case SpriteShapeGeneratorResult.ErrorDefaultQuadCreated:
					Debug.LogWarningFormat(base.gameObject, "Fill tessellation (C# Job) encountered errors. Please disable it to use default tessellation for fill geometry.");
					break;
				}
			}
		}

		internal void BakeMeshForced()
		{
			if (spriteShapeRenderer != null && HasSplineDataChanged())
			{
				BakeMesh();
				CommandBuffer commandBuffer = new CommandBuffer();
				commandBuffer.GetTemporaryRT(0, 256, 256, 0);
				commandBuffer.SetRenderTarget(0);
				commandBuffer.DrawRenderer(spriteShapeRenderer, spriteShapeRenderer.sharedMaterial);
				commandBuffer.ReleaseTemporaryRT(0);
				Graphics.ExecuteCommandBuffer(commandBuffer);
			}
		}

		private Texture2D GetTextureFromIndex(int index)
		{
			if (index == 0)
			{
				if (!spriteShape)
				{
					return null;
				}
				return spriteShape.fillTexture;
			}
			index--;
			if (index < m_EdgeSpriteArray.Length)
			{
				return GetSpriteTexture(m_EdgeSpriteArray[index]);
			}
			index -= m_EdgeSpriteArray.Length;
			return GetSpriteTexture(m_CornerSpriteArray[index]);
		}

		private Texture2D GetSpriteTexture(Sprite sprite)
		{
			if ((bool)sprite)
			{
				return sprite.texture;
			}
			return null;
		}
	}
}
