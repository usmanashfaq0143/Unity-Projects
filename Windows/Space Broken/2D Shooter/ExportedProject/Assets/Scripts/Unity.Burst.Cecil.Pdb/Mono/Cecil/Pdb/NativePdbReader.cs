using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Cci;
using Microsoft.Cci.Pdb;
using Mono.Cecil.Cil;
using Mono.Collections.Generic;

namespace Mono.Cecil.Pdb
{
	public class NativePdbReader : ISymbolReader, IDisposable
	{
		private int age;

		private Guid guid;

		private readonly Disposable<Stream> pdb_file;

		private readonly Dictionary<string, Document> documents = new Dictionary<string, Document>();

		private readonly Dictionary<uint, PdbFunction> functions = new Dictionary<uint, PdbFunction>();

		private readonly Dictionary<PdbScope, ImportDebugInformation> imports = new Dictionary<PdbScope, ImportDebugInformation>();

		internal NativePdbReader(Disposable<Stream> file)
		{
			pdb_file = file;
		}

		public ISymbolWriterProvider GetWriterProvider()
		{
			return new NativePdbWriterProvider();
		}

		public bool ProcessDebugHeader(ImageDebugHeader header)
		{
			if (!header.HasEntries)
			{
				return false;
			}
			ImageDebugHeaderEntry codeViewEntry = header.GetCodeViewEntry();
			if (codeViewEntry == null)
			{
				return false;
			}
			ImageDebugDirectory directory = codeViewEntry.Directory;
			if (directory.Type != ImageDebugType.CodeView)
			{
				return false;
			}
			if (directory.MajorVersion != 0 || directory.MinorVersion != 0)
			{
				return false;
			}
			byte[] data = codeViewEntry.Data;
			if (data.Length < 24)
			{
				return false;
			}
			if (ReadInt32(data, 0) != 1396986706)
			{
				return false;
			}
			byte[] array = new byte[16];
			Buffer.BlockCopy(data, 4, array, 0, 16);
			guid = new Guid(array);
			age = ReadInt32(data, 20);
			return PopulateFunctions();
		}

		private static int ReadInt32(byte[] bytes, int start)
		{
			return bytes[start] | (bytes[start + 1] << 8) | (bytes[start + 2] << 16) | (bytes[start + 3] << 24);
		}

		private bool PopulateFunctions()
		{
			using (pdb_file)
			{
				Dictionary<uint, PdbTokenLine> tokenToSourceMapping;
				string sourceServerData;
				int num;
				Guid guid;
				PdbFunction[] array = PdbFile.LoadFunctions(pdb_file.value, out tokenToSourceMapping, out sourceServerData, out num, out guid);
				if (this.guid != guid)
				{
					return false;
				}
				PdbFunction[] array2 = array;
				foreach (PdbFunction pdbFunction in array2)
				{
					functions.Add(pdbFunction.token, pdbFunction);
				}
			}
			return true;
		}

		public MethodDebugInformation Read(MethodDefinition method)
		{
			MetadataToken metadataToken = method.MetadataToken;
			if (!functions.TryGetValue(metadataToken.ToUInt32(), out var value))
			{
				return null;
			}
			MethodDebugInformation methodDebugInformation = new MethodDebugInformation(method);
			ReadSequencePoints(value, methodDebugInformation);
			methodDebugInformation.scope = ((!value.scopes.IsNullOrEmpty()) ? ReadScopeAndLocals(value.scopes[0], methodDebugInformation) : new ScopeDebugInformation
			{
				Start = new InstructionOffset(0),
				End = new InstructionOffset((int)value.length)
			});
			if (value.tokenOfMethodWhoseUsingInfoAppliesToThisMethod != method.MetadataToken.ToUInt32() && value.tokenOfMethodWhoseUsingInfoAppliesToThisMethod != 0)
			{
				methodDebugInformation.scope.import = GetImport(value.tokenOfMethodWhoseUsingInfoAppliesToThisMethod, method.Module);
			}
			if (value.scopes.Length > 1)
			{
				for (int i = 1; i < value.scopes.Length; i++)
				{
					ScopeDebugInformation scopeDebugInformation = ReadScopeAndLocals(value.scopes[i], methodDebugInformation);
					if (!AddScope(methodDebugInformation.scope.Scopes, scopeDebugInformation))
					{
						methodDebugInformation.scope.Scopes.Add(scopeDebugInformation);
					}
				}
			}
			if (value.iteratorScopes != null)
			{
				foreach (ILocalScope iteratorScope in value.iteratorScopes)
				{
					methodDebugInformation.CustomDebugInformations.Add(new StateMachineScopeDebugInformation((int)iteratorScope.Offset, (int)(iteratorScope.Offset + iteratorScope.Length + 1)));
				}
			}
			if (value.synchronizationInformation != null)
			{
				AsyncMethodBodyDebugInformation asyncMethodBodyDebugInformation = new AsyncMethodBodyDebugInformation((int)value.synchronizationInformation.GeneratedCatchHandlerOffset);
				PdbSynchronizationPoint[] synchronizationPoints = value.synchronizationInformation.synchronizationPoints;
				foreach (PdbSynchronizationPoint pdbSynchronizationPoint in synchronizationPoints)
				{
					asyncMethodBodyDebugInformation.Yields.Add(new InstructionOffset((int)pdbSynchronizationPoint.SynchronizeOffset));
					asyncMethodBodyDebugInformation.Resumes.Add(new InstructionOffset((int)pdbSynchronizationPoint.ContinuationOffset));
				}
				methodDebugInformation.CustomDebugInformations.Add(asyncMethodBodyDebugInformation);
				asyncMethodBodyDebugInformation.MoveNextMethod = method;
				methodDebugInformation.StateMachineKickOffMethod = (MethodDefinition)method.Module.LookupToken((int)value.synchronizationInformation.kickoffMethodToken);
			}
			return methodDebugInformation;
		}

		private Collection<ScopeDebugInformation> ReadScopeAndLocals(PdbScope[] scopes, MethodDebugInformation info)
		{
			Collection<ScopeDebugInformation> collection = new Collection<ScopeDebugInformation>(scopes.Length);
			foreach (PdbScope pdbScope in scopes)
			{
				if (pdbScope != null)
				{
					collection.Add(ReadScopeAndLocals(pdbScope, info));
				}
			}
			return collection;
		}

		private ScopeDebugInformation ReadScopeAndLocals(PdbScope scope, MethodDebugInformation info)
		{
			ScopeDebugInformation scopeDebugInformation = new ScopeDebugInformation();
			scopeDebugInformation.Start = new InstructionOffset((int)scope.offset);
			scopeDebugInformation.End = new InstructionOffset((int)(scope.offset + scope.length));
			if (!scope.slots.IsNullOrEmpty())
			{
				scopeDebugInformation.variables = new Collection<VariableDebugInformation>(scope.slots.Length);
				PdbSlot[] slots = scope.slots;
				foreach (PdbSlot pdbSlot in slots)
				{
					if (pdbSlot.flags != 1)
					{
						VariableDebugInformation variableDebugInformation = new VariableDebugInformation((int)pdbSlot.slot, pdbSlot.name);
						if (pdbSlot.flags == 4)
						{
							variableDebugInformation.IsDebuggerHidden = true;
						}
						scopeDebugInformation.variables.Add(variableDebugInformation);
					}
				}
			}
			if (!scope.constants.IsNullOrEmpty())
			{
				scopeDebugInformation.constants = new Collection<ConstantDebugInformation>(scope.constants.Length);
				PdbConstant[] constants = scope.constants;
				foreach (PdbConstant pdbConstant in constants)
				{
					TypeReference typeReference = info.Method.Module.Read(pdbConstant, (PdbConstant c, MetadataReader r) => r.ReadConstantSignature(new MetadataToken(c.token)));
					object obj = pdbConstant.value;
					if (typeReference != null && !typeReference.IsValueType && obj is int && (int)obj == 0)
					{
						obj = null;
					}
					scopeDebugInformation.constants.Add(new ConstantDebugInformation(pdbConstant.name, typeReference, obj));
				}
			}
			if (!scope.usedNamespaces.IsNullOrEmpty())
			{
				if (imports.TryGetValue(scope, out var value))
				{
					scopeDebugInformation.import = value;
				}
				else
				{
					value = GetImport(scope, info.Method.Module);
					imports.Add(scope, value);
					scopeDebugInformation.import = value;
				}
			}
			scopeDebugInformation.scopes = ReadScopeAndLocals(scope.scopes, info);
			return scopeDebugInformation;
		}

		private static bool AddScope(Collection<ScopeDebugInformation> scopes, ScopeDebugInformation scope)
		{
			foreach (ScopeDebugInformation scope2 in scopes)
			{
				if (scope2.HasScopes && AddScope(scope2.Scopes, scope))
				{
					return true;
				}
				if (scope.Start.Offset >= scope2.Start.Offset && scope.End.Offset <= scope2.End.Offset)
				{
					scope2.Scopes.Add(scope);
					return true;
				}
			}
			return false;
		}

		private ImportDebugInformation GetImport(uint token, ModuleDefinition module)
		{
			if (!functions.TryGetValue(token, out var value))
			{
				return null;
			}
			if (value.scopes.Length != 1)
			{
				return null;
			}
			PdbScope pdbScope = value.scopes[0];
			if (imports.TryGetValue(pdbScope, out var value2))
			{
				return value2;
			}
			value2 = GetImport(pdbScope, module);
			imports.Add(pdbScope, value2);
			return value2;
		}

		private static ImportDebugInformation GetImport(PdbScope scope, ModuleDefinition module)
		{
			if (scope.usedNamespaces.IsNullOrEmpty())
			{
				return null;
			}
			ImportDebugInformation importDebugInformation = new ImportDebugInformation();
			string[] usedNamespaces = scope.usedNamespaces;
			foreach (string text in usedNamespaces)
			{
				if (string.IsNullOrEmpty(text))
				{
					continue;
				}
				ImportTarget importTarget = null;
				string text2 = text.Substring(1);
				switch (text[0])
				{
				case 'U':
					importTarget = new ImportTarget(ImportTargetKind.ImportNamespace)
					{
						@namespace = text2
					};
					break;
				case 'T':
				{
					TypeReference type2 = module.GetType(text2, runtimeName: true);
					if (type2 != null)
					{
						importTarget = new ImportTarget(ImportTargetKind.ImportType)
						{
							type = type2
						};
					}
					break;
				}
				case 'A':
				{
					int num = text.IndexOf(' ');
					string alias = text.Substring(1, num - 1);
					string text3 = text.Substring(num + 2);
					switch (text[num + 1])
					{
					case 'U':
						importTarget = new ImportTarget(ImportTargetKind.DefineNamespaceAlias)
						{
							alias = alias,
							@namespace = text3
						};
						break;
					case 'T':
					{
						TypeReference type = module.GetType(text3, runtimeName: true);
						if (type != null)
						{
							importTarget = new ImportTarget(ImportTargetKind.DefineTypeAlias)
							{
								alias = alias,
								type = type
							};
						}
						break;
					}
					}
					break;
				}
				}
				if (importTarget != null)
				{
					importDebugInformation.Targets.Add(importTarget);
				}
			}
			return importDebugInformation;
		}

		private void ReadSequencePoints(PdbFunction function, MethodDebugInformation info)
		{
			if (function.lines != null)
			{
				info.sequence_points = new Collection<SequencePoint>();
				PdbLines[] lines = function.lines;
				foreach (PdbLines lines2 in lines)
				{
					ReadLines(lines2, info);
				}
			}
		}

		private void ReadLines(PdbLines lines, MethodDebugInformation info)
		{
			Document document = GetDocument(lines.file);
			PdbLine[] lines2 = lines.lines;
			for (int i = 0; i < lines2.Length; i++)
			{
				ReadLine(lines2[i], document, info);
			}
		}

		private static void ReadLine(PdbLine line, Document document, MethodDebugInformation info)
		{
			SequencePoint sequencePoint = new SequencePoint((int)line.offset, document);
			sequencePoint.StartLine = (int)line.lineBegin;
			sequencePoint.StartColumn = line.colBegin;
			sequencePoint.EndLine = (int)line.lineEnd;
			sequencePoint.EndColumn = line.colEnd;
			info.sequence_points.Add(sequencePoint);
		}

		private Document GetDocument(PdbSource source)
		{
			string name = source.name;
			if (documents.TryGetValue(name, out var value))
			{
				return value;
			}
			value = new Document(name)
			{
				Language = source.language.ToLanguage(),
				LanguageVendor = source.vendor.ToVendor(),
				Type = source.doctype.ToType()
			};
			documents.Add(name, value);
			return value;
		}

		public void Dispose()
		{
			pdb_file.Dispose();
		}
	}
}
