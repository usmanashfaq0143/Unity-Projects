using System;
using System.Linq;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Profiling;
using Unity.SpriteShape.External.LibTessDotNet;
using UnityEngine.U2D.Common.UTess;

namespace UnityEngine.U2D
{
	[BurstCompile]
	internal struct SpriteShapeGenerator : IJob
	{
		private struct JobParameters
		{
			public int4 shapeData;

			public int4 splineData;

			public float4 curveData;

			public float4 fillData;
		}

		private struct JobSpriteInfo
		{
			public float4 texRect;

			public float4 texData;

			public float4 uvInfo;

			public float4 metaInfo;

			public float4 border;
		}

		private struct JobAngleRange
		{
			public float4 spriteAngles;

			public int4 spriteData;
		}

		private struct JobControlPoint
		{
			public int4 cpData;

			public int4 exData;

			public float2 cpInfo;

			public float2 position;

			public float2 tangentLt;

			public float2 tangentRt;
		}

		private struct JobContourPoint
		{
			public float2 position;

			public float2 ptData;
		}

		private struct JobIntersectPoint
		{
			public float2 top;

			public float2 bottom;
		}

		private struct JobSegmentInfo
		{
			public int4 sgInfo;

			public float4 spriteInfo;
		}

		private struct JobCornerInfo
		{
			public float2 bottom;

			public float2 top;

			public float2 left;

			public float2 right;

			public int2 cornerData;
		}

		private struct JobShapeVertex
		{
			public float2 pos;

			public float2 uv;

			public float4 tan;

			public float2 meta;

			public int4 sprite;
		}

		public ProfilerMarker generateGeometry;

		public ProfilerMarker generateCollider;

		[ReadOnly]
		private JobParameters m_ShapeParams;

		[ReadOnly]
		[DeallocateOnJobCompletion]
		private NativeArray<JobSpriteInfo> m_SpriteInfos;

		[ReadOnly]
		[DeallocateOnJobCompletion]
		private NativeArray<JobSpriteInfo> m_CornerSpriteInfos;

		[ReadOnly]
		[DeallocateOnJobCompletion]
		private NativeArray<JobAngleRange> m_AngleRanges;

		[DeallocateOnJobCompletion]
		private NativeArray<JobSegmentInfo> m_Segments;

		private int m_SegmentCount;

		[DeallocateOnJobCompletion]
		private NativeArray<JobContourPoint> m_ContourPoints;

		private int m_ContourPointCount;

		[DeallocateOnJobCompletion]
		private NativeArray<JobCornerInfo> m_Corners;

		private int m_CornerCount;

		[DeallocateOnJobCompletion]
		private NativeArray<float2> m_TessPoints;

		private int m_TessPointCount;

		[DeallocateOnJobCompletion]
		private NativeArray<JobShapeVertex> m_VertexData;

		[DeallocateOnJobCompletion]
		private NativeArray<JobShapeVertex> m_OutputVertexData;

		[DeallocateOnJobCompletion]
		private NativeArray<JobControlPoint> m_ControlPoints;

		private int m_ControlPointCount;

		[DeallocateOnJobCompletion]
		private NativeArray<float2> m_CornerCoordinates;

		[DeallocateOnJobCompletion]
		private NativeArray<float2> m_TempPoints;

		[DeallocateOnJobCompletion]
		private NativeArray<JobControlPoint> m_GeneratedControlPoints;

		[DeallocateOnJobCompletion]
		private NativeArray<int2> m_SpriteIndices;

		[DeallocateOnJobCompletion]
		private NativeArray<JobIntersectPoint> m_Intersectors;

		private int m_IndexArrayCount;

		public NativeArray<ushort> m_IndexArray;

		private int m_VertexArrayCount;

		public NativeSlice<Vector3> m_PosArray;

		public NativeSlice<Vector2> m_Uv0Array;

		public NativeSlice<Vector4> m_TanArray;

		private int m_GeomArrayCount;

		public NativeArray<SpriteShapeSegment> m_GeomArray;

		private int m_ColliderPointCount;

		public NativeArray<float2> m_ColliderPoints;

		public NativeArray<Bounds> m_Bounds;

		public NativeArray<SpriteShapeGeneratorStats> m_Stats;

		private int m_IndexDataCount;

		private int m_VertexDataCount;

		private int m_ColliderDataCount;

		private int m_ActiveIndexCount;

		private int m_ActiveVertexCount;

		private float2 m_FirstLT;

		private float2 m_FirstLB;

		private float4x4 m_Transform;

		private int kModeLinear;

		private int kModeContinous;

		private int kModeBroken;

		private int kModeUTess;

		private int kCornerTypeOuterTopLeft;

		private int kCornerTypeOuterTopRight;

		private int kCornerTypeOuterBottomLeft;

		private int kCornerTypeOuterBottomRight;

		private int kCornerTypeInnerTopLeft;

		private int kCornerTypeInnerTopRight;

		private int kCornerTypeInnerBottomLeft;

		private int kCornerTypeInnerBottomRight;

		private int kControlPointCount;

		private float kEpsilon;

		private float kEpsilonOrder;

		private float kEpsilonRelaxed;

		private float kExtendSegment;

		private float kRenderQuality;

		private float kOptimizeRender;

		private float kColliderQuality;

		private float kOptimizeCollider;

		private float kLowestQualityTolerance;

		private float kHighestQualityTolerance;

		private int vertexDataCount => m_VertexDataCount;

		private int vertexArrayCount => m_VertexArrayCount;

		private int indexDataCount => m_IndexDataCount;

		private int spriteCount => m_SpriteInfos.Length;

		private int cornerSpriteCount => m_CornerSpriteInfos.Length;

		private int angleRangeCount => m_AngleRanges.Length;

		private int controlPointCount => m_ControlPointCount;

		private int contourPointCount => m_ContourPointCount;

		private int segmentCount => m_SegmentCount;

		private bool hasCollider => m_ShapeParams.splineData.w == 1;

		private float colliderPivot => m_ShapeParams.curveData.x;

		private float borderPivot => m_ShapeParams.curveData.y;

		private int splineDetail => m_ShapeParams.splineData.y;

		private bool isCarpet => m_ShapeParams.shapeData.x == 1;

		private bool isAdaptive => m_ShapeParams.shapeData.y == 1;

		private bool hasSpriteBorder => m_ShapeParams.shapeData.z == 1;

		private JobSpriteInfo GetSpriteInfo(int index)
		{
			return m_SpriteInfos[index];
		}

		private JobSpriteInfo GetCornerSpriteInfo(int index)
		{
			int index2 = index - 1;
			return m_CornerSpriteInfos[index2];
		}

		private JobAngleRange GetAngleRange(int index)
		{
			return m_AngleRanges[index];
		}

		private JobControlPoint GetControlPoint(int index)
		{
			return m_ControlPoints[index];
		}

		private JobContourPoint GetContourPoint(int index)
		{
			return m_ContourPoints[index];
		}

		private JobSegmentInfo GetSegmentInfo(int index)
		{
			return m_Segments[index];
		}

		private int GetContourIndex(int index)
		{
			return index * m_ShapeParams.splineData.y;
		}

		private int GetEndContourIndexOfSegment(JobSegmentInfo isi)
		{
			return GetContourIndex(isi.sgInfo.y) - 1;
		}

		private void SetResult(SpriteShapeGeneratorResult result)
		{
			if (m_Stats.IsCreated)
			{
				SpriteShapeGeneratorStats value = m_Stats[0];
				value.status = result;
				m_Stats[0] = value;
			}
		}

		private static void CopyToNativeArray<T>(NativeArray<T> from, int length, ref NativeArray<T> to) where T : struct
		{
			to = new NativeArray<T>(length, Allocator.TempJob);
			for (int i = 0; i < length; i++)
			{
				to[i] = from[i];
			}
		}

		private static void SafeDispose<T>(NativeArray<T> na) where T : struct
		{
			if (na.Length > 0)
			{
				na.Dispose();
			}
		}

		private static bool IsPointOnLine(float epsilon, float2 a, float2 b, float2 c)
		{
			if (math.abs((c.y - a.y) * (b.x - a.x) - (c.x - a.x) * (b.y - a.y)) > epsilon)
			{
				return false;
			}
			float num = (c.x - a.x) * (b.x - a.x) + (c.y - a.y) * (b.y - a.y);
			if (num < 0f)
			{
				return false;
			}
			float num2 = (b.x - a.x) * (b.x - a.x) + (b.y - a.y) * (b.y - a.y);
			if (num > num2)
			{
				return false;
			}
			return true;
		}

		private static bool IsPointOnLines(float epsilon, float2 p1, float2 p2, float2 p3, float2 p4, float2 r)
		{
			if (IsPointOnLine(epsilon, p1, p2, r))
			{
				return IsPointOnLine(epsilon, p3, p4, r);
			}
			return false;
		}

		private static bool Colinear(float2 p, float2 q, float2 r)
		{
			if (q.x <= math.max(p.x, r.x) && q.x >= math.min(p.x, r.x) && q.y <= math.max(p.y, r.y))
			{
				return q.y >= math.min(p.y, r.y);
			}
			return false;
		}

		private static int Det(float epsilon, float2 p, float2 q, float2 r)
		{
			float num = (q.y - p.y) * (r.x - q.x) - (q.x - p.x) * (r.y - q.y);
			if (num > 0f - epsilon && num < epsilon)
			{
				return 0;
			}
			if (!(num > 0f))
			{
				return 2;
			}
			return 1;
		}

		private static bool LineIntersectionTest(float epsilon, float2 p1, float2 q1, float2 p2, float2 q2)
		{
			int num = Det(epsilon, p1, q1, p2);
			int num2 = Det(epsilon, p1, q1, q2);
			int num3 = Det(epsilon, p2, q2, p1);
			int num4 = Det(epsilon, p2, q2, q1);
			if (num != num2 && num3 != num4)
			{
				return true;
			}
			if (num == 0 && Colinear(p1, p2, q1))
			{
				return true;
			}
			if (num2 == 0 && Colinear(p1, q2, q1))
			{
				return true;
			}
			if (num3 == 0 && Colinear(p2, p1, q2))
			{
				return true;
			}
			if (num4 == 0 && Colinear(p2, q1, q2))
			{
				return true;
			}
			return false;
		}

		private static bool LineIntersection(float epsilon, float2 p1, float2 p2, float2 p3, float2 p4, ref float2 result)
		{
			if (!LineIntersectionTest(epsilon, p1, p2, p3, p4))
			{
				return false;
			}
			float num = p2.x - p1.x;
			float num2 = p2.y - p1.y;
			float num3 = p4.x - p3.x;
			float num4 = p4.y - p3.y;
			float num5 = num * num4 - num2 * num3;
			if (math.abs(num5) < epsilon)
			{
				return false;
			}
			float num6 = p3.x - p1.x;
			float num7 = p3.y - p1.y;
			float num8 = (num6 * num4 - num7 * num3) / num5;
			if (num8 >= 0f - epsilon && num8 <= 1f + epsilon)
			{
				result.x = p1.x + num8 * num;
				result.y = p1.y + num8 * num2;
				return true;
			}
			return false;
		}

		private static float AngleBetweenVector(float2 a, float2 b)
		{
			float x = math.dot(a, b);
			return math.atan2(a.x * b.y - b.x * a.y, x) * 57.29578f;
		}

		private static bool GenerateColumnsBi(float2 a, float2 b, float2 whsize, bool flip, ref float2 rt, ref float2 rb, float cph, float pivot)
		{
			float2 x = (flip ? (a - b) : (b - a));
			if (math.length(x) < 1E-30f)
			{
				return false;
			}
			float2 @float = new float2(-1f, 1f);
			float2 x2 = x.yx * @float;
			float2 float2 = new float2(whsize.y * cph);
			float2 float3 = math.normalize(x2) * float2;
			rt = a - float3;
			rb = a + float3;
			float2 float4 = (rb - rt) * pivot;
			rt += float4;
			rb += float4;
			return true;
		}

		private static bool GenerateColumnsTri(float2 a, float2 b, float2 c, float2 whsize, bool flip, ref float2 rt, ref float2 rb, float cph, float pivot)
		{
			float2 @float = new float2(-1f, 1f);
			float2 float2 = b - a;
			float2 float3 = c - b;
			float2 = float2.yx * @float;
			float3 = float3.yx * @float;
			float2 x = math.normalize(float2) + math.normalize(float3);
			if (math.length(x) < 1E-30f)
			{
				return false;
			}
			x = math.normalize(x);
			float2 float4 = new float2(whsize.y * cph);
			float2 float5 = x * float4;
			rt = b - float5;
			rb = b + float5;
			float2 float6 = (rb - rt) * pivot;
			rt += float6;
			rb += float6;
			return true;
		}

		private void AppendCornerCoordinates(ref NativeArray<float2> corners, ref int cornerCount, float2 a, float2 b, float2 c, float2 d)
		{
			corners[cornerCount++] = a;
			corners[cornerCount++] = b;
			corners[cornerCount++] = c;
			corners[cornerCount++] = d;
		}

		private unsafe void PrepareInput(SpriteShapeParameters shapeParams, int maxArrayCount, NativeArray<ShapeControlPoint> shapePoints, bool optimizeGeometry, bool updateCollider, bool optimizeCollider, float colliderOffset, float colliderDetail)
		{
			kModeLinear = 0;
			kModeContinous = 1;
			kModeBroken = 2;
			kCornerTypeOuterTopLeft = 1;
			kCornerTypeOuterTopRight = 2;
			kCornerTypeOuterBottomLeft = 3;
			kCornerTypeOuterBottomRight = 4;
			kCornerTypeInnerTopLeft = 5;
			kCornerTypeInnerTopRight = 6;
			kCornerTypeInnerBottomLeft = 7;
			kCornerTypeInnerBottomRight = 8;
			m_IndexDataCount = 0;
			m_VertexDataCount = 0;
			m_ColliderDataCount = 0;
			m_ActiveIndexCount = 0;
			m_ActiveVertexCount = 0;
			kEpsilon = 1E-05f;
			kEpsilonOrder = -0.0001f;
			kEpsilonRelaxed = 0.001f;
			kExtendSegment = 10000f;
			kLowestQualityTolerance = 4f;
			kHighestQualityTolerance = 16f;
			kColliderQuality = math.clamp(colliderDetail, kLowestQualityTolerance, kHighestQualityTolerance);
			kOptimizeCollider = (optimizeCollider ? 1 : 0);
			kColliderQuality = (kHighestQualityTolerance - kColliderQuality + 2f) * 0.002f;
			colliderOffset = ((colliderOffset == 0f) ? kEpsilonRelaxed : (0f - colliderOffset));
			kOptimizeRender = (optimizeGeometry ? 1 : 0);
			kRenderQuality = math.clamp(shapeParams.splineDetail, kLowestQualityTolerance, kHighestQualityTolerance);
			kRenderQuality = (kHighestQualityTolerance - kRenderQuality + 2f) * 0.0002f;
			m_ShapeParams.shapeData = new int4(shapeParams.carpet ? 1 : 0, shapeParams.adaptiveUV ? 1 : 0, shapeParams.spriteBorders ? 1 : 0, (shapeParams.fillTexture != null) ? 1 : 0);
			m_ShapeParams.splineData = new int4(shapeParams.stretchUV ? 1 : 0, (int)((shapeParams.splineDetail > 4) ? shapeParams.splineDetail : 4), 0, updateCollider ? 1 : 0);
			m_ShapeParams.curveData = new float4(colliderOffset, shapeParams.borderPivot, shapeParams.angleThreshold, 0f);
			float y = 0f;
			float z = 0f;
			if (shapeParams.fillTexture != null)
			{
				y = (float)shapeParams.fillTexture.width * (1f / (float)shapeParams.fillScale);
				z = (float)shapeParams.fillTexture.height * (1f / (float)shapeParams.fillScale);
			}
			m_ShapeParams.fillData = new float4(shapeParams.fillScale, y, z, 0f);
			UnsafeUtility.MemClear(m_GeomArray.GetUnsafePtr(), m_GeomArray.Length * UnsafeUtility.SizeOf<SpriteShapeSegment>());
			m_Transform = new float4x4(shapeParams.transform.m00, shapeParams.transform.m01, shapeParams.transform.m02, shapeParams.transform.m03, shapeParams.transform.m10, shapeParams.transform.m11, shapeParams.transform.m12, shapeParams.transform.m13, shapeParams.transform.m20, shapeParams.transform.m21, shapeParams.transform.m22, shapeParams.transform.m23, shapeParams.transform.m30, shapeParams.transform.m31, shapeParams.transform.m32, shapeParams.transform.m33);
			kControlPointCount = shapePoints.Length * (int)shapeParams.splineDetail * 32;
			m_Segments = new NativeArray<JobSegmentInfo>(shapePoints.Length * 2, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
			m_ContourPoints = new NativeArray<JobContourPoint>(kControlPointCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
			m_TessPoints = new NativeArray<float2>(shapePoints.Length * (int)shapeParams.splineDetail * 128, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
			m_VertexData = new NativeArray<JobShapeVertex>(maxArrayCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
			m_OutputVertexData = new NativeArray<JobShapeVertex>(maxArrayCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
			m_CornerCoordinates = new NativeArray<float2>(32, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
			m_Intersectors = new NativeArray<JobIntersectPoint>(kControlPointCount, Allocator.TempJob);
			m_TempPoints = new NativeArray<float2>(maxArrayCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
			m_GeneratedControlPoints = new NativeArray<JobControlPoint>(kControlPointCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
			m_SpriteIndices = new NativeArray<int2>(kControlPointCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
			int cornerCount = 0;
			AppendCornerCoordinates(ref m_CornerCoordinates, ref cornerCount, new float2(1f, 1f), new float2(0f, 1f), new float2(1f, 0f), new float2(0f, 0f));
			AppendCornerCoordinates(ref m_CornerCoordinates, ref cornerCount, new float2(1f, 0f), new float2(1f, 1f), new float2(0f, 0f), new float2(0f, 1f));
			AppendCornerCoordinates(ref m_CornerCoordinates, ref cornerCount, new float2(0f, 1f), new float2(0f, 0f), new float2(1f, 1f), new float2(1f, 0f));
			AppendCornerCoordinates(ref m_CornerCoordinates, ref cornerCount, new float2(0f, 0f), new float2(1f, 0f), new float2(0f, 1f), new float2(1f, 1f));
			AppendCornerCoordinates(ref m_CornerCoordinates, ref cornerCount, new float2(0f, 0f), new float2(0f, 1f), new float2(1f, 0f), new float2(1f, 1f));
			AppendCornerCoordinates(ref m_CornerCoordinates, ref cornerCount, new float2(0f, 1f), new float2(1f, 1f), new float2(0f, 0f), new float2(1f, 0f));
			AppendCornerCoordinates(ref m_CornerCoordinates, ref cornerCount, new float2(1f, 0f), new float2(0f, 0f), new float2(1f, 1f), new float2(0f, 1f));
			AppendCornerCoordinates(ref m_CornerCoordinates, ref cornerCount, new float2(1f, 1f), new float2(1f, 0f), new float2(0f, 1f), new float2(0f, 0f));
		}

		private void TransferSprites(ref NativeArray<JobSpriteInfo> spriteInfos, Sprite[] sprites, int maxCount)
		{
			for (int i = 0; i < sprites.Length && i < maxCount; i++)
			{
				JobSpriteInfo value = spriteInfos[i];
				Sprite sprite = sprites[i];
				if (sprite != null)
				{
					Texture2D texture = sprite.texture;
					value.texRect = new float4(sprite.textureRect.x, sprite.textureRect.y, sprite.textureRect.width, sprite.textureRect.height);
					value.texData = new float4(texture.width, texture.height, texture.texelSize.x, texture.texelSize.y);
					value.border = new float4(sprite.border.x, sprite.border.y, sprite.border.z, sprite.border.w);
					value.uvInfo = new float4(value.texRect.x / value.texData.x, value.texRect.y / value.texData.y, value.texRect.z / value.texData.x, value.texRect.w / value.texData.y);
					value.metaInfo = new float4(sprite.pixelsPerUnit, sprite.pivot.y / sprite.textureRect.height, sprite.rect.width, sprite.rect.height);
					if (!math.any(value.texRect))
					{
						SetResult(SpriteShapeGeneratorResult.ErrorSpritesTightPacked);
					}
				}
				spriteInfos[i] = value;
			}
		}

		private void PrepareSprites(Sprite[] edgeSprites, Sprite[] cornerSprites)
		{
			m_SpriteInfos = new NativeArray<JobSpriteInfo>(edgeSprites.Length, Allocator.TempJob);
			TransferSprites(ref m_SpriteInfos, edgeSprites, edgeSprites.Length);
			m_CornerSpriteInfos = new NativeArray<JobSpriteInfo>(kCornerTypeInnerBottomRight, Allocator.TempJob);
			TransferSprites(ref m_CornerSpriteInfos, cornerSprites, cornerSprites.Length);
		}

		private void PrepareAngleRanges(AngleRangeInfo[] angleRanges)
		{
			m_AngleRanges = new NativeArray<JobAngleRange>(angleRanges.Length, Allocator.TempJob);
			for (int i = 0; i < angleRanges.Length; i++)
			{
				JobAngleRange value = m_AngleRanges[i];
				AngleRangeInfo angleRangeInfo = angleRanges[i];
				int[] sprites = angleRangeInfo.sprites;
				if (angleRangeInfo.start > angleRangeInfo.end)
				{
					float start = angleRangeInfo.start;
					angleRangeInfo.start = angleRangeInfo.end;
					angleRangeInfo.end = start;
				}
				value.spriteAngles = new float4(angleRangeInfo.start + 90f, angleRangeInfo.end + 90f, 0f, 0f);
				value.spriteData = new int4((int)angleRangeInfo.order, sprites.Length, 32, 0);
				m_AngleRanges[i] = value;
			}
		}

		private void PrepareControlPoints(NativeArray<ShapeControlPoint> shapePoints, NativeArray<SplinePointMetaData> metaData)
		{
			float2 @float = new float2(0f, 0f);
			m_ControlPoints = new NativeArray<JobControlPoint>(kControlPointCount, Allocator.TempJob);
			for (int i = 0; i < shapePoints.Length; i++)
			{
				JobControlPoint value = m_ControlPoints[i];
				ShapeControlPoint shapeControlPoint = shapePoints[i];
				SplinePointMetaData splinePointMetaData = metaData[i];
				value.position = new float2(shapeControlPoint.position.x, shapeControlPoint.position.y);
				value.tangentLt = ((shapeControlPoint.mode == kModeLinear) ? @float : new float2(shapeControlPoint.leftTangent.x, shapeControlPoint.leftTangent.y));
				value.tangentRt = ((shapeControlPoint.mode == kModeLinear) ? @float : new float2(shapeControlPoint.rightTangent.x, shapeControlPoint.rightTangent.y));
				value.cpInfo = new float2(splinePointMetaData.height, 0f);
				value.cpData = new int4((int)splinePointMetaData.spriteIndex, splinePointMetaData.cornerMode, shapeControlPoint.mode, 0);
				value.exData = new int4(-1, 0, 0, shapeControlPoint.mode);
				m_ControlPoints[i] = value;
			}
			m_ControlPointCount = shapePoints.Length;
			m_Corners = new NativeArray<JobCornerInfo>(shapePoints.Length, Allocator.TempJob);
			GenerateControlPoints();
		}

		private bool WithinRange(JobAngleRange angleRange, float inputAngle)
		{
			float num = angleRange.spriteAngles.y - angleRange.spriteAngles.x;
			float num2 = Mathf.Repeat(inputAngle - angleRange.spriteAngles.x, 360f);
			if (num2 >= 0f)
			{
				return num2 <= num;
			}
			return false;
		}

		private bool AngleWithinRange(float t, float a, float b)
		{
			if (a != 0f && b != 0f)
			{
				if (t >= a)
				{
					return t <= b;
				}
				return false;
			}
			return false;
		}

		private static float2 BezierPoint(float2 st, float2 sp, float2 ep, float2 et, float t)
		{
			float2 @float = new float2(t);
			float2 float2 = new float2(1f - t);
			float2 float3 = new float2(3f);
			return sp * float2 * float2 * float2 + st * float2 * float2 * @float * float3 + et * float2 * @float * @float * float3 + ep * @float * @float * @float;
		}

		private static float SlopeAngle(float2 dirNormalized)
		{
			float2 y = new float2(0f, 1f);
			float2 y2 = new float2(1f, 0f);
			float num = math.dot(dirNormalized, y2);
			float num2 = math.dot(dirNormalized, y);
			float num3 = math.acos(num2);
			float num4 = ((num >= 0f) ? 1f : (-1f));
			float num5 = num3 * 57.29578f * num4;
			num5 = ((num2 != 1f) ? num5 : 0f);
			return (num2 != -1f) ? num5 : (-180f);
		}

		private static float SlopeAngle(float2 start, float2 end)
		{
			return SlopeAngle(math.normalize(start - end));
		}

		private bool ResolveAngle(float angle, int activeIndex, ref float renderOrder, ref int spriteIndex, ref int firstSpriteIndex)
		{
			int num = 0;
			int num2 = 0;
			for (int i = 0; i < m_AngleRanges.Length; i++)
			{
				if (WithinRange(m_AngleRanges[i], angle))
				{
					int num3 = ((activeIndex < m_AngleRanges[i].spriteData.y) ? activeIndex : 0);
					renderOrder = num + num3;
					spriteIndex = num2 + num3;
					firstSpriteIndex = num2;
					return true;
				}
				num += m_AngleRanges[i].spriteData.z;
				num2 += m_AngleRanges[i].spriteData.y;
			}
			return false;
		}

		private int GetSpriteIndex(int index, int previousIndex, ref int resolved)
		{
			int index2 = (index + 1) % controlPointCount;
			int spriteIndex = -1;
			int firstSpriteIndex = -1;
			float renderOrder = 0f;
			JobControlPoint controlPoint = GetControlPoint(index);
			float angle = SlopeAngle(GetControlPoint(index2).position, controlPoint.position);
			bool flag = ResolveAngle(angle, controlPoint.cpData.x, ref renderOrder, ref spriteIndex, ref firstSpriteIndex);
			resolved = (flag ? 1 : 0);
			if (!flag)
			{
				return previousIndex;
			}
			return spriteIndex;
		}

		private void GenerateSegments()
		{
			int num = 0;
			int num2 = 0;
			int firstSpriteIndex = -1;
			JobSegmentInfo value = m_Segments[0];
			value.sgInfo = int4.zero;
			value.spriteInfo = int4.zero;
			float angle = 0f;
			for (int i = 0; i < controlPointCount; i++)
			{
				int num3 = (i + 1) % controlPointCount;
				bool flag = false;
				if (num3 == 0)
				{
					if (!isCarpet)
					{
						continue;
					}
					num3 = 1;
					flag = true;
				}
				JobControlPoint controlPoint = GetControlPoint(i);
				JobControlPoint controlPoint2 = GetControlPoint(num3);
				if (controlPoint.exData.x > 0 && controlPoint.exData.x == controlPoint2.exData.x && controlPoint.exData.z == 2)
				{
					continue;
				}
				int4 cpData = controlPoint.cpData;
				float2 cpInfo = controlPoint.cpInfo;
				int num4 = ((i < num3) ? i : num3);
				int num5 = ((i > num3) ? i : num3);
				bool flag2 = controlPoint.cpData.z == kModeContinous;
				bool flag3 = false;
				if (!flag2 || num2 == 0)
				{
					angle = SlopeAngle(controlPoint2.position, controlPoint.position);
				}
				if (!ResolveAngle(angle, cpData.x, ref cpInfo.y, ref cpData.w, ref firstSpriteIndex) && !flag)
				{
					cpData.w = num;
					controlPoint.cpData = cpData;
					m_ControlPoints[i] = controlPoint;
					value = m_Segments[num2];
					value.sgInfo.x = num4;
					value.sgInfo.y = num5;
					value.sgInfo.z = -1;
					m_Segments[num2] = value;
					num2++;
					continue;
				}
				num = cpData.w;
				controlPoint.cpData = cpData;
				controlPoint.cpInfo = cpInfo;
				m_ControlPoints[i] = controlPoint;
				if (flag)
				{
					continue;
				}
				if (num2 != 0)
				{
					flag2 = flag2 && m_SpriteIndices[value.sgInfo.x].y != 0 && num == value.sgInfo.z;
				}
				if (flag2 && i != controlPointCount - 1)
				{
					for (int j = 0; j < num2; j++)
					{
						value = m_Segments[j];
						if (value.sgInfo.x - num4 == 1)
						{
							flag3 = true;
							value.sgInfo.x = num4;
							m_Segments[j] = value;
							break;
						}
						if (num5 - value.sgInfo.y == 1)
						{
							flag3 = true;
							value.sgInfo.y = num5;
							m_Segments[j] = value;
							break;
						}
					}
				}
				if (!flag3)
				{
					value = m_Segments[num2];
					JobSpriteInfo spriteInfo = GetSpriteInfo(controlPoint.cpData.w);
					value.sgInfo.x = num4;
					value.sgInfo.y = num5;
					value.sgInfo.z = num;
					value.sgInfo.w = firstSpriteIndex;
					value.spriteInfo.x = spriteInfo.texRect.z;
					value.spriteInfo.y = spriteInfo.texRect.w;
					value.spriteInfo.z = cpInfo.y;
					m_Segments[num2] = value;
					num2++;
				}
			}
			m_SegmentCount = num2;
		}

		private void UpdateSegments()
		{
			for (int i = 0; i < segmentCount; i++)
			{
				JobSegmentInfo segmentInfo = GetSegmentInfo(i);
				if (segmentInfo.spriteInfo.z >= 0f)
				{
					segmentInfo.spriteInfo.w = SegmentDistance(segmentInfo);
					m_Segments[i] = segmentInfo;
				}
			}
		}

		private bool GetSegmentBoundaryColumn(JobSegmentInfo segment, JobSpriteInfo sprInfo, float2 whsize, float2 startPos, float2 endPos, bool end, ref float2 top, ref float2 bottom)
		{
			bool flag = false;
			float pivot = 0.5f - sprInfo.metaInfo.y;
			if (!end)
			{
				JobControlPoint controlPoint = GetControlPoint(segment.sgInfo.x);
				if (math.any(controlPoint.tangentRt))
				{
					endPos = controlPoint.tangentRt + startPos;
				}
				return GenerateColumnsBi(startPos, endPos, whsize, end, ref top, ref bottom, controlPoint.cpInfo.x * 0.5f, pivot);
			}
			JobControlPoint controlPoint2 = GetControlPoint(segment.sgInfo.y);
			if (math.any(controlPoint2.tangentLt))
			{
				endPos = controlPoint2.tangentLt + startPos;
			}
			return GenerateColumnsBi(startPos, endPos, whsize, end, ref top, ref bottom, controlPoint2.cpInfo.x * 0.5f, pivot);
		}

		private void GenerateControlPoints()
		{
			int activePoint = 0;
			int previousIndex = 0;
			int num = 0;
			int num2 = controlPointCount;
			_ = controlPointCount;
			int2 value = new int2(0, 0);
			for (int i = 0; i < controlPointCount; i++)
			{
				int resolved = 0;
				int spriteIndex = GetSpriteIndex(i, previousIndex, ref resolved);
				previousIndex = (value.x = spriteIndex);
				value.y = resolved;
				m_SpriteIndices[i] = value;
			}
			if (!isCarpet)
			{
				JobControlPoint controlPoint = GetControlPoint(0);
				controlPoint.cpData.z = ((controlPoint.cpData.z == kModeContinous) ? kModeBroken : controlPoint.cpData.z);
				m_GeneratedControlPoints[activePoint++] = controlPoint;
				num = 1;
				num2 = controlPointCount - 1;
			}
			for (int j = num; j < num2; j++)
			{
				bool cornerConsidered = false;
				if (!InsertCorner(j, ref m_SpriteIndices, ref m_GeneratedControlPoints, ref activePoint, ref cornerConsidered))
				{
					JobControlPoint controlPoint2 = GetControlPoint(j);
					controlPoint2.exData.z = ((cornerConsidered && controlPoint2.cpData.y == 2) ? 1 : 0);
					m_GeneratedControlPoints[activePoint++] = controlPoint2;
				}
			}
			if (!isCarpet)
			{
				JobControlPoint value2 = m_GeneratedControlPoints[0];
				value2.exData.z = 1;
				m_GeneratedControlPoints[0] = value2;
				JobControlPoint controlPoint3 = GetControlPoint(num2);
				controlPoint3.cpData.z = ((controlPoint3.cpData.z == kModeContinous) ? kModeBroken : controlPoint3.cpData.z);
				controlPoint3.exData.z = 1;
				m_GeneratedControlPoints[activePoint++] = controlPoint3;
			}
			else
			{
				JobControlPoint value3 = m_GeneratedControlPoints[0];
				m_GeneratedControlPoints[activePoint++] = value3;
			}
			for (int k = 0; k < activePoint; k++)
			{
				m_ControlPoints[k] = m_GeneratedControlPoints[k];
			}
			m_ControlPointCount = activePoint;
			for (int l = 0; l < controlPointCount; l++)
			{
				int resolved2 = 0;
				int spriteIndex2 = GetSpriteIndex(l, previousIndex, ref resolved2);
				previousIndex = (value.x = spriteIndex2);
				value.y = resolved2;
				m_SpriteIndices[l] = value;
			}
		}

		private float SegmentDistance(JobSegmentInfo isi)
		{
			float num = 0f;
			int contourIndex = GetContourIndex(isi.sgInfo.x);
			int endContourIndexOfSegment = GetEndContourIndexOfSegment(isi);
			for (int i = contourIndex; i < endContourIndexOfSegment; i++)
			{
				int index = i + 1;
				JobContourPoint contourPoint = GetContourPoint(i);
				JobContourPoint contourPoint2 = GetContourPoint(index);
				num += math.distance(contourPoint.position, contourPoint2.position);
			}
			return num;
		}

		private void GenerateContour()
		{
			int num = controlPointCount - 1;
			int num2 = 0;
			float num3 = splineDetail - 1;
			for (int i = 0; i < num; i++)
			{
				int index = i + 1;
				JobControlPoint controlPoint = GetControlPoint(i);
				JobControlPoint controlPoint2 = GetControlPoint(index);
				bool flag = controlPoint.exData.w == kModeContinous || controlPoint2.exData.w == kModeContinous;
				float2 position = controlPoint.position;
				float2 position2 = controlPoint2.position;
				float2 y = position;
				float2 st = position + controlPoint.tangentRt;
				float2 et = position2 + controlPoint2.tangentLt;
				int index2 = num2;
				float num4 = 0f;
				float num5 = 0f;
				for (int j = 0; j < splineDetail; j++)
				{
					JobContourPoint value = m_ContourPoints[num2];
					float t = (float)j / num3;
					float2 @float = (value.position = BezierPoint(st, position, position2, et, t));
					num4 += math.distance(@float, y);
					m_ContourPoints[num2++] = value;
					y = @float;
				}
				y = position;
				for (int k = 0; k < splineDetail; k++)
				{
					JobContourPoint value2 = m_ContourPoints[index2];
					num5 += math.distance(value2.position, y);
					value2.ptData.x = (flag ? InterpolateSmooth(controlPoint.cpInfo.x, controlPoint2.cpInfo.x, num5 / num4) : InterpolateLinear(controlPoint.cpInfo.x, controlPoint2.cpInfo.x, num5 / num4));
					m_ContourPoints[index2++] = value2;
					y = value2.position;
				}
			}
			m_ContourPointCount = num2;
			int tessPointCount = 0;
			for (int l = 0; l < contourPointCount; l++)
			{
				if ((l + 1) % splineDetail == 0)
				{
					continue;
				}
				int num6 = ((l == 0) ? (contourPointCount - 1) : (l - 1));
				int index3 = (l + 1) % contourPointCount;
				num6 = ((l % splineDetail == 0) ? (num6 - 1) : num6);
				JobContourPoint contourPoint = GetContourPoint(num6);
				JobContourPoint contourPoint2 = GetContourPoint(l);
				JobContourPoint contourPoint3 = GetContourPoint(index3);
				float2 x = contourPoint2.position - contourPoint.position;
				float2 x2 = contourPoint3.position - contourPoint2.position;
				if (!(math.length(x) < kEpsilon) && !(math.length(x2) < kEpsilon))
				{
					float2 float2 = math.normalize(x);
					float2 float3 = math.normalize(x2);
					float2 = new float2(0f - float2.y, float2.x);
					float3 = new float2(0f - float3.y, float3.x);
					float2 x3 = math.normalize(float2) + math.normalize(float3);
					float2 float4 = math.normalize(x3);
					if (math.any(x3) && math.any(float4))
					{
						m_TessPoints[tessPointCount++] = contourPoint2.position + float4 * borderPivot;
					}
				}
			}
			m_TessPointCount = tessPointCount;
		}

		private void TessellateContour()
		{
			GenerateContour();
			SpriteShapeSegment value = m_GeomArray[0];
			value.vertexCount = 0;
			value.geomIndex = 0;
			value.indexCount = 0;
			value.spriteIndex = -1;
			if (math.all(m_ShapeParams.shapeData.xw) && m_TessPointCount > 0)
			{
				if (kOptimizeRender > 0f)
				{
					OptimizePoints(kRenderQuality, ref m_TessPoints, ref m_TessPointCount);
				}
				int tessPointCount = m_TessPointCount;
				NativeArray<int2> edges = new NativeArray<int2>(tessPointCount - 1, Allocator.Temp);
				NativeArray<float2> points = new NativeArray<float2>(tessPointCount - 1, Allocator.Temp);
				for (int i = 0; i < points.Length; i++)
				{
					points[i] = m_TessPoints[i];
				}
				for (int j = 0; j < tessPointCount - 2; j++)
				{
					int2 value2 = edges[j];
					value2.x = j;
					value2.y = j + 1;
					edges[j] = value2;
				}
				int2 value3 = edges[tessPointCount - 2];
				value3.x = tessPointCount - 2;
				value3.y = 0;
				edges[tessPointCount - 2] = value3;
				int outVertexCount = 0;
				int outIndexCount = 0;
				int outEdgeCount = 0;
				NativeArray<float2> outVertices = new NativeArray<float2>(m_TessPointCount * m_TessPointCount, Allocator.Temp);
				NativeArray<int> outIndices = new NativeArray<int>(m_TessPointCount * m_TessPointCount, Allocator.Temp);
				NativeArray<int2> outEdges = new NativeArray<int2>(m_TessPointCount * m_TessPointCount, Allocator.Temp);
				UnityEngine.U2D.Common.UTess.ModuleHandle.Tessellate(Allocator.Temp, in points, in edges, ref outVertices, out outVertexCount, ref outIndices, out outIndexCount, ref outEdges, out outEdgeCount);
				if (outIndexCount > 0)
				{
					for (m_ActiveIndexCount = 0; m_ActiveIndexCount < outIndexCount; m_ActiveIndexCount++)
					{
						m_IndexArray[m_ActiveIndexCount] = (ushort)outIndices[m_ActiveIndexCount];
					}
					for (m_ActiveVertexCount = 0; m_ActiveVertexCount < outVertexCount; m_ActiveVertexCount++)
					{
						m_PosArray[m_ActiveVertexCount] = new Vector3(outVertices[m_ActiveVertexCount].x, outVertices[m_ActiveVertexCount].y, 0f);
					}
					int num = (value.indexCount = m_ActiveIndexCount);
					m_IndexDataCount = num;
					num = (value.vertexCount = m_ActiveVertexCount);
					m_VertexDataCount = num;
				}
				else
				{
					SetResult(SpriteShapeGeneratorResult.ErrorDefaultQuadCreated);
				}
				outVertices.Dispose();
				outIndices.Dispose();
				outEdges.Dispose();
				edges.Dispose();
				points.Dispose();
			}
			if (m_TanArray.Length > 1)
			{
				for (int k = 0; k < m_ActiveVertexCount; k++)
				{
					m_TanArray[k] = new Vector4(1f, 0f, 0f, -1f);
				}
			}
			m_GeomArray[0] = value;
		}

		private void TessellateContourMainThread()
		{
			GenerateContour();
			SpriteShapeSegment value = m_GeomArray[0];
			value.vertexCount = 0;
			value.geomIndex = 0;
			value.indexCount = 0;
			value.spriteIndex = -1;
			if (math.all(m_ShapeParams.shapeData.xw) && m_TessPointCount > 0)
			{
				if (kOptimizeRender > 0f)
				{
					OptimizePoints(kRenderQuality, ref m_TessPoints, ref m_TessPointCount);
				}
				ContourVertex[] array = new ContourVertex[m_TessPointCount];
				for (int j = 0; j < m_TessPointCount; j++)
				{
					array[j] = new ContourVertex
					{
						Position = new Vec3
						{
							X = m_TessPoints[j].x,
							Y = m_TessPoints[j].y
						}
					};
				}
				Tess tess = new Tess();
				tess.AddContour(array, ContourOrientation.Original);
				tess.Tessellate(WindingRule.NonZero, ElementType.Polygons, 3);
				ushort[] array2 = tess.Elements.Select((int i) => (ushort)i).ToArray();
				Vector2[] array3 = tess.Vertices.Select((ContourVertex v) => new Vector2(v.Position.X, v.Position.Y)).ToArray();
				m_IndexDataCount = array2.Length;
				m_VertexDataCount = array3.Length;
				if (array3.Length != 0)
				{
					for (m_ActiveIndexCount = 0; m_ActiveIndexCount < m_IndexDataCount; m_ActiveIndexCount++)
					{
						m_IndexArray[m_ActiveIndexCount] = array2[m_ActiveIndexCount];
					}
					for (m_ActiveVertexCount = 0; m_ActiveVertexCount < m_VertexDataCount; m_ActiveVertexCount++)
					{
						Vector3 value2 = new Vector3(array3[m_ActiveVertexCount].x, array3[m_ActiveVertexCount].y, 0f);
						m_PosArray[m_ActiveVertexCount] = value2;
					}
					value.indexCount = m_ActiveIndexCount;
					value.vertexCount = m_ActiveVertexCount;
				}
			}
			if (m_TanArray.Length > 1)
			{
				for (int k = 0; k < m_ActiveVertexCount; k++)
				{
					m_TanArray[k] = new Vector4(1f, 0f, 0f, -1f);
				}
			}
			m_GeomArray[0] = value;
		}

		private void CalculateBoundingBox()
		{
			if (vertexArrayCount != 0 || contourPointCount != 0)
			{
				Bounds value = default(Bounds);
				float2 @float = ((vertexArrayCount != 0) ? new float2(m_PosArray[0].x, m_PosArray[0].y) : new float2(m_ContourPoints[0].position.x, m_ContourPoints[0].position.y));
				float2 x = @float;
				for (int i = 0; i < vertexArrayCount; i++)
				{
					float3 float2 = m_PosArray[i];
					@float = math.min(@float, float2.xy);
					x = math.max(x, float2.xy);
				}
				for (int j = 0; j < contourPointCount; j++)
				{
					float2 y = new float2(m_ContourPoints[j].position.x, m_ContourPoints[j].position.y);
					@float = math.min(@float, y);
					x = math.max(x, y);
				}
				value.SetMinMax(new Vector3(@float.x, @float.y, 0f), new Vector3(x.x, x.y, 0f));
				m_Bounds[0] = value;
			}
		}

		private void CalculateTexCoords()
		{
			SpriteShapeSegment spriteShapeSegment = m_GeomArray[0];
			if (m_ShapeParams.splineData.x > 0)
			{
				float3 @float = m_Bounds[0].extents * 2f;
				float3 float2 = m_Bounds[0].center - m_Bounds[0].extents;
				for (int i = 0; i < spriteShapeSegment.vertexCount; i++)
				{
					Vector3 vector = m_PosArray[i];
					Vector2 value = m_Uv0Array[i];
					float3 float3 = (new float3(vector.x, vector.y, vector.z) - float2) / @float * m_ShapeParams.fillData.x;
					value.x = float3.x;
					value.y = float3.y;
					m_Uv0Array[i] = value;
				}
			}
			else
			{
				for (int j = 0; j < spriteShapeSegment.vertexCount; j++)
				{
					Vector3 vector2 = m_PosArray[j];
					Vector2 value2 = m_Uv0Array[j];
					float3 float4 = math.transform(m_Transform, new float3(vector2.x, vector2.y, vector2.z));
					value2.x = float4.x / m_ShapeParams.fillData.y;
					value2.y = float4.y / m_ShapeParams.fillData.z;
					m_Uv0Array[j] = value2;
				}
			}
		}

		private void CopyVertexData(ref NativeSlice<Vector3> outPos, ref NativeSlice<Vector2> outUV0, ref NativeSlice<Vector4> outTan, int outIndex, NativeArray<JobShapeVertex> inVertices, int inIndex, float sOrder)
		{
			_ = outPos[outIndex];
			_ = outUV0[outIndex];
			float3 @float = new float3(inVertices[inIndex].pos.x, inVertices[inIndex].pos.y, sOrder);
			float3 float2 = new float3(inVertices[inIndex + 1].pos.x, inVertices[inIndex + 1].pos.y, sOrder);
			float3 float3 = new float3(inVertices[inIndex + 2].pos.x, inVertices[inIndex + 2].pos.y, sOrder);
			float3 float4 = new float3(inVertices[inIndex + 3].pos.x, inVertices[inIndex + 3].pos.y, sOrder);
			outPos[outIndex] = @float;
			outUV0[outIndex] = inVertices[inIndex].uv;
			outPos[outIndex + 1] = float2;
			outUV0[outIndex + 1] = inVertices[inIndex + 1].uv;
			outPos[outIndex + 2] = float3;
			outUV0[outIndex + 2] = inVertices[inIndex + 2].uv;
			outPos[outIndex + 3] = float4;
			outUV0[outIndex + 3] = inVertices[inIndex + 3].uv;
			if (outTan.Length > 1)
			{
				outTan[outIndex] = inVertices[inIndex].tan;
				outTan[outIndex + 1] = inVertices[inIndex + 1].tan;
				outTan[outIndex + 2] = inVertices[inIndex + 2].tan;
				outTan[outIndex + 3] = inVertices[inIndex + 3].tan;
			}
		}

		private int CopySegmentRenderData(JobSpriteInfo ispr, ref NativeSlice<Vector3> outPos, ref NativeSlice<Vector2> outUV0, ref NativeSlice<Vector4> outTan, ref int outCount, ref NativeArray<ushort> indexData, ref int indexCount, NativeArray<JobShapeVertex> inVertices, int inCount, float sOrder)
		{
			if (inCount < 4)
			{
				return -1;
			}
			int num = 0;
			if (indexCount + inCount + inCount / 2 >= indexData.Length)
			{
				SetResult(SpriteShapeGeneratorResult.ErrorVertexLimitReached);
			}
			int num2 = 0;
			while (num2 < inCount)
			{
				CopyVertexData(ref outPos, ref outUV0, ref outTan, outCount, inVertices, num2, sOrder);
				indexData[indexCount++] = (ushort)num;
				indexData[indexCount++] = (ushort)(3 + num);
				indexData[indexCount++] = (ushort)(1 + num);
				indexData[indexCount++] = (ushort)num;
				indexData[indexCount++] = (ushort)(2 + num);
				indexData[indexCount++] = (ushort)(3 + num);
				num2 += 4;
				outCount += 4;
				num += 4;
			}
			return outCount;
		}

		private void GetLineSegments(JobSpriteInfo sprInfo, JobSegmentInfo segment, float2 whsize, ref float2 vlt, ref float2 vlb, ref float2 vrt, ref float2 vrb)
		{
			JobControlPoint controlPoint = GetControlPoint(segment.sgInfo.x);
			JobControlPoint controlPoint2 = GetControlPoint(segment.sgInfo.y);
			GetSegmentBoundaryColumn(segment, sprInfo, whsize, controlPoint.position, controlPoint2.position, end: false, ref vlt, ref vlb);
			GetSegmentBoundaryColumn(segment, sprInfo, whsize, controlPoint2.position, controlPoint.position, end: true, ref vrt, ref vrb);
		}

		private void TessellateSegment(int segmentIndex, JobSpriteInfo sprInfo, JobSegmentInfo segment, float2 whsize, float4 border, float pxlWidth, NativeArray<JobShapeVertex> vertices, int vertexCount, bool useClosure, bool validHead, bool validTail, bool firstSegment, bool finalSegment, ref NativeArray<JobShapeVertex> outputVertices, ref int outputCount)
		{
			int num = 0;
			float2 rt;
			float2 zero;
			float2 @float;
			float2 rb = (rt = (@float = (zero = float2.zero)));
			float4 stretcher = new float4(1f, 1f, 0f, 0f);
			JobShapeVertex value = default(JobShapeVertex);
			JobShapeVertex value2 = default(JobShapeVertex);
			JobShapeVertex value3 = default(JobShapeVertex);
			JobShapeVertex value4 = default(JobShapeVertex);
			int num2 = vertexCount - 1;
			int num3 = num2 - 1;
			int num4 = outputCount + num2 * 4;
			int4 sprite = vertices[0].sprite;
			if (num4 >= outputVertices.Length)
			{
				SetResult(SpriteShapeGeneratorResult.ErrorVertexLimitReached);
			}
			float num5 = 0f;
			float x = border.x;
			float num6 = whsize.x - border.z;
			float x2 = whsize.x;
			float num7 = num6 - x;
			float x3 = x / x2;
			float num8 = num7 / pxlWidth;
			float pivot = 0.5f - sprInfo.metaInfo.y;
			bool flag = false;
			if (math.abs(segment.sgInfo.x - segment.sgInfo.y) == 1 && segmentCount > 1)
			{
				flag = FetchStretcher(segmentIndex, sprInfo, segment, whsize, validHead, validTail, ref stretcher);
			}
			for (int i = 0; i < num2; i++)
			{
				bool flag2 = num2 > 1 && i == num3;
				bool num9 = i != 0 && !flag2;
				JobShapeVertex jobShapeVertex = vertices[i];
				JobShapeVertex jobShapeVertex2 = vertices[i + 1];
				float2 float2 = (flag2 ? jobShapeVertex.pos : vertices[i + 2].pos);
				zero = value2.pos;
				@float = value4.pos;
				if (num9)
				{
					GenerateColumnsTri(jobShapeVertex.pos, jobShapeVertex2.pos, float2, whsize, flag2, ref rt, ref rb, jobShapeVertex2.meta.x * 0.5f, pivot);
				}
				else
				{
					if (!flag2)
					{
						GetSegmentBoundaryColumn(segment, sprInfo, whsize, jobShapeVertex.pos, jobShapeVertex2.pos, end: false, ref zero, ref @float);
					}
					if (flag2 && useClosure)
					{
						rb = m_FirstLB;
						rt = m_FirstLT;
					}
					else
					{
						GetSegmentBoundaryColumn(segment, sprInfo, whsize, jobShapeVertex2.pos, float2, flag2, ref rt, ref rb);
					}
				}
				if (i == 0 && segment.sgInfo.x == 0)
				{
					m_FirstLB = @float;
					m_FirstLT = zero;
				}
				if ((!math.any(zero) && !math.any(@float)) || (!math.any(rt) && !math.any(rb)))
				{
					continue;
				}
				float2 float3 = math.normalize(rt - zero);
				float4 tan = new float4(float3.x, float3.y, 0f, -1f);
				value.pos = zero;
				value.meta = jobShapeVertex.meta;
				value.sprite = sprite;
				value.tan = tan;
				value2.pos = rt;
				value2.meta = jobShapeVertex2.meta;
				value2.sprite = sprite;
				value2.tan = tan;
				value3.pos = @float;
				value3.meta = jobShapeVertex.meta;
				value3.sprite = sprite;
				value3.tan = tan;
				value4.pos = rb;
				value4.meta = jobShapeVertex2.meta;
				value4.sprite = sprite;
				value4.tan = tan;
				if (validHead && i == 0)
				{
					value.uv.x = (value.uv.y = (value2.uv.y = (value3.uv.x = 0f)));
					value2.uv.x = (value4.uv.x = border.x / whsize.x);
					value3.uv.y = (value4.uv.y = 1f);
					value.sprite.z = (value3.sprite.z = ((!firstSegment) ? 1 : 0));
				}
				else if (validTail && i == num3)
				{
					value.uv.y = (value2.uv.y = 0f);
					value.uv.x = (value3.uv.x = (whsize.x - border.z) / whsize.x);
					value2.uv.x = (value3.uv.y = (value4.uv.x = (value4.uv.y = 1f)));
					value2.sprite.z = (value4.sprite.z = ((!finalSegment) ? 1 : 0));
				}
				else
				{
					if (num7 - num5 < kEpsilonRelaxed)
					{
						x3 = x / x2;
						num5 = 0f;
					}
					num5 += math.distance(jobShapeVertex2.pos, jobShapeVertex.pos) * num8;
					float num10 = (num5 + x) / x2;
					if (num5 - num7 > kEpsilonRelaxed)
					{
						num10 = num6 / x2;
						num5 = num6;
					}
					value.uv.y = (value2.uv.y = 0f);
					value.uv.x = (value3.uv.x = x3);
					value2.uv.x = (value4.uv.x = num10);
					value3.uv.y = (value4.uv.y = 1f);
					x3 = num10;
				}
				value.uv.x = value.uv.x * sprInfo.uvInfo.z + sprInfo.uvInfo.x;
				value.uv.y = value.uv.y * sprInfo.uvInfo.w + sprInfo.uvInfo.y;
				outputVertices[num++] = value;
				value2.uv.x = value2.uv.x * sprInfo.uvInfo.z + sprInfo.uvInfo.x;
				value2.uv.y = value2.uv.y * sprInfo.uvInfo.w + sprInfo.uvInfo.y;
				outputVertices[num++] = value2;
				value3.uv.x = value3.uv.x * sprInfo.uvInfo.z + sprInfo.uvInfo.x;
				value3.uv.y = value3.uv.y * sprInfo.uvInfo.w + sprInfo.uvInfo.y;
				outputVertices[num++] = value3;
				value4.uv.x = value4.uv.x * sprInfo.uvInfo.z + sprInfo.uvInfo.x;
				value4.uv.y = value4.uv.y * sprInfo.uvInfo.w + sprInfo.uvInfo.y;
				outputVertices[num++] = value4;
			}
			if (flag)
			{
				StretchCorners(segment, outputVertices, num, validHead, validTail, stretcher);
			}
			outputCount = num;
		}

		private bool SkipSegment(JobSegmentInfo isi)
		{
			bool flag = isi.sgInfo.z < 0;
			if (!flag)
			{
				flag = !math.any(GetSpriteInfo(isi.sgInfo.z).uvInfo);
			}
			if (flag)
			{
				int i = GetContourIndex(isi.sgInfo.x);
				for (int endContourIndexOfSegment = GetEndContourIndexOfSegment(isi); i < endContourIndexOfSegment; i++)
				{
					JobContourPoint contourPoint = GetContourPoint(i);
					m_ColliderPoints[m_ColliderDataCount++] = contourPoint.position;
				}
			}
			return flag;
		}

		private float InterpolateLinear(float a, float b, float t)
		{
			return math.lerp(a, b, t);
		}

		private float InterpolateSmooth(float a, float b, float t)
		{
			float num = (1f - math.cos(t * MathF.PI)) / 2f;
			return a * (1f - num) + b * num;
		}

		private void TessellateSegments()
		{
			bool flag = GetControlPoint(0).cpData.z == kModeContinous && isCarpet;
			new float2(0f, 0f);
			for (int i = 0; i < segmentCount; i++)
			{
				JobSegmentInfo segmentInfo = GetSegmentInfo(i);
				if (SkipSegment(segmentInfo))
				{
					continue;
				}
				JobShapeVertex value = default(JobShapeVertex);
				JobSpriteInfo spriteInfo = GetSpriteInfo(segmentInfo.sgInfo.z);
				int num = 0;
				int z = segmentInfo.sgInfo.z;
				float num2 = 1f / spriteInfo.metaInfo.x;
				float2 whsize = new float2(spriteInfo.metaInfo.z, spriteInfo.metaInfo.w) * num2;
				float4 border = spriteInfo.border * num2;
				JobControlPoint controlPoint = GetControlPoint(segmentInfo.sgInfo.x);
				JobControlPoint controlPoint2 = GetControlPoint(segmentInfo.sgInfo.y);
				bool flag2 = m_ControlPoints[0].cpData.z == kModeContinous && segmentInfo.sgInfo.y == controlPointCount - 1;
				bool flag3 = i == 0 && !isCarpet && !flag2;
				bool flag4 = hasSpriteBorder && border.x > 0f && (controlPoint.exData.z == 0 || flag3);
				flag4 = ((m_ControlPoints[0].cpData.z != kModeContinous) ? flag4 : (flag4 && !isCarpet));
				bool flag5 = i == segmentCount - 1 && !isCarpet && !flag2;
				bool flag6 = hasSpriteBorder && border.z > 0f && (controlPoint2.exData.z == 0 || flag5);
				flag6 = ((m_ControlPoints[0].cpData.z != kModeContinous) ? flag6 : (flag6 && !isCarpet));
				float num3 = 0f;
				float x = border.x;
				float num4 = whsize.x - border.z - x;
				float w = segmentInfo.spriteInfo.w;
				float num5 = math.floor(w / num4);
				num5 = ((num5 == 0f) ? 1f : num5);
				num4 = (isAdaptive ? (w / num5) : num4);
				if (num4 < kEpsilon)
				{
					SetResult(SpriteShapeGeneratorResult.ErrorSpritesWrongBorder);
				}
				int contourIndex = GetContourIndex(segmentInfo.sgInfo.x);
				int endContourIndexOfSegment = GetEndContourIndexOfSegment(segmentInfo);
				if (contourIndex == 0)
				{
					flag4 = flag4 && !flag;
				}
				if (flag4)
				{
					JobContourPoint contourPoint = GetContourPoint(contourIndex);
					float2 position = contourPoint.position;
					float2 position2 = GetContourPoint(contourIndex + 1).position;
					value.pos = position + math.normalize(position - position2) * border.x;
					value.meta.x = contourPoint.ptData.x;
					value.sprite.x = z;
					m_VertexData[num++] = value;
				}
				float num6 = 0f;
				int j = contourIndex;
				int num7 = 0;
				value.sprite.z = 0;
				for (; j < endContourIndexOfSegment; j++)
				{
					num7 = j + 1;
					JobContourPoint contourPoint2 = GetContourPoint(j);
					JobContourPoint contourPoint3 = GetContourPoint(num7);
					float2 @float = contourPoint2.position;
					float2 float2 = @float;
					float2 x2 = contourPoint3.position - @float;
					float num8 = math.length(x2);
					if (!(num8 > kEpsilon))
					{
						continue;
					}
					float x3 = contourPoint2.ptData.x;
					float x4 = contourPoint3.ptData.x;
					float num9 = 0f;
					num6 += num8;
					bool flag7 = num == 0;
					float2 float3 = math.normalize(x2);
					value.pos = contourPoint2.position;
					value.meta.x = contourPoint2.ptData.x;
					value.sprite.x = z;
					if (num > 0)
					{
						flag7 = math.length(m_VertexData[num - 1].pos - value.pos) > kEpsilonRelaxed;
					}
					if (flag7)
					{
						m_VertexData[num++] = value;
					}
					while (num6 > num4)
					{
						float v = num4 - num3;
						float2 float4 = new float2(v);
						float2 = @float + float3 * float4;
						num9 += math.length(float2 - @float);
						value.pos = float2;
						value.meta.x = InterpolateLinear(x3, x4, num9 / num8);
						value.sprite.x = z;
						if (math.any(m_VertexData[num - 1].pos - value.pos))
						{
							m_VertexData[num++] = value;
						}
						num6 -= num4;
						@float = float2;
						num3 = 0f;
					}
					num3 = num6;
				}
				if (num6 > kEpsilon)
				{
					JobContourPoint contourPoint4 = GetContourPoint(endContourIndexOfSegment);
					value.pos = contourPoint4.position;
					value.meta.x = contourPoint4.ptData.x;
					value.sprite.x = z;
					m_VertexData[num++] = value;
				}
				if (flag6)
				{
					JobContourPoint contourPoint5 = GetContourPoint(endContourIndexOfSegment);
					float2 position3 = contourPoint5.position;
					float2 position4 = GetContourPoint(endContourIndexOfSegment - 1).position;
					value.pos = position3 + math.normalize(position3 - position4) * border.z;
					value.meta.x = contourPoint5.ptData.x;
					value.sprite.x = z;
					m_VertexData[num++] = value;
				}
				int outputCount = 0;
				TessellateSegment(i, spriteInfo, segmentInfo, whsize, border, num4, m_VertexData, num, flag2, flag4, flag6, flag3, flag5, ref m_OutputVertexData, ref outputCount);
				if (outputCount != 0)
				{
					float sOrder = (float)(i + 1) * kEpsilonOrder + (float)segmentInfo.sgInfo.z * kEpsilonOrder * 0.001f;
					CopySegmentRenderData(spriteInfo, ref m_PosArray, ref m_Uv0Array, ref m_TanArray, ref m_VertexDataCount, ref m_IndexArray, ref m_IndexDataCount, m_OutputVertexData, outputCount, sOrder);
					if (hasCollider)
					{
						JobSpriteInfo jobSpriteInfo = ((spriteInfo.metaInfo.x == 0f) ? GetSpriteInfo(segmentInfo.sgInfo.w) : spriteInfo);
						outputCount = 0;
						num2 = 1f / jobSpriteInfo.metaInfo.x;
						whsize = new float2(jobSpriteInfo.metaInfo.z, jobSpriteInfo.metaInfo.w) * num2;
						border = jobSpriteInfo.border * num2;
						x = border.x;
						num4 = whsize.x - border.z - x;
						TessellateSegment(i, jobSpriteInfo, segmentInfo, whsize, border, num4, m_VertexData, num, flag2, flag4, flag6, flag3, flag5, ref m_OutputVertexData, ref outputCount);
						UpdateCollider(segmentInfo, jobSpriteInfo, m_OutputVertexData, outputCount, ref m_ColliderPoints, ref m_ColliderDataCount);
					}
					SpriteShapeSegment value2 = m_GeomArray[i + 1];
					value2.geomIndex = i + 1;
					value2.indexCount = m_IndexDataCount - m_ActiveIndexCount;
					value2.vertexCount = m_VertexDataCount - m_ActiveVertexCount;
					value2.spriteIndex = segmentInfo.sgInfo.z;
					m_GeomArray[i + 1] = value2;
					m_ActiveIndexCount = m_IndexDataCount;
					m_ActiveVertexCount = m_VertexDataCount;
				}
			}
			m_GeomArrayCount = segmentCount + 1;
			m_IndexArrayCount = m_IndexDataCount;
			m_VertexArrayCount = m_VertexDataCount;
			m_ColliderPointCount = m_ColliderDataCount;
		}

		private bool FetchStretcher(int segmentIndex, JobSpriteInfo sprInfo, JobSegmentInfo segment, float2 whsize, bool validHead, bool validTail, ref float4 stretcher)
		{
			bool flag = false;
			bool flag2 = false;
			int num = segmentCount - 1;
			int index = ((segmentIndex == 0) ? num : (segmentIndex - 1));
			int index2 = ((segmentIndex != num) ? (segmentIndex + 1) : 0);
			JobSegmentInfo segmentInfo = GetSegmentInfo(index);
			JobSegmentInfo segmentInfo2 = GetSegmentInfo(index2);
			JobControlPoint controlPoint = GetControlPoint(segment.sgInfo.x);
			JobControlPoint controlPoint2 = GetControlPoint(segment.sgInfo.y);
			bool flag3 = controlPoint.cpData.y == 2 && math.abs(segmentInfo.sgInfo.x - segmentInfo.sgInfo.y) == 1;
			bool flag4 = controlPoint2.cpData.y == 2 && math.abs(segmentInfo2.sgInfo.x - segmentInfo2.sgInfo.y) == 1;
			int num2 = controlPointCount - 1;
			if (!isCarpet)
			{
				flag3 = flag3 && segment.sgInfo.x != 0;
				flag4 = flag4 && segment.sgInfo.y != num2;
			}
			if (flag3 || flag4)
			{
				float2 vlt = float2.zero;
				float2 vlb = float2.zero;
				float2 vrt = float2.zero;
				float2 vrb = float2.zero;
				GetLineSegments(sprInfo, segment, whsize, ref vlt, ref vlb, ref vrt, ref vrb);
				float2 @float = vlt;
				float2 float2 = vlb;
				float2 float3 = vrt;
				float2 float4 = vrb;
				float2 result = vlt;
				float2 result2 = vlb;
				float2 result3 = vrt;
				float2 result4 = vrb;
				ExtendSegment(ref vlt, ref vrt);
				ExtendSegment(ref vlb, ref vrb);
				if (flag3)
				{
					if (math.any(m_Intersectors[segment.sgInfo.x].top) && math.any(m_Intersectors[segment.sgInfo.x].bottom))
					{
						result = m_Intersectors[segment.sgInfo.x].top;
						result2 = m_Intersectors[segment.sgInfo.x].bottom;
						flag = true;
					}
					else
					{
						if (1 == controlPoint.exData.z)
						{
							float2 vlt2 = float2.zero;
							float2 vlb2 = float2.zero;
							float2 vrt2 = float2.zero;
							float2 vrb2 = float2.zero;
							GetLineSegments(sprInfo, segmentInfo, whsize, ref vlt2, ref vlb2, ref vrt2, ref vrb2);
							ExtendSegment(ref vlt2, ref vrt2);
							ExtendSegment(ref vlb2, ref vrb2);
							bool num3 = LineIntersection(kEpsilon, vlt2, vrt2, vlt, vrt, ref result);
							bool flag5 = LineIntersection(kEpsilon, vlb2, vrb2, vlb, vrb, ref result2);
							flag = num3 && flag5;
						}
						if (flag)
						{
							JobIntersectPoint value = m_Intersectors[segment.sgInfo.x];
							value.top = result;
							value.bottom = result2;
							m_Intersectors[segment.sgInfo.x] = value;
						}
					}
				}
				if (flag4)
				{
					if (math.any(m_Intersectors[segment.sgInfo.y].top) && math.any(m_Intersectors[segment.sgInfo.y].bottom))
					{
						result3 = m_Intersectors[segment.sgInfo.y].top;
						result4 = m_Intersectors[segment.sgInfo.y].bottom;
						flag2 = true;
					}
					else
					{
						if (1 == controlPoint2.exData.z)
						{
							float2 vlt3 = float2.zero;
							float2 vlb3 = float2.zero;
							float2 vrt3 = float2.zero;
							float2 vrb3 = float2.zero;
							GetLineSegments(sprInfo, segmentInfo2, whsize, ref vlt3, ref vlb3, ref vrt3, ref vrb3);
							ExtendSegment(ref vlt3, ref vrt3);
							ExtendSegment(ref vlb3, ref vrb3);
							bool num4 = LineIntersection(kEpsilon, vlt, vrt, vlt3, vrt3, ref result3);
							bool flag6 = LineIntersection(kEpsilon, vlb, vrb, vlb3, vrb3, ref result4);
							flag2 = num4 && flag6;
						}
						if (flag2)
						{
							JobIntersectPoint value2 = m_Intersectors[segment.sgInfo.y];
							value2.top = result3;
							value2.bottom = result4;
							m_Intersectors[segment.sgInfo.y] = value2;
						}
					}
				}
				if (flag || flag2)
				{
					float2 float5 = (@float + float2) * 0.5f;
					float2 float6 = (float3 + float4) * 0.5f;
					float num5 = math.length(float5 - float6);
					float num6 = math.length(result - result3);
					float num7 = math.length(result2 - result4);
					stretcher.x = num6 / num5;
					stretcher.y = num7 / num5;
					stretcher.z = (flag ? 1f : 0f);
					stretcher.w = (flag2 ? 1f : 0f);
				}
			}
			return flag || flag2;
		}

		private void StretchCorners(JobSegmentInfo segment, NativeArray<JobShapeVertex> vertices, int vertexCount, bool validHead, bool validTail, float4 stretcher)
		{
			if (vertexCount > 0)
			{
				int num = (validHead ? 4 : 0);
				float2 @float = vertices[num].pos;
				float2 pos = vertices[num].pos;
				float2 pos2 = vertices[vertexCount - 3].pos;
				_ = vertices[vertexCount - 3];
				float2 float2 = vertices[num + 2].pos;
				float2 pos3 = vertices[num + 2].pos;
				float2 pos4 = vertices[vertexCount - 1].pos;
				_ = vertices[vertexCount - 1];
				if (math.any(m_Intersectors[segment.sgInfo.x].top) && math.any(m_Intersectors[segment.sgInfo.x].bottom))
				{
					@float = m_Intersectors[segment.sgInfo.x].top;
					float2 = m_Intersectors[segment.sgInfo.x].bottom;
				}
				if (math.any(m_Intersectors[segment.sgInfo.y].top) && math.any(m_Intersectors[segment.sgInfo.y].bottom))
				{
					pos2 = m_Intersectors[segment.sgInfo.y].top;
					pos4 = m_Intersectors[segment.sgInfo.y].bottom;
				}
				for (int i = num; i < vertexCount; i += 4)
				{
					JobShapeVertex value = vertices[i];
					JobShapeVertex value2 = vertices[i + 1];
					JobShapeVertex value3 = vertices[i + 2];
					JobShapeVertex value4 = vertices[i + 3];
					value.pos = @float + (vertices[i].pos - pos) * stretcher.x;
					value2.pos = @float + (vertices[i + 1].pos - pos) * stretcher.x;
					value3.pos = float2 + (vertices[i + 2].pos - pos3) * stretcher.y;
					value4.pos = float2 + (vertices[i + 3].pos - pos3) * stretcher.y;
					vertices[i] = value;
					vertices[i + 1] = value2;
					vertices[i + 2] = value3;
					vertices[i + 3] = value4;
				}
				JobShapeVertex value5 = vertices[num];
				JobShapeVertex value6 = vertices[num + 2];
				value5.pos = @float;
				value6.pos = float2;
				vertices[num] = value5;
				vertices[num + 2] = value6;
				JobShapeVertex value7 = vertices[vertexCount - 3];
				JobShapeVertex value8 = vertices[vertexCount - 1];
				value7.pos = pos2;
				value8.pos = pos4;
				vertices[vertexCount - 3] = value7;
				vertices[vertexCount - 1] = value8;
			}
		}

		private void ExtendSegment(ref float2 l0, ref float2 r0)
		{
			float2 @float = l0;
			float2 float2 = r0;
			float2 float3 = math.normalize(float2 - @float);
			r0 = float2 + float3 * kExtendSegment;
			l0 = @float + -float3 * kExtendSegment;
		}

		private bool GetIntersection(int cp, int ct, JobSpriteInfo ispr, ref float2 lt0, ref float2 lb0, ref float2 rt0, ref float2 rb0, ref float2 lt1, ref float2 lb1, ref float2 rt1, ref float2 rb1, ref float2 tp, ref float2 bt)
		{
			new float2(0f, 0f);
			int index = ((cp == 0) ? (controlPointCount - 1) : (cp - 1));
			int index2 = (cp + 1) % controlPointCount;
			float pivot = 0.5f - ispr.metaInfo.y;
			JobControlPoint controlPoint = GetControlPoint(index);
			JobControlPoint controlPoint2 = GetControlPoint(cp);
			JobControlPoint controlPoint3 = GetControlPoint(index2);
			float num = 1f / ispr.metaInfo.x;
			float2 whsize = new float2(ispr.texRect.z, ispr.texRect.w) * num;
			float4 @float = ispr.border * num;
			float y = @float.y;
			_ = whsize.y - @float.y;
			GenerateColumnsBi(controlPoint.position, controlPoint2.position, whsize, flip: false, ref lb0, ref lt0, 0.5f, pivot);
			GenerateColumnsBi(controlPoint2.position, controlPoint.position, whsize, flip: false, ref rt0, ref rb0, 0.5f, pivot);
			GenerateColumnsBi(controlPoint2.position, controlPoint3.position, whsize, flip: false, ref lb1, ref lt1, 0.5f, pivot);
			GenerateColumnsBi(controlPoint3.position, controlPoint2.position, whsize, flip: false, ref rt1, ref rb1, 0.5f, pivot);
			rt0 += math.normalize(rt0 - lt0) * kExtendSegment;
			rb0 += math.normalize(rb0 - lb0) * kExtendSegment;
			lt1 += math.normalize(lt1 - rt1) * kExtendSegment;
			lb1 += math.normalize(lb1 - rb1) * kExtendSegment;
			bool flag = LineIntersection(kEpsilon, lt0, rt0, lt1, rt1, ref tp);
			if (!LineIntersection(kEpsilon, lb0, rb0, lb1, rb1, ref bt) && !flag)
			{
				return false;
			}
			return true;
		}

		private bool AttachCorner(int cp, int ct, JobSpriteInfo ispr, ref NativeArray<JobControlPoint> newPoints, ref int activePoint)
		{
			float2 rt2;
			float2 lb2;
			float2 lt2;
			float2 rb;
			float2 rt;
			float2 lb;
			float2 lt;
			float2 bt;
			float2 tp;
			float2 rb2 = (rt2 = (lb2 = (lt2 = (rb = (rt = (lb = (lt = (bt = (tp = new float2(0f, 0f))))))))));
			float pivot = 0.5f - ispr.metaInfo.y;
			int index = ((cp == 0) ? (controlPointCount - 1) : (cp - 1));
			int index2 = (cp + 1) % controlPointCount;
			JobControlPoint controlPoint = GetControlPoint(index);
			JobControlPoint controlPoint2 = GetControlPoint(cp);
			JobControlPoint controlPoint3 = GetControlPoint(index2);
			float num = 1f / ispr.metaInfo.x;
			float2 whsize = new float2(ispr.texRect.z, ispr.texRect.w) * num;
			float4 @float = ispr.border * num;
			float y = @float.y;
			float num2 = whsize.y - @float.y - y;
			if (!GetIntersection(cp, ct, ispr, ref lt, ref lb, ref rt, ref rb, ref lt2, ref lb2, ref rt2, ref rb2, ref tp, ref bt))
			{
				return false;
			}
			float2 position = controlPoint2.position;
			float2 x = controlPoint.position - position;
			float2 x2 = controlPoint3.position - position;
			float num3 = math.length(x);
			float num4 = math.length(x2);
			if (num3 < num2 || num4 < num2)
			{
				return false;
			}
			float num5 = 0f;
			float num6 = 0f;
			float num7 = AngleBetweenVector(math.normalize(controlPoint.position - controlPoint2.position), math.normalize(controlPoint3.position - controlPoint2.position));
			if (num7 > 0f)
			{
				num5 = num3 - math.distance(lb, bt);
				num6 = num4 - math.distance(bt, rb2);
			}
			else
			{
				num5 = num3 - math.distance(lt, tp);
				num6 = num4 - math.distance(tp, rt2);
			}
			float2 float2 = position + math.normalize(x) * num5;
			float2 float3 = position + math.normalize(x2) * num6;
			controlPoint2.exData.x = ct;
			controlPoint2.exData.z = 2;
			controlPoint2.position = float2;
			newPoints[activePoint++] = controlPoint2;
			controlPoint2.exData.x = ct;
			controlPoint2.exData.z = 3;
			controlPoint2.position = float3;
			newPoints[activePoint++] = controlPoint2;
			JobCornerInfo value = m_Corners[m_CornerCount];
			if (num7 > 0f)
			{
				value.bottom = bt;
				value.top = tp;
				GenerateColumnsBi(float2, controlPoint.position, whsize, flip: false, ref lt, ref lb, ispr.metaInfo.y, pivot);
				GenerateColumnsBi(float3, controlPoint3.position, whsize, flip: false, ref lt2, ref lb2, ispr.metaInfo.y, pivot);
				value.left = lt;
				value.right = lb2;
			}
			else
			{
				value.bottom = tp;
				value.top = bt;
				GenerateColumnsBi(float2, controlPoint.position, whsize, flip: false, ref lt, ref lb, ispr.metaInfo.y, pivot);
				GenerateColumnsBi(float3, controlPoint3.position, whsize, flip: false, ref lt2, ref lb2, ispr.metaInfo.y, pivot);
				value.left = lb;
				value.right = lt2;
			}
			value.cornerData.x = ct;
			value.cornerData.y = activePoint;
			m_Corners[m_CornerCount] = value;
			m_CornerCount++;
			return true;
		}

		private float2 CornerTextureCoordinate(int cornerType, int index)
		{
			int num = (cornerType - 1) * 4;
			return m_CornerCoordinates[num + index];
		}

		private int CalculateCorner(int index, float angle, float2 lt, float2 rt)
		{
			int num = 0;
			float num2 = SlopeAngle(lt);
			float2[] array = new float2[4]
			{
				new float2(-135f, -35f),
				new float2(35f, 135f),
				new float2(-35f, 35f),
				new float2(-135f, 135f)
			};
			int2[] array2 = new int2[4]
			{
				new int2(kCornerTypeInnerTopLeft, kCornerTypeOuterBottomLeft),
				new int2(kCornerTypeInnerBottomRight, kCornerTypeOuterTopRight),
				new int2(kCornerTypeInnerTopRight, kCornerTypeOuterTopLeft),
				new int2(kCornerTypeInnerBottomLeft, kCornerTypeOuterBottomRight)
			};
			for (int i = 0; i < 3; i++)
			{
				if (num2 > array[i].x && num2 < array[i].y)
				{
					num = ((angle > 0f) ? array2[i].x : array2[i].y);
					break;
				}
			}
			if (num == 0)
			{
				num = ((angle > 0f) ? kCornerTypeInnerBottomLeft : kCornerTypeOuterBottomRight);
			}
			return num;
		}

		private bool InsertCorner(int index, ref NativeArray<int2> cpSpriteIndices, ref NativeArray<JobControlPoint> newPoints, ref int activePoint, ref bool cornerConsidered)
		{
			int index2 = ((index == 0) ? (controlPointCount - 1) : (index - 1));
			int index3 = (index + 1) % controlPointCount;
			if (cpSpriteIndices[index2].x >= spriteCount || cpSpriteIndices[index].x >= spriteCount)
			{
				return false;
			}
			if (cpSpriteIndices[index2].y == 0 || cpSpriteIndices[index].y == 0)
			{
				return false;
			}
			JobControlPoint controlPoint = GetControlPoint(index2);
			JobControlPoint controlPoint2 = GetControlPoint(index);
			JobControlPoint controlPoint3 = GetControlPoint(index3);
			if (controlPoint2.cpData.y == 0 || controlPoint.cpData.z != kModeLinear || controlPoint2.cpData.z != kModeLinear || controlPoint3.cpData.z != kModeLinear)
			{
				return false;
			}
			if (controlPoint.cpInfo.x != controlPoint2.cpInfo.x || controlPoint2.cpInfo.x != controlPoint3.cpInfo.x)
			{
				return false;
			}
			JobSpriteInfo spriteInfo = GetSpriteInfo(cpSpriteIndices[index2].x);
			JobSpriteInfo spriteInfo2 = GetSpriteInfo(cpSpriteIndices[index].x);
			if (spriteInfo.metaInfo.y != spriteInfo2.metaInfo.y)
			{
				return false;
			}
			float2 @float = math.normalize(controlPoint3.position - controlPoint2.position);
			float2 b = math.normalize(controlPoint.position - controlPoint2.position);
			float num = AngleBetweenVector(@float, b);
			float t = math.abs(num);
			cornerConsidered = AngleWithinRange(t, 90f - m_ShapeParams.curveData.z, 90f + m_ShapeParams.curveData.z) || m_ShapeParams.curveData.z == 90f;
			if (cornerConsidered && controlPoint2.cpData.y == 1)
			{
				float2 lt = math.normalize(controlPoint2.position - controlPoint.position);
				int num2 = CalculateCorner(index, num, lt, @float);
				if (num2 > 0)
				{
					JobSpriteInfo cornerSpriteInfo = GetCornerSpriteInfo(num2);
					return AttachCorner(index, num2, cornerSpriteInfo, ref newPoints, ref activePoint);
				}
			}
			return false;
		}

		private void TessellateCorners()
		{
			for (int i = 1; i <= kCornerTypeInnerBottomRight; i++)
			{
				JobSpriteInfo cornerSpriteInfo = GetCornerSpriteInfo(i);
				if (cornerSpriteInfo.metaInfo.x == 0f)
				{
					continue;
				}
				int num = 0;
				int num2 = 0;
				Vector3 value = m_PosArray[num];
				Vector2 value2 = m_Uv0Array[num];
				bool flag = i <= kCornerTypeOuterBottomRight;
				int num3 = m_VertexArrayCount;
				for (int j = 0; j < m_CornerCount; j++)
				{
					JobCornerInfo jobCornerInfo = m_Corners[j];
					if (jobCornerInfo.cornerData.x == i)
					{
						value.x = jobCornerInfo.top.x;
						value.y = jobCornerInfo.top.y;
						value2.x = CornerTextureCoordinate(i, 1).x * cornerSpriteInfo.uvInfo.z + cornerSpriteInfo.uvInfo.x;
						value2.y = CornerTextureCoordinate(i, 1).y * cornerSpriteInfo.uvInfo.w + cornerSpriteInfo.uvInfo.y;
						m_PosArray[m_VertexArrayCount] = value;
						m_Uv0Array[m_VertexArrayCount++] = value2;
						value.x = jobCornerInfo.right.x;
						value.y = jobCornerInfo.right.y;
						value2.x = CornerTextureCoordinate(i, 0).x * cornerSpriteInfo.uvInfo.z + cornerSpriteInfo.uvInfo.x;
						value2.y = CornerTextureCoordinate(i, 0).y * cornerSpriteInfo.uvInfo.w + cornerSpriteInfo.uvInfo.y;
						m_PosArray[m_VertexArrayCount] = value;
						m_Uv0Array[m_VertexArrayCount++] = value2;
						value.x = jobCornerInfo.left.x;
						value.y = jobCornerInfo.left.y;
						value2.x = CornerTextureCoordinate(i, 3).x * cornerSpriteInfo.uvInfo.z + cornerSpriteInfo.uvInfo.x;
						value2.y = CornerTextureCoordinate(i, 3).y * cornerSpriteInfo.uvInfo.w + cornerSpriteInfo.uvInfo.y;
						m_PosArray[m_VertexArrayCount] = value;
						m_Uv0Array[m_VertexArrayCount++] = value2;
						value.x = jobCornerInfo.bottom.x;
						value.y = jobCornerInfo.bottom.y;
						value2.x = CornerTextureCoordinate(i, 2).x * cornerSpriteInfo.uvInfo.z + cornerSpriteInfo.uvInfo.x;
						value2.y = CornerTextureCoordinate(i, 2).y * cornerSpriteInfo.uvInfo.w + cornerSpriteInfo.uvInfo.y;
						m_PosArray[m_VertexArrayCount] = value;
						m_Uv0Array[m_VertexArrayCount++] = value2;
						m_IndexArray[m_IndexArrayCount++] = (ushort)num2;
						m_IndexArray[m_IndexArrayCount++] = (ushort)(num2 + (flag ? 1 : 3));
						m_IndexArray[m_IndexArrayCount++] = (ushort)(num2 + ((!flag) ? 1 : 3));
						m_IndexArray[m_IndexArrayCount++] = (ushort)num2;
						m_IndexArray[m_IndexArrayCount++] = (ushort)(num2 + (flag ? 3 : 2));
						m_IndexArray[m_IndexArrayCount++] = (ushort)(num2 + (flag ? 2 : 3));
						num2 += 4;
						num += 6;
					}
				}
				if (m_TanArray.Length > 1)
				{
					for (int k = num3; k < m_VertexArrayCount; k++)
					{
						m_TanArray[k] = new Vector4(1f, 0f, 0f, -1f);
					}
				}
				if (num > 0 && num2 > 0)
				{
					SpriteShapeSegment value3 = m_GeomArray[m_GeomArrayCount];
					value3.geomIndex = m_GeomArrayCount;
					value3.indexCount = num;
					value3.vertexCount = num2;
					value3.spriteIndex = m_SpriteInfos.Length + (i - 1);
					m_GeomArray[m_GeomArrayCount++] = value3;
				}
			}
		}

		private bool AreCollinear(float2 a, float2 b, float2 c, float t)
		{
			float num = (a.y - b.y) * (a.x - c.x);
			float num2 = (a.y - c.y) * (a.x - b.x);
			return math.abs(num - num2) < t;
		}

		private void OptimizePoints(float tolerance, ref NativeArray<float2> pointSet, ref int pointCount)
		{
			int num = 8;
			if (pointCount < num)
			{
				return;
			}
			int num2 = 0;
			int num3 = pointCount - 2;
			bool flag = true;
			m_TempPoints[0] = pointSet[0];
			for (int i = 0; i < num3; i++)
			{
				int num4 = i;
				float2 a = pointSet[i];
				float2 @float = pointSet[i + 1];
				float2 c = pointSet[i + 2];
				flag = true;
				while (flag && num4 < num3)
				{
					flag = AreCollinear(a, @float, c, tolerance);
					if (!flag)
					{
						m_TempPoints[++num2] = @float;
						i = num4;
						break;
					}
					num4++;
					@float = pointSet[num4 + 1];
					c = pointSet[num4 + 2];
				}
			}
			m_TempPoints[++num2] = pointSet[num3];
			m_TempPoints[++num2] = pointSet[num3 + 1];
			if (isCarpet)
			{
				m_TempPoints[++num2] = pointSet[0];
			}
			int num5 = num2 + 1;
			if (num5 <= 0)
			{
				return;
			}
			pointCount = 0;
			pointSet[pointCount++] = m_TempPoints[0];
			for (int j = 1; j < num5; j++)
			{
				if (math.distance(pointSet[pointCount - 1], m_TempPoints[j]) > 0.0001f)
				{
					pointSet[pointCount++] = m_TempPoints[j];
				}
			}
		}

		private void AttachCornerToCollider(JobSegmentInfo isi, float pivot, ref NativeArray<float2> colliderPoints, ref int colliderPointCount)
		{
			float2 @float = new float2(0f, 0f);
			int num = isi.sgInfo.x + 1;
			for (int i = 0; i < m_CornerCount; i++)
			{
				JobCornerInfo jobCornerInfo = m_Corners[i];
				if (num == jobCornerInfo.cornerData.y)
				{
					float2 float2 = @float;
					float2 float3 = @float;
					float3 = ((jobCornerInfo.cornerData.x <= kCornerTypeOuterBottomRight) ? jobCornerInfo.bottom : jobCornerInfo.top);
					float2 float4 = @float;
					float4 = ((jobCornerInfo.cornerData.x <= kCornerTypeOuterBottomRight) ? jobCornerInfo.top : jobCornerInfo.bottom);
					float2 = (float3 - float4) * pivot;
					float2 = (float4 + float2 + float3 + float2) * 0.5f;
					colliderPoints[colliderPointCount++] = float2;
					break;
				}
			}
		}

		private float2 UpdateCollider(JobSegmentInfo isi, JobSpriteInfo ispr, NativeArray<JobShapeVertex> vertices, int count, ref NativeArray<float2> colliderPoints, ref int colliderPointCount)
		{
			float2 @float = new float2(0f, 0f);
			float num = 0f;
			num += colliderPivot;
			AttachCornerToCollider(isi, num, ref colliderPoints, ref colliderPointCount);
			float2 float2 = @float;
			float2 float3 = @float;
			float2 float4 = @float;
			for (int i = 0; i < count; i += 4)
			{
				float3 = vertices[i].pos;
				float4 = vertices[i + 2].pos;
				float2 = (float3 - float4) * num;
				if (vertices[i].sprite.z == 0)
				{
					colliderPoints[colliderPointCount++] = (float4 + float2 + float3 + float2) * 0.5f;
				}
			}
			float2 pos = vertices[count - 1].pos;
			float2 pos2 = vertices[count - 3].pos;
			float2 = (pos2 - pos) * num;
			if (vertices[count - 1].sprite.z == 0)
			{
				colliderPoints[colliderPointCount++] = (pos + float2 + pos2 + float2) * 0.5f;
			}
			return float2;
		}

		private void TrimOverlaps(int cpCount)
		{
			int num = 4;
			if (m_ColliderPointCount < num)
			{
				return;
			}
			int num2 = 0;
			int i = 0;
			int num3 = m_ColliderPointCount / 2;
			int num4 = math.clamp(splineDetail * 3, 0, 8);
			int num5 = ((num4 > num3) ? num3 : num4);
			num5 = ((num5 > cpCount) ? cpCount : num5);
			if (!isCarpet)
			{
				m_TempPoints[num2++] = m_ColliderPoints[0];
			}
			while (i < m_ColliderPointCount)
			{
				int index = ((i > 0) ? (i - 1) : (m_ColliderPointCount - 1));
				bool flag = true;
				float2 @float = m_ColliderPoints[index];
				float2 float2 = m_ColliderPoints[i];
				for (int num6 = num5; num6 > 1; num6--)
				{
					int index2 = (i + num6 - 1) % m_ColliderPointCount;
					int num7 = (i + num6) % m_ColliderPointCount;
					if (num7 != 0 && i != 0)
					{
						float2 p = m_ColliderPoints[index2];
						float2 float3 = m_ColliderPoints[num7];
						if (math.abs(math.length(@float - float3)) < kEpsilon)
						{
							break;
						}
						float2 result = @float;
						if (LineIntersection(kEpsilonRelaxed, @float, float2, p, float3, ref result) && IsPointOnLines(kEpsilonRelaxed, @float, float2, p, float3, result))
						{
							flag = false;
							m_TempPoints[num2++] = result;
							i += num6;
							break;
						}
					}
				}
				if (flag)
				{
					m_TempPoints[num2++] = float2;
					i++;
				}
			}
			for (; i < m_ColliderPointCount; i++)
			{
				m_TempPoints[num2++] = m_ColliderPoints[i];
			}
			i = 0;
			m_ColliderPoints[i++] = m_TempPoints[0];
			float2 float4 = m_TempPoints[0];
			for (int j = 1; j < num2; j++)
			{
				if (math.length(m_TempPoints[j] - float4) > kEpsilon)
				{
					m_ColliderPoints[i++] = m_TempPoints[j];
				}
				float4 = m_TempPoints[j];
			}
			num2 = i;
			if (num2 > 4)
			{
				float2 result2 = m_ColliderPoints[0];
				if (LineIntersection(kEpsilonRelaxed, m_ColliderPoints[0], m_ColliderPoints[1], m_ColliderPoints[num2 - 1], m_ColliderPoints[num2 - 2], ref result2))
				{
					ref NativeArray<float2> colliderPoints = ref m_ColliderPoints;
					float2 value = (m_ColliderPoints[num2 - 1] = result2);
					colliderPoints[0] = value;
				}
			}
			m_ColliderPointCount = num2;
		}

		private void OptimizeCollider()
		{
			if (!hasCollider)
			{
				return;
			}
			if (kColliderQuality > 0f)
			{
				OptimizePoints(kColliderQuality, ref m_ColliderPoints, ref m_ColliderPointCount);
				TrimOverlaps(m_ControlPointCount - 1);
				m_ColliderPoints[m_ColliderPointCount++] = new float2(0f, 0f);
				m_ColliderPoints[m_ColliderPointCount++] = new float2(0f, 0f);
			}
			if (m_ColliderPointCount <= 4)
			{
				for (int i = 0; i < m_TessPointCount; i++)
				{
					m_ColliderPoints[i] = m_TessPoints[i];
				}
				m_ColliderPoints[m_TessPointCount] = new float2(0f, 0f);
				m_ColliderPoints[m_TessPointCount + 1] = new float2(0f, 0f);
				m_ColliderPointCount = m_TessPointCount + 2;
			}
		}

		[Obsolete]
		public void Prepare(SpriteShapeController controller, SpriteShapeParameters shapeParams, int maxArrayCount, NativeArray<ShapeControlPoint> shapePoints, NativeArray<SpriteShapeMetaData> metaData, AngleRangeInfo[] angleRanges, Sprite[] segmentSprites, Sprite[] cornerSprites)
		{
			PrepareInput(shapeParams, maxArrayCount, shapePoints, controller.optimizeGeometry, controller.autoUpdateCollider, controller.optimizeCollider, controller.colliderOffset, controller.colliderDetail);
			PrepareSprites(segmentSprites, cornerSprites);
			PrepareAngleRanges(angleRanges);
			NativeArray<SplinePointMetaData> metaData2 = new NativeArray<SplinePointMetaData>(metaData.Length, Allocator.Temp);
			for (int i = 0; i < metaData.Length; i++)
			{
				SplinePointMetaData value = default(SplinePointMetaData);
				value.height = metaData[i].height;
				value.spriteIndex = metaData[i].spriteIndex;
				value.cornerMode = (metaData[i].corner ? 1 : 0);
				metaData2[i] = value;
			}
			PrepareControlPoints(shapePoints, metaData2);
			metaData2.Dispose();
			kModeUTess = 0;
			TessellateContourMainThread();
		}

		internal void Prepare(SpriteShapeController controller, SpriteShapeParameters shapeParams, int maxArrayCount, NativeArray<ShapeControlPoint> shapePoints, NativeArray<SplinePointMetaData> metaData, AngleRangeInfo[] angleRanges, Sprite[] segmentSprites, Sprite[] cornerSprites, bool UseUTess)
		{
			PrepareInput(shapeParams, maxArrayCount, shapePoints, controller.optimizeGeometry, controller.autoUpdateCollider, controller.optimizeCollider, controller.colliderOffset, controller.colliderDetail);
			PrepareSprites(segmentSprites, cornerSprites);
			PrepareAngleRanges(angleRanges);
			PrepareControlPoints(shapePoints, metaData);
			kModeUTess = (UseUTess ? 1 : 0);
			if (kModeUTess == 0)
			{
				TessellateContourMainThread();
			}
		}

		public void Execute()
		{
			SetResult(SpriteShapeGeneratorResult.Success);
			if (kModeUTess != 0)
			{
				TessellateContour();
			}
			GenerateSegments();
			UpdateSegments();
			TessellateSegments();
			TessellateCorners();
			CalculateTexCoords();
			CalculateBoundingBox();
			OptimizeCollider();
		}

		public void Cleanup()
		{
			SafeDispose(m_Corners);
			SafeDispose(m_CornerSpriteInfos);
			SafeDispose(m_SpriteInfos);
			SafeDispose(m_AngleRanges);
			SafeDispose(m_Segments);
			SafeDispose(m_ControlPoints);
			SafeDispose(m_ContourPoints);
			SafeDispose(m_TempPoints);
			SafeDispose(m_GeneratedControlPoints);
			SafeDispose(m_SpriteIndices);
			SafeDispose(m_Intersectors);
			SafeDispose(m_TessPoints);
			SafeDispose(m_VertexData);
			SafeDispose(m_OutputVertexData);
			SafeDispose(m_CornerCoordinates);
		}
	}
}
