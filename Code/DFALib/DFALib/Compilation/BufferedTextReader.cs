using System;
using System.IO;
using System.Collections.Generic;

namespace FSMLib.Compilation {
  internal class BufferedTextReader : BufferedProducer<int> {

    private readonly TextReader Reader;

    /// <summary>
    /// Initializes a new instance of the <see cref="T:DFALib.PositionedStreamReader"/> class.
    /// </summary>
    /// <param name="reader">The StreamReader to wrap.</param>
    internal BufferedTextReader(TextReader reader) : base(-1) {
      Reader = reader;
    }

    protected override int ProducerRead() {
      return Reader.Read();
    }

    protected override int ProducerPeek() {
      return Reader.Peek();
    }

    protected override StreamPosition AdvancePosition(int nextStreamValue) {
      return Position.NewAdvanced((char)nextStreamValue);
    }
  }
}
