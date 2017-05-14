using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

namespace FSMLib.Compilation.Tokenizing
{
  internal class Tokenizer
  {
    #region Fields

    private static readonly HashSet<char> WhiteSpaceChars = new HashSet<char>() {
      '\n', ' ', '\t'
    };

    #endregion

    /// <summary>
    /// Converts the incoming data from a stream into a tokenized representation.
    /// </summary>
    /// <param name="reader">Stream.</param>
    internal Token[] Tokenize(TextReader reader) {
      BufferedTextReader positionedReader = new BufferedTextReader(reader);
      List<Token> tokens = new List<Token>();
      
      while (positionedReader.Peek() != -1) {
        Token token = NextToken(positionedReader);
        tokens.Add(token);
      }

      return tokens.ToArray();
    }

    internal Token NextToken(BufferedTextReader reader) {
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

      Token longestToken = LongestToken(competingTokens);

      // If we weren't able to find any valid tokens, this means we have a syntax error, throw
      if (longestToken == null) {
        // TODO: Make this error handling mechanism more robust/extensible.
        throw new ArgumentException(String.Format("Invalid syntax at {0}", tokenStart.ToString()));
      }

      reader.FlushBuffer(longestToken.TokenEnd);

      return longestToken;
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
        new FnScriptToken(tokenStart)
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
