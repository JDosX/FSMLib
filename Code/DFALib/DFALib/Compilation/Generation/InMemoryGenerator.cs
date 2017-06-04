using System;
using System.Collections.Generic;

using FSMLib.Compilation.AST;

namespace FSMLib.Compilation.Generation {
  internal sealed class InMemoryGenerator<T> {

    // TODO: should I put the list of final states here or have them as a local variable?
    // For efficiency reasons this might be good to have here.
    private Dictionary<string, FSM<T>.State> States;

    public FSM<T> FromAST(FSMNode fsmNode) {
      throw new NotImplementedException("damn man");
    }
  }
}
