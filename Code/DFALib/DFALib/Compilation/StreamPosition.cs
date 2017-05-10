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

    internal StreamPosition NewAdvanced (char c) {
      StreamPosition newPosition = new StreamPosition(this);
      newPosition.Advance(c);

      return newPosition;
    }

    /// <summary>
    /// Whether this StreamPosition is before or equal to the provided one.
    /// TODO: replace this with an overload of the <= operator when you have internet.
    /// </summary>
    /// <returns><c>true</c>, if or equal was befored, <c>false</c> otherwise.</returns>
    /// <param name="position">Position.</param>
    internal bool IsBeforeOrEqual(StreamPosition position) {
      return (
        LineNumber < position.LineNumber ||
        LineNumber == position.LineNumber && ColumnNumber <= position.ColumnNumber
      );
    }

    public override String ToString() {
      return string.Format("({0}, {1})", ColumnNumber, LineNumber);
    }
  }
}
