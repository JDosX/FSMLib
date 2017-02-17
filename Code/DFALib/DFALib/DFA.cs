using System;
using System.IO;
using System.Collections.Generic;

using FunctionScript;

/// <summary>
/// T represents the data type of the objects the DFA is traversing over
/// (E.g: if you are trying to analyse a list of ints, the O would be List<int>
/// and the T would be int)
/// </summary>
class DFA<T>
{
	protected State[] StartingStates;

	protected class State
	{
		/// <summary>
		/// The name of the state
		/// </summary>
		private string Name;

		/// <summary>
		/// Whether the state is accepting or not
		/// </summary>
		public bool Accepting;
		private Dictionary<State, List<FnScriptExpression<bool>>> Transitions;

		public State(string name) : this(name, false) { }

		public State(string name, bool accepting)
		{
			Name        = name;
			Accepting   = accepting;
			Transitions = new Dictionary<State, List<FnScriptExpression<bool>>> ();
		}

		/// <summary>
		/// Adds a new transition from this state to another
		/// </summary>
		/// /// <param name="state">The state to transition to</param>
		/// <param name="transitionFunction">The transition function to use</param>
		public void AddTransition(State state, FnScriptExpression<bool> transitionFunction)
		{
			if (!Transitions.ContainsKey(state))
			{
				Transitions.Add(state, new List<FnScriptExpression<bool>> ());
				// throw new ArgumentException("Transition already added from State " + Name + " to State " + state.Name);
			}

			Transitions[state].Add(transitionFunction);
		}

		public State Traverse(T input)
		{
			// todo: finish
			// iterate through all the states and all their transition functions. On the first successful
			// transition function, return a next state.

			// todo: PROBLEM: What if there are multiple successful states. E.g: one transition function might be
			// (((length > 6))) and another might be (((length == 7))), which one do you choose?
			// Do you iterate through both? Just choose the first one that's defined? Etc...
			// Easiest: Choose the first one that is defined in the file.
			// Even easier: Remove the possibility for multiple transition functions between states. This
			// is why you enabled FnScript and Regex for use in DFALib after all, to remove the need for
			// multiple transition functions
			return null;
		}
	}


	public DFA()
	{
		// todo: finish constructor
	}

	public bool Traverse(IEnumerable<T> input)
	{
		bool valid = false;

		for (int i = 0; !valid && i < StartingStates.Length; ++i)
		{
			State state = StartingStates[i];
			foreach (T item in input)
			{
				state = state.Traverse(item);
				if (state == null) break;
			}

			// The input is only valid if the DFA consumed
			// all the input and if the DFA finished on a
			// valid end state
			valid = state != null && state.Accepting;
		}

		return valid;
	}

	public bool Traverse(IEnumerator<T> input)
	{
		bool valid = false;

		for (int i = 0; !valid && i < StartingStates.Length; ++i)
		{
			State state = StartingStates[i];

			while (input.MoveNext())
			{
				state = state.Traverse(input.Current);
				if (state == null) break;
			}

			// The input is only valid if the DFA consumed
			// all the input and if the DFA finished on a
			// valid end state
			valid = state != null && state.Accepting;
			input.Reset();
		}

		return valid;
	}

	public void LoadDFA(string instructions)
	{
		LoadDFA(new StringReader(instructions));
	}

	public void LoadDFA(TextReader reader)
	{

	}
}