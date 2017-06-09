using System;
using System.Text;
using System.Collections.Generic;

using FSMLib.Compilation.Traversal;

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
    internal string ContentToString() {
      // TODO: Consider DFA implementation for sanitizing content. E.g:
      // use a DFA with event handlers to build the sanitised content
      // representation during compilation.
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
        SimpleDFA<char>.Transition nextTransition = CurrentNode.GetTransition(c);

        // If next char is invalid
        // TODO: does this mean that there must always be a next character in order for this traversal to work? What
        // happens if this reaches the end of the file and there is no newline character to provide the next char to
        // make this work?
        if (nextTransition != null) {
          State = FeedState.InProgress;
          CurrentNode = nextTransition.Destination;
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
      validStartingChars.UnionWith(CharUtils.AllCharsInRange('A', 'Z'));
      validStartingChars.UnionWith(CharUtils.AllCharsInRange('a', 'z'));
      validStartingChars.Add('_');

      HashSet<char> validFollowingChars = new HashSet<char>();
      validFollowingChars.UnionWith(validStartingChars);
      validFollowingChars.UnionWith(CharUtils.AllCharsInRange('0', '9'));

      Head.SetTransition(validStartingChars, ValidName);
      ValidName.SetTransition(validFollowingChars, ValidName);
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

      Head.SetTransition('\'', CharStart);
      CharStart.SetTransition(CharUtils.AllCharsExcept(new char[] { '\\', '\'' }), CharProvided);
      CharStart.SetTransition('\\', EscapeSlash);

      EscapeSlash.SetTransition(simpleEscapeCharacters, CharProvided);

      CharProvided.SetTransition('\'', CharEnd);
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

      Head.SetTransition('"', StringStart);
      StringStart.SetTransition(CharUtils.AllCharsExcept(new char[] { '\\', '"' }), StringStart);
      StringStart.SetTransition('\\', EscapeSlash);
      StringStart.SetTransition('"', StringEnd);

      EscapeSlash.SetTransition(simpleEscapeCharacters, StringStart);
    }
  }

  internal class RegexToken : DFAToken {
    internal RegexToken(StreamPosition tokenStart) : base(tokenStart) {
      SimpleDFA<char>.Node Head = DFA.Head;
      SimpleDFA<char>.Node RegexStart = new SimpleDFA<char>.Node(false);
      SimpleDFA<char>.Node EscapeSlash = new SimpleDFA<char>.Node(false);
      SimpleDFA<char>.Node RegexEnd = new SimpleDFA<char>.Node(true);

      // Note: the / character is added here because it's used as the delimiter for regex. It's not
      // actually a system-wide escape character
      char[] simpleEscapeCharacters = new char[] {
        'a', 'b', 'f', 'n', 'r', 't', 'v', '\'', '"', '\\', '/'
      };

      Head.SetTransition('/', RegexStart);
      RegexStart.SetTransition(CharUtils.AllCharsExcept(new char[] { '\\', '/' }), RegexStart);
      RegexStart.SetTransition('\\', EscapeSlash);
      RegexStart.SetTransition('/', RegexEnd);

      EscapeSlash.SetTransition(simpleEscapeCharacters, RegexStart);
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

      head.SetTransition('`', script);
      script.SetTransition(CharUtils.AllCharsExcept(new char[] { '"', '\'', '[', '`' }), script);
      script.SetTransition('"', scriptString);
      script.SetTransition('\'', scriptChar);
      script.SetTransition('[', scriptParameter);

      // Strings in an Fnscript expression.
      scriptString.SetTransition(CharUtils.AllCharsExcept(new char[] { '"', '\\' }), scriptString);
      scriptString.SetTransition('\\', stringEscape);
      scriptString.SetTransition('"', script);
      stringEscape.SetTransition(CharUtils.AllChars(), scriptString);

      // Chars in an Fnscript expression.
      scriptChar.SetTransition(CharUtils.AllCharsExcept(new char[] { '\'', '\\' }), scriptChar);
      scriptChar.SetTransition('\\', charEscape);
      scriptChar.SetTransition('\'', script);
      charEscape.SetTransition(CharUtils.AllChars(), scriptChar);

      // FnScript Parameters.
      scriptParameter.SetTransition(CharUtils.AllCharsExcept(new char[] { ']' }), scriptParameter);
      scriptParameter.SetTransition(']', script);

      // Ending FnScript Expression.
      script.SetTransition('`', scriptEnd);
    }
  }

  internal class SingleLineCommentToken : DFAToken {
    internal SingleLineCommentToken(StreamPosition tokenStart) : base(tokenStart) {
      SimpleDFA<char>.Node head = DFA.Head;
      SimpleDFA<char>.Node comment = new SimpleDFA<char>.Node(true);

      head.SetTransition('#', comment);
      comment.SetTransition(CharUtils.AllCharsExcept(new char[] {'\n'}), comment);
    }
  }

  internal class MultiLineCommentToken : DFAToken {
    internal MultiLineCommentToken(StreamPosition tokenStart) : base(tokenStart) {
      SimpleDFA<char>.Node               head = DFA.Head;
      SimpleDFA<char>.Node      commentPrefix = new SimpleDFA<char>.Node(false);
      SimpleDFA<char>.Node            comment = new SimpleDFA<char>.Node(false);
      SimpleDFA<char>.Node commentSuffixStart = new SimpleDFA<char>.Node(false);
      SimpleDFA<char>.Node   commentSuffixEnd = new SimpleDFA<char>.Node(true);

      head.SetTransition('<', commentPrefix);
      commentPrefix.SetTransition('#', comment);

      comment.SetTransition(CharUtils.AllCharsExcept(new char[] { '#' }), comment);
      comment.SetTransition('#', commentSuffixStart);

      commentSuffixStart.SetTransition(CharUtils.AllCharsExcept(new char[] { '>' }), comment);
      commentSuffixStart.SetTransition('>', commentSuffixEnd);
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

  internal class ArrowToken : SingleMatchToken {
    internal ArrowToken(StreamPosition tokenStart) : base(tokenStart, "->") { }
  }

  internal class CommaSeparatorToken : SingleMatchToken {
    internal CommaSeparatorToken(StreamPosition tokenStart) : base(tokenStart, ",") { }
  }
}
