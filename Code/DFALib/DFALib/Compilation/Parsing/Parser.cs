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
      if (!(DestructiveReadToken() is KeywordFSMToken)) {
        throw new ArgumentException("no starting fsm keyword.");
      }

      Token token = DestructiveReadToken();
      string name;
      if (token is StateNameToken) {
        name = token.ContentToString();
      } else {
        throw new ArgumentException("No valid state name yo");
      }

      // {
        if (!(DestructiveReadToken() is ScopeOpenToken)) {
        throw new ArgumentException("no scope open token.");
      }

      // state declaration list
      ICollection<StateDeclNode> stateDeclList = ParseStateDeclList();

      // }
      if (!(DestructiveReadToken() is ScopeCloseToken)) {
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
      StateNameNode stateName;
      LinkedList<TransitionNode> transitions;

      // TODO: Parse the StateName and do the rest of the things.

      // + and *.
      Token token = Tokenizer.Read();
      if (token is StartStateToken) {
        isStarting = true;
        token = Tokenizer.Read();
        if (token is AcceptingStateToken) {
          isAccepting = true;
          token = Tokenizer.Read();
        }
      } else if (token is AcceptingStateToken) {
        isAccepting = true;
        token = Tokenizer.Read();
        if (token is StartStateToken) {
          isStarting = true;
          token = Tokenizer.Read();
        }
      }

      // State Name.
      if (token is StateNameToken) {
        stateName = new StateNameNode(token.ContentToString());
      } else {
        throw new ArgumentException("Shit a state name was expected here yo.");
      }

      // ->
      token = Tokenizer.Read();
      if (!(token is ArrowToken)) {
        throw new ArgumentException("Aww shit an arrow token was expected.");
      }

      // Transitions
      transitions = ParseTransitionList();
      if (transitions.Count == 0) {
        throw new ArgumentException("Fuark man why u no put any transition functions.");
      }

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

    private LinkedList<TransitionNode> ParseTransitionList() {
      LinkedList<TransitionNode> Transitions = new LinkedList<TransitionNode>();

      while (Tokenizer.Peek() is ScopeOpenToken) {
        Transitions.AddLast(ParseTransition());
      }

      return Transitions;
    }

    private TransitionNode ParseTransition() {
      Token token = Tokenizer.Read();

      // {
      if (!(token is ScopeOpenToken)) {
        throw new ArgumentException("Scope not opened here yo.");
      }

      // Transition Function.
      token = Tokenizer.Read();
      TransitionFunctionNode functionNode = new TransitionFunctionNode();
      if (token is CharToken) {
        functionNode.TransitionType = TransitionFunctionNode.TransitionTypes.Char;
        functionNode.TransitionFunction = token.ContentToString(); 
      } else if (token is StringToken) {
        functionNode.TransitionType = TransitionFunctionNode.TransitionTypes.String;
        functionNode.TransitionFunction = token.ContentToString();
      } else if (token is RegexToken) {
        functionNode.TransitionType = TransitionFunctionNode.TransitionTypes.Regex;
        functionNode.TransitionFunction = token.ContentToString();
      } else if (token is FnScriptToken) {
        functionNode.TransitionType = TransitionFunctionNode.TransitionTypes.FunctionScript;
        functionNode.TransitionFunction = token.ContentToString();
      } else {
        throw new ArgumentException("fuar you didn't put a valid transition function in here didya?");
      }

      // ->
      if (!(Tokenizer.Read() is ArrowToken)) {
        throw new ArgumentException("whoah man you forgot the arrow token here");
      }

      // Destination State List
      LinkedList<StateNameNode> goalStateList = ParseStateNameList();
      if (goalStateList.Count == 0) {
        throw new ArgumentException("shit bro you forgot your destination list");
      }

      // }
      token = Tokenizer.Read();
      if (!(token is ScopeCloseToken)) {
        throw new ArgumentException("You gotta close the scope here gurl.");
      }

      Tokenizer.FlushBuffer();
      return new TransitionNode(functionNode, goalStateList);
    }

    private LinkedList<StateNameNode> ParseStateNameList() {
      LinkedList<StateNameNode> stateNames = new LinkedList<StateNameNode>();

      // StateName1
      if (Tokenizer.Peek() is StateNameToken) {
        stateNames.AddLast(new StateNameNode(Tokenizer.Read().ContentToString()));

        // , StateName2, StateName3 ...
        while (Tokenizer.Peek() is CommaSeparatorToken) {
          Tokenizer.Read(); // eat the comma
          Token token = Tokenizer.Read();
          if (token is StateNameToken) {
            stateNames.AddLast(new StateNameNode(token.ContentToString()));
          } else {
            throw new ArgumentException("aww man you put in a comma but no next state.");
          }
        }
      }

      return stateNames;
    }

    private Token DestructiveReadToken() {
      Token token = Tokenizer.Read();
      Tokenizer.FlushBuffer();

      return token;
    }
  }
}
