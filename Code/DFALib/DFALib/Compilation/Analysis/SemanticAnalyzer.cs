using System;
using System.Collections.Generic;

using FSMLib.Compilation.AST;

namespace FSMLib.Compilation.Analysis {
  internal class SemanticAnalyzer {

    // TODO: Consider whether some of the AST validation done in the Parser should be done here (e.g: making sure that 
    public void Analyze(FSMNode fsm) {
      HashSet<string> declaredStateNames = VerifyStateDeclNames(fsm.StateDeclList);

      foreach (StateDeclNode stateNode in fsm.StateDeclList) {
        foreach (TransitionNode transition in stateNode.Transitions) {
          VerifyStateNameList(transition.GoalStateList, declaredStateNames);
        }
      }
    }

    /// <summary>
    /// Returns a set of the names of all the declared states. Throws an exception if multiple state declarations
    /// have the same name, or if there is no starting state.
    /// </summary>
    /// <param name="stateDecls">State Declaratin List to Analyze</param>
    /// <returns></returns>
    private HashSet<string> VerifyStateDeclNames(ICollection<StateDeclNode> stateDecls) {
      bool hasStartingState = false;
      HashSet<string> names = new HashSet<string>();

      foreach (StateDeclNode stateDecl in stateDecls) {
        if (names.Contains(stateDecl.StateName.StateName)) {
          throw new ArgumentException("aww shit son you have two states declared with the same name");
        } else {
          names.Add(stateDecl.StateName.StateName);
        }

        hasStartingState = hasStartingState || stateDecl.Starting;
      }

      if (!hasStartingState) {
        throw new ArgumentException("fuck man you forgot to put in a starting state");
      }

      return names;
    }

    /// <summary>
    /// Verifies the integrity of the provided state name list. Throws an exception if the state name list
    /// calls a state that isn't defined, or if a state name is listed more than once.
    /// </summary>
    /// <param name="stateNames">The list of state names to verify.</param>
    /// <param name="declaredStateNames">The names of states that have been declared in this FSM.</param>
    private void VerifyStateNameList(ICollection<StateNameNode> stateNames, HashSet<string> declaredStateNames) {
      HashSet<string> seenNames = new HashSet<string>();
      foreach (StateNameNode stateName in stateNames) {
        // Check for undeclared state name.
        if (!declaredStateNames.Contains(stateName.StateName)) {
          throw new ArgumentException("aww damn you didn't declare this state name bro");
        }

        // Check for listing state name twice.
        if (seenNames.Contains(stateName.StateName)) {
          throw new ArgumentException("dude you already provided this state name");
        }

        seenNames.Add(stateName.StateName);
      }
    }
  }
}
