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
    internal Token[] Tokenize(StreamReader reader) {
      List<Token> tokens = new List<Token>();

      string token = "";
      char c = '\0';

      while ((char)reader.Peek() != '\0')
      {
        
        c = (char)reader.Read();


      }

      return tokens.ToArray();
    }

    internal Token NextToken(StreamReader reader) {
      Token token;

      SkipReading(reader, WhiteSpaceChars);


      return token;
    }

    private static void SkipReading(TextReader reader, HashSet<char> skipChars) {
      while (skipChars.Contains((char)reader.Peek())) {
        reader.Read();
      }
    }
  }
}
