using System;
using UnityEngine;

namespace Unity.VisualScripting
{
	public abstract class LudiqBehaviour : MonoBehaviour, ISerializationCallbackReceiver
	{
		[SerializeField]
		[DoNotSerialize]
		protected SerializationData _data;

		void ISerializationCallbackReceiver.OnBeforeSerialize()
		{
			if (!Serialization.isCustomSerializing)
			{
				Serialization.isUnitySerializing = true;
				try
				{
					OnBeforeSerialize();
					_data = this.Serialize(forceReflected: true);
					OnAfterSerialize();
				}
				catch (Exception arg)
				{
					Debug.LogError($"Failed to serialize behaviour.\n{arg}", this);
				}
				Serialization.isUnitySerializing = false;
			}
		}

		void ISerializationCallbackReceiver.OnAfterDeserialize()
		{
			if (!Serialization.isCustomSerializing)
			{
				Serialization.isUnitySerializing = true;
				try
				{
					object instance = this;
					OnBeforeDeserialize();
					_data.DeserializeInto(ref instance, forceReflected: true);
					OnAfterDeserialize();
				}
				catch (Exception arg)
				{
					Debug.LogError($"Failed to deserialize behaviour.\n{arg}", this);
				}
				Serialization.isUnitySerializing = false;
			}
		}

		protected virtual void OnBeforeSerialize()
		{
		}

		protected virtual void OnAfterSerialize()
		{
		}

		protected virtual void OnBeforeDeserialize()
		{
		}

		protected virtual void OnAfterDeserialize()
		{
		}

		protected virtual void ShowData()
		{
			_data.ShowString(ToString());
		}

		public override string ToString()
		{
			return this.ToSafeString();
		}
	}
}
