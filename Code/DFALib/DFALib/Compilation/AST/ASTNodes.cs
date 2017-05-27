using System;
using System.Collections.Generic;

using FSMLib.Compilation.Tokenizing;

namespace FSMLib.Compilation.AST {
  internal interface IASTNode {}

  internal sealed class FSMNode : IASTNode {
    internal LinkedList<StateDeclNode> StateDeclList;

    internal FSMNode(ICollection<StateDeclNode> stateDeclList) {
      StateDeclList = new LinkedList<StateDeclNode>(stateDeclList);
    }
  }

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
    
  }

  internal sealed class TransitionNode : IASTNode {
    
  }
}
