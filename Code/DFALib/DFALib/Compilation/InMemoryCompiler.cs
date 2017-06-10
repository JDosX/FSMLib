using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

using FSMLib.Compilation.Tokenizing;
using FSMLib.Compilation.Parsing;
using FSMLib.Compilation.AST;
using FSMLib.Compilation.Analysis;
using FSMLib.Compilation.Generation;

namespace FSMLib.Compilation
{
  internal static class InMemoryCompiler
  {
    internal static FSM<T> FromReader<T>(TextReader reader)
    {
      BufferedTextReader positionedReader = new BufferedTextReader(reader);
      Tokenizer tokenizer = new Tokenizer(positionedReader);
      Parser parser = new Parser(tokenizer);
      SemanticAnalyzer analyzer = new SemanticAnalyzer();
      InMemoryGenerator<T> generator = new InMemoryGenerator<T>();

      FSMNode fsmAst = parser.ParseFSM();
      analyzer.Analyze(fsmAst);

      FSM<T> fsm = generator.FromAST(fsmAst);

      return fsm;
    }
  }
}
