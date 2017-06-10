using System;
using System.IO;
using System.Collections.Generic;

using FunctionScript;

using FSMLib.Compilation;

/// <summary>
/// T represents the data type of the objects the DFA is traversing over
/// (E.g: if you are trying to analyse a list of ints, the O would be List<int>
/// and the T would be int)
/// </summary>
public class FSM<T>
{
  private string Name;
  private Dictionary<string, State> States;

	private List<State> StartingStates;

  #region FnScript Collection Parameters
  public const string CURRENT_ITEM_FNVARIABLE_NAME = "s";
	protected FnVariable<T> CurrentItem;
  #endregion

  private Dictionary<string, FnObject> FSMCollectionParameters;

  /// <summary>
  /// Constructor.
  /// </summary>
  /// <param name="name">The name of the FSM.</param>
  public FSM(string name) {
    States = new Dictionary<string, State>();
    StartingStates = new List<State>();
    FSMCollectionParameters = new Dictionary<string, FnObject>();

    CurrentItem = new FnVariable<T>(default(T));
    Name = name;

    ConstructCollectionParameters();
	}

  private void ConstructCollectionParameters() {
    FSMCollectionParameters.Add(CURRENT_ITEM_FNVARIABLE_NAME, CurrentItem);
  }

  /// <summary>
  /// Adds a new state to the FSM.
  /// </summary>
  /// <param name="name">The name of the state.</param>
  /// <param name="starting">Whether the state is a starting state for the FSM.</param>
  /// <param name="accepting">Whether the state is an accepting state.</param>
  public void AddState(string name, bool starting, bool accepting) {
    State state = new State(name, accepting, FSMCollectionParameters);
    States.Add(name, state);
    if (starting) {
      StartingStates.Add(state);
    }
  }

  public void AddTransition(string fromState, string transitionFunction, string toState) {
    AddTransition(fromState, transitionFunction, new string[] { toState });
  }

  public void AddTransition(string fromState, string transitionFunction, ICollection<string> toStates) {
    State sourceState = States[fromState];
    HashSet<State> destinationStates = new HashSet<State>();
    foreach (string toState in toStates) {
      destinationStates.Add(States[toState]);
    }

    sourceState.AddTransition(transitionFunction, destinationStates);
  }

	public bool Traverse(IEnumerable<T> input)
	{
		return Traverse(input.GetEnumerator());
	}

	public bool Traverse(IEnumerator<T> input)
	{
		bool valid = false;
		Queue<State> states = new Queue<State>(StartingStates);

		while (input.MoveNext() && states.Count > 0)
		{
			for (int i = states.Count - 1; i >= 0; i--)
			{
				HashSet<State> newStates = states.Dequeue().Traverse(input.Current);
				foreach (State s in newStates) { states.Enqueue(s); }
			}
		}

		// Check that we've landed on a valid
		// state once all the input is consumed.
		foreach (State s in states)
		{
			valid = s.Accepting;
			if (valid) break;
		}

		return valid;
	}

  public static FSM<T> FromReader(TextReader reader) {
    return InMemoryCompiler.FromReader<T>(reader);
  }

  #region Nested Classes

  internal class Transition {
    public HashSet<State> Destinations;
    public FnScriptExpression<bool> Function;

    public Transition(string fnScriptFunction, Dictionary<string, FnObject> collectionParameters, ICollection<State> destinations) {
      FnScriptCompiler compiler = new FnScriptCompiler();
      Function = compiler.Compile<bool>(fnScriptFunction, null, collectionParameters);
      Destinations = new HashSet<State>(destinations);
    }
  }

  internal class State {
    /// <summary>
    /// The name of the state.
    /// </summary>
    private string Name;

    /// <summary>
    /// If set, then ending a traversal of the FSM on this state would be considered a successful traversal.
    /// </summary>
    public bool Accepting;

    /// <summary>
    /// Transitions from this state to destination states.
    /// </summary>
    private List<Transition> Transitions;

    /// <summary>
    /// Collection of FnScript parameters to use in transitions
    /// </summary>
    private Dictionary<string, FnObject> CollectionParameters;

    public State(string name, bool accepting, Dictionary<string, FnObject> collectionParameters) {
      Name = name;
      Accepting = accepting;
      CollectionParameters = collectionParameters;

      Transitions = new List<Transition>();
    }

    /// <summary>
    /// Adds a new transition from this state to another.
    /// </summary>
    /// <param name="states">The state to transition to</param>
    /// <param name="transitionFunction">The transition function to use</param>
    public void AddTransition(string transitionFunction, ICollection<State> states) {
      Transitions.Add(new Transition(transitionFunction, CollectionParameters, states));
    }

    public HashSet<State> Traverse(T input) {
      HashSet<State> destinations = new HashSet<State>();

      foreach (Transition transition in Transitions) {
        transition.Function.SetParameter(CURRENT_ITEM_FNVARIABLE_NAME, input);

        if (transition.Function.Execute()) {
          destinations.UnionWith(transition.Destinations);
        }
      }

      return destinations;
    }
  }

  #endregion
}