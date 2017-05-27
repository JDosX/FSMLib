using System;
using System.Collections.Generic;

using FSMLib;
using FSMLib.Compilation.Tokenizing;
using FSMLib.Compilation.AST;

namespace FSMLib.Compilation.Parsing {
  internal class Parser {
    // TODO: Make the error messages in this file production ready.

    /// <summary>
    /// Constructs an AST from the Tokens produced by the provided Tokenizer.
    /// </summary>
    /// <param name="tokenizer">Tokenizer.</param>
    /// <param name="reader">BufferedTextReader to tokenize.</param>
    internal FSMNode ParseTokens(ITokenProvider tokenizer, BufferedTextReader reader) {
      FSMNode fsmAST;

      // fsm
      if (!(NextToken(tokenizer, reader) is KeywordFSMToken)) {
        throw new ArgumentException("no starting fsm keyword.");
      }

      // {
      if (!(NextToken(tokenizer, reader) is ScopeOpenToken)) {
        throw new ArgumentException("no scope open token.");
      }

      // state declaration list
      ICollection<StateDeclNode> stateDeclList = ParseStateDeclList(tokenizer, reader);

      // }
      if (!(NextToken(tokenizer, reader) is ScopeCloseToken)) {
        throw new ArgumentException("no scope close token.");
      }

      return new FSMNode(stateDeclList);
    }

    private LinkedList<StateDeclNode> ParseStateDeclList(ITokenProvider tokenizer, BufferedTextReader reader) {
      throw new NotImplementedException();
    }

    /// <summary>
    /// Returns the next Token, skipping over single and multiline comment tokens.
    /// </summary>
    /// <param name="tokenizer">Tokenizer.</param>
    /// <param name="reader">BufferedTextReader to tokenize.</param>
    private Token NextToken(ITokenProvider tokenizer, BufferedTextReader reader) {
      Token token = tokenizer.NextToken(reader);
      while (token is SingleLineCommentToken || token is MultiLineCommentToken) {
        token = tokenizer.NextToken(reader);
      }

      return token;
    }
  }
}
