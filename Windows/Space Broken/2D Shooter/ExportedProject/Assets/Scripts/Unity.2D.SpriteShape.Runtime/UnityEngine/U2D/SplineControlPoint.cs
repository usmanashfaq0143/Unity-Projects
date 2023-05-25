using System;

namespace UnityEngine.U2D
{
	[Serializable]
	public class SplineControlPoint
	{
		public Vector3 position;

		public Vector3 leftTangent;

		public Vector3 rightTangent;

		public ShapeTangentMode mode;

		public float height = 1f;

		public int spriteIndex;

		public bool corner;

		[SerializeField]
		private Corner m_CornerMode;

		public Corner cornerMode
		{
			get
			{
				return m_CornerMode;
			}
			set
			{
				m_CornerMode = value;
			}
		}

		public override int GetHashCode()
		{
			int num = ((int)position.x).GetHashCode() ^ ((int)position.y).GetHashCode() ^ position.GetHashCode() ^ (leftTangent.GetHashCode() << 2) ^ (rightTangent.GetHashCode() >> 2);
			int num2 = (int)mode;
			return num ^ num2.GetHashCode() ^ height.GetHashCode() ^ spriteIndex.GetHashCode() ^ corner.GetHashCode() ^ (m_CornerMode.GetHashCode() << 2);
		}
	}
}
