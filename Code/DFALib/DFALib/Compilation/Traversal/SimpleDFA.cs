using System;
using System.Collections.Generic;

namespace FSMLib.Compilation.Traversal {
  internal class SimpleDFA<T> {

    internal readonly Node Head;

    internal SimpleDFA() : this(new Node()) {}

    internal SimpleDFA(Node head) {
      Head = head ?? throw new ArgumentNullException("Head cannot be null");
    }

    /// <summary>
    /// TODO: pretty up this comment
    /// Returns the accepting node created by this direct match.
    /// </summary>
    internal Node AddDirectMatch(ICollection<T> match) {
      Node acceptingNode = Head;

      foreach (T element in match) {
        acceptingNode.SetTransition(element, new Node());
        acceptingNode = acceptingNode.GetTransition(element).Destination;
      }

      acceptingNode.Accepting = true;

      return acceptingNode;
    }

    internal bool Traverse(ICollection<T> input) {
      Node finalNode = Head;

      foreach (T element in input) {
        Transition transition = finalNode.GetTransition(element);
        if (transition == null) {
          return false;
        } else {
          transition.InvokeOnTransitioned(element);
          finalNode = transition.Destination;
        }
      }

      return finalNode.Accepting;
    }

    internal class Node {

      internal bool Accepting;

      private Dictionary<T, Transition> Transitions;

      internal Node() : this(false) {}

      internal Node(bool accepting) {
        Accepting = accepting;
        Transitions = new Dictionary<T, Transition>();
      }

      internal bool TransitionExists(T connector) {
        return Transitions.ContainsKey(connector);
      }

      internal Transition SetTransition(T connector, Node connectedNode) {
        Transition transition = new Transition(connectedNode);
        Transitions[connector] = transition;
        return transition;
      }

      internal Transition SetTransition(ICollection<T> connectors, Node connectedNode) {
        Transition transition = new Transition(connectedNode);
        foreach (T connector in connectors) {
          Transitions[connector] = transition;
        }

        return transition;
      }

      internal Transition GetTransition(T connector) {
        if (!TransitionExists(connector)) {
          return null;
        }

        return Transitions[connector];
      }
    }

    internal class Transition {
      public Node Destination;
      public event EventHandler<DFAEventArgs> OnTransition;

      internal Transition(Node destination) {
        OnTransition = delegate { };
        Destination = destination;
      }

      internal void InvokeOnTransitioned(T element) {
        OnTransition(this, new DFAEventArgs(element));
      }
    }

    public class DFAEventArgs : EventArgs {
      /// <summary>
      /// The element that was consumed in the previous traversal.
      /// </summary>
      public T Element;

      public DFAEventArgs(T element) {
        Element = element;
      }
    }
  }
}
