﻿
using System;

namespace EarleCode.Runtime.Instructions
{
    internal class NopInstuction : IInstruction
    {
        public void Handle(EarleStackFrameExecutor loop)
        {
        }
    }
}