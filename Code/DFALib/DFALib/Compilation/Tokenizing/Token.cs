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

    /// <summary>
    /// All the chars in the range [0, 127] (inclusive).
    /// </summary>
    /// <returns>The chars.</returns>
    protected HashSet<char> AllChars() {
      HashSet<char> allChars = new HashSet<char>();

      for (char c = (char)0; c <= (char)127; c++) {
        allChars.Add(c);
      }

      return allChars;
    }

    protected HashSet<char> AllCharsExcept(ICollection<char> exclusions) {
      HashSet<char> allChars = AllChars();

      foreach (char exclusion in exclusions) {
        allChars.Remove(exclusion);
      }

      return allChars;
    }

    protected HashSet<char> AllCharsInRange(char min, char max) {
      HashSet<char> allChars = new HashSet<char>();

      for (char c = min; c <= max; c++) {
        allChars.Add(c);
      }

      return allChars;
    }
  }

  internal abstract class SingleMatchToken : DFAToken {

    internal SingleMatchToken(StreamPosition tokenStart, string match) : base(tokenStart) {
      DFA.AddDirectMatch(match.ToCharArray());
    }
  }

  internal abstract class MultiMatchToken : DFAToken {

    internal MultiMatchToken(StreamPosition tokenStart, ICollection<string> matches) : base(tokenStart) {
      foreach (string match in matches) {
        DFA.AddDirectMatch(match.ToCharArray());
      }
    }
  }

  // TODO: Use reflection to make sure all of these are automatically added into Tokenizer.GenerateCompetingTokens

  internal class StateNameToken : DFAToken {
    internal StateNameToken(StreamPosition tokenStart) : base(tokenStart) {
      SimpleDFA<char>.Node Head = DFA.Head;
      SimpleDFA<char>.Node ValidName = new SimpleDFA<char>.Node(true);

      HashSet<char> validStartingChars = new HashSet<char>();
      validStartingChars.UnionWith(AllCharsInRange('A', 'Z'));
      validStartingChars.UnionWith(AllCharsInRange('a', 'z'));
      validStartingChars.Add('_');

      HashSet<char> validFollowingChars = new HashSet<char>();
      validFollowingChars.UnionWith(validStartingChars);
      validFollowingChars.UnionWith(AllCharsInRange('0', '9'));

      Head.TryAddConnections(validStartingChars, ValidName);
      ValidName.TryAddConnections(validFollowingChars, ValidName);
    }
  }

  // TODO: make sure to implement all escape characters (literal escape question mark and below not implemented):
  // https://msdn.microsoft.com/en-us/library/h21280bw.aspx

  internal class CharToken : DFAToken {
    internal CharToken(StreamPosition tokenStart) : base(tokenStart) {
      SimpleDFA<char>.Node Head = DFA.Head;
      SimpleDFA<char>.Node CharStart = new SimpleDFA<char>.Node(false);
      SimpleDFA<char>.Node EscapeSlash = new SimpleDFA<char>.Node(false);
      SimpleDFA<char>.Node CharProvided = new SimpleDFA<char>.Node(false);
      SimpleDFA<char>.Node CharEnd = new SimpleDFA<char>.Node(true);

      char[] simpleEscapeCharacters = new char[] {
        'a', 'b', 'f', 'n', 'r', 't', 'v', '\'', '"', '\\'
      };

      Head.TryAddConnection('\'', CharStart);
      CharStart.TryAddConnections(AllCharsExcept(new char[] { '\\', '\'' }), CharProvided);
      CharStart.TryAddConnection('\\', EscapeSlash);

      EscapeSlash.TryAddConnections(simpleEscapeCharacters, CharProvided);

      CharProvided.TryAddConnection('\'', CharEnd);
    }
  }

  internal class StringToken : DFAToken {
    internal StringToken(StreamPosition tokenStart) : base(tokenStart) {
      SimpleDFA<char>.Node Head = DFA.Head;
      SimpleDFA<char>.Node StringStart = new SimpleDFA<char>.Node(false);
      SimpleDFA<char>.Node EscapeSlash = new SimpleDFA<char>.Node(false);
      SimpleDFA<char>.Node StringEnd = new SimpleDFA<char>.Node(true);

      char[] simpleEscapeCharacters = new char[] {
        'a', 'b', 'f', 'n', 'r', 't', 'v', '\'', '"', '\\'
      };

      Head.TryAddConnection('"', StringStart);
      StringStart.TryAddConnections(AllCharsExcept(new char[] { '\\', '"' }), StringStart);
      StringStart.TryAddConnection('\\', EscapeSlash);
      StringStart.TryAddConnection('"', StringEnd);

      EscapeSlash.TryAddConnections(simpleEscapeCharacters, StringStart);
    }
  }

  internal class FnScriptToken : DFAToken {
    internal FnScriptToken(StreamPosition tokenStart) : base(tokenStart) {
      SimpleDFA<char>.Node head = DFA.Head;
      SimpleDFA<char>.Node script = new SimpleDFA<char>.Node(false);

      SimpleDFA<char>.Node scriptString = new SimpleDFA<char>.Node(false);
      SimpleDFA<char>.Node stringEscape = new SimpleDFA<char>.Node(false);

      SimpleDFA<char>.Node scriptChar = new SimpleDFA<char>.Node(false);
      SimpleDFA<char>.Node charEscape = new SimpleDFA<char>.Node(false);

      SimpleDFA<char>.Node scriptParameter = new SimpleDFA<char>.Node(false);

      SimpleDFA<char>.Node scriptEnd = new SimpleDFA<char>.Node(true);

      head.TryAddConnection('`', script);
      script.TryAddConnections(AllCharsExcept(new char[] { '"', '\'', '[', '`' }), script);
      script.TryAddConnection('"', scriptString);
      script.TryAddConnection('\'', scriptChar);
      script.TryAddConnection('[', scriptParameter);

      // Strings in an Fnscript expression.
      scriptString.TryAddConnections(AllCharsExcept(new char[] { '"', '\\' }), scriptString);
      scriptString.TryAddConnection('\\', stringEscape);
      scriptString.TryAddConnection('"', script);
      stringEscape.TryAddConnections(AllChars(), scriptString);

      // Chars in an Fnscript expression.
      scriptChar.TryAddConnections(AllCharsExcept(new char[] { '\'', '\\' }), scriptChar);
      scriptChar.TryAddConnection('\\', charEscape);
      scriptChar.TryAddConnection('\'', script);
      charEscape.TryAddConnections(AllChars(), scriptChar);

      // FnScript Parameters.
      scriptParameter.TryAddConnections(AllCharsExcept(new char[] { ']' }), scriptParameter);
      scriptParameter.TryAddConnection(']', script);

      // Ending FnScript Expression.
      script.TryAddConnection('`', scriptEnd);
    }
  }

  internal class SingleLineCommentToken : DFAToken {
    internal SingleLineCommentToken(StreamPosition tokenStart) : base(tokenStart) {
      SimpleDFA<char>.Node head = DFA.Head;
      SimpleDFA<char>.Node comment = new SimpleDFA<char>.Node(true);

      head.TryAddConnection('#', comment);
      comment.TryAddConnections(AllCharsExcept(new char[] {'\n'}), comment);
    }
  }

  internal class MultiLineCommentToken : DFAToken {
    internal MultiLineCommentToken(StreamPosition tokenStart) : base(tokenStart) {
      SimpleDFA<char>.Node               head = DFA.Head;
      SimpleDFA<char>.Node      commentPrefix = new SimpleDFA<char>.Node(false);
      SimpleDFA<char>.Node            comment = new SimpleDFA<char>.Node(false);
      SimpleDFA<char>.Node commentSuffixStart = new SimpleDFA<char>.Node(false);
      SimpleDFA<char>.Node   commentSuffixEnd = new SimpleDFA<char>.Node(true);

      head.TryAddConnection('<', commentPrefix);
      commentPrefix.TryAddConnection('#', comment);

      comment.TryAddConnections(AllCharsExcept(new char[] { '#' }), comment);
      comment.TryAddConnection('#', commentSuffixStart);

      commentSuffixStart.TryAddConnections(AllCharsExcept(new char[] { '>' }), comment);
      commentSuffixStart.TryAddConnection('>', commentSuffixEnd);
    }
  }

  internal class KeywordFSMToken : SingleMatchToken {
    internal const string KEYWORD = "fsm";

    internal KeywordFSMToken(StreamPosition tokenStart) : base(tokenStart, KEYWORD) {}
  }

  /// <summary>
  /// A token encapsulating a marker that indicates a starting state.
  /// </summary>
  internal class StartStateToken : SingleMatchToken {
    internal StartStateToken(StreamPosition tokenStart) : base(tokenStart, "+") {}
  }

  /// <summary>
  /// A token encapsulating a marker that indicates an accepting state.
  /// </summary>
  internal class AcceptingStateToken : SingleMatchToken {
    internal AcceptingStateToken(StreamPosition tokenStart) : base(tokenStart, "*") { }
  }

  /// <summary>
  /// A token encapsulating a marker that indicates the opening of a new scope.
  /// </summary>
  internal class ScopeOpenToken : SingleMatchToken {
    internal ScopeOpenToken(StreamPosition tokenStart) : base(tokenStart, "{") { }
  }

  /// <summary>
  /// A token encapsulating a marker that indicates the closing of an existing scope.
  /// </summary>
  internal class ScopeCloseToken : SingleMatchToken {
    internal ScopeCloseToken(StreamPosition tokenStart) : base(tokenStart, "}") { }
  }

  /// <summary>
  /// A token encapsulating a marker that indicates the opening of a new scope.
  /// </summary>
  internal class SquareOpenToken : SingleMatchToken {
    internal SquareOpenToken(StreamPosition tokenStart) : base(tokenStart, "[") { }
  }

  /// <summary>
  /// A token encapsulating a marker that indicates the closing of an existing scope.
  /// </summary>
  internal class SquareCloseToken : SingleMatchToken {
    internal SquareCloseToken(StreamPosition tokenStart) : base(tokenStart, "]") { }
  }

  internal class ArrowToken : SingleMatchToken {
    internal ArrowToken(StreamPosition tokenStart) : base(tokenStart, "->") { }
  }

  internal class CommaSeparatorToken : SingleMatchToken {
    internal CommaSeparatorToken(StreamPosition tokenStart) : base(tokenStart, ",") { }
  }
}
