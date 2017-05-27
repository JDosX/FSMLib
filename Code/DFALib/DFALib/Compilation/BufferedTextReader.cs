using System;
using System.IO;
using System.Collections.Generic;

namespace FSMLib.Compilation {
  internal class BufferedTextReader {

    private readonly TextReader Reader;

    private readonly LinkedList<BufferItem> StreamBuffer;
    private LinkedListNode<BufferItem> CurrentBufferNode;

    internal StreamPosition Position {
      get {
        return CurrentBufferNode.Value.Position;
      }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="T:DFALib.PositionedStreamReader"/> class.
    /// </summary>
    /// <param name="reader">The StreamReader to wrap.</param>
    internal BufferedTextReader(TextReader reader) {
      Reader = reader;

      StreamBuffer = new LinkedList<BufferItem>();
      BufferItem BufferStart = new BufferItem(-1, new StreamPosition());
      StreamBuffer.AddFirst(BufferStart);

      CurrentBufferNode = StreamBuffer.First; // CurrentBufferNode == BufferStart.
    }

    internal int Read() {
      if (CurrentBufferNode == StreamBuffer.Last) {
        int nextStreamValue = Reader.Read();
        StreamPosition nextPosition = Position.NewAdvanced((char)nextStreamValue);
        BufferItem nextBufferEntry = new BufferItem(nextStreamValue, nextPosition);

        StreamBuffer.AddLast(nextBufferEntry);
      }

      CurrentBufferNode = CurrentBufferNode.Next;
      return CurrentBufferNode.Value.StreamValue;
    }

    /// <summary>
    /// Flushes the buffer up to the currently active Position of the BufferedTextReader (inclusive).
    /// </summary>
    internal void FlushBuffer() {
      FlushBuffer(Position);
    }

    /// <summary>
    /// Flushes the buffer up to the specified StreamPosition (inclusive), and resets the head to the start of the buffer.
    /// TODO: always assumes input is valid. If invalid input is provided then shit will get fucked yo.
    /// </summary>
    /// <param name="position">Position.</param>
    internal void FlushBuffer(StreamPosition position) {
      LinkedListNode<BufferItem> currentItem;

      currentItem = StreamBuffer.First.Next;
      while (currentItem != null && currentItem.Value.Position.IsBeforeOrEqual(position)) {
        // Move the position of the first buffer node so it reflects the position before the first true buffered entry at
        // the end of this flushing process.
        LinkedListNode<BufferItem> nextItem = currentItem.Next;
        StreamBuffer.First.Value.Position = currentItem.Value.Position;
        StreamBuffer.Remove(currentItem);
        currentItem = nextItem;
      }

      // Reset the pointer into the buffer.
      CurrentBufferNode = StreamBuffer.First;
    }

    internal int Peek() {
      int peek;

      if (CurrentBufferNode == StreamBuffer.Last) {
        peek = Reader.Peek();
      } else {
        peek = CurrentBufferNode.Next.Value.StreamValue;
      }

      return peek;
    }

    private class BufferItem {
      internal int StreamValue;
      internal StreamPosition Position;

      internal BufferItem(int streamValue, StreamPosition position) {
        StreamValue = streamValue;
        Position = position;
      }
    }
  }
}
