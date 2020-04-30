using System;
namespace SemiPatch.MonoMod.Compiler {
    public class CompilerException : Exception {
        public CompilerException(string msg) : base(msg) { }
    }
}
