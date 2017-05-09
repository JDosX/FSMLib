using System;
using System.IO;
using System.Collections.Generic;

namespace FSMLib.Compilation {
  internal class PositionedTextReader {

    private readonly TextReader Reader;

    // TODO: implement buffer, as we read from the reader, buffer the input and perform a partial flush of the buffer
    // up to the specificed StreamPosition once a Token has successfully consumed a selection of the input. We need to do
    // this because some tokens can only know that they are done when they have seen the next character is not valid
    // (for instance, an int can only know it is done when it sees the next character is not a digit). This means that the
    // unconsumed token character is lost to the ether forever. Some tokenising will need an arbitrary number of lookahead
    // characters without wanting to destructively consume them if they are not needed as part of the valid input.

    // TODO: Also, refactor Tokens to be DFA only as we don't need the other types.

    internal StreamPosition Position { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="T:DFALib.PositionedStreamReader"/> class.
    /// </summary>
    /// <param name="reader">The StreamReader to wrap.</param>
    internal PositionedTextReader(TextReader reader) {
      Reader = reader;
      Position = new StreamPosition();
    }

    internal int Read() {
      int next = Reader.Read();
      Position.Advance((char)next);

      return next;
    }

    internal int Peek() {
      return Reader.Peek();
    }
  }
}
