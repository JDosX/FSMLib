using System;
using System.IO;

using FSMLib;

namespace FSMLibUserTest {
  class MainClass {
    public static void Main(string[] args) {
      string fsm1 =
        "fsm FSM1 {" +
        "  +*State1 -> { 'c' -> State1, State2, State3 }" +
        "    State2 -> { 'd' -> State1 }" +
        "    State3 -> { 'e' -> State1 }" +
        "}";

      FSM<string> fsm = FSM<string>.FromReader(new StringReader(fsm1));
    }
  }
}
