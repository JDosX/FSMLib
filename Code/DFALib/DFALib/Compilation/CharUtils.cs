using System;
using System.Collections.Generic;

namespace FSMLib.Compilation {
  internal static class CharUtils {
    /// <summary>
    /// All the chars in the range [0, 127] (inclusive).
    /// </summary>
    /// <returns>The chars.</returns>
    public static HashSet<char> AllChars() {
      HashSet<char> allChars = new HashSet<char>();

      for (char c = (char)0; c <= (char)127; c++) {
        allChars.Add(c);
      }

      return allChars;
    }

    public static HashSet<char> AllCharsExcept(char exclusion) {
      return AllCharsExcept(new char[] { exclusion });
    }

    public static HashSet<char> AllCharsExcept(ICollection<char> exclusions) {
      HashSet<char> allChars = AllChars();

      foreach (char exclusion in exclusions) {
        allChars.Remove(exclusion);
      }

      return allChars;
    }

    public static HashSet<char> AllCharsInRange(char min, char max) {
      HashSet<char> allChars = new HashSet<char>();

      for (char c = min; c <= max; c++) {
        allChars.Add(c);
      }

      return allChars;
    }
  }
}
