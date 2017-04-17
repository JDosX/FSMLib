using System;
using System.IO;

namespace FSMLib.Compilation {
  internal class PositionedStreamReader {

    private readonly StreamReader Reader;

    private readonly StreamPosition Position;

    /// <summary>
    /// Initializes a new instance of the <see cref="T:DFALib.PositionedStreamReader"/> class.
    /// </summary>
    /// <param name="reader">The StreamReader to wrap.</param>
    internal PositionedStreamReader(StreamReader reader) {
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
