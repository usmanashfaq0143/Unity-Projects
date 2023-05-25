using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.Scripting;

namespace UnityEngine.InputSystem
{
	[InputControlLayout(displayName = "Pressure")]
	[Preserve]
	public class PressureSensor : Sensor
	{
		[InputControl(displayName = "Atmospheric Pressure", noisy = true)]
		public AxisControl atmosphericPressure { get; private set; }

		public static PressureSensor current { get; private set; }

		public override void MakeCurrent()
		{
			base.MakeCurrent();
			current = this;
		}

		protected override void OnRemoved()
		{
			base.OnRemoved();
			if (current == this)
			{
				current = null;
			}
		}

		protected override void FinishSetup()
		{
			atmosphericPressure = GetChildControl<AxisControl>("atmosphericPressure");
			base.FinishSetup();
		}
	}
}
