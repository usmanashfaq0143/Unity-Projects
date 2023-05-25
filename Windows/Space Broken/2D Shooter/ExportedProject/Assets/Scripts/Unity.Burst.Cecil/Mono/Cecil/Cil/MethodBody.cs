using System;
using System.Collections.Generic;
using System.Threading;
using Mono.Collections.Generic;

namespace Mono.Cecil.Cil
{
	public sealed class MethodBody
	{
		internal readonly MethodDefinition method;

		internal ParameterDefinition this_parameter;

		internal int max_stack_size;

		internal int code_size;

		internal bool init_locals;

		internal MetadataToken local_var_token;

		internal Collection<Instruction> instructions;

		internal Collection<ExceptionHandler> exceptions;

		internal Collection<VariableDefinition> variables;

		private ScopeDebugInformation scope;

		internal Dictionary<Instruction, MetadataToken> instructionTokens;

		public MethodDefinition Method => method;

		public int MaxStackSize
		{
			get
			{
				return max_stack_size;
			}
			set
			{
				max_stack_size = value;
			}
		}

		public int CodeSize => code_size;

		public bool InitLocals
		{
			get
			{
				return init_locals;
			}
			set
			{
				init_locals = value;
			}
		}

		public MetadataToken LocalVarToken
		{
			get
			{
				return local_var_token;
			}
			set
			{
				local_var_token = value;
			}
		}

		public Collection<Instruction> Instructions
		{
			get
			{
				if (instructions == null)
				{
					Interlocked.CompareExchange(ref instructions, new InstructionCollection(method), null);
				}
				return instructions;
			}
		}

		public bool HasExceptionHandlers => !exceptions.IsNullOrEmpty();

		public Collection<ExceptionHandler> ExceptionHandlers
		{
			get
			{
				if (exceptions == null)
				{
					Interlocked.CompareExchange(ref exceptions, new Collection<ExceptionHandler>(), null);
				}
				return exceptions;
			}
		}

		public bool HasVariables => !variables.IsNullOrEmpty();

		public Collection<VariableDefinition> Variables
		{
			get
			{
				if (variables == null)
				{
					Interlocked.CompareExchange(ref variables, new VariableDefinitionCollection(), null);
				}
				return variables;
			}
		}

		public ScopeDebugInformation Scope
		{
			get
			{
				return scope;
			}
			set
			{
				scope = value;
			}
		}

		public ParameterDefinition ThisParameter
		{
			get
			{
				if (method == null || method.DeclaringType == null)
				{
					throw new NotSupportedException();
				}
				if (!method.HasThis)
				{
					return null;
				}
				if (this_parameter == null)
				{
					Interlocked.CompareExchange(ref this_parameter, CreateThisParameter(method), null);
				}
				return this_parameter;
			}
		}

		private static ParameterDefinition CreateThisParameter(MethodDefinition method)
		{
			TypeReference typeReference = method.DeclaringType;
			if (typeReference.HasGenericParameters)
			{
				GenericInstanceType genericInstanceType = new GenericInstanceType(typeReference);
				for (int i = 0; i < typeReference.GenericParameters.Count; i++)
				{
					genericInstanceType.GenericArguments.Add(typeReference.GenericParameters[i]);
				}
				typeReference = genericInstanceType;
			}
			if (typeReference.IsValueType || typeReference.IsPrimitive)
			{
				typeReference = new ByReferenceType(typeReference);
			}
			return new ParameterDefinition(typeReference, method);
		}

		public MethodBody(MethodDefinition method)
		{
			this.method = method;
			instructionTokens = new Dictionary<Instruction, MetadataToken>();
		}

		public ILProcessor GetILProcessor()
		{
			return new ILProcessor(this);
		}

		public bool GetInstructionToken(Instruction instruction, out MetadataToken token)
		{
			return instructionTokens.TryGetValue(instruction, out token);
		}
	}
}
