using UnityEngine.Scripting;

namespace UnityEngine.InputSystem.Processors
{
	[Preserve]
	internal class AxisDeadzoneProcessor : InputProcessor<float>
	{
		public float min;

		public float max;

		private float minOrDefault
		{
			get
			{
				if (min != 0f)
				{
					return min;
				}
				return InputSystem.settings.defaultDeadzoneMin;
			}
		}

		private float maxOrDefault
		{
			get
			{
				if (max != 0f)
				{
					return max;
				}
				return InputSystem.settings.defaultDeadzoneMax;
			}
		}

		public override float Process(float value, InputControl control = null)
		{
			float num = minOrDefault;
			float num2 = maxOrDefault;
			float num3 = Mathf.Abs(value);
			if (num3 < num)
			{
				return 0f;
			}
			if (num3 > num2)
			{
				return Mathf.Sign(value);
			}
			return Mathf.Sign(value) * ((num3 - num) / (num2 - num));
		}
	}
}
