using System;
using System.Collections.Generic;

using FSMLib;
using FSMLib.Compilation.Tokenizing;
using FSMLib.Compilation.AST;

namespace FSMLib.Compilation.Parsing {
  internal class Parser {

    #region Fields
    private TokenProvider Tokenizer;
    #endregion

    // TODO: Make the error messages in this file production ready.

    internal Parser(TokenProvider tokenizer) {
      Tokenizer = tokenizer;
    }

    /// <summary>
    /// Constructs an AST from the Tokens produced by the provided Tokenizer.
    /// </summary>
    /// <param name="tokenizer">Tokenizer.</param>
    /// <param name="reader">BufferedTextReader to tokenize.</param>
    internal FSMNode ParseFSM() {
      // fsm
      if (!(DestructiveNextToken() is KeywordFSMToken)) {
        throw new ArgumentException("no starting fsm keyword.");
      }

      // {
      if (!(DestructiveNextToken() is ScopeOpenToken)) {
        throw new ArgumentException("no scope open token.");
      }

      // state declaration list
      ICollection<StateDeclNode> stateDeclList = ParseStateDeclList();

      // }
      if (!(DestructiveNextToken() is ScopeCloseToken)) {
        throw new ArgumentException("no scope close token.");
      }

      return new FSMNode(stateDeclList);
    }

    #region Parsing State Declataions

    private LinkedList<StateDeclNode> ParseStateDeclList() {
      LinkedList<StateDeclNode> StateDeclList = new LinkedList<StateDeclNode>();

      while (IsStateDeclStartToken(Tokenizer.Peek())) {
        StateDeclList.AddLast(ParseStateDecl());
      }

      return StateDeclList;
    }

    private StateDeclNode ParseStateDecl() {
      // Valid orderings:
      // StateName
      // +StateName
      // +*StateName
      // *StateName
      // *+StateName

      bool startingState;
      bool acceptingState;
      // TODO: Parse the StateName and do the rest of the things.

      Token token = NextToken();
      if (token is StartStateToken) {
        startingState = true;

      } else if (token is AcceptingStateToken) {
        acceptingState = true;
      } else if (token is StateNameNode) {

      } else {
        throw new ArgumentException("yo you didn't have a valid state declaration right at the start.");
      }
    }

    private bool IsStateDeclStartToken(Token token) {
      return (
           token is StateNameToken
        || token is StartStateToken
        || token is AcceptingStateToken
      );
    }

    #endregion

    /// <summary>
    /// Returns the next Token, skipping over single and multiline comment tokens.
    /// </summary>
    private Token NextToken() {
      Token token = Tokenizer.Read();
      while (token is SingleLineCommentToken || token is MultiLineCommentToken) {
        token = Tokenizer.Read();
      }

      return token;
    }

    private Token DestructiveNextToken() {
      Token token = NextToken();
      Tokenizer.FlushBuffer();

      return token;
    }
  }
}
