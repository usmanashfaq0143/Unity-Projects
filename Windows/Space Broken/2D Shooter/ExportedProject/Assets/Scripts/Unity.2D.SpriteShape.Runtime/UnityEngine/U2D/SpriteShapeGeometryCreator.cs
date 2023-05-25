using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace UnityEngine.U2D
{
	public abstract class SpriteShapeGeometryCreator : ScriptableObject
	{
		public abstract int GetVertexArrayCount(SpriteShapeController spriteShapeController);

		public abstract JobHandle MakeCreatorJob(SpriteShapeController spriteShapeController, NativeArray<ushort> indices, NativeSlice<Vector3> positions, NativeSlice<Vector2> texCoords, NativeSlice<Vector4> tangents, NativeArray<SpriteShapeSegment> segments, NativeArray<float2> colliderData);
	}
}
