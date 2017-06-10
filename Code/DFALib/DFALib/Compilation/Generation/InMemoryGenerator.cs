using System;
using System.Collections.Generic;

using FSMLib.Compilation.AST;

namespace FSMLib.Compilation.Generation {
  internal sealed class InMemoryGenerator<T> {

    public FSM<T> FromAST(FSMNode fsmNode) {
      FSM<T> fsm = new FSM<T>(fsmNode.Name);

      Dictionary<string, FSM<T>.State> stateTable = BuildStateTable(fsmNode.StateDeclList);

      return fsm;
    }

    private Dictionary<string, FSM<T>.State> BuildStateTable(ICollection<StateDeclNode> stateDecls) {
      Dictionary<string, FSM<T>.State> stateTable = new Dictionary<string, FSM<T>.State>();

      foreach (StateDeclNode stateDecl in stateDecls) {
        string name = stateDecl.StateName.StateName;
        FSM<T>.State state = new FSM<T>.State(name, stateDecl.Accepting);
        stateTable.Add(name, state);
      }

      return stateTable;
    } 
  }
}
