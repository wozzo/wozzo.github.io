Title: There's fast code and there's fast code
Published: 24/5/2015
Tags:
- CSharp
- C#
- Math
- Timing
---
I was solving a problem recently and came across a number of different ways of solving it. I decided to test them each out to see which was the quickest. The functions purpose was to return the number of digits in an integer. I've tried four different methods out. Two of them are single line solutions, one uses a bit of looping to solve the problem and the final one... well you'll see.

# Log Method

Logarithms return the power a number needs to be raised to in order to give that number. For example if our number is 1000. Log to the base 10 will return 3, because 1000 = 10^3. Any number up to 10000 will return a decimal value which rounds down to 3. e.g. log10(9999) = 3.99956568.... so we can use a combination of Math.Floor and Math.Log10 to give us the number of digits required. Our function should look something like this.

```csharp
private int IntegerLength_Log(int num)
{
    return (int)Math.Floor(Math.Log10(num) + 1);
}
```
Note: You need to add 1 to the return of the log value to get the number of digits.

# String method

This method relies on built in methods of converting the integer to a string and using the length property. This is probably the simplest and most versatile technique (it should handle negatives), but I was also convinced that because of the overhead associated with converting the integer to a string that this would be the least efficient method.

The only code required is

```csharp
private int IntegerLength_String(int num)
{
    return num.ToString().Length;
}
```

# Looping method

This method loops, whilst increasing a variable's value by a power of 10 each cycle until that number is larger than the input.

```csharp
private int IntegerLength_Loop(int num)
{
    int length = 1;
    int comparator = 10;
    while (num > comparator) 
    {
        comparator *= 10;
        length++;
    }
    return length;
}
```

# if statements

The final method is easily the lengthiest to write, and also one that as a problem solving mathematical type makes me cringe a little. There's no elegance to this solution, just a bit of brute force and lots of repetitive code. It requires using a series of if statements to check the integers value and return the length when we know what number it is less than. This is effectively the same as the above method but without the loop.

```csharp
private int IntegerLength_If(int num)
{
    if (num < 10)
    {
        return 1;
    }
    else if (num < 100)
    {
        return 2;
    }
    else if (num < 1000)
    {
        return 3;
    }
    else if (num < 10000)
    {
        return 4;
    }
    else if (num < 100000)
    {
        return 5;
    }
    else if (num < 1000000)
    {
        return 6;
    }
    else if (num < 10000000)
    {
        return 7;
    }
    else if (num < 100000000)
    {
        return 8;
    }
    else if (num < 1000000000)
    {
        return 9;
    }
    else
    {
        return 10;
    }
}
```

If using a comparison like this on a BigInteger even more if statements would be required to cover up to the maximum value, but for C#'s int type this has us covered.

# Timing

Each of these functions was then run using a for loop varying the input from 1'000 to 10'000'000 and the number of ticks taken to complete was recorded.

The results are in: String method 2'937'886 ticks, Log Method 1'119'507, the Looping method 642'857 & the If method 437'401. The cringey set of if statements method is nearly 7 times faster than the string conversion method and 2.5 times faster than the log method.

I think if this were for production code I'd probably use the looping method after changing it to handle integers of any type (i.e. BigInteger) and negative values because of the versatility and ease of rewriting should any problems be found, but this had definitely made me rethink some the assumptions I've built up over the years about how quickly code will run in comparison to how many lines of code there are.