using System;
using System.IO;

namespace FSMLib.Compilation {
  internal class PositionedTextReader {

    private readonly TextReader Reader;

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
