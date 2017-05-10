using System;
using System.IO;

using FSMLib;

namespace FSMLibUserTest {
  class MainClass {
    public static void Main(string[] args) {
      FSM<string> fsm = FSM<string>.FromReader(new StringReader("fsm+++***->->+fsm"));
    }
  }
}
