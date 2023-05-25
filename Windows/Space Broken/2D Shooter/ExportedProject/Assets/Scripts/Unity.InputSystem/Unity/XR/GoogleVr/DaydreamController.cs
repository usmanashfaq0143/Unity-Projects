using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.XR;
using UnityEngine.Scripting;

namespace Unity.XR.GoogleVr
{
	[InputControlLayout(displayName = "Daydream Controller", commonUsages = new string[] { "LeftHand", "RightHand" })]
	[Preserve]
	public class DaydreamController : XRController
	{
		[InputControl]
		[Preserve]
		public Vector2Control touchpad { get; private set; }

		[InputControl]
		[Preserve]
		public ButtonControl volumeUp { get; private set; }

		[InputControl]
		[Preserve]
		public ButtonControl recentered { get; private set; }

		[InputControl]
		[Preserve]
		public ButtonControl volumeDown { get; private set; }

		[InputControl]
		[Preserve]
		public ButtonControl recentering { get; private set; }

		[InputControl]
		[Preserve]
		public ButtonControl app { get; private set; }

		[InputControl]
		[Preserve]
		public ButtonControl home { get; private set; }

		[InputControl]
		[Preserve]
		public ButtonControl touchpadClicked { get; private set; }

		[InputControl]
		[Preserve]
		public ButtonControl touchpadTouched { get; private set; }

		[InputControl(noisy = true)]
		[Preserve]
		public Vector3Control deviceVelocity { get; private set; }

		[InputControl(noisy = true)]
		[Preserve]
		public Vector3Control deviceAcceleration { get; private set; }

		protected override void FinishSetup()
		{
			base.FinishSetup();
			touchpad = GetChildControl<Vector2Control>("touchpad");
			volumeUp = GetChildControl<ButtonControl>("volumeUp");
			recentered = GetChildControl<ButtonControl>("recentered");
			volumeDown = GetChildControl<ButtonControl>("volumeDown");
			recentering = GetChildControl<ButtonControl>("recentering");
			app = GetChildControl<ButtonControl>("app");
			home = GetChildControl<ButtonControl>("home");
			touchpadClicked = GetChildControl<ButtonControl>("touchpadClicked");
			touchpadTouched = GetChildControl<ButtonControl>("touchpadTouched");
			deviceVelocity = GetChildControl<Vector3Control>("deviceVelocity");
			deviceAcceleration = GetChildControl<Vector3Control>("deviceAcceleration");
		}
	}
}
