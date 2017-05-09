using System.Text;

namespace FSMLib.Compilation.Tokenizing
{
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

    private readonly char ValidChar;

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

  // TODO: Use reflection to make sure all of these are automatically added into Tokenizer.GenerateCompetingTokens

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
