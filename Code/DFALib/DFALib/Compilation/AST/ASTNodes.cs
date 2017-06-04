using System;
using System.Collections.Generic;

using FSMLib.Compilation.Tokenizing;

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

  internal sealed class TransitionFunctionNode : IASTNode {
    internal enum TransitionTypes {
      Char, String, Regex, FunctionScript
    }

    internal TransitionTypes TransitionType;

    internal string TransitionFunction;
  }

  internal sealed class TransitionNode : IASTNode {
    internal TransitionFunctionNode TransitionFunction;

    // TODO: consider making a StateNameNode for this purpose, makes it a little
    // more rigid than just strings.
    internal LinkedList<StateNameNode> GoalStateList;

    internal TransitionNode(TransitionFunctionNode transitionFunction, ICollection<StateNameNode> goalStateList) {
      TransitionFunction = transitionFunction;
      GoalStateList = new LinkedList<StateNameNode>(goalStateList);
    }
  }
}
