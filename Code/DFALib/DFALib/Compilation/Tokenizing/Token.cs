using System;

namespace FSMLib.Compilation.Tokenizing
{
  internal abstract class Token {
    internal readonly string Contents;

    internal Token(string contents) {
      Contents = contents;
    }
  }

  internal class StringToken : Token {
    internal StringToken(string contents) : base(contents) { }
  }
}
