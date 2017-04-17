using System;
using System.IO;
using System.Collections.Generic;

using FunctionScript;

/// <summary>
/// T represents the data type of the objects the DFA is traversing over
/// (E.g: if you are trying to analyse a list of ints, the O would be List<int>
/// and the T would be int)
/// </summary>
class FSM<T>
{
	protected State[] StartingStates;
	protected FnVariable<T> CurrentItem;

	protected class State
	{
		public const string CURRENT_ITEM_FNVARIABLE_NAME = "T";

		/// <summary>
		/// The name of the state.
		/// </summary>
		private string Name;

		/// <summary>
		/// If set, then ending a traversal of the FSM on this state would be considered a successful traversal.
		/// </summary>
		public bool Accepting;
		private List<Tuple<State, FnScriptExpression<bool>>> Transitions;

		// private Dictionary<State, List<FnScriptExpression<bool>>> Transitions;

		public State(string name) : this(name, false) { }

		public State(string name, bool accepting)
		{
			Name        = name;
			Accepting   = accepting;
			Transitions = new List<Tuple<State, FnScriptExpression<bool>>>();
			// Transitions = new Dictionary<State, List<FnScriptExpression<bool>>> ();
		}

		/// <summary>
		/// Adds a new transition from this state to another.
		/// </summary>
		/// /// <param name="state">The state to transition to</param>
		/// <param name="transitionFunction">The transition function to use</param>
		public void AddTransition(State state, FnScriptExpression<bool> transitionFunction)
		{
			Transitions.Add(new Tuple<State, FnScriptExpression<bool>> (state, transitionFunction));
		}

		public HashSet<State> Traverse(T input)
		{
			HashSet<State> transitions = new HashSet<State>();

			// todo: finish
			// iterate through all the states and all their transition functions.
			// Return a list of all the states who's transition functions returned true.
			foreach (Tuple<State, FnScriptExpression<bool>> t in Transitions)
			{
				t.Item2.SetParameter(CURRENT_ITEM_FNVARIABLE_NAME, t.Item1);

				if (t.Item2.Execute())
				{
					transitions.Add(t.Item1);
				}
			}

			return transitions;
		}
	}


	public FSM()
	{
		// todo: finish constructor
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

  public static FSM<T> FromStream(TextReader reader) {
    throw new NotImplementedException();
  }
}