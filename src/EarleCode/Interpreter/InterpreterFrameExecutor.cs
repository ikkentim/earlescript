// EarleCode
// Copyright 2018 Tim Potze
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using EarleCode.Compiling.Earle.AST;

namespace EarleCode.Interpreter
{
	/// <summary>
	///     Executor of a "function"
	/// </summary>
	public class InterpreterFrameExecutor : IFrameExecutor
	{
		private readonly EarleArithmeticService _arithmetic = new EarleArithmeticService();

		public EarleFunction Function;
		public EarleInterpreter Interpreter;
		public List<int> Path = new List<int>();
		public Stack<Scope> Scopes = new Stack<Scope>();
		public Stack<EarleValue> Stack = new Stack<EarleValue>();
		public IFrameExecutor SubFrame;
		public EarleValue Target;
		
		public string Name => Function.Name;

		private bool RunSubFrame()
		{
			if (SubFrame != null)
			{
				var subResult = SubFrame.Run(this);
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

		private bool Run(IASTNode node, int pathIndex)
		{
			switch (RunWithState(node, pathIndex))
			{
				case ExecState.Pause:
					return false;
				case ExecState.Normal:
				case ExecState.Finished:
				case ExecState.Return:
					return true;
				case ExecState.Break:
				case ExecState.Continue:
					throw new RuntimeException("Cannot break or continue in this context");
				default:
					throw new RuntimeException("Unknown state");
			}
		}

		private ExecState RunWithState(IASTNode node, int pathIndex)
		{
			var continuation = true;
			if (Path.Count == pathIndex)
			{
				continuation = false;
				Path.Add(0);
			}
			else if (Path.Count < pathIndex)
			{
				throw new Exception("Unexpected frame path jump");
			}

			var length = GetLength(node);
			var index = Path[pathIndex];
			var state = ExecState.Normal;
			while (index < length)
			{
				if (SubFrame != null)
				{
					Path[pathIndex] = index;
					return ExecState.Pause;
				}

				switch (state = Execute(node, ref index, continuation, pathIndex))
				{
					case ExecState.Pause:
						Path[pathIndex] = index;
						return ExecState.Pause;
					case ExecState.Finished:
					case ExecState.Break:
					case ExecState.Continue:
					case ExecState.Return:
						index = length;
						break;
				}

				continuation = false;
			}


			Path[pathIndex] = index;
			
			if (pathIndex != Path.Count - 1)
				throw new Exception("Frame path corrupted");

			Path.RemoveAt(pathIndex);

			return state;
		}

		private void PushScope()
		{
			Scopes.Push(new Scope(Scopes.Peek()));
		}

		private void PopScope()
		{
			Scopes.Pop();
		}

		private ExecState Execute(IASTNode node, ref int index, bool continuation, int pathIndex)
		{
			ExecState state;

			switch (node)
			{
				case FunctionDeclaration decl:
					if(index == 0 && !continuation)
						PushScope();

					switch (RunWithState(decl.Statements[index], pathIndex + 1))
					{
						case ExecState.Pause:
							return ExecState.Pause;
						case ExecState.Return:
							PopScope();
							return ExecState.Return;
						case ExecState.Break:
						case ExecState.Continue:
							throw new RuntimeException("Cannot break or continue in this context");
					}

					if(index + 1 == decl.Statements.Count)
						PopScope();
					break;

				#region Statements

				case StatementBlock statement:
					if (statement.Statements == null)
						return ExecState.Finished;

					if(index == 0 && !continuation)
						PushScope();

					switch (state = RunWithState(statement.Statements[index], pathIndex + 1))
					{
						case ExecState.Pause:
							return ExecState.Pause;
						case ExecState.Break:
						case ExecState.Continue:
						case ExecState.Return:
							PopScope();
							return state;
					}

					if(index + 1 == statement.Statements.Count)
						PopScope();

					break;
				case StatementFunctionCall statement:
					switch (index)
					{
						case 0:
						{
							if (!Run(statement.Function, pathIndex + 1))
								return ExecState.Pause;
							break;
						}
						case 1:
							Stack.Pop();
							break;
					}

					break;
				case StatementIf statement:
					if (index == 0)
					{
						if (!Run(statement.Condition, pathIndex + 1))
							return ExecState.Pause;

						var result = _arithmetic.IsTrue(Stack.Pop());
						
						if (result && statement.Statements == null)
							return ExecState.Finished;

						if (!result && statement.ElseStatements == null)
							return ExecState.Finished;

						if (!result)
						{
							// hacks
							index += 100000;
						}

						PushScope();
					}
					else
					{
						var subStatements = index > 100000 ? statement.ElseStatements : statement.Statements;
						var subIndex = index > 100000 ? index - 100001 : index - 1;
						switch (state = RunWithState(subStatements[subIndex], pathIndex + 1))
						{
							case ExecState.Pause:
								return ExecState.Pause;
							case ExecState.Break:
							case ExecState.Continue:
							case ExecState.Return:
								PopScope();
								return state;
						}

						if (subStatements.Count == subIndex + 1)
						{
							PopScope();
							return ExecState.Finished;
						}
					}
					break;
				case StatementAssignments statement:
					if (!Run(statement.Assignments[index], pathIndex + 1))
						return ExecState.Pause;
					Stack.Pop();

					break;
				case StatementBreak _:
					return ExecState.Break;
				case StatementContinue _:
					return ExecState.Continue;
				case StatementDoWhile statement:
					if (statement.Statements != null && index < statement.Statements.Count)
					{
						if (index == 0 && !continuation)
							PushScope();
						
						switch (RunWithState(statement.Statements[index], pathIndex + 1))
						{
							case ExecState.Pause:
								return ExecState.Pause;
							case ExecState.Return:
								PopScope();
								return ExecState.Return;
							case ExecState.Break:
								PopScope();
								return ExecState.Finished;
							case ExecState.Continue:
								PopScope();
								index = statement.Statements.Count;
								return ExecState.Normal;
						}
						
						if (statement.Statements.Count == index + 1)
							PopScope();
					}
					else
					{
						if (!Run(statement.Condition, pathIndex + 1))
							return ExecState.Pause;

						if (!_arithmetic.IsTrue(Stack.Pop()))
							return ExecState.Finished;
						
						index = 0;
						return ExecState.Normal;
					}

					break;
				case StatementFor statement:
					var assignments = statement.Assignments?.Count ?? 0;
					var statements = statement.Statements?.Count ?? 0;
					var increments = statement.Increments?.Count ?? 0;
					if (index < assignments)
					{ // Assignments
						if (!Run(statement.Assignments[index], pathIndex + 1))
							return ExecState.Pause;

						Stack.Pop();
					}
					else if (index == assignments)
					{ // Condition
						if (statement.Condition == null)
							break;

						if (!Run(statement.Condition, pathIndex + 1))
							return ExecState.Pause;

						if (!_arithmetic.IsTrue(Stack.Pop()))
							return ExecState.Finished;
					}
					else if (index < assignments + 1 + statements)
					{ // Statements
						if(index == assignments + 1 && !continuation)
							PushScope();

						switch (RunWithState(statement.Statements[index - assignments - 1], pathIndex + 1))
						{
							case ExecState.Pause:
								return ExecState.Pause;
							case ExecState.Return:
								PopScope();
								return ExecState.Return;
							case ExecState.Break:
								PopScope();
								return ExecState.Finished;
							case ExecState.Continue:
								PopScope();
								index = assignments + 1 + statements;
								return ExecState.Normal;
						}

						if(index + 1 == assignments + 1 + statements)
							PopScope();
					}
					else if (index < assignments + 1 + statements + increments)
					{ // Increments
						if(!Run(statement.Increments[index - assignments - 1 - statements], pathIndex + 1))
							return ExecState.Pause;

						Stack.Pop();
					}

					if (index + 1 == assignments + 1 + statements + increments)
					{
						index = assignments;
						return ExecState.Normal;
					}

					break;
				case StatementReturn statement:
					if (statement.Expression != null)
					{
						if (!Run(statement.Expression, pathIndex + 1))
							return ExecState.Pause;
					}
					else
					{
						Stack.Push(EarleValue.Null);
					}

					return ExecState.Return;
				case StatementWhile statement:
					if (index == 0)
					{
						if (!Run(statement.Condition, pathIndex + 1))
							return ExecState.Pause;

						if (!_arithmetic.IsTrue(Stack.Pop()))
							return ExecState.Finished;
					}
					else
					{
						if (index == 1 && !continuation)
							PushScope();
						
						switch (RunWithState(statement.Statements[index - 1], pathIndex + 1))
						{
							case ExecState.Pause:
								return ExecState.Pause;
							case ExecState.Return:
								PopScope();
								return ExecState.Return;
							case ExecState.Break:
								PopScope();
								return ExecState.Finished;
							case ExecState.Continue:
								PopScope();
								index = 0;
								return ExecState.Normal;
						}

						if (statement.Statements.Count == index)
						{
							PopScope();
							index = 0;
							return ExecState.Normal;
						}
					}

					break;

				#endregion

				#region Expressions

				case FunctionCall expression:

					var argCount = expression.Arguments?.Count ?? 0;
					var targetCount = expression.Target == null ? 0 : 1;
					if (index < argCount + targetCount)
					{
						if (index == 0 && expression.Target != null && !Run(expression.Target, pathIndex + 1))
							return ExecState.Pause;
						
						if (index >= targetCount && !Run(expression.Arguments[index - targetCount], pathIndex + 1))
							return ExecState.Pause;
					}
					else if (index == argCount + targetCount)
					{
						IEarleFunction callingFunction;
						switch (expression.FunctionIdentifier)
						{
							case ImplicitFunctionIdentifier impl:
								callingFunction = Function.File[impl.Name] ?? Interpreter.GetFunction(impl.Name);
								if (callingFunction == null)
								{
									// TODO: This should be in a table
									foreach (var inc in Function.File.Node.Includes)
									{
										var file = Interpreter.GetFile(inc.Path);
										if (file == null)
										{
											throw new Exception("Included not found");
										}

										callingFunction = file[impl.Name];

										if (callingFunction != null)
											break;
									}
								}
								break;
							case ExplicitFunctionIdentifier exp:
							{
								var file = Interpreter.GetFile(exp.Path);
								if (file == null)
								{
									throw new Exception("Included not found");
								}

								callingFunction = file[exp.Name];
								break;
							}

							case UnboxedFunctionIdentifier unb:
								if (!Run(unb.Expression, pathIndex + 1))
									return ExecState.Pause;

								callingFunction = Stack.Pop().FunctionValue;
								break;
							default:
								throw new RuntimeException("Unknown function identifier type: " + expression.FunctionIdentifier.GetType());
						}

						if (callingFunction == null)
						{
							// TODO: More error info
							throw new RuntimeException("Function not found: " + expression.FunctionIdentifier);
						}

						var lim = Math.Min(callingFunction.ParameterCount, argCount);

						var args = new EarleValue[lim];

						for (var i = 0; i < lim; i++)
							args[i] = Stack.Pop();

						// Sync stack
						for (var i = lim; i < argCount; i++) 
							Stack.Pop();
						
						var target = EarleValue.Null;
						if (expression.Target != null)
						{
							target = Stack.Pop();
						}
						
						var frame = callingFunction.GetFrameExecutor(target, args);

						if (expression.IsThreaded)
						{
							Interpreter.EnqueueFrame(frame);
							Stack.Push(EarleValue.Null);
						}
						else
							SubFrame = frame;

						return ExecState.Finished;
					}

					break;
				case StatementWait expression:
					switch (index)
					{
						case 0 when !Run(expression.Delay, pathIndex + 1):
							return ExecState.Pause;
						case 1:
						{
							IEarleFunction callingFunction = Interpreter.GetFunction("wait");

							if (callingFunction == null)
							{
								// TODO: More error info
								throw new RuntimeException("Function not found: wait");
							}

							var args = new EarleValue[1];
							args[0] = Stack.Pop();

							var frame = callingFunction.GetFrameExecutor(EarleValue.Null, args);

							SubFrame = frame;
							break;
						}

						case 2:
							Stack.Pop();
							return ExecState.Finished;
					}

					break;
				case StringExpression expression:
					Stack.Push(new EarleValue(expression.Value));
					break;
				case NumberExpression expression:
					Stack.Push(expression.IsFloat ? new EarleValue(expression.FloatValue) : new EarleValue(expression.IntValue));
					break;
				case AddExpression expression:
					if (RunBinaryExpression(index, expression, pathIndex, _arithmetic.Add))
						return ExecState.Pause;
					break;

				case AndExpression expression:
					switch(index) {
						case 0:
							if (!Run(expression.Lhs, pathIndex + 1))
								return ExecState.Pause;

							if (!_arithmetic.IsTrue(Stack.Pop()))
							{
								Stack.Push(EarleValue.False);
								return ExecState.Finished;
							}

							break;
						case 1:
							if (!Run(expression.Rhs, pathIndex + 1))
								return ExecState.Pause;
						
							Stack.Push(_arithmetic.IsTrue(Stack.Pop()));
							return ExecState.Finished;
					}

					break;
				case BitwiseAndExpression expression:
					if (RunBinaryExpression(index, expression, pathIndex, _arithmetic.BitwiseAnd))
						return ExecState.Pause;
					break;


				case BitwiseOrExpression expression:
					if (RunBinaryExpression(index, expression, pathIndex, _arithmetic.BitwiseOr))
						return ExecState.Pause;
					break;

				case BitwiseXorExpression expression:
					if (RunBinaryExpression(index, expression, pathIndex, _arithmetic.BitwiseXor))
						return ExecState.Pause;
					break;

				case DivideExpression expression:
					if (RunBinaryExpression(index, expression, pathIndex, _arithmetic.Divide))
						return ExecState.Pause;
					break;

				case EqualExpression expression:
					if (RunBinaryExpression(index, expression, pathIndex, _arithmetic.Equal))
						return ExecState.Pause;
					break;

				case GreaterOrEqualExpression expression:
					if (RunBinaryExpression(index, expression, pathIndex, _arithmetic.GreaterOrEqual))
						return ExecState.Pause;
					break;

				case GreaterThanExpression expression:
					if (RunBinaryExpression(index, expression, pathIndex, _arithmetic.GreaterThan))
						return ExecState.Pause;
					break;

				case LessOrEqualExpression expression:
					if (RunBinaryExpression(index, expression, pathIndex, _arithmetic.LessOrEqual))
						return ExecState.Pause;
					break;

				case LessThanExpression expression:
					if (RunBinaryExpression(index, expression, pathIndex, _arithmetic.LessThan))
						return ExecState.Pause;
					break;

				case ModuloExpression expression:
					if (RunBinaryExpression(index, expression, pathIndex, _arithmetic.Modulo))
						return ExecState.Pause;
					break;

				case MultiplyExpression expression:
					if (RunBinaryExpression(index, expression, pathIndex, _arithmetic.Multiply))
						return ExecState.Pause;
					break;

				case NotEqualExpression expression:
					if (RunBinaryExpression(index, expression, pathIndex, _arithmetic.NotEqual))
						return ExecState.Pause;
					break;

				case OrExpression expression:
					switch(index) {
						case 0:
							if (!Run(expression.Lhs, pathIndex + 1))
								return ExecState.Pause;

							var value = Stack.Pop();
							if (_arithmetic.IsTrue(value))
							{
								Stack.Push(value);
								return ExecState.Finished;
							}

							break;
						case 1:
							if (!Run(expression.Rhs, pathIndex + 1))
								return ExecState.Pause;
						
							return ExecState.Finished;
					}
					break;

				case ShiftLeftExpression expression:
					if (RunBinaryExpression(index, expression, pathIndex, _arithmetic.ShiftLeft))
						return ExecState.Pause;
					break;

				case ShiftRightExpression expression:
					if (RunBinaryExpression(index, expression, pathIndex, _arithmetic.ShiftRight))
						return ExecState.Pause;
					break;

				case SubtractExpression expression:
					if (RunBinaryExpression(index, expression, pathIndex, _arithmetic.Subtract))
						return ExecState.Pause;
					break;

				case AssignmentExpression expression:
					if (!Run(expression.Value, pathIndex + 1))
						return ExecState.Pause;
					
					Scopes.Peek()[expression.Variable.Name] = Stack.Peek();

					break;

				case OrAssignmentExpression expression:
				{
					if (!Run(expression.Value, pathIndex + 1))
						return ExecState.Pause;

					var result = _arithmetic.BitwiseOr(Scopes.Peek()[expression.Variable.Name], Stack.Pop());
					Scopes.Peek()[expression.Variable.Name] = result;
					Stack.Push(result);
					break;
				}
				case AndAssignmentExpression expression:
				{
					if (!Run(expression.Value, pathIndex + 1))
						return ExecState.Pause;

					var result = _arithmetic.BitwiseAnd(Scopes.Peek()[expression.Variable.Name], Stack.Pop());
					Scopes.Peek()[expression.Variable.Name] = result;
					Stack.Push(result);
					break;
				}
				case XorAssignmentExpression expression:
				{
					if (!Run(expression.Value, pathIndex + 1))
						return ExecState.Pause;

					var result = _arithmetic.BitwiseXor(Scopes.Peek()[expression.Variable.Name], Stack.Pop());
					Scopes.Peek()[expression.Variable.Name] = result;
					Stack.Push(result);
					break;
				}
				case LeftShiftAssignmentExpression expression:
				{
					if (!Run(expression.Value, pathIndex + 1))
						return ExecState.Pause;

					var result = _arithmetic.ShiftLeft(Scopes.Peek()[expression.Variable.Name], Stack.Pop());
					Scopes.Peek()[expression.Variable.Name] = result;
					Stack.Push(result);
					break;
				}
				case RightShiftAssignmentExpression expression:
				{
					if (!Run(expression.Value, pathIndex + 1))
						return ExecState.Pause;

					var result = _arithmetic.ShiftRight(Scopes.Peek()[expression.Variable.Name], Stack.Pop());
					Scopes.Peek()[expression.Variable.Name] = result;
					Stack.Push(result);
					break;
				}
				case AdditionAssignmentExpression expression:
				{
					if (!Run(expression.Value, pathIndex + 1))
						return ExecState.Pause;

					var result = _arithmetic.Add(Scopes.Peek()[expression.Variable.Name], Stack.Pop());
					Scopes.Peek()[expression.Variable.Name] = result;
					Stack.Push(result);
					break;
				}
				case SubtractionAssignmentExpression expression:
				{
					if (!Run(expression.Value, pathIndex + 1))
						return ExecState.Pause;

					var result = _arithmetic.Subtract(Scopes.Peek()[expression.Variable.Name], Stack.Pop());
					Scopes.Peek()[expression.Variable.Name] = result;
					Stack.Push(result);
					break;
				}
				case MultiplicationAssignmentExpression expression:
				{
					if (!Run(expression.Value, pathIndex + 1))
						return ExecState.Pause;

					var result = _arithmetic.Multiply(Scopes.Peek()[expression.Variable.Name], Stack.Pop());
					Scopes.Peek()[expression.Variable.Name] = result;
					Stack.Push(result);
					break;
				}
				case DivisionAssignmentExpression expression:
				{
					if (!Run(expression.Value, pathIndex + 1))
						return ExecState.Pause;

					var result = _arithmetic.Divide(Scopes.Peek()[expression.Variable.Name], Stack.Pop());
					Scopes.Peek()[expression.Variable.Name] = result;
					Stack.Push(result);
					break;
				}
				case ModuloAssignmentExpression expression:
				{
					if (!Run(expression.Value, pathIndex + 1))
						return ExecState.Pause;

					var result = _arithmetic.Modulo(Scopes.Peek()[expression.Variable.Name], Stack.Pop());
					Scopes.Peek()[expression.Variable.Name] = result;
					Stack.Push(result);
					break;
				}
				case ExplicitFunctionIdentifierExpression expression:
					Stack.Push(new EarleValue(GetFunction(expression.Value)));
					break;
				case VariableExpression expression:
					Stack.Push(expression.Variable.Name == "self" ? Target : Scopes.Peek()[expression.Variable.Name]);
					break;
				case PostfixAdditionAssignmentExpression expression:
				{
					var value = Scopes.Peek()[expression.Variable.Name];
					Stack.Push(value);
					value = _arithmetic.Add(value, new EarleValue(1));
					Scopes.Peek()[expression.Variable.Name] = value;
					break;
				}
				case PostfixSubtractionAssignmentExpression expression:
				{
					var value = Scopes.Peek()[expression.Variable.Name];
					Stack.Push(value);
					value = _arithmetic.Subtract(value, new EarleValue(1));
					Scopes.Peek()[expression.Variable.Name] = value;
					break;
				}
				case PrefixAdditionAssignmentExpression expression:
				{
					var value = Scopes.Peek()[expression.Variable.Name];
					value = _arithmetic.Add(value, new EarleValue(1));
					Stack.Push(value);
					Scopes.Peek()[expression.Variable.Name] = value;
					break;
				}
				case PrefixSubtractionAssignmentExpression expression:
				{
					var value = Scopes.Peek()[expression.Variable.Name];
					value = _arithmetic.Subtract(value, new EarleValue(1));
					Stack.Push(value);
					Scopes.Peek()[expression.Variable.Name] = value;
					break;
				}

				#endregion

				default:
					throw new RuntimeException("Unknown AST node type: " + node.GetType());
			}

			index++;
			return ExecState.Normal;
		}

		private IEarleFunction GetFunction(ExplicitFunctionIdentifier id)
		{
			IEarleFunction func = id.Path == null
				? Function.File[id.Name]
				: Interpreter[id.Path][id.Name];

			if (id.Path == null && func == null)
				func = Interpreter.GetFunction(id.Name);
			
			return func ?? throw new RuntimeException("Function not found");
		}

		private bool RunBinaryExpression(int index, BinaryExpression expression, int pathIndex, Func<EarleValue,EarleValue,EarleValue> calc)
		{
			switch (index)
			{
				case 0:
					if (!Run(expression.Lhs, pathIndex + 1))
						return true;
					break;
				case 1:
					if (!Run(expression.Rhs, pathIndex + 1))
						return true;

					var rhs = Stack.Pop();
					var lhs = Stack.Pop();
					Stack.Push(calc(lhs, rhs));
					break;
			}

			return false;
		}

		public EarleValue? Run(IFrameExecutor parent)
		{
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
		
		private static int GetLength(IASTNode node)
		{
			switch (node)
			{
				case FunctionDeclaration decl:
					return decl.Statements?.Count ?? 0;

				#region Statements

				case StatementBlock statement:
					return statement.Statements?.Count ?? 0;
				case StatementFunctionCall _:
					return 2;
				case StatementIf statement:
					return int.MaxValue;// Let implementation decide because if/else branch is unknown
				case StatementDoWhile statement:
					return 1 + (statement.Statements?.Count ?? 0);
				case StatementWhile statement:
					return 1 + (statement.Statements?.Count ?? 0);
				case StatementFor statement:
					return (statement.Assignments?.Count ?? 0) + 1 + (statement.Statements?.Count ?? 0) + (statement.Increments?.Count ?? 0);
				case StatementAssignments statement:
					return statement.Assignments.Count;
				case StatementBreak _:
				case StatementContinue _:
				case StatementReturn _:
					return 1;

				#endregion

				#region Expressions

				case FunctionCall call:
					return (call.Target == null ? 0 : 1) + (call.Arguments?.Count ?? 0) + 1;
				case StatementWait _:
					return 3; // expression + call + stack pop
				case BinaryExpression _:
					return 2;
				case StringExpression _:
				case NumberExpression _:
				case AssignmentExpression _:
				case ExplicitFunctionIdentifierExpression _:
				case ValueExpression _:
				case VariableExpression _:
				case OrAssignmentExpression _:
				case AndAssignmentExpression _:
				case XorAssignmentExpression _:
				case LeftShiftAssignmentExpression _:
				case RightShiftAssignmentExpression _:
				case AdditionAssignmentExpression _:
				case SubtractionAssignmentExpression _:
				case MultiplicationAssignmentExpression _:
				case DivisionAssignmentExpression _:
				case ModuloAssignmentExpression _:
				case PostfixAdditionAssignmentExpression _:
				case PrefixAdditionAssignmentExpression _:
				case PostfixSubtractionAssignmentExpression _:
				case PrefixSubtractionAssignmentExpression _:
					return 1;
				#endregion

				default:
					throw new RuntimeException("Unknown AST node type: " + node.GetType());
			}
		}

		private enum ExecState
		{
			Pause,
			Normal,
			Finished,
			Break,
			Continue,
			Return
		}
	}
}