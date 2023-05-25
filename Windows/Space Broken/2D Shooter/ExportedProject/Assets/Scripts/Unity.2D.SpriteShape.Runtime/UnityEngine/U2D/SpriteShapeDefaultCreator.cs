using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace UnityEngine.U2D
{
	internal class SpriteShapeDefaultCreator : SpriteShapeGeometryCreator
	{
		private static SpriteShapeDefaultCreator creator;

		internal static SpriteShapeDefaultCreator defaultInstance
		{
			get
			{
				if (null == creator)
				{
					creator = ScriptableObject.CreateInstance<SpriteShapeDefaultCreator>();
					creator.hideFlags = HideFlags.DontSave;
				}
				return creator;
			}
		}

		public override int GetVertexArrayCount(SpriteShapeController sc)
		{
			NativeArray<ShapeControlPoint> shapeControlPoints = sc.GetShapeControlPoints();
			sc.CalculateMaxArrayCount(shapeControlPoints);
			shapeControlPoints.Dispose();
			return sc.maxArrayCount;
		}

		public override JobHandle MakeCreatorJob(SpriteShapeController sc, NativeArray<ushort> indices, NativeSlice<Vector3> positions, NativeSlice<Vector2> texCoords, NativeSlice<Vector4> tangents, NativeArray<SpriteShapeSegment> segments, NativeArray<float2> colliderData)
		{
			bool useUTess = sc.ValidateUTess2D();
			NativeArray<Bounds> bounds = sc.spriteShapeRenderer.GetBounds();
			SpriteShapeGenerator spriteShapeGenerator = default(SpriteShapeGenerator);
			spriteShapeGenerator.m_Bounds = bounds;
			spriteShapeGenerator.m_PosArray = positions;
			spriteShapeGenerator.m_Uv0Array = texCoords;
			spriteShapeGenerator.m_TanArray = tangents;
			spriteShapeGenerator.m_GeomArray = segments;
			spriteShapeGenerator.m_IndexArray = indices;
			spriteShapeGenerator.m_ColliderPoints = colliderData;
			spriteShapeGenerator.m_Stats = sc.stats;
			SpriteShapeGenerator jobData = spriteShapeGenerator;
			jobData.generateCollider = SpriteShapeController.generateCollider;
			jobData.generateGeometry = SpriteShapeController.generateGeometry;
			NativeArray<ShapeControlPoint> shapeControlPoints = sc.GetShapeControlPoints();
			NativeArray<SplinePointMetaData> splinePointMetaData = sc.GetSplinePointMetaData();
			jobData.Prepare(sc, sc.spriteShapeParameters, sc.maxArrayCount, shapeControlPoints, splinePointMetaData, sc.angleRangeInfoArray, sc.edgeSpriteArray, sc.cornerSpriteArray, useUTess);
			JobHandle result = jobData.Schedule();
			shapeControlPoints.Dispose();
			splinePointMetaData.Dispose();
			return result;
		}
	}
}
