using System;
using System.Collections.Generic;

using FSMLib.Compilation.AST;
using FSMLib.Compilation.Sanitization;

namespace FSMLib.Compilation.Generation {
  internal sealed class InMemoryGenerator<T> {

    private CharToFnScript CharSanitizer;
    private StringToFnScript StringSanitizer;
    private RegexRawToFnScript RegexSanitizer;
    private QuotedFnScriptToFnScript FnScriptSanitizer;

    public InMemoryGenerator() {
      CharSanitizer = new CharToFnScript();
      StringSanitizer = new StringToFnScript();
      RegexSanitizer = new RegexRawToFnScript();
      FnScriptSanitizer = new QuotedFnScriptToFnScript();
    }

    public FSM<T> FromAST(FSMNode fsmNode) {
      FSM<T> fsm = new FSM<T>(fsmNode.Name);

      CreateStates(fsm, fsmNode.StateDeclList);

      foreach (StateDeclNode stateDecl in fsmNode.StateDeclList) {
        AddTransitions(fsm, stateDecl);
      }

      return fsm;
    }

    private void CreateStates(FSM<T> fsm, ICollection<StateDeclNode> stateDecls) {
      foreach (StateDeclNode stateDecl in stateDecls) {
        string name = stateDecl.StateName.StateName;
        fsm.AddState(name, stateDecl.Starting, stateDecl.Accepting);
      }
    }

    private void AddTransitions(FSM<T> fsm, StateDeclNode stateDecl) {
      foreach (TransitionNode transition in stateDecl.Transitions) {
        string transitionFunction = SanitizeTransitionFunction(transition.TransitionFunction);
        HashSet<string> destinations = GetStateNames(transition.GoalStateList);
        fsm.AddTransition(stateDecl.StateName.StateName, transitionFunction, destinations);
      }
    }

    private string SanitizeTransitionFunction(TransitionFunctionNode transitionFunction) {
      string sanitizedFunction = "";

      // Sanitize based on function type.
      switch (transitionFunction.TransitionType) {
        case TransitionFunctionNode.TransitionTypes.Char:
          sanitizedFunction = CharSanitizer.Sanitize(transitionFunction.TransitionFunction);
          break;
        case TransitionFunctionNode.TransitionTypes.String:
          sanitizedFunction = StringSanitizer.Sanitize(transitionFunction.TransitionFunction);
          break;
        case TransitionFunctionNode.TransitionTypes.Regex:
          sanitizedFunction = RegexSanitizer.Sanitize(transitionFunction.TransitionFunction);
          break;
        case TransitionFunctionNode.TransitionTypes.FunctionScript:
          sanitizedFunction = FnScriptSanitizer.Sanitize(transitionFunction.TransitionFunction);
          break;
      }

      return sanitizedFunction;
    }

    private HashSet<string> GetStateNames(ICollection<StateNameNode> stateNames) {
      HashSet<string> stringStateNames = new HashSet<string>();
      foreach (StateNameNode stateName in stateNames) {
        stringStateNames.Add(stateName.StateName);
      }
      return stringStateNames;
    }
  }
}
