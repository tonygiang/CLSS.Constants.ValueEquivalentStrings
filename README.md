# CLSS.Constants.ValueEquivalentStrings

### Problem

Among all value types in C#, most of them have equivalent default string representation (as returned by [`ToString()`](https://docs.microsoft.com/en-us/dotnet/api/system.object.tostring?view=net-6.0)) if they have equivalent values. This is always the case for [built-in value types](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/built-in-types), this is always the case for `enum` types, this is always the case for every struct types that can be found in the BCL. The only exception is custom struct types that have custom `ToString` override and equality conditions that don't align with each other. Those custom structs would create different string content for 2 struct instances that satisfy custom equality conditions.

Since `string` is a reference type, each `ToString` invocation always creates an allocation, but since strings are also immutable, there is no real benefit from 2 strings identical in content allocated on 2 different places on the heap. Such a scenario only creates memory inefficiency as well as garbage for the GC to clean up.

```
string Method1(int value) { return value.ToString(); }
string Method2(int value) { return value.ToString(); }

// These both create "4" strings on the heap at different places in a program
var res1 = Method1(4);
var res2 = Method2(4);
```

### Solution

This package provides the `ValueEquivalentStrings<T>` class that contains a static, global cache of strings equivalent to value types. The first time it converts a value to a default string (meaning a `ToString` conversion with no additional format or format provider), it caches that string so that all subsequent conversions simply return that existing string.

```
int num1 = 4, num2 = 4;
string str1 = ValueEquivalentStrings<int>.Get(num1);
string str2 = ValueEquivalentStrings<int>.Get(num2); // no string allocation for this call
```

`ValueEquivalentStrings` accepts all value types in its type parameter, including custom struct types. If you want to cache string equivalences to custom struct types, make sure to align their `ToString` and `Equals` override implementations.

```
using CLSS;

public struct City
{
  public NameEnum Name;
  public CountryEnum Country;
  public static bool operator ==(City lhs, City rhs) => lhs.Equals(rhs);
  public static bool operator !=(City lhs, City rhs) => !lhs.Equals(rhs);
  public override string ToString() => $"(Name = {Name}, Country = {Country})";
  public override bool Equals(object obj)
  {
    if (obj is City otherCity) return this.Name == otherCity.Name;
    return false;
  }
}

// These 2 instances are treated as
// equivalent keys in the Dictionary backing ValueEquivalentStrings.
var paris1 = new City { Name = NameEnum.Paris, Country = CountryEnum.France };
var paris2 = new City { Name = NameEnum.Paris, Country = CountryEnum.USA };
var str1 = ValueEquivalentStrings<City>.Get(paris1);
var str2 = ValueEquivalentStrings<City>.Get(paris2); // returns "(Name = Paris, Country = France)"
```

At the heart of `ValueEquivalentStrings<T>` is a [`MemoizedFunc`](https://www.nuget.org/packages/CLSS.Types.MemoizedFunc) - another CLSS type and the only dependency of this package. This memoizer is also publicly exposed should you need to manipulate it directly for some reason.

```
// Invoking the memoizer directly does the same thing as Get, but longer syntax.
char charW = ValueEquivalentStrings<char>.Memoizer.Invoke('W');
// Clears the cache to free up some memory
ValueEquivalentStrings<int>.Memoizer.MemoizedResults.Clear();
```

This package also comes with the `ToCachedString` extension method for all value types as an alternative shorter, more functional-style syntax to `Get`. The same caution for custom struct types applies to this syntax.

```
// String.Join does not accept char as a separator type in pre-2.1 versions of .NET Standard
var commaSeparatedLine = String.Join(','.ToCachedString(), rowValues);
```

**Note**: Since `ValueEquivalentStrings<T>` is backed by a dictionary, all optimization rules to avoid boxing allocation for a dictionary key type - such as explicit underlying type for enum and implementing [`IEquatable<T>`](https://docs.microsoft.com/en-us/dotnet/api/system.iequatable-1?view=net-6.0) for struct - also apply to the type parameter of `ValueEquivalentStrings<T>`.

##### This package is a part of the [C# Language Syntactic Sugar suite](https://github.com/tonygiang/CLSS).