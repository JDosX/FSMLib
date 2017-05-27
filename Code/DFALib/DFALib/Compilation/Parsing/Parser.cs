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

      Token token = DestructiveNextToken();
      string name;
      if (token is StateNameToken) {
        name = token.GetSanitizedContent();
      } else {
        throw new ArgumentException("No valid state name yo");
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

      return new FSMNode(name, stateDeclList);
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

      bool isStarting = false;
      bool isAccepting = false;
      string stateName;
      LinkedList<ITransitionNode> transitions;

      // TODO: Parse the StateName and do the rest of the things.

      // + and *.
      Token token = NextToken();
      if (token is StartStateToken) {
        isStarting = true;
        token = NextToken();
        if (token is AcceptingStateToken) {
          isAccepting = true;
          token = NextToken();
        }
      } else if (token is AcceptingStateToken) {
        isAccepting = true;
        token = NextToken();
        if (token is StartStateToken) {
          isStarting = true;
          token = NextToken();
        }
      }

      // State Name.
      if (token is StateNameToken) {
        stateName = token.GetSanitizedContent();
      } else {
        throw new ArgumentException("Shit a state name was expected here yo.");
      }

      // ->
      token = NextToken();
      if (!(token is ArrowToken)) {
        throw new ArgumentException("Aww shit an arrow token was expected.");
      }

      // Transitions
      transitions = ParseTransitionList();

      Tokenizer.FlushBuffer();
      return new StateDeclNode(isStarting, isAccepting, stateName, transitions);
    }

    private bool IsStateDeclStartToken(Token token) {
      return (
           token is StateNameToken
        || token is StartStateToken
        || token is AcceptingStateToken
      );
    }

    #endregion

    private LinkedList<ITransitionNode> ParseTransitionList() {
      LinkedList<ITransitionNode> Transitions = new LinkedList<ITransitionNode>();

      while (Tokenizer.Peek() is ScopeOpenToken) {
        ITransitionNode transition = ParseTransition();
        if (!(transition is NullTransitionNode)) {
          Transitions.AddLast(transition);
        }
      }

      return Transitions;
    }

    private ITransitionNode ParseTransition() {
      ITransitionNode transitionNode;

      Token token = NextToken();
      if (!(token is ScopeOpenToken)) {
        throw new ArgumentException("Scope not opened here yo.");
      }

      token = NextToken();
      if (token is ScopeCloseToken) {
        transitionNode = new NullTransitionNode();
      } else {
        throw new NotImplementedException("TODO: Implement this.");
      }

      Tokenizer.FlushBuffer();
      return transitionNode;
    }

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
