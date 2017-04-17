using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

namespace FSMLib.Factory
{
  internal static class FSMFactory
  {
    #region Fields

    private const string COMMENT_START = "//";

    private static readonly HashSet<char> WhiteSpaceChars = new HashSet<char>() {
      '\n', ' ', '\t'
    };

    #endregion

    internal static FSM<T> FromReader<T>(TextReader reader)
    {
      string token = "";
      char c = '\0';

      while ((char)reader.Peek() != '\0')
      {
        SkipReading(reader, WhiteSpaceChars);
        c = (char)reader.Read();

        // discard comments
        if (MatchAhead(reader, COMMENT_START))
        {

        }
      }

      // TODO: finish
      throw new NotImplementedException();
    }

    private static void SkipReading(TextReader reader, HashSet<char> skipChars)
    {
      while (skipChars.Contains((char)reader.Peek()))
      {
        reader.Read();
      }
    }

    private static bool MatchAhead(TextReader reader, string match)
    {
      bool retval = false;

      // TODO: check if the next characters in the reader match the specified string, and return true if so
      // might be better to iterate through a string instead of a streamreader, but a reader lets us accept
      // data through another source like console input

      return retval;
    }
  }
}
