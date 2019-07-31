using System;

namespace EarleCode.Interpreter
{
	[Flags]
	public enum EarleValueType
	{
		NumberInt     = 0b0000000001,
		NumberFloat   = 0b0000000010,
		String        = 0b0000000100,
		FunctionPointer=0b0000001000,
		Number        = 0b0000000011,
		Struct        = 0b0000010000, // todo
		Vector2       = 0b0000100000, // todo
		Vector3       = 0b0001000000, // todo
		Vector4       = 0b0010000000, // todo
		Array         = 0b0100000000, // todo
		Null          = 0b0000000000
	}
}