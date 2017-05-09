using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

namespace FSMLib.Compilation.Tokenizing
{
  internal class Tokenizer
  {
    #region Fields

    private const string COMMENT_START = "//";
    private const string TRANSITION_SEPARATOR = "->";
    private const char INITIAL_STATE = '+';
    private const char ACCEPTING_STATE = '*';

    private static readonly HashSet<char> WhiteSpaceChars = new HashSet<char>() {
      '\n', ' ', '\t'
    };

    #endregion

    /// <summary>
    /// Converts the incoming data from a stream into a tokenized representation.
    /// </summary>
    /// <param name="reader">Stream.</param>
    internal Token[] Tokenize(TextReader reader) {
      PositionedTextReader positionedReader = new PositionedTextReader(reader);
      List<Token> tokens = new List<Token>();

      while ((char)reader.Peek() != '\0')
      {
        Token token = NextToken(positionedReader);
        if (token == null) {
          // TODO: better handle this error detection
          throw new ArgumentException("Content of the reader ain't right yo.");
        } else {
          tokens.Add(token);
        }
      }

      return tokens.ToArray();
    }

    internal Token NextToken(PositionedTextReader reader) {
      SkipReading(reader, WhiteSpaceChars);

      StreamPosition tokenStart = new StreamPosition(reader.Position);
      HashSet<Token> competingTokens = GenerateCompetingTokens(tokenStart);

      while (!AllTokensDone(competingTokens)) {

        char c = (char)reader.Read();
        StreamPosition tokenEnd = new StreamPosition(reader.Position);

        foreach (Token token in competingTokens) {
          token.Feed(c, tokenEnd);
        }

        // Remove any tokens that are now in an invalid state
        competingTokens.RemoveWhere(checkedToken => checkedToken.State == Token.FeedState.Invalid);
      }

      return LongestToken(competingTokens);
    }

    private HashSet<Token> GenerateCompetingTokens(StreamPosition tokenStart) {
      return new HashSet<Token>() {
        new StartStateToken(tokenStart),
        new AcceptingStateToken(tokenStart),
        new ScopeOpenToken(tokenStart),
        new ScopeCloseToken(tokenStart),
        new KeywordFSMToken(tokenStart)
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

    private static void SkipReading(PositionedTextReader reader, HashSet<char> skipChars) {
      while (skipChars.Contains((char)reader.Peek())) {
        reader.Read();
      }
    }
  }
}
