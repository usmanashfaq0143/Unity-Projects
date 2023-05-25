using System.Collections.Generic;

namespace UnityEngine.U2D
{
	[HelpURL("https://docs.unity3d.com/Packages/com.unity.2d.spriteshape@latest/index.html?subfolder=/manual/SSProfile.html")]
	public class SpriteShape : ScriptableObject
	{
		[SerializeField]
		private List<AngleRange> m_Angles = new List<AngleRange>();

		[SerializeField]
		private Texture2D m_FillTexture;

		[SerializeField]
		private List<CornerSprite> m_CornerSprites = new List<CornerSprite>();

		[SerializeField]
		private float m_FillOffset;

		[SerializeField]
		private bool m_UseSpriteBorders = true;

		public List<AngleRange> angleRanges
		{
			get
			{
				return m_Angles;
			}
			set
			{
				m_Angles = value;
			}
		}

		public Texture2D fillTexture
		{
			get
			{
				return m_FillTexture;
			}
			set
			{
				m_FillTexture = value;
			}
		}

		public List<CornerSprite> cornerSprites
		{
			get
			{
				return m_CornerSprites;
			}
			set
			{
				m_CornerSprites = value;
			}
		}

		public float fillOffset
		{
			get
			{
				return m_FillOffset;
			}
			set
			{
				m_FillOffset = value;
			}
		}

		public bool useSpriteBorders
		{
			get
			{
				return m_UseSpriteBorders;
			}
			set
			{
				m_UseSpriteBorders = value;
			}
		}

		private CornerSprite GetCornerSprite(CornerType cornerType)
		{
			CornerSprite cornerSprite = new CornerSprite();
			cornerSprite.cornerType = cornerType;
			cornerSprite.sprites = new List<Sprite>();
			cornerSprite.sprites.Insert(0, null);
			return cornerSprite;
		}

		private void ResetCornerList()
		{
			m_CornerSprites.Clear();
			m_CornerSprites.Insert(0, GetCornerSprite(CornerType.OuterTopLeft));
			m_CornerSprites.Insert(1, GetCornerSprite(CornerType.OuterTopRight));
			m_CornerSprites.Insert(2, GetCornerSprite(CornerType.OuterBottomLeft));
			m_CornerSprites.Insert(3, GetCornerSprite(CornerType.OuterBottomRight));
			m_CornerSprites.Insert(4, GetCornerSprite(CornerType.InnerTopLeft));
			m_CornerSprites.Insert(5, GetCornerSprite(CornerType.InnerTopRight));
			m_CornerSprites.Insert(6, GetCornerSprite(CornerType.InnerBottomLeft));
			m_CornerSprites.Insert(7, GetCornerSprite(CornerType.InnerBottomRight));
		}

		private void OnValidate()
		{
			if (m_CornerSprites.Count != 8)
			{
				ResetCornerList();
			}
		}

		private void Reset()
		{
			m_Angles.Clear();
			ResetCornerList();
		}

		internal static int GetSpriteShapeHashCode(SpriteShape spriteShape)
		{
			int num = -2128831035;
			num = (num * 16777619) ^ spriteShape.angleRanges.Count;
			for (int i = 0; i < spriteShape.angleRanges.Count; i++)
			{
				num = (num * 16777619) ^ (spriteShape.angleRanges[i].GetHashCode() + i);
			}
			num = (num * 16777619) ^ spriteShape.cornerSprites.Count;
			for (int j = 0; j < spriteShape.cornerSprites.Count; j++)
			{
				num = (num * 16777619) ^ (spriteShape.cornerSprites[j].GetHashCode() + j);
			}
			return num;
		}
	}
}
