using System;
using System.Collections.Generic;

using FSMLib.Compilation.Tokenizing;

using FunctionScript;

namespace FSMLib.Compilation.AST {
  internal interface IASTNode {}

  internal sealed class FSMNode : IASTNode {
    internal string Name;
    internal LinkedList<StateDeclNode> StateDeclList;

    internal FSMNode(string name, ICollection<StateDeclNode> stateDeclList) {
      Name = name;
      StateDeclList = new LinkedList<StateDeclNode>(stateDeclList);
    }
  }

  internal sealed class StateDeclNode : IASTNode {
    internal bool Starting;
    internal bool Accepting;
    internal string StateName;

    internal LinkedList<TransitionNode> Transitions;

    internal StateDeclNode(
      bool starting, bool accepting, string stateName, ICollection<TransitionNode> transitions
    ) {
      Starting = starting;
      Accepting = accepting;
      StateName = stateName;
      Transitions = new LinkedList<TransitionNode>(transitions);
    }
  }

  internal interface ITransitionFunctionNode : IASTNode {
    FnScriptExpression ToFnScriptExpression();
  }

  // TODO: I don't know if this is a good way to do it. It may be better to
  // store the raw representation of the transition function, and worry about the
  // translation process at a later date. Also maybe consider using the visitor
  // pattern for this process, and maybe even for the AST -> In Memory Representation.
  internal sealed class CharTransitionFunction : ITransitionFunctionNode {
    public FnScriptExpression ToFnScriptExpression() {
      throw new NotImplementedException();
    }
  }

  internal sealed class StringTransitionFunction : ITransitionFunctionNode {
    public FnScriptExpression ToFnScriptExpression() {
      throw new NotImplementedException();
    }
  }

  internal sealed class RegexTransitionFunction : ITransitionFunctionNode {
    public FnScriptExpression ToFnScriptExpression() {
      throw new NotImplementedException();
    }
  }

  internal sealed class FnScriptTransitionFunction : ITransitionFunctionNode {
    public FnScriptExpression ToFnScriptExpression() {
      throw new NotImplementedException();
    }
  }

  internal sealed class TransitionNode : IASTNode {
    internal ITransitionFunctionNode TransitionFunction;

    // TODO: consider making a StateNameNode for this purpose, makes it a little
    // more rigid than just strings.
    internal LinkedList<string> GoalStateList;

    internal TransitionNode (ITransitionFunctionNode transitionFunction, ICollection<string> goalStateList) {
      TransitionFunction = transitionFunction;
      GoalStateList = new LinkedList<string>(goalStateList);
    }
  }
}
