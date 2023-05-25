using System;
using System.IO;
using Mono.Cecil.Cil;
using Mono.Cecil.Metadata;

namespace Mono.Cecil.PE
{
	internal sealed class ImageReader : BinaryStreamReader
	{
		private readonly Image image;

		private DataDirectory cli;

		private DataDirectory metadata;

		private uint table_heap_offset;

		public ImageReader(Disposable<Stream> stream, string file_name)
			: base(stream.value)
		{
			image = new Image();
			image.Stream = stream;
			image.FileName = file_name;
		}

		private void MoveTo(DataDirectory directory)
		{
			BaseStream.Position = image.ResolveVirtualAddress(directory.VirtualAddress);
		}

		private void ReadImage()
		{
			if (BaseStream.Length < 128)
			{
				throw new BadImageFormatException();
			}
			if (ReadUInt16() != 23117)
			{
				throw new BadImageFormatException();
			}
			Advance(58);
			MoveTo(ReadUInt32());
			if (ReadUInt32() != 17744)
			{
				throw new BadImageFormatException();
			}
			image.Architecture = ReadArchitecture();
			ushort count = ReadUInt16();
			image.Timestamp = ReadUInt32();
			Advance(10);
			ushort characteristics = ReadUInt16();
			ReadOptionalHeaders(out var subsystem, out var dll_characteristics);
			ReadSections(count);
			ReadCLIHeader();
			ReadMetadata();
			ReadDebugHeader();
			image.Kind = GetModuleKind(characteristics, subsystem);
			image.Characteristics = (ModuleCharacteristics)dll_characteristics;
		}

		private TargetArchitecture ReadArchitecture()
		{
			return (TargetArchitecture)ReadUInt16();
		}

		private static ModuleKind GetModuleKind(ushort characteristics, ushort subsystem)
		{
			if ((characteristics & 0x2000u) != 0)
			{
				return ModuleKind.Dll;
			}
			if (subsystem == 2 || subsystem == 9)
			{
				return ModuleKind.Windows;
			}
			return ModuleKind.Console;
		}

		private void ReadOptionalHeaders(out ushort subsystem, out ushort dll_characteristics)
		{
			bool flag = ReadUInt16() == 523;
			Advance(66);
			subsystem = ReadUInt16();
			dll_characteristics = ReadUInt16();
			Advance(flag ? 88 : 72);
			image.Debug = ReadDataDirectory();
			Advance(56);
			cli = ReadDataDirectory();
			if (cli.IsZero)
			{
				throw new BadImageFormatException();
			}
			Advance(8);
		}

		private string ReadAlignedString(int length)
		{
			int num = 0;
			char[] array = new char[length];
			while (num < length)
			{
				byte b = ReadByte();
				if (b == 0)
				{
					break;
				}
				array[num++] = (char)b;
			}
			Advance(-1 + ((num + 4) & -4) - num);
			return new string(array, 0, num);
		}

		private string ReadZeroTerminatedString(int length)
		{
			int num = 0;
			char[] array = new char[length];
			byte[] array2 = ReadBytes(length);
			while (num < length)
			{
				byte b = array2[num];
				if (b == 0)
				{
					break;
				}
				array[num++] = (char)b;
			}
			return new string(array, 0, num);
		}

		private void ReadSections(ushort count)
		{
			Section[] array = new Section[count];
			for (int i = 0; i < count; i++)
			{
				Section section = new Section();
				section.Name = ReadZeroTerminatedString(8);
				Advance(4);
				section.VirtualAddress = ReadUInt32();
				section.SizeOfRawData = ReadUInt32();
				section.PointerToRawData = ReadUInt32();
				Advance(16);
				array[i] = section;
			}
			image.Sections = array;
		}

		private void ReadCLIHeader()
		{
			MoveTo(cli);
			Advance(8);
			metadata = ReadDataDirectory();
			image.Attributes = (ModuleAttributes)ReadUInt32();
			image.EntryPointToken = ReadUInt32();
			image.Resources = ReadDataDirectory();
			image.StrongName = ReadDataDirectory();
		}

		private void ReadMetadata()
		{
			MoveTo(metadata);
			if (ReadUInt32() != 1112167234)
			{
				throw new BadImageFormatException();
			}
			Advance(8);
			image.RuntimeVersion = ReadZeroTerminatedString(ReadInt32());
			Advance(2);
			ushort num = ReadUInt16();
			Section sectionAtVirtualAddress = image.GetSectionAtVirtualAddress(metadata.VirtualAddress);
			if (sectionAtVirtualAddress == null)
			{
				throw new BadImageFormatException();
			}
			image.MetadataSection = sectionAtVirtualAddress;
			for (int i = 0; i < num; i++)
			{
				ReadMetadataStream(sectionAtVirtualAddress);
			}
			if (image.PdbHeap != null)
			{
				ReadPdbHeap();
			}
			if (image.TableHeap != null)
			{
				ReadTableHeap();
			}
		}

		private void ReadDebugHeader()
		{
			if (image.Debug.IsZero)
			{
				image.DebugHeader = new ImageDebugHeader(Empty<ImageDebugHeaderEntry>.Array);
				return;
			}
			MoveTo(image.Debug);
			ImageDebugHeaderEntry[] array = new ImageDebugHeaderEntry[(int)image.Debug.Size / 28];
			for (int i = 0; i < array.Length; i++)
			{
				ImageDebugDirectory imageDebugDirectory = default(ImageDebugDirectory);
				imageDebugDirectory.Characteristics = ReadInt32();
				imageDebugDirectory.TimeDateStamp = ReadInt32();
				imageDebugDirectory.MajorVersion = ReadInt16();
				imageDebugDirectory.MinorVersion = ReadInt16();
				imageDebugDirectory.Type = (ImageDebugType)ReadInt32();
				imageDebugDirectory.SizeOfData = ReadInt32();
				imageDebugDirectory.AddressOfRawData = ReadInt32();
				imageDebugDirectory.PointerToRawData = ReadInt32();
				ImageDebugDirectory directory = imageDebugDirectory;
				if (directory.AddressOfRawData == 0)
				{
					array[i] = new ImageDebugHeaderEntry(directory, Empty<byte>.Array);
					continue;
				}
				int position = base.Position;
				try
				{
					MoveTo((uint)directory.PointerToRawData);
					byte[] data = ReadBytes(directory.SizeOfData);
					array[i] = new ImageDebugHeaderEntry(directory, data);
				}
				finally
				{
					base.Position = position;
				}
			}
			image.DebugHeader = new ImageDebugHeader(array);
		}

		private void ReadMetadataStream(Section section)
		{
			uint offset = metadata.VirtualAddress - section.VirtualAddress + ReadUInt32();
			uint size = ReadUInt32();
			byte[] data = ReadHeapData(offset, size);
			switch (ReadAlignedString(16))
			{
			case "#~":
			case "#-":
				image.TableHeap = new TableHeap(data);
				table_heap_offset = offset;
				break;
			case "#Strings":
				image.StringHeap = new StringHeap(data);
				break;
			case "#Blob":
				image.BlobHeap = new BlobHeap(data);
				break;
			case "#GUID":
				image.GuidHeap = new GuidHeap(data);
				break;
			case "#US":
				image.UserStringHeap = new UserStringHeap(data);
				break;
			case "#Pdb":
				image.PdbHeap = new PdbHeap(data);
				break;
			}
		}

		private byte[] ReadHeapData(uint offset, uint size)
		{
			long position = BaseStream.Position;
			MoveTo(offset + image.MetadataSection.PointerToRawData);
			byte[] result = ReadBytes((int)size);
			BaseStream.Position = position;
			return result;
		}

		private void ReadTableHeap()
		{
			TableHeap tableHeap = image.TableHeap;
			MoveTo(table_heap_offset + image.MetadataSection.PointerToRawData);
			Advance(6);
			byte sizes = ReadByte();
			Advance(1);
			tableHeap.Valid = ReadInt64();
			tableHeap.Sorted = ReadInt64();
			if (image.PdbHeap != null)
			{
				for (int i = 0; i < 58; i++)
				{
					if (image.PdbHeap.HasTable((Table)i))
					{
						tableHeap.Tables[i].Length = image.PdbHeap.TypeSystemTableRows[i];
					}
				}
			}
			for (int j = 0; j < 58; j++)
			{
				if (tableHeap.HasTable((Table)j))
				{
					tableHeap.Tables[j].Length = ReadUInt32();
				}
			}
			SetIndexSize(image.StringHeap, sizes, 1);
			SetIndexSize(image.GuidHeap, sizes, 2);
			SetIndexSize(image.BlobHeap, sizes, 4);
			ComputeTableInformations();
		}

		private static void SetIndexSize(Heap heap, uint sizes, byte flag)
		{
			if (heap != null)
			{
				heap.IndexSize = (((sizes & flag) != 0) ? 4 : 2);
			}
		}

		private int GetTableIndexSize(Table table)
		{
			return image.GetTableIndexSize(table);
		}

		private int GetCodedIndexSize(CodedIndex index)
		{
			return image.GetCodedIndexSize(index);
		}

		private void ComputeTableInformations()
		{
			uint num = (uint)((int)BaseStream.Position - (int)table_heap_offset) - image.MetadataSection.PointerToRawData;
			int indexSize = image.StringHeap.IndexSize;
			int num2 = ((image.GuidHeap != null) ? image.GuidHeap.IndexSize : 2);
			int num3 = ((image.BlobHeap != null) ? image.BlobHeap.IndexSize : 2);
			TableHeap tableHeap = image.TableHeap;
			TableInformation[] tables = tableHeap.Tables;
			for (int i = 0; i < 58; i++)
			{
				Table table = (Table)i;
				if (tableHeap.HasTable(table))
				{
					int num4 = table switch
					{
						Table.Module => 2 + indexSize + num2 * 3, 
						Table.TypeRef => GetCodedIndexSize(CodedIndex.ResolutionScope) + indexSize * 2, 
						Table.TypeDef => 4 + indexSize * 2 + GetCodedIndexSize(CodedIndex.TypeDefOrRef) + GetTableIndexSize(Table.Field) + GetTableIndexSize(Table.Method), 
						Table.FieldPtr => GetTableIndexSize(Table.Field), 
						Table.Field => 2 + indexSize + num3, 
						Table.MethodPtr => GetTableIndexSize(Table.Method), 
						Table.Method => 8 + indexSize + num3 + GetTableIndexSize(Table.Param), 
						Table.ParamPtr => GetTableIndexSize(Table.Param), 
						Table.Param => 4 + indexSize, 
						Table.InterfaceImpl => GetTableIndexSize(Table.TypeDef) + GetCodedIndexSize(CodedIndex.TypeDefOrRef), 
						Table.MemberRef => GetCodedIndexSize(CodedIndex.MemberRefParent) + indexSize + num3, 
						Table.Constant => 2 + GetCodedIndexSize(CodedIndex.HasConstant) + num3, 
						Table.CustomAttribute => GetCodedIndexSize(CodedIndex.HasCustomAttribute) + GetCodedIndexSize(CodedIndex.CustomAttributeType) + num3, 
						Table.FieldMarshal => GetCodedIndexSize(CodedIndex.HasFieldMarshal) + num3, 
						Table.DeclSecurity => 2 + GetCodedIndexSize(CodedIndex.HasDeclSecurity) + num3, 
						Table.ClassLayout => 6 + GetTableIndexSize(Table.TypeDef), 
						Table.FieldLayout => 4 + GetTableIndexSize(Table.Field), 
						Table.StandAloneSig => num3, 
						Table.EventMap => GetTableIndexSize(Table.TypeDef) + GetTableIndexSize(Table.Event), 
						Table.EventPtr => GetTableIndexSize(Table.Event), 
						Table.Event => 2 + indexSize + GetCodedIndexSize(CodedIndex.TypeDefOrRef), 
						Table.PropertyMap => GetTableIndexSize(Table.TypeDef) + GetTableIndexSize(Table.Property), 
						Table.PropertyPtr => GetTableIndexSize(Table.Property), 
						Table.Property => 2 + indexSize + num3, 
						Table.MethodSemantics => 2 + GetTableIndexSize(Table.Method) + GetCodedIndexSize(CodedIndex.HasSemantics), 
						Table.MethodImpl => GetTableIndexSize(Table.TypeDef) + GetCodedIndexSize(CodedIndex.MethodDefOrRef) + GetCodedIndexSize(CodedIndex.MethodDefOrRef), 
						Table.ModuleRef => indexSize, 
						Table.TypeSpec => num3, 
						Table.ImplMap => 2 + GetCodedIndexSize(CodedIndex.MemberForwarded) + indexSize + GetTableIndexSize(Table.ModuleRef), 
						Table.FieldRVA => 4 + GetTableIndexSize(Table.Field), 
						Table.EncLog => 8, 
						Table.EncMap => 4, 
						Table.Assembly => 16 + num3 + indexSize * 2, 
						Table.AssemblyProcessor => 4, 
						Table.AssemblyOS => 12, 
						Table.AssemblyRef => 12 + num3 * 2 + indexSize * 2, 
						Table.AssemblyRefProcessor => 4 + GetTableIndexSize(Table.AssemblyRef), 
						Table.AssemblyRefOS => 12 + GetTableIndexSize(Table.AssemblyRef), 
						Table.File => 4 + indexSize + num3, 
						Table.ExportedType => 8 + indexSize * 2 + GetCodedIndexSize(CodedIndex.Implementation), 
						Table.ManifestResource => 8 + indexSize + GetCodedIndexSize(CodedIndex.Implementation), 
						Table.NestedClass => GetTableIndexSize(Table.TypeDef) + GetTableIndexSize(Table.TypeDef), 
						Table.GenericParam => 4 + GetCodedIndexSize(CodedIndex.TypeOrMethodDef) + indexSize, 
						Table.MethodSpec => GetCodedIndexSize(CodedIndex.MethodDefOrRef) + num3, 
						Table.GenericParamConstraint => GetTableIndexSize(Table.GenericParam) + GetCodedIndexSize(CodedIndex.TypeDefOrRef), 
						Table.Document => num3 + num2 + num3 + num2, 
						Table.MethodDebugInformation => GetTableIndexSize(Table.Document) + num3, 
						Table.LocalScope => GetTableIndexSize(Table.Method) + GetTableIndexSize(Table.ImportScope) + GetTableIndexSize(Table.LocalVariable) + GetTableIndexSize(Table.LocalConstant) + 8, 
						Table.LocalVariable => 4 + indexSize, 
						Table.LocalConstant => indexSize + num3, 
						Table.ImportScope => GetTableIndexSize(Table.ImportScope) + num3, 
						Table.StateMachineMethod => GetTableIndexSize(Table.Method) + GetTableIndexSize(Table.Method), 
						Table.CustomDebugInformation => GetCodedIndexSize(CodedIndex.HasCustomDebugInformation) + num2 + num3, 
						_ => throw new NotSupportedException(), 
					};
					tables[i].RowSize = (uint)num4;
					tables[i].Offset = num;
					num += (uint)(num4 * (int)tables[i].Length);
				}
			}
		}

		private void ReadPdbHeap()
		{
			PdbHeap pdbHeap = image.PdbHeap;
			ByteBuffer byteBuffer = new ByteBuffer(pdbHeap.data);
			pdbHeap.Id = byteBuffer.ReadBytes(20);
			pdbHeap.EntryPoint = byteBuffer.ReadUInt32();
			pdbHeap.TypeSystemTables = byteBuffer.ReadInt64();
			pdbHeap.TypeSystemTableRows = new uint[58];
			for (int i = 0; i < 58; i++)
			{
				Table table = (Table)i;
				if (pdbHeap.HasTable(table))
				{
					pdbHeap.TypeSystemTableRows[i] = byteBuffer.ReadUInt32();
				}
			}
		}

		public static Image ReadImage(Disposable<Stream> stream, string file_name)
		{
			try
			{
				ImageReader imageReader = new ImageReader(stream, file_name);
				imageReader.ReadImage();
				return imageReader.image;
			}
			catch (EndOfStreamException inner)
			{
				throw new BadImageFormatException(stream.value.GetFileName(), inner);
			}
		}

		public static Image ReadPortablePdb(Disposable<Stream> stream, string file_name)
		{
			try
			{
				ImageReader imageReader = new ImageReader(stream, file_name);
				uint num = (uint)stream.value.Length;
				imageReader.image.Sections = new Section[1]
				{
					new Section
					{
						PointerToRawData = 0u,
						SizeOfRawData = num,
						VirtualAddress = 0u,
						VirtualSize = num
					}
				};
				imageReader.metadata = new DataDirectory(0u, num);
				imageReader.ReadMetadata();
				return imageReader.image;
			}
			catch (EndOfStreamException inner)
			{
				throw new BadImageFormatException(stream.value.GetFileName(), inner);
			}
		}
	}
}
