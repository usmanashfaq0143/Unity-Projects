namespace Mono.Cecil.Cil
{
	public sealed class Document : DebugInformation
	{
		private string url;

		private byte type;

		private byte hash_algorithm;

		private byte language;

		private byte language_vendor;

		private byte[] hash;

		public string Url
		{
			get
			{
				return url;
			}
			set
			{
				url = value;
			}
		}

		public DocumentType Type
		{
			get
			{
				return (DocumentType)type;
			}
			set
			{
				type = (byte)value;
			}
		}

		public DocumentHashAlgorithm HashAlgorithm
		{
			get
			{
				return (DocumentHashAlgorithm)hash_algorithm;
			}
			set
			{
				hash_algorithm = (byte)value;
			}
		}

		public DocumentLanguage Language
		{
			get
			{
				return (DocumentLanguage)language;
			}
			set
			{
				language = (byte)value;
			}
		}

		public DocumentLanguageVendor LanguageVendor
		{
			get
			{
				return (DocumentLanguageVendor)language_vendor;
			}
			set
			{
				language_vendor = (byte)value;
			}
		}

		public byte[] Hash
		{
			get
			{
				return hash;
			}
			set
			{
				hash = value;
			}
		}

		public Document(string url)
		{
			this.url = url;
			hash = Empty<byte>.Array;
			token = new MetadataToken(TokenType.Document);
		}
	}
}
