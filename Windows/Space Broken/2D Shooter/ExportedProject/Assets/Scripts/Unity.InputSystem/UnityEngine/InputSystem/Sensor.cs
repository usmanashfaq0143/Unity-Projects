using System;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.Scripting;

namespace UnityEngine.InputSystem
{
	[InputControlLayout(isGenericTypeOfDevice = true)]
	[Preserve]
	public class Sensor : InputDevice
	{
		public float samplingFrequency
		{
			get
			{
				QuerySamplingFrequencyCommand command = QuerySamplingFrequencyCommand.Create();
				if (ExecuteCommand(ref command) >= 0)
				{
					return command.frequency;
				}
				throw new NotSupportedException($"Device '{this}' does not support querying sampling frequency");
			}
			set
			{
				SetSamplingFrequencyCommand command = SetSamplingFrequencyCommand.Create(value);
				ExecuteCommand(ref command);
			}
		}
	}
}
