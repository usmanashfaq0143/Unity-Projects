using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.XR;

namespace UnityEngine.InputSystem.XR
{
	internal class XRLayoutBuilder
	{
		private string parentLayout;

		private string interfaceName;

		private XRDeviceDescriptor descriptor;

		private static uint GetSizeOfFeature(XRFeatureDescriptor featureDescriptor)
		{
			return featureDescriptor.featureType switch
			{
				FeatureType.Binary => 1u, 
				FeatureType.DiscreteStates => 4u, 
				FeatureType.Axis1D => 4u, 
				FeatureType.Axis2D => 8u, 
				FeatureType.Axis3D => 12u, 
				FeatureType.Rotation => 16u, 
				FeatureType.Hand => 104u, 
				FeatureType.Bone => 32u, 
				FeatureType.Eyes => 76u, 
				FeatureType.Custom => featureDescriptor.customSize, 
				_ => 0u, 
			};
		}

		private static string SanitizeString(string original, bool allowPaths = false)
		{
			int length = original.Length;
			StringBuilder stringBuilder = new StringBuilder(length);
			for (int i = 0; i < length; i++)
			{
				char c = original[i];
				if (char.IsUpper(c) || char.IsLower(c) || char.IsDigit(c) || (allowPaths && c == '/'))
				{
					stringBuilder.Append(c);
				}
			}
			return stringBuilder.ToString();
		}

		internal static string OnFindLayoutForDevice(ref InputDeviceDescription description, string matchedLayout, InputDeviceExecuteCommandDelegate executeCommandDelegate)
		{
			if (description.interfaceName != "XRInputV1" && description.interfaceName != "XRInput")
			{
				return null;
			}
			if (string.IsNullOrEmpty(description.capabilities))
			{
				return null;
			}
			XRDeviceDescriptor xRDeviceDescriptor;
			try
			{
				xRDeviceDescriptor = XRDeviceDescriptor.FromJson(description.capabilities);
			}
			catch (Exception)
			{
				return null;
			}
			if (xRDeviceDescriptor == null)
			{
				return null;
			}
			if (string.IsNullOrEmpty(matchedLayout))
			{
				if ((xRDeviceDescriptor.characteristics & InputDeviceCharacteristics.HeadMounted) != 0)
				{
					matchedLayout = "XRHMD";
				}
				else if ((xRDeviceDescriptor.characteristics & (InputDeviceCharacteristics.HeldInHand | InputDeviceCharacteristics.Controller)) == (InputDeviceCharacteristics.HeldInHand | InputDeviceCharacteristics.Controller))
				{
					matchedLayout = "XRController";
				}
			}
			string text = ((!string.IsNullOrEmpty(description.manufacturer)) ? (SanitizeString(description.interfaceName) + "::" + SanitizeString(description.manufacturer) + "::" + SanitizeString(description.product)) : (SanitizeString(description.interfaceName) + "::" + SanitizeString(description.product)));
			XRLayoutBuilder layout = new XRLayoutBuilder
			{
				descriptor = xRDeviceDescriptor,
				parentLayout = matchedLayout,
				interfaceName = description.interfaceName
			};
			InputSystem.RegisterLayoutBuilder(() => layout.Build(), text, matchedLayout);
			return text;
		}

		private static string ConvertPotentialAliasToName(InputControlLayout layout, string nameOrAlias)
		{
			InternedString internedString = new InternedString(nameOrAlias);
			ReadOnlyArray<InputControlLayout.ControlItem> controls = layout.controls;
			for (int i = 0; i < controls.Count; i++)
			{
				InputControlLayout.ControlItem controlItem = controls[i];
				if (controlItem.name == internedString)
				{
					return nameOrAlias;
				}
				ReadOnlyArray<InternedString> aliases = controlItem.aliases;
				for (int j = 0; j < aliases.Count; j++)
				{
					if (aliases[j] == nameOrAlias)
					{
						return controlItem.name.ToString();
					}
				}
			}
			return nameOrAlias;
		}

		private InputControlLayout Build()
		{
			InputControlLayout.Builder builder = new InputControlLayout.Builder
			{
				stateFormat = new FourCC('X', 'R', 'S', '0'),
				extendsLayout = parentLayout,
				updateBeforeRender = true
			};
			InputControlLayout inputControlLayout = ((!string.IsNullOrEmpty(parentLayout)) ? InputSystem.LoadLayout(parentLayout) : null);
			List<string> list = new List<string>();
			uint num = 0u;
			foreach (XRFeatureDescriptor inputFeature in descriptor.inputFeatures)
			{
				list.Clear();
				if (inputFeature.usageHints != null)
				{
					foreach (UsageHint usageHint in inputFeature.usageHints)
					{
						if (!string.IsNullOrEmpty(usageHint.content))
						{
							list.Add(usageHint.content);
						}
					}
				}
				string name = inputFeature.name;
				name = SanitizeString(name, allowPaths: true);
				if (inputControlLayout != null)
				{
					name = ConvertPotentialAliasToName(inputControlLayout, name);
				}
				name = name.ToLower();
				uint sizeOfFeature = GetSizeOfFeature(inputFeature);
				if (!(interfaceName == "XRInput") && sizeOfFeature >= 4 && num % 4u != 0)
				{
					num += 4 - num % 4u;
				}
				switch (inputFeature.featureType)
				{
				case FeatureType.Binary:
					builder.AddControl(name).WithLayout("Button").WithByteOffset(num)
						.WithFormat(InputStateBlock.FormatBit)
						.WithUsages(list);
					break;
				case FeatureType.DiscreteStates:
					builder.AddControl(name).WithLayout("Integer").WithByteOffset(num)
						.WithFormat(InputStateBlock.FormatInt)
						.WithUsages(list);
					break;
				case FeatureType.Axis1D:
					builder.AddControl(name).WithLayout("Analog").WithByteOffset(num)
						.WithFormat(InputStateBlock.FormatFloat)
						.WithUsages(list);
					break;
				case FeatureType.Axis2D:
					builder.AddControl(name).WithLayout("Vector2").WithByteOffset(num)
						.WithFormat(InputStateBlock.FormatVector2)
						.WithUsages(list);
					break;
				case FeatureType.Axis3D:
					builder.AddControl(name).WithLayout("Vector3").WithByteOffset(num)
						.WithFormat(InputStateBlock.FormatVector3)
						.WithUsages(list);
					break;
				case FeatureType.Rotation:
					builder.AddControl(name).WithLayout("Quaternion").WithByteOffset(num)
						.WithFormat(InputStateBlock.FormatQuaternion)
						.WithUsages(list);
					break;
				case FeatureType.Bone:
					builder.AddControl(name).WithLayout("Bone").WithByteOffset(num)
						.WithUsages(list);
					break;
				case FeatureType.Eyes:
					builder.AddControl(name).WithLayout("Eyes").WithByteOffset(num)
						.WithUsages(list);
					break;
				}
				num += sizeOfFeature;
			}
			return builder.Build();
		}
	}
}
