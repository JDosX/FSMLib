using System;
using System.Text;
using System.Collections.Generic;

using FSMLib.Traversal;

namespace FSMLib.Compilation.Tokenizing {
  internal abstract class Token {

    /// <summary>
    /// The raw contents of this token.
    /// </summary>
    internal StringBuilder Contents;

    internal StreamPosition TokenStart;
    internal StreamPosition TokenEnd;

    internal enum FeedState {
      Start,
      InProgress,
      Invalid,
      Done
    }

    internal FeedState State;

    /// <summary>
    /// Initializes a new instance of the <see cref="T:FSMLib.Compilation.Tokenizing.Token"/> class.
    /// </summary>
    /// <param name="tokenStart">The starting StreamPosition of the token.</param>
    internal Token(StreamPosition tokenStart) {
      Contents = new StringBuilder();
      TokenStart = tokenStart;
      State = FeedState.Start;
    }

    /// <summary>
    /// Feed the next character into the token and perform validation. Returns true if the fed character is a valid part
    /// of the token, false if not.
    /// TODO: Fix this comment, it's kinda whack.
    /// </summary>
    /// <param name="c">Character to inspect.</param>
    internal abstract FeedState Feed(char c, StreamPosition tokenEnd);

    /// <summary>
    /// Removes any marker characters from the provided string, leaving only the raw content
    /// </summary>
    /// <param name="content">Initial content string to sanitize.</param>
    protected virtual string GetSanitizedContent(string content) {
      return Contents.ToString();
    }

    public override string ToString() {
      return string.Format("{0}: {1}", this.GetType().Name, Contents.ToString());
    }
  }

  internal abstract class SingleCharToken : Token {

    char ValidChar;

    internal SingleCharToken(StreamPosition tokenStart, char validChar) : base(tokenStart) {
      ValidChar = validChar;
    }

    internal override FeedState Feed(char c, StreamPosition tokenEnd) {

      if (State == FeedState.Start) {
        if (c == ValidChar) {
          Contents.Append(c);
          TokenEnd = tokenEnd;
          State = FeedState.Done;
        } else {
          // TODO: Does this methodology work?
          State = FeedState.Invalid;
        }
      }
                           
      return State;
    }
  }

  internal abstract class DFAToken : Token {
    protected SimpleDFA<char> DFA;
    protected SimpleDFA<char>.Node CurrentNode;

    internal DFAToken(StreamPosition tokenStart) : base(tokenStart) {
      DFA = new SimpleDFA<char>();
      CurrentNode = DFA.Head;
    }

    internal override FeedState Feed(char c, StreamPosition tokenEnd) {
      if (State != FeedState.Invalid || State != FeedState.Done) {
        SimpleDFA<char>.Node nextNode = CurrentNode.TryGetConnection(c);

        // If next char is invalid
        // TODO: does this mean that there must always be a next character in order for this traversal to work? What
        // happens if this reaches the end of the file and there is no newline character to provide the next char to
        // make this work?
        if (nextNode != null) {
          State = FeedState.InProgress;
          CurrentNode = nextNode;
          Contents.Append(c);
          TokenEnd = tokenEnd;
        } else if (CurrentNode.Accepting) {
          State = FeedState.Done;
        } else {
          State = FeedState.Invalid;
        }
      }

      return State;
    }
  }

  internal abstract class SingleMatchToken : DFAToken {

    internal SingleMatchToken(StreamPosition tokenStart, string match) : base(tokenStart) {
      DFA.AddDirectMatch(match.ToCharArray());
    }
  }

  internal abstract class MultiMatchToken : DFAToken {

    internal MultiMatchToken(StreamPosition tokenStart, ICollection<string> matches) : base(tokenStart) {
      foreach(string match in matches) {
        DFA.AddDirectMatch(match.ToCharArray());
      }
    }
  }

  // TODO: Use reflection to make sure all of these are automatically added into Tokenizer.GenerateCompetingTokens

  internal class KeywordFSMToken : SingleMatchToken {
    internal const string KEYWORD = "fsm";

    internal KeywordFSMToken(StreamPosition tokenStart) : base(tokenStart, KEYWORD) {}
  }

  /// <summary>
  /// A token encapsulating a marker that indicates a starting state.
  /// </summary>
  internal class StartStateToken : SingleCharToken {
    internal StartStateToken(StreamPosition tokenStart) : base(tokenStart, '+') {}
  }

  /// <summary>
  /// A token encapsulating a marker that indicates an accepting state.
  /// </summary>
  internal class AcceptingStateToken : SingleCharToken {
    internal AcceptingStateToken(StreamPosition tokenStart) : base(tokenStart, '*') { }
  }

  /// <summary>
  /// A token encapsulating a marker that indicates the opening of a new scope.
  /// </summary>
  internal class ScopeOpenToken : SingleCharToken {
    internal ScopeOpenToken(StreamPosition tokenStart) : base(tokenStart, '{') { }
  }

  /// <summary>
  /// A token encapsulating a marker that indicates the closing of an existing scope.
  /// </summary>
  internal class ScopeCloseToken : SingleCharToken {
    internal ScopeCloseToken(StreamPosition tokenStart) : base(tokenStart, '}') { }
  }
}
