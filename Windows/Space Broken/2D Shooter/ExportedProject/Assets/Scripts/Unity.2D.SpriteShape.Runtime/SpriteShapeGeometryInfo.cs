using System;
using UnityEngine;

[Serializable]
internal struct SpriteShapeGeometryInfo
{
	[SerializeField]
	internal int geomIndex;

	[SerializeField]
	internal int indexCount;

	[SerializeField]
	internal int vertexCount;

	[SerializeField]
	internal int spriteIndex;
}
