using System;
using System.Collections.Generic;

namespace UnityEngine.U2D
{
	[Serializable]
	public class Spline
	{
		private static readonly string KErrorMessage = "Internal error: Point too close to neighbor";

		private static readonly float KEpsilon = 0.01f;

		[SerializeField]
		private bool m_IsOpenEnded;

		[SerializeField]
		private List<SplineControlPoint> m_ControlPoints = new List<SplineControlPoint>();

		public bool isOpenEnded
		{
			get
			{
				if (GetPointCount() < 3)
				{
					return true;
				}
				return m_IsOpenEnded;
			}
			set
			{
				m_IsOpenEnded = value;
			}
		}

		private bool IsPositionValid(int index, int next, Vector3 point)
		{
			int num = ((index == 0) ? (m_ControlPoints.Count - 1) : (index - 1));
			next = ((next < m_ControlPoints.Count) ? next : 0);
			if (num >= 0 && (m_ControlPoints[num].position - point).magnitude < KEpsilon)
			{
				return false;
			}
			if (next < m_ControlPoints.Count && (m_ControlPoints[next].position - point).magnitude < KEpsilon)
			{
				return false;
			}
			return true;
		}

		public void Clear()
		{
			m_ControlPoints.Clear();
		}

		public int GetPointCount()
		{
			return m_ControlPoints.Count;
		}

		public void InsertPointAt(int index, Vector3 point)
		{
			if (!IsPositionValid(index, index, point))
			{
				throw new ArgumentException(KErrorMessage);
			}
			m_ControlPoints.Insert(index, new SplineControlPoint
			{
				position = point,
				height = 1f,
				cornerMode = Corner.Automatic
			});
		}

		public void RemovePointAt(int index)
		{
			if (m_ControlPoints.Count > 2)
			{
				m_ControlPoints.RemoveAt(index);
			}
		}

		public Vector3 GetPosition(int index)
		{
			return m_ControlPoints[index].position;
		}

		public void SetPosition(int index, Vector3 point)
		{
			if (!IsPositionValid(index, index + 1, point))
			{
				throw new ArgumentException(KErrorMessage);
			}
			SplineControlPoint splineControlPoint = m_ControlPoints[index];
			splineControlPoint.position = point;
			m_ControlPoints[index] = splineControlPoint;
		}

		public Vector3 GetLeftTangent(int index)
		{
			if (GetTangentMode(index) == ShapeTangentMode.Linear)
			{
				return Vector3.zero;
			}
			return m_ControlPoints[index].leftTangent;
		}

		public void SetLeftTangent(int index, Vector3 tangent)
		{
			if (GetTangentMode(index) != 0)
			{
				SplineControlPoint splineControlPoint = m_ControlPoints[index];
				splineControlPoint.leftTangent = tangent;
				m_ControlPoints[index] = splineControlPoint;
			}
		}

		public Vector3 GetRightTangent(int index)
		{
			if (GetTangentMode(index) == ShapeTangentMode.Linear)
			{
				return Vector3.zero;
			}
			return m_ControlPoints[index].rightTangent;
		}

		public void SetRightTangent(int index, Vector3 tangent)
		{
			if (GetTangentMode(index) != 0)
			{
				SplineControlPoint splineControlPoint = m_ControlPoints[index];
				splineControlPoint.rightTangent = tangent;
				m_ControlPoints[index] = splineControlPoint;
			}
		}

		public ShapeTangentMode GetTangentMode(int index)
		{
			return m_ControlPoints[index].mode;
		}

		public void SetTangentMode(int index, ShapeTangentMode mode)
		{
			SplineControlPoint splineControlPoint = m_ControlPoints[index];
			splineControlPoint.mode = mode;
			m_ControlPoints[index] = splineControlPoint;
		}

		public float GetHeight(int index)
		{
			return m_ControlPoints[index].height;
		}

		public void SetHeight(int index, float value)
		{
			m_ControlPoints[index].height = value;
		}

		public int GetSpriteIndex(int index)
		{
			return m_ControlPoints[index].spriteIndex;
		}

		public void SetSpriteIndex(int index, int value)
		{
			m_ControlPoints[index].spriteIndex = value;
		}

		public bool GetCorner(int index)
		{
			return GetCornerMode(index) != Corner.Disable;
		}

		public void SetCorner(int index, bool value)
		{
			m_ControlPoints[index].corner = value;
			m_ControlPoints[index].cornerMode = (value ? Corner.Automatic : Corner.Disable);
		}

		internal void SetCornerMode(int index, Corner value)
		{
			m_ControlPoints[index].corner = value != Corner.Disable;
			m_ControlPoints[index].cornerMode = value;
		}

		internal Corner GetCornerMode(int index)
		{
			if (m_ControlPoints[index].cornerMode == Corner.Disable && m_ControlPoints[index].corner)
			{
				m_ControlPoints[index].cornerMode = Corner.Automatic;
				return Corner.Automatic;
			}
			return m_ControlPoints[index].cornerMode;
		}

		public override int GetHashCode()
		{
			int num = -2128831035;
			for (int i = 0; i < GetPointCount(); i++)
			{
				num = (num * 16777619) ^ m_ControlPoints[i].GetHashCode();
			}
			return (num * 16777619) ^ m_IsOpenEnded.GetHashCode();
		}
	}
}
