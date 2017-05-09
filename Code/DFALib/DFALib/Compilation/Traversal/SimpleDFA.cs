using System;
using System.Collections.Generic;

namespace FSMLib.Traversal {
  internal class SimpleDFA<T> {

    internal readonly Node Head;

    internal SimpleDFA() : this(new Node()) {}

    internal SimpleDFA(Node head) {

      // TODO: look into the new syntax here and see if you can use it to remove this explicit exception throw.
      if (head == null) {
        throw new ArgumentNullException("head");
      }

      Head = head;
    }

    /// <summary>
    /// TODO: pretty up this comment
    /// Returns the accepting node created by this direct match.
    /// </summary>
    internal Node AddDirectMatch(ICollection<T> match) {
      Node acceptingNode = Head;

      foreach (T element in match) {
        acceptingNode.TryAddConnection(element, new Node());
        acceptingNode = acceptingNode.GetConnection(element);
      }

      acceptingNode.Accepting = true;

      return acceptingNode;
    }

    internal bool Traverse(ICollection<T> input) {
      Node finalNode = Head;

      foreach (T element in input) {
        finalNode = finalNode.TryGetConnection(element);
        if (finalNode == null) {
          return false;
        }
      }

      return finalNode.Accepting;
    }

    internal class Node {
      internal bool Accepting;
      private Dictionary<T, Node> Connections;

      internal Node() : this(false) {}

      internal Node(bool accepting) {
        Accepting = accepting;
        Connections = new Dictionary<T, Node>();
      }

      internal bool ConnectionExists(T connector) {
        return Connections.ContainsKey(connector);
      }

      internal bool TryAddConnection(T connector, Node connectedNode) {
        if (ConnectionExists(connector)) {
          return false;
        }

        Connections.Add(connector, connectedNode);
        return true;
      }

      internal Node GetConnection(T connector) {
        return Connections[connector];
      }

      internal Node TryGetConnection(T connector) {
        if (!ConnectionExists(connector)) {
          return null;
        }

        return GetConnection(connector);
      }
    }
  }
}
