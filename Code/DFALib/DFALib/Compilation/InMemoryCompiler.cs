using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

using FSMLib.Compilation.Tokenizing;
using FSMLib.Compilation.Parsing;
using FSMLib.Compilation.AST;

namespace FSMLib.Compilation
{
  internal static class InMemoryCompiler
  {
    internal static FSM<T> FromReader<T>(TextReader reader)
    {
      BufferedTextReader positionedReader = new BufferedTextReader(reader);
      Tokenizer tokenizer = new Tokenizer(positionedReader);
      Parser parser = new Parser(tokenizer);

      FSMNode fsm = parser.ParseFSM();

      return null;
    }
  }
}
