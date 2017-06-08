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
        acceptingNode.TryAddConnection(element, new Node());
        acceptingNode = acceptingNode.GetConnection(element).Destination;
      }

      acceptingNode.Accepting = true;

      return acceptingNode;
    }

    internal bool Traverse(ICollection<T> input) {
      Node finalNode = Head;

      foreach (T element in input) {
        Transition transition = finalNode.TryGetConnection(element);
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

      private Dictionary<T, Transition> Connections;

      internal Node() : this(false) {}

      internal Node(bool accepting) {
        Accepting = accepting;
        Connections = new Dictionary<T, Transition>();
      }

      internal bool ConnectionExists(T connector) {
        return Connections.ContainsKey(connector);
      }

      internal Transition TryAddConnection(T connector, Node connectedNode) {
        if (ConnectionExists(connector)) {
          return null;
        }

        Transition transition = new Transition(connectedNode);
        Connections.Add(connector, transition);
        return transition;
      }

      internal T[] TryAddConnections(ICollection<T> connectors, Node connectedNode) {
        List<T> failed = new List<T>();

        foreach (T connector in connectors) {
          if (TryAddConnection(connector, connectedNode) == null) { failed.Add(connector); }
        }

        return failed.ToArray();
      }

      internal Transition GetConnection(T connector) {
        return Connections[connector];
      }

      internal Transition TryGetConnection(T connector) {
        if (!ConnectionExists(connector)) {
          return null;
        }

        return GetConnection(connector);
      }
    }

    internal class Transition {
      public Node Destination;
      public event EventHandler<DFAEventArgs> OnTransition;

      internal Transition(Node destination) {
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
