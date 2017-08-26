Title: Closures in C#
Published: 28/5/2015
Tags:
- CSharp
- C#
- Closures
- JavaScript
- Prime numbers
- Euler Problems
RedirectFrom: closures-in-c
---
Closures are a vital part of any language that passes functions around as variables. They ensure that everything that was available when we defined the function is available when it's called later, even if it's called in a different scope or long after the original scope has completely ended. There's a vast amount of information available on closures in Javascript because as a classless language where functions are always first class objects closures are regularly used to imitate class like functionality.

For example:

```javascript
function fakeClass() {
    var publicVar = "This variable will be public";
    this.publicVar = publicVar;
  
    var privateVar = "This variable will be private";
    this.getPrivateVar = function() {
        return privateVar;
    }
}

var instance = new fakeClass();
 
// This will output "This variable will be public"
console.log(instance.publicVar);
// This will output "This variable will be private"
console.log(instance.getPrivateVar());
// This will throw a type undefined error
console.log(instance.privateVar);
```
In the above code the publicVar variable is attached to the this object and can be set and read through accessing fakeClass.publicVar. However, the privateVar variable cannot be accessed since it has not been attached to the this object. That is where the getPrivateVar function comes in. The privateVar variable was available at the time the function was defined and so it will be available when the getPrivateVar function is called. The privateVar itself is no longer in scope at the bottom of the example and since it wasn't attached to the this object when we try to access it, we'll get an error.

This is just an example of how closures can be used and what they do. In C# we obviously have far better ways of declaring private variables, namely the private keyword. That's not to say that closures are suddenly useless in C#, in fact any time we want a function to have data that persists between uses we can use a closure to do so. The alternative would be to create a class that contains properties for all of the data we want to use, but closures are much briefer in terms of how much code is needed.

# Prime Generator

For the Project Euler problems I realised I was needing to copy code between problems and one of the things I was regularly doing was generating prime numbers. Doing so with a normal function would require that I knew how many primes I needed, and could then return an array with all of the primes up to that point. .Net's IEnumerable achieves something very similar to this; closures are just another tool in the box. I like how simple they are to write and use, but sometimes we may need the more advanced functionality of the IEnumerable.
The pseudo code for generating prime numbers looks something like the following, where isPrime(num) is a function which returns a boolean telling us if the num variable passed is a prime number or not (more notes on that at the end).

```
private int getPrime() {
    int current = 2;
    while (true) {
        if (isPrime(current)) {
            return current;
        }
        current++;
    }
}
```
You've possibly already spotted the problem. As soon as a prime is found, which is immediately since we start with a prime, the function will return and the current variable will never get incremented. The next time the function is called the current value will once again be set as 2 and will be returned immediately. Enter the closure.

What if we could put the while loop into a separate function that incremented the current value? We first need to change the above function so that it returns a delegate function. Making a function a delegate means it can be passed around like a variable in the same way as all functions can be in javascript.

The function declaration needs to be changed to `private Func\<int\> getPrimeGenerator()`. This tells the compiler that we will be expecting the return from this function to be a delegate function which itself has a return type of int. Next we declare and initialise the variable that stores our current position, then we declare the function that will return our primes.

```csharp
private Func<int> getPrimes()
{
    int current = 1;
    Func<int> nextPrime = delegate()
    {
        current++;
        while (!isPrime(current))
        {
            current++;
        }
        return current;
    };
    return nextPrime;
}
```
The pseudo code has been modified slightly so that the if statement has been implemented with the while loop, but the functionality isn't any different. Each time the delegate function is called it will increment the current variable until a new prime is found and return that. A reference to the current variable has been closed into the delegate function. Let's see how this is used to get the first hundred prime numbers.

```csharp
var getNextPrime = getPrimes();
for (int i = 0; i < 100; i++) {
    Console.WriteLine(getNextPrime());
}
```
That's it. We call the first function and store the returned delegate in another variable, then we call that as many times as we like, each time it will return the next prime number that can be found. Eventually this code will hit the maximum value for an integer and will go back to the beginning, but dealing with that is left as an exercise for the reader.