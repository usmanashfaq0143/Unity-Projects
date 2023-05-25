using System.Collections.Generic;
using Unity.Profiling;
using UnityEngine.Animations;
using UnityEngine.Scripting.APIUpdating;
using UnityEngine.U2D.Common;

namespace UnityEngine.U2D.IK
{
	[DefaultExecutionOrder(-2)]
	[MovedFrom("UnityEngine.Experimental.U2D.IK")]
	[ExecuteInEditMode]
	public class IKManager2D : MonoBehaviour, IPreviewable, UnityEngine.Animations.IAnimationPreviewable
	{
		[SerializeField]
		private List<Solver2D> m_Solvers = new List<Solver2D>();

		[SerializeField]
		[Range(0f, 1f)]
		private float m_Weight = 1f;

		public float weight
		{
			get
			{
				return m_Weight;
			}
			set
			{
				m_Weight = Mathf.Clamp01(value);
			}
		}

		public List<Solver2D> solvers => m_Solvers;

		private void OnValidate()
		{
			m_Weight = Mathf.Clamp01(m_Weight);
			OnEditorDataValidate();
		}

		private void Reset()
		{
			FindChildSolvers();
			OnEditorDataValidate();
		}

		private void FindChildSolvers()
		{
			m_Solvers.Clear();
			List<Solver2D> list = new List<Solver2D>();
			base.transform.GetComponentsInChildren(includeInactive: true, list);
			foreach (Solver2D item in list)
			{
				if (item.GetComponentInParent<IKManager2D>() == this)
				{
					AddSolver(item);
				}
			}
		}

		public void AddSolver(Solver2D solver)
		{
			if (!m_Solvers.Contains(solver))
			{
				m_Solvers.Add(solver);
				AddSolverEditorData();
			}
		}

		public void RemoveSolver(Solver2D solver)
		{
			RemoveSolverEditorData(solver);
			m_Solvers.Remove(solver);
		}

		public void UpdateManager()
		{
			new ProfilerMarker("IKManager2D.UpdateManager");
			foreach (Solver2D solver in m_Solvers)
			{
				if (!(solver == null) && solver.isActiveAndEnabled)
				{
					if (!solver.isValid)
					{
						solver.Initialize();
					}
					solver.UpdateIK(weight);
				}
			}
		}

		public void OnPreviewUpdate()
		{
		}

		private static bool IsInGUIUpdateLoop()
		{
			return Event.current != null;
		}

		private void LateUpdate()
		{
			UpdateManager();
		}

		private void OnEditorDataValidate()
		{
		}

		private void AddSolverEditorData()
		{
		}

		private void RemoveSolverEditorData(Solver2D solver)
		{
		}
	}
}
