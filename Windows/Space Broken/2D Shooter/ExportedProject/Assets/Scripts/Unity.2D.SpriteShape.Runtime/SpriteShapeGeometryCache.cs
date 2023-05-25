using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.U2D;

[AddComponentMenu("")]
internal class SpriteShapeGeometryCache : MonoBehaviour
{
	[SerializeField]
	[HideInInspector]
	private int m_MaxArrayCount;

	[SerializeField]
	[HideInInspector]
	private Vector3[] m_PosArray;

	[SerializeField]
	[HideInInspector]
	private Vector2[] m_Uv0Array;

	[SerializeField]
	[HideInInspector]
	private Vector4[] m_TanArray;

	[SerializeField]
	[HideInInspector]
	private ushort[] m_IndexArray;

	[SerializeField]
	[HideInInspector]
	private SpriteShapeGeometryInfo[] m_GeomArray;

	private bool m_RequiresUpdate;

	private bool m_RequiresUpload;

	private NativeSlice<Vector3> m_PosArrayCache;

	private NativeSlice<Vector2> m_Uv0ArrayCache;

	private NativeSlice<Vector4> m_TanArrayCache;

	private NativeArray<ushort> m_IndexArrayCache;

	private NativeArray<SpriteShapeSegment> m_GeomArrayCache;

	internal ushort[] indexArray => m_IndexArray;

	internal Vector3[] posArray => m_PosArray;

	internal int maxArrayCount => m_MaxArrayCount;

	private void OnEnable()
	{
		m_RequiresUpload = true;
		m_RequiresUpdate = false;
	}

	internal void SetGeometryCache(int _maxArrayCount, NativeSlice<Vector3> _posArray, NativeSlice<Vector2> _uv0Array, NativeSlice<Vector4> _tanArray, NativeArray<ushort> _indexArray, NativeArray<SpriteShapeSegment> _geomArray)
	{
		m_RequiresUpdate = true;
		m_PosArrayCache = _posArray;
		m_Uv0ArrayCache = _uv0Array;
		m_TanArrayCache = _tanArray;
		m_GeomArrayCache = _geomArray;
		m_IndexArrayCache = _indexArray;
		m_MaxArrayCount = _maxArrayCount;
	}

	internal void UpdateGeometryCache()
	{
		if (!m_RequiresUpdate || !m_GeomArrayCache.IsCreated || !m_IndexArrayCache.IsCreated)
		{
			return;
		}
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		for (int i = 0; i < m_GeomArrayCache.Length; i++)
		{
			SpriteShapeSegment spriteShapeSegment = m_GeomArrayCache[i];
			num2 += spriteShapeSegment.indexCount;
			num3 += spriteShapeSegment.vertexCount;
			if (spriteShapeSegment.vertexCount > 0)
			{
				num = i + 1;
			}
		}
		m_GeomArray = new SpriteShapeGeometryInfo[num];
		NativeArray<SpriteShapeGeometryInfo> nativeArray = m_GeomArrayCache.Reinterpret<SpriteShapeGeometryInfo>();
		SpriteShapeCopyUtility<SpriteShapeGeometryInfo>.Copy(m_GeomArray, nativeArray, num);
		m_PosArray = new Vector3[num3];
		m_Uv0Array = new Vector2[num3];
		m_TanArray = new Vector4[num3];
		m_IndexArray = new ushort[num2];
		SpriteShapeCopyUtility<ushort>.Copy(m_IndexArray, m_IndexArrayCache, num2);
		SpriteShapeCopyUtility<Vector3>.Copy(m_PosArray, m_PosArrayCache, num3);
		SpriteShapeCopyUtility<Vector2>.Copy(m_Uv0Array, m_Uv0ArrayCache, num3);
		if (m_TanArrayCache.Length >= num3)
		{
			SpriteShapeCopyUtility<Vector4>.Copy(m_TanArray, m_TanArrayCache, num3);
		}
		m_MaxArrayCount = ((num3 > num2) ? num3 : num2);
		m_RequiresUpdate = false;
	}

	internal JobHandle Upload(SpriteShapeRenderer sr, SpriteShapeController sc)
	{
		JobHandle jobHandle = default(JobHandle);
		if (m_RequiresUpload)
		{
			sr.GetSegments(m_GeomArray.Length).Reinterpret<SpriteShapeGeometryInfo>().CopyFrom(m_GeomArray);
			NativeArray<ushort> indices;
			NativeSlice<Vector3> vertices;
			NativeSlice<Vector2> texcoords;
			if (sc.enableTangents)
			{
				sr.GetChannels(m_MaxArrayCount, out indices, out vertices, out texcoords, out NativeSlice<Vector4> tangents);
				SpriteShapeCopyUtility<Vector4>.Copy(tangents, m_TanArray, m_TanArray.Length);
			}
			else
			{
				sr.GetChannels(m_MaxArrayCount, out indices, out vertices, out texcoords);
			}
			SpriteShapeCopyUtility<Vector3>.Copy(vertices, m_PosArray, m_PosArray.Length);
			SpriteShapeCopyUtility<Vector2>.Copy(texcoords, m_Uv0Array, m_Uv0Array.Length);
			SpriteShapeCopyUtility<ushort>.Copy(indices, m_IndexArray, m_IndexArray.Length);
			sr.Prepare(jobHandle, sc.spriteShapeParameters, sc.spriteArray);
			m_RequiresUpload = false;
		}
		return jobHandle;
	}
}
