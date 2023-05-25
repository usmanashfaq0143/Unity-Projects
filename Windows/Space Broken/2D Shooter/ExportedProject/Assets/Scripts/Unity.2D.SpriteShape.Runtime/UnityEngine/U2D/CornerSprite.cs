using System;
using System.Collections.Generic;

namespace UnityEngine.U2D
{
	[Serializable]
	public class CornerSprite : ICloneable
	{
		[SerializeField]
		private CornerType m_CornerType;

		[SerializeField]
		private List<Sprite> m_Sprites;

		public CornerType cornerType
		{
			get
			{
				return m_CornerType;
			}
			set
			{
				m_CornerType = value;
			}
		}

		public List<Sprite> sprites
		{
			get
			{
				return m_Sprites;
			}
			set
			{
				m_Sprites = value;
			}
		}

		public object Clone()
		{
			CornerSprite obj = MemberwiseClone() as CornerSprite;
			obj.sprites = new List<Sprite>(obj.sprites);
			return obj;
		}

		public override bool Equals(object obj)
		{
			if (!(obj is CornerSprite cornerSprite))
			{
				return false;
			}
			if (!cornerType.Equals(cornerSprite.cornerType))
			{
				return false;
			}
			if (sprites.Count != cornerSprite.sprites.Count)
			{
				return false;
			}
			for (int i = 0; i < sprites.Count; i++)
			{
				if (sprites[i] != cornerSprite.sprites[i])
				{
					return false;
				}
			}
			return true;
		}

		public override int GetHashCode()
		{
			int num = cornerType.GetHashCode();
			if (sprites != null)
			{
				for (int i = 0; i < sprites.Count; i++)
				{
					Sprite sprite = sprites[i];
					if ((bool)sprite)
					{
						num ^= i + 1;
						num ^= sprite.GetHashCode();
					}
				}
			}
			return num;
		}
	}
}
