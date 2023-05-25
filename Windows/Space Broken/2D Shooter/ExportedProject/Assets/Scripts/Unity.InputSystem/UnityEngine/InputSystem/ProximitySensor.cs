using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.Scripting;

namespace UnityEngine.InputSystem
{
	[InputControlLayout(displayName = "Proximity")]
	[Preserve]
	public class ProximitySensor : Sensor
	{
		[InputControl(displayName = "Distance", noisy = true)]
		public AxisControl distance { get; private set; }

		public static ProximitySensor current { get; private set; }

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
			distance = GetChildControl<AxisControl>("distance");
			base.FinishSetup();
		}
	}
}
