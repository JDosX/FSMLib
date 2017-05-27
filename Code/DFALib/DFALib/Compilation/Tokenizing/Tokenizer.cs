using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

namespace FSMLib.Compilation.Tokenizing
{
  internal class Tokenizer : TokenProvider
  {
    #region Fields

    private static readonly HashSet<char> WhiteSpaceChars = new HashSet<char>() {
      '\n', ' ', '\t'
    };

    private readonly BufferedTextReader Reader;

    #endregion

    internal Tokenizer(BufferedTextReader reader) {
      Reader = reader;
    }

    /// <summary>
    /// Converts the incoming data from a stream into a tokenized representation. Returns null once all the items in
    /// the stream have been read.
    /// </summary>
    protected override Token ProducerRead() {
      // If the stream is exhausted.
      // TODO: This might not be the best way to do it. If the stream results in null then you may have ended up with
      // invalid data. We need to think about how to gracefully structure exception throwing.
      if (Reader.Peek() == -1) {
        return null;
      }

      SkipReading(Reader, WhiteSpaceChars);

      StreamPosition tokenStart = new StreamPosition(Reader.Position);
      HashSet<Token> competingTokens = GenerateCompetingTokens(tokenStart);

      while (!AllTokensDone(competingTokens)) {

        char c = (char)Reader.Read();
        StreamPosition tokenEnd = new StreamPosition(Reader.Position);

        foreach (Token token in competingTokens) {
          token.Feed(c, tokenEnd);
        }

        // Remove any tokens that are now in an invalid state
        competingTokens.RemoveWhere(checkedToken => checkedToken.State == Token.FeedState.Invalid);
      }

      Token longestToken = LongestToken(competingTokens);

      // If we weren't able to find any valid tokens, this means we have a syntax error, throw
      if (longestToken == null) {
        // TODO: Make this error handling mechanism more robust/extensible.
        throw new ArgumentException(String.Format("Invalid syntax at {0}", tokenStart.ToString()));
      }

      Reader.FlushBuffer(longestToken.TokenEnd);

      return longestToken;
    }

    protected override Token ProducerPeek() {
      // Perform a read and then add it to the end of the buffer
      // without incrementing the current node. This way, in future
      // peeks, this value will just be read off the back without any
      // further reads.
      Token token = ProducerRead();
      StreamPosition position = AdvancePosition(token);
      ProducerBuffer.AddLast(new BufferItem(token, position));

      return token;
    }

    protected override StreamPosition AdvancePosition(Token nextStreamValue) {
      return new StreamPosition(nextStreamValue.TokenStart);
    }

    private HashSet<Token> GenerateCompetingTokens(StreamPosition tokenStart) {
      return new HashSet<Token>() {
        new StartStateToken(tokenStart),
        new AcceptingStateToken(tokenStart),
        new ScopeOpenToken(tokenStart),
        new ScopeCloseToken(tokenStart),
        new KeywordFSMToken(tokenStart),
        new ArrowToken(tokenStart),
        new SquareOpenToken(tokenStart),
        new SquareCloseToken(tokenStart),
        new CommaSeparatorToken(tokenStart),
        new StateNameToken(tokenStart),
        new StringToken(tokenStart),
        new CharToken(tokenStart),
        new FnScriptToken(tokenStart),
        new SingleLineCommentToken(tokenStart),
        new MultiLineCommentToken(tokenStart)
      };
    }

    private bool AllTokensDone(ICollection<Token> tokens) {
      bool allDone = true;

      foreach (Token token in tokens) {
        if (token.State != Token.FeedState.Done) {
          allDone = false;
          break;
        }
      }

      return allDone;
    }

    private Token LongestToken(ICollection<Token> tokens) {
      Token longestToken = null;

      foreach (Token token in tokens) {
        if (longestToken == null) {
          longestToken = token;
        } else if (token.Contents.Length > longestToken.Contents.Length) {
          longestToken = token;
        }
      }

      return longestToken;
    }

    private static void SkipReading(BufferedTextReader reader, HashSet<char> skipChars) {
      while (skipChars.Contains((char)reader.Peek())) {
        reader.Read();
      }

      reader.FlushBuffer();        
    }
  }
}
