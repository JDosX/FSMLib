using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

using FSMLib.Compilation.Tokenizing;

namespace FSMLib.Compilation
{
  internal static class InMemoryCompiler
  {
    internal static FSM<T> FromReader<T>(TextReader reader)
    {
      Tokenizer tokenizer = new Tokenizer();
      Token[] tokens = tokenizer.Tokenize(reader);

      // TODO: finish.
      return null;
    }
  }
}
