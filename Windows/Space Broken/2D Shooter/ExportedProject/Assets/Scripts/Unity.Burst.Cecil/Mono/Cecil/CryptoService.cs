using System;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using Mono.Cecil.PE;

namespace Mono.Cecil
{
	internal static class CryptoService
	{
		public static void StrongName(Stream stream, ImageWriter writer, StrongNameKeyPair key_pair)
		{
			int strong_name_pointer;
			byte[] strong_name = CreateStrongName(key_pair, HashStream(stream, writer, out strong_name_pointer));
			PatchStrongName(stream, strong_name_pointer, strong_name);
		}

		private static void PatchStrongName(Stream stream, int strong_name_pointer, byte[] strong_name)
		{
			stream.Seek(strong_name_pointer, SeekOrigin.Begin);
			stream.Write(strong_name, 0, strong_name.Length);
		}

		private static byte[] CreateStrongName(StrongNameKeyPair key_pair, byte[] hash)
		{
			using RSA key = key_pair.CreateRSA();
			RSAPKCS1SignatureFormatter rSAPKCS1SignatureFormatter = new RSAPKCS1SignatureFormatter(key);
			rSAPKCS1SignatureFormatter.SetHashAlgorithm("SHA1");
			byte[] array = rSAPKCS1SignatureFormatter.CreateSignature(hash);
			Array.Reverse((Array)array);
			return array;
		}

		private static byte[] HashStream(Stream stream, ImageWriter writer, out int strong_name_pointer)
		{
			Section text = writer.text;
			int headerSize = (int)writer.GetHeaderSize();
			int pointerToRawData = (int)text.PointerToRawData;
			DataDirectory strongNameSignatureDirectory = writer.GetStrongNameSignatureDirectory();
			if (strongNameSignatureDirectory.Size == 0)
			{
				throw new InvalidOperationException();
			}
			strong_name_pointer = (int)(pointerToRawData + (strongNameSignatureDirectory.VirtualAddress - text.VirtualAddress));
			int size = (int)strongNameSignatureDirectory.Size;
			SHA1Managed sHA1Managed = new SHA1Managed();
			byte[] buffer = new byte[8192];
			using (CryptoStream dest_stream = new CryptoStream(Stream.Null, sHA1Managed, CryptoStreamMode.Write))
			{
				stream.Seek(0L, SeekOrigin.Begin);
				CopyStreamChunk(stream, dest_stream, buffer, headerSize);
				stream.Seek(pointerToRawData, SeekOrigin.Begin);
				CopyStreamChunk(stream, dest_stream, buffer, strong_name_pointer - pointerToRawData);
				stream.Seek(size, SeekOrigin.Current);
				CopyStreamChunk(stream, dest_stream, buffer, (int)(stream.Length - (strong_name_pointer + size)));
			}
			return sHA1Managed.Hash;
		}

		private static void CopyStreamChunk(Stream stream, Stream dest_stream, byte[] buffer, int length)
		{
			while (length > 0)
			{
				int num = stream.Read(buffer, 0, System.Math.Min(buffer.Length, length));
				dest_stream.Write(buffer, 0, num);
				length -= num;
			}
		}

		public static byte[] ComputeHash(string file)
		{
			if (!File.Exists(file))
			{
				return Empty<byte>.Array;
			}
			using FileStream stream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read);
			return ComputeHash(stream);
		}

		public static byte[] ComputeHash(Stream stream)
		{
			SHA1Managed sHA1Managed = new SHA1Managed();
			byte[] buffer = new byte[8192];
			using (CryptoStream dest_stream = new CryptoStream(Stream.Null, sHA1Managed, CryptoStreamMode.Write))
			{
				CopyStreamChunk(stream, dest_stream, buffer, (int)stream.Length);
			}
			return sHA1Managed.Hash;
		}

		public static byte[] ComputeHash(params ByteBuffer[] buffers)
		{
			SHA1Managed sHA1Managed = new SHA1Managed();
			using (CryptoStream cryptoStream = new CryptoStream(Stream.Null, sHA1Managed, CryptoStreamMode.Write))
			{
				for (int i = 0; i < buffers.Length; i++)
				{
					cryptoStream.Write(buffers[i].buffer, 0, buffers[i].length);
				}
			}
			return sHA1Managed.Hash;
		}

		public static Guid ComputeGuid(byte[] hash)
		{
			byte[] array = new byte[16];
			Buffer.BlockCopy(hash, 0, array, 0, 16);
			array[7] = (byte)((array[7] & 0xFu) | 0x40u);
			array[8] = (byte)((array[8] & 0x3Fu) | 0x80u);
			return new Guid(array);
		}
	}
}
