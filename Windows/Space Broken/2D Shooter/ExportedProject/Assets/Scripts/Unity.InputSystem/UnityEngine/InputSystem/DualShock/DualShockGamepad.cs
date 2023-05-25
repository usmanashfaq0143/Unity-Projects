using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.Haptics;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.Scripting;

namespace UnityEngine.InputSystem.DualShock
{
	[InputControlLayout(displayName = "PS4 Controller")]
	[Preserve]
	public class DualShockGamepad : Gamepad, IDualShockHaptics, IDualMotorRumble, IHaptics
	{
		[InputControl(name = "buttonWest", displayName = "Square", shortDisplayName = "Square")]
		[InputControl(name = "buttonNorth", displayName = "Triangle", shortDisplayName = "Triangle")]
		[InputControl(name = "buttonEast", displayName = "Circle", shortDisplayName = "Circle")]
		[InputControl(name = "buttonSouth", displayName = "Cross", shortDisplayName = "Cross")]
		[InputControl]
		public ButtonControl touchpadButton { get; private set; }

		[InputControl(name = "start", displayName = "Options")]
		public ButtonControl optionsButton { get; private set; }

		[InputControl(name = "select", displayName = "Share")]
		public ButtonControl shareButton { get; private set; }

		[InputControl(name = "leftShoulder", displayName = "L1", shortDisplayName = "L1")]
		public ButtonControl L1 { get; private set; }

		[InputControl(name = "rightShoulder", displayName = "R1", shortDisplayName = "R1")]
		public ButtonControl R1 { get; private set; }

		[InputControl(name = "leftTrigger", displayName = "L2", shortDisplayName = "L2")]
		public ButtonControl L2 { get; private set; }

		[InputControl(name = "rightTrigger", displayName = "R2", shortDisplayName = "R2")]
		public ButtonControl R2 { get; private set; }

		[InputControl(name = "leftStickPress", displayName = "L3", shortDisplayName = "L3")]
		public ButtonControl L3 { get; private set; }

		[InputControl(name = "rightStickPress", displayName = "R3", shortDisplayName = "R3")]
		public ButtonControl R3 { get; private set; }

		public new static DualShockGamepad current { get; private set; }

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
			base.FinishSetup();
			touchpadButton = GetChildControl<ButtonControl>("touchpadButton");
			optionsButton = base.startButton;
			shareButton = base.selectButton;
			L1 = base.leftShoulder;
			R1 = base.rightShoulder;
			L2 = base.leftTrigger;
			R2 = base.rightTrigger;
			L3 = base.leftStickButton;
			R3 = base.rightStickButton;
		}

		public virtual void SetLightBarColor(Color color)
		{
		}
	}
}
