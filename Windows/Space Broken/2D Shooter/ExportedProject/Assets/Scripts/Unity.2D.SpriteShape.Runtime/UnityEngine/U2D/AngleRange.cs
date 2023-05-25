using System;
using System.Collections.Generic;

namespace UnityEngine.U2D
{
	[Serializable]
	public class AngleRange : ICloneable
	{
		[SerializeField]
		private float m_Start;

		[SerializeField]
		private float m_End;

		[SerializeField]
		private int m_Order;

		[SerializeField]
		private List<Sprite> m_Sprites = new List<Sprite>();

		public float start
		{
			get
			{
				return m_Start;
			}
			set
			{
				m_Start = value;
			}
		}

		public float end
		{
			get
			{
				return m_End;
			}
			set
			{
				m_End = value;
			}
		}

		public int order
		{
			get
			{
				return m_Order;
			}
			set
			{
				m_Order = value;
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
			AngleRange obj = MemberwiseClone() as AngleRange;
			obj.sprites = new List<Sprite>(obj.sprites);
			return obj;
		}

		public override bool Equals(object obj)
		{
			if (!(obj is AngleRange angleRange))
			{
				return false;
			}
			if (!start.Equals(angleRange.start) || !end.Equals(angleRange.end) || !order.Equals(angleRange.order))
			{
				return false;
			}
			if (sprites.Count != angleRange.sprites.Count)
			{
				return false;
			}
			for (int i = 0; i < sprites.Count; i++)
			{
				if (sprites[i] != angleRange.sprites[i])
				{
					return false;
				}
			}
			return true;
		}

		public override int GetHashCode()
		{
			int num = start.GetHashCode() ^ end.GetHashCode() ^ order.GetHashCode();
			if (sprites != null)
			{
				for (int i = 0; i < sprites.Count; i++)
				{
					Sprite sprite = sprites[i];
					if ((bool)sprite)
					{
						num = (num * 16777619) ^ (sprite.GetHashCode() + i);
					}
				}
			}
			return num;
		}
	}
}
