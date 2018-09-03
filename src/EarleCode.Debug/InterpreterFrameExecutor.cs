using System;
using System.Collections.Generic;
using EarleCode.Compiling.Earle.AST;

namespace EarleCode.Debug
{
	/// <summary>
	/// Executor of a "function"
	/// </summary>
	public class InterpreterFrameExecutor : IFrameExecutor
	{
		public EarleValue Target;
		public List<int> Path = new List<int>();
		public Stack<EarleValue> Stack = new Stack<EarleValue>();
		public Stack<Scope> Scopes = new Stack<Scope>();
		public IFrameExecutor SubFrame;

		public EarleFunction Function;
		public EarleInterpreter Interpreter;

		private bool RunSubFrame()
		{
			if (SubFrame != null)
			{
				var subResult = SubFrame.Run();
				if (subResult != null)
				{
					Stack.Push(subResult.Value);
					SubFrame = null;
				}
				else
					return true;
			}

			return false;
		}
		public EarleValue? Run()
		{
			if (Function is Print)
			{
				Scopes.Peek().Variables.TryGetValue("value", out var str);
				Console.WriteLine(str);
				return EarleValue.Null;
			}

			if (RunSubFrame())
				return null;

			do
			{
				if (Run(Function.Node, 0))
				{
					// TODO: Stack.count should never be 0 at this point
					return Stack.Count == 0 ? EarleValue.Null : Stack.Pop();
				}
			} while (!RunSubFrame());

			return null;
		}

		private bool Run(IASTNode node, int pathIndex)
		{
			if (Path.Count == pathIndex)
			{
				// new start
				Path.Add(0);
			}
			else if(Path.Count < pathIndex)
			{
				// error
				throw new Exception("Unexpected frame path jump");
			}

			var length = GetLength(node);
			var index = Path[pathIndex];
			while (index < length)
			{
				if (SubFrame != null)
				{
					Path[pathIndex] = index;
					return false;
				}

				switch (Execute(node, index, pathIndex))
				{
					case ExecState.Break:
						Path[pathIndex] = index;
						return false;
					case ExecState.Continue:
						index++;
						break;
					case ExecState.Finished:
						index = length;
						break;
				}
			}


			Path[pathIndex] = index;

            
			// done
			if(pathIndex != Path.Count - 1) 
				throw new Exception("Frame path corrupted");
            
			Path.RemoveAt(pathIndex);

			return true;
		}
		
		enum ExecState{Break,Continue,Finished}
		private ExecState Execute(IASTNode node, int index, int pathIndex)
		{
			// return true on early finish (before index hits length)
			switch (node)
			{
				case FunctionDeclaration decl:
					if (!Run(decl.Statements[index], pathIndex + 1)) return ExecState.Break;
					break;
				case StatementBlock block:
					if (!Run(block.Statements[index], pathIndex + 1)) return ExecState.Break;
					break;
				case StatementFunctionCall statementCall:
					switch (index)
					{
						case 0:
						{
							if (!Run(statementCall.Function, pathIndex + 1))
								return ExecState.Break;
							break;
						}
						case 1:
							Stack.Pop();
							break;
					}
					break;
				case FunctionCall call:
					var argCount = call.Arguments?.Count ?? 0;
					if (index < argCount)
					{
						Run(call.Arguments[index], pathIndex + 1);
					}
					else if (index == argCount)
					{
						EarleFunction callingFunction;
						switch (call.FunctionIdentifier)
						{
							case ImplicitFunctionIdentifier impl:
								callingFunction = Function.File[impl.Name] ?? Interpreter.GetFunction(impl.Name);
								break;
							default:
								throw new NotImplementedException();
						}

						if (callingFunction == null)
						{
							// TODO: More error info
							throw new RuntimeException("Function not found: " + call.FunctionIdentifier);
						}

						var frm = new InterpreterFrameExecutor
						{
							Target = EarleValue.Null,
							Function = callingFunction,
							Interpreter = Interpreter
						};

						var scope = new Scope();

						if (callingFunction.Parameters != null)
						{
							for (var i = callingFunction.Parameters.Count - 1; i >= 0; i--)
							{
								scope.Variables[callingFunction.Parameters[i]] = argCount <= i
									? EarleValue.Null
									: Stack.Pop();
							}

							for (var i = callingFunction.Parameters.Count; i < argCount; i++)
								Stack.Pop();
						}

						frm.Scopes.Push(scope);
						SubFrame = frm;

						return ExecState.Finished;
					}

					break;
				case StringExpression str:
					Stack.Push(new EarleValue(str.Value));
					break;
				default:
					throw new NotImplementedException();
			}

			return ExecState.Continue;
		}
        
		private int GetLength(IASTNode node)
		{
			switch (node)
			{
				case FunctionDeclaration decl:
					return decl.Statements?.Count ?? 0;
				case StatementBlock block:
					return block.Statements?.Count ?? 0;
				case StatementFunctionCall _:
					return 2;
				case FunctionCall call:
					return (call.Arguments?.Count ?? 0) + 1;
				case StringExpression _:
					return 1;
				default:
					throw new NotImplementedException();
			}
		}
	}
}