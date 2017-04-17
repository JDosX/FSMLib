using System;
namespace FSMLib.Compilation {
  internal class StreamPosition {

    #region Fields

    internal int ColumnNumber { get; private set; }
    internal int   LineNumber { get; private set; }

    #endregion

    internal StreamPosition() {
      ColumnNumber = 1;
      LineNumber = 1;
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
  }
}
