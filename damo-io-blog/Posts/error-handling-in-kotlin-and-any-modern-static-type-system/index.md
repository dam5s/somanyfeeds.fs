# Error handling in Kotlin and any modern static type system

This article is about leveraging a technique called **Railway Oriented
Programming**, in **Kotlin**. It is extensively documented in functional
languages,
[particularly well in the F# community.](https://fsharpforfunandprofit.com/rop/)
So I’m going to try to explain how to implement some of it in **Kotlin**.

## Some background

Having written my fair share of **Java**, **Go**, **Ruby**… I’ve seen quite a
few different paradigms for error handling.

**In Ruby**, one would return different types based on the error status of a
given function. We would (for example) use symbols to indicate the specific type
of error that was reached (e.g. `:not_found`, `:connection_failed`…).

It then becomes the responsibility of the consumer of the function to figure out
the different possible results. The programmer ends up having to read the tests
for the function, relying on the documentation being accurate, or reading
through the code of the function itself.

**In Java**, we use exceptions for indicating the function is not successful.
Non-`Runtime` exceptions have to be caught and handled, which ends up with a lot
of code like this -

```java
try {
    myFunction();
} catch (IOException e) {
    throw new RuntimeException(e);
}
```

And that obviously can result with a lot of run-time problems…

**In Go**, a function can have multiple return values. The convention is to
return an error and the value that is wanted. Then the programmer has to check
for the error before continuing the execution. This results in code similar to
this -

```go
value, err := myFunction()
if err != nil {
    return nil, err
}
```

Unfortunately, it can be very easy to forget to handle the error case. The
compiler will be just fine if we forget to look at the `err` variable or if we
don’t assign it. Also this has the unfortunate side effect of spreading the
“happy path” code in many small chunks of code separated by error checks.

## Enter discriminated unions

Although I am far from being an expert (really far), I have been toying around
with some functional languages lately. In particular, I like the simplicity of
**Elm** and **F#**, but I’m looking forward to learning more about advanced
functional programming with **Haskell** and/or **Scala** eventually.

Regardless, in all these languages there is the concept of a *discriminated
union*
in **F#** or *union type* in **Elm**. It allows the programmer to represent a
type that can have one or many states that can each have their own complexity.
Think about it as an “enum on steroids”! Actually **Rust** and **Swift** enums
are *union types*.

For example in **F#** you can create an `Optional` type like this -

```fsharp
type Optional<'a> =
    | Just of 'a
    | Nothing
```

That means that a value of type `Optional<string>` can either have the value
`Nothing` or it can be `Just` and would contain something of type `string`. This
`Optional` type is a great way of representing the potential absence of a value,
and it helps avoiding `null` pointer exceptions. Now you might say “but **
Kotlin**
already has a way to avoid null pointer exceptions built-in”, and you are right.
So let’s look at a type that is built-into **F#**.

```fsharp
type Result<'success, 'error> =
    | Success of 'success
    | Error of 'error
```

If I write a function that returns something of
type `Result<User, ErrorMessage>`
then I know that I will either get a `Success` back containing a `User` or I
will get an `Error` back and it contains an `ErrorMessage`. And the **F#**
compiler would ask me to handle both cases.

This is actually very similar to a type that you will find in most functional
languages, `Either`. It
exists [in Scala](https://www.scala-lang.org/api/2.12.0/scala/util/Either.html)
and [in Haskell.](https://hackage.haskell.org/package/base-4.11.1.0/docs/Data-Either.html)

And now you might say “but does Kotlin even have any way to do that at all?!”,
and you are in luck, because it does!

## Enter Kotlin’s sealed classes

The same type that we just represented in **F#** can be represented as follows
in **Kotlin**.

```kotlin
sealed class Result<T, E>

data class Success<T, E>(val value: T): Result<T, E>()

data class Error<T, E>(val value: E): Result<T, E>()
```

And you could use the type like this:

```kotlin
data class User(val name: String)
data class ErrorMessage(val message: String)

fun myFunction(): Result<User, ErrorMessage> =
    Error(ErrorMessage("Oops"))

when (val result = myFunction()) {
    is Success -> println("Success we got the user ${result.value.name}")
    is Error -> println("Oops we got a failure ${result.value.message}")
}
```

Now this is very basic but already usable, and the compiler will require that we
do match both cases: `Success` and `Error`. It may seem a bit tedious to always
have to match on result after calling the function. After all, the extra
boilerplate is why **Java** developers tend to use a lot of `RuntimeException`s
instead of having to catch or re-throw them all over the place.

So let’s add a few functions to the `Result` class to help handle it.

```kotlin
sealed class Result<T, E> {
    abstract fun <NewT> map(mapping: (T) -> NewT): Result<NewT, E>
    abstract fun <NewT> flatMap(mapping: (T) -> Result<NewT, E>): Result<NewT, E>
    abstract fun <NewE> mapFailure(mapping: (E) -> NewE): Result<T, NewE>
    abstract fun <NewE> flatMapFailure(mapping: (E) -> Result<T, NewE>): Result<T, NewE>
    abstract fun orElse(other: T): T
    abstract fun orElse(function: (E) -> T): T
}
```

The full implementation can be found
[as a Gist on my Github.](https://gist.github.com/dam5s/7fad877656fa891640c115688dbe0f5a)

With these functions you will be able to write code that handles errors very
simply and concisely. For example,

```kotlin
fun fetchUsers(): Result<List<User>, ErrorMessage> =
    buildRequest("GET", "http://example.com/api/users")
        .execute()
        .flatMap { it.parseJson<UserListJson>() }
        .map { it.users }
```

In this example, I executed an HTTP request using a function that returns a
`Result` then I parsed the response if the `Result` was a `Success`. The parsing
is also a function that returns a `Result` so I used `flatMap`. Finally I return
the list of `Users` from the parsed `UserListJson`.

At no point in that function did I have to handle the error branches (because my
functions are always using `ErrorMessage` for the failure case).

This makes for code that is **a lot easier to maintain**. The compiler is going
to do most of the heavy lifting for us.

This is [Railway Oriented Programming](https://fsharpforfunandprofit.com/rop/)
(I highly recommend reading that article).

I would encourage you to try and use this style of programming more and more if
you have the privilege of using a language that offers this kind of feature. If
you are using any external library that throws exceptions, make sure to write
some small wrapper functions that will instead return a `Result` type.

Enjoy your exception free codebase!
