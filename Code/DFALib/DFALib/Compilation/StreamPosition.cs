using System;


namespace FSMLib.Compilation {
  internal class StreamPosition {

    #region Fields

    internal int ColumnNumber { get; private set; }
    internal int   LineNumber { get; private set; }

    #endregion

    /// <summary>
    /// Initializes a new instance of the <see cref="T:FSMLib.Compilation.StreamPosition"/> class at position (1, 1).
    /// </summary>
    internal StreamPosition() {
      ColumnNumber = 1;
      LineNumber = 1;
    }

    internal StreamPosition(StreamPosition other) {
      ColumnNumber = other.ColumnNumber;
      LineNumber = other.LineNumber;
    }

    // TODO: Write out documentation explaining why this works well, with reference to this example below:
    //
    // Text: abc col: 1
    // Consume Col
    // a       2
    // b       3
    // c       4
    // Error: col 1-4

    internal void Advance (char c) {
      if (c == '\n') {
        LineNumber++;
        ColumnNumber = 1;
      } else {
        ColumnNumber++;
      }
    }

    public override String ToString() {
      return string.Format("({0}, {1})", ColumnNumber, LineNumber);
    }
  }
}
