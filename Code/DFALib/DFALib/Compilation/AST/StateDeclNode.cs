using System;
using System.Collections.Generic;

namespace FSMLib.Compilation.AST {
  internal sealed class StateDeclNode : IASTNode {
    internal bool Starting;
    internal bool Accepting;
    internal StateNameNode StateName;

    internal LinkedList<TransitionNode> Transitions;

    internal StateDeclNode(
      bool starting, bool accepting, StateNameNode stateName, ICollection<TransitionNode> transitions
    ) {
      Starting = starting;
      Accepting = accepting;
      StateName = stateName;
      Transitions = new LinkedList<TransitionNode>(transitions);
    }
  }

  internal sealed class StateNameNode : IASTNode {
    public string StateName;

    internal StateNameNode(string stateName) {
      StateName = stateName;
    }
  }
}
