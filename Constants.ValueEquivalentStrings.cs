// A part of the C# Language Syntactic Sugar suite.

namespace CLSS
{
  /// <summary>
  /// Provides access to a global cache of strings equalences to values of value
  /// types.
  /// </summary>
  /// <typeparam name="T">The value type to cache string equivalences.
  /// </typeparam>
  public partial class ValueEquivalentStrings<T> where T : struct
  {
    /// <summary>
    /// The backing <see cref="MemoizedFunc{T1, TResult}"/> that caches all
    /// equivalent strings.
    /// </summary>
    public static readonly MemoizedFunc<T, string> Memoizer
      = MemoizedFunc<T, string>.From(value => value.ToString());

    /// <summary>
    /// Generates and adds the default string equivalence of the specified value
    /// to the global cache of string equivalences if it does not already exist.
    /// Returns the generated default string equivalence, or the existing one if
    /// it exists.
    /// </summary>
    /// <param name="value">The value to use for looking up string equivalence.
    /// </param>
    /// <returns>The default string representation for the lookup value.
    /// </returns>
    public static string Get(T value) { return Memoizer.Invoke(value); }
  }

  public static class ValueEquivalentStringsExtension
  {
    /// <inheritdoc cref="ValueEquivalentStrings{T}.Get(T)"/>
    public static string ToCachedString<T>(this T value) where T : struct
    { return ValueEquivalentStrings<T>.Memoizer.Invoke(value); }
  }
}