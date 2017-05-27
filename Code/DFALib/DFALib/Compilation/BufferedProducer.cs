using System;
using System.Collections.Generic;

namespace FSMLib.Compilation {
  public abstract class BufferedProducer<T> {

    private readonly LinkedList<BufferItem> ProducerBuffer;
    private LinkedListNode<BufferItem> CurrentBufferNode;

    public BufferedProducer() {
    }

    public T Read() {
      
    }
  }
}
