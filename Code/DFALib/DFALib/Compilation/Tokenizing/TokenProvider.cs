using System;

namespace FSMLib.Compilation.Tokenizing {
  internal abstract class TokenProvider : BufferedProducer<Token> {
    internal TokenProvider() : base(null) { }
  }
}
