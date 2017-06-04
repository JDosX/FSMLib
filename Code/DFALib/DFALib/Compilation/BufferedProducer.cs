using System;
using System.Collections.Generic;

namespace FSMLib.Compilation {
  public abstract class BufferedProducer<T> {

    protected readonly LinkedList<BufferItem> ProducerBuffer;
    protected LinkedListNode<BufferItem> CurrentBufferNode;

    public StreamPosition Position {
      get {
        return CurrentBufferNode.Value.Position;
      }
    }

    #region Abstract Fields
    protected abstract T ProducerRead();
    protected abstract T ProducerPeek();
    protected abstract StreamPosition AdvancePosition(T nextStreamValue);
    #endregion

    #region Virtual Fields
    protected virtual bool Keep(T next) {
      return true;
    }
    #endregion

    public BufferedProducer(T nullStart) {
      // TODO: Consider blocking spurious reads when EOF data starts coming through
      // The way it is right now, every time you do a read at EOF, another EOF token
      // will be added to the back, which is going to lead to memory leakage if something
      // continuously reads without flushing the buffer.
      ProducerBuffer = new LinkedList<BufferItem>();
      BufferItem BufferStart = new BufferItem(nullStart, new StreamPosition());
      ProducerBuffer.AddFirst(BufferStart);

      CurrentBufferNode = ProducerBuffer.First; // CurrentBufferNode == BufferStart.
    }


    public T Read() {
      if (CurrentBufferNode == ProducerBuffer.Last) {
        T nextStreamValue = ProducerRead();
        StreamPosition nextPosition = AdvancePosition(nextStreamValue);

        // Continue reading until we find a value we don't want to screen out.
        while (!Keep(nextStreamValue)) {
          nextStreamValue = ProducerRead();
          nextPosition = AdvancePosition(nextStreamValue);
        }

        BufferItem nextBufferEntry = new BufferItem(nextStreamValue, nextPosition);

        ProducerBuffer.AddLast(nextBufferEntry);
      }

      CurrentBufferNode = CurrentBufferNode.Next;
      return CurrentBufferNode.Value.Value;
    }

    public T Peek() {
      T peek;

      if (CurrentBufferNode == ProducerBuffer.Last) {
        peek = ProducerPeek();
      } else {
        peek = CurrentBufferNode.Next.Value.Value;
      }

      return peek;
    }

    /// <summary>
    /// Flushes the buffer up to the currently active Position of the BufferedTextReader (inclusive).
    /// </summary>
    public void FlushBuffer() {
      FlushBuffer(Position);
    }

    /// <summary>
    /// Flushes the buffer up to the specified StreamPosition (inclusive), and resets the head to the start of the buffer.
    /// TODO: always assumes input is valid. If invalid input is provided then shit will get fucked yo.
    /// </summary>
    /// <param name="position">Position.</param>
    public void FlushBuffer(StreamPosition position) {
      LinkedListNode<BufferItem> currentItem;

      currentItem = ProducerBuffer.First.Next;
      while (currentItem != null && currentItem.Value.Position.IsBeforeOrEqual(position)) {
        // Move the position of the first buffer node so it reflects the position before the first true buffered entry at
        // the end of this flushing process.
        LinkedListNode<BufferItem> nextItem = currentItem.Next;
        ProducerBuffer.First.Value.Position = currentItem.Value.Position;
        ProducerBuffer.Remove(currentItem);
        currentItem = nextItem;
      }

      // Reset the pointer into the buffer.
      CurrentBufferNode = ProducerBuffer.First;
    }

    protected class BufferItem {
      public T Value;
      public StreamPosition Position;

      public BufferItem(T value, StreamPosition position) {
        Value = value;
        Position = position;
      }
    }
  }
}
