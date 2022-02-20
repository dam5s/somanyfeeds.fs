# Things to learn in React and Redux

There is a lot of "tutorials" out there teaching React and Redux. Most of them
cover a lot more than what is actually useful for building production applications.
Unfortunately this makes it really hard for beginners to understand how much they
actually need to learn in order to be efficient.

Having built a fair amount of frontend applications using React/Redux
or an equivalent, [on iOS](https://github.com/ReSwift/ReSwift) for example,
or [with Elm](https://elm-lang.org), [with F#](https://github.com/elmish/elmish)...
I thought I'd share what I found to be the fundamentals of building frontend applications
with React and using Redux's Unidirectional Dataflow architecture.

## React

In React, there are only a few things needed to build full-fledged 
production applications. Like for any other frontend applications,
you'll need to be able to manage state, load state from remote APIs
asynchronously, and have access to state and configuration in deep
nested object structures.

Most of these React features can be used in 
[functional components](https://reactjs.org/docs/components-and-props.html)
via the use of [hooks](https://reactjs.org/docs/hooks-intro.html).

1. [useState](https://reactjs.org/docs/hooks-state.html)

    This function lets you create a state holder and a way to modify state while
    automatically updating any view components using that state.

1. [useEffect](https://reactjs.org/docs/hooks-effect.html)

    This React hook lets you trigger side effects.
    A typical example of a side effect is loading remote data asynchronously
    then updating state.
    
    * The `useEffect` function lets you pass-in dependencies
    that would trigger the effect again if they changed.
      
    * The function passed to `useEffect` can also return a function that 
    will be called when the view component is removed/dismounted. 
    This is useful for ensuring asynchronous side effects no longer happen
    when the state is no longer available.

1. [createContext/useContext](https://reactjs.org/docs/context.html)

    React's `context` is designed to help inject values in deeply nested view
    component trees. It is convenient for injecting configuration or shared stated. 
    If you are going to use it for storing state, you might instead consider using
    `Redux`.

1. [React testing library](https://testing-library.com/docs/react-testing-library/intro/)

    You can't write React without using its testing library. If you don't write tests
    for your React applications, it's time you change that bad habit. When combined with
    the [Jest](https://jestjs.io) test runner, it is the fastest and simplest way to test
    your view components. While it does not remove entirely the need for end-to-end testing,
    with something like [Cypress](https://www.cypress.io) for example, it is worth learning
    and having thorough of your components.

## Redux

When your application starts growing, the ability to share state and update it across
components will become more and more important.

While this is all possible using the standard React hooks for state and context management,
it will become increasingly difficult to maintain over time if multiplying contexts and
passing down state and state setters to child components.

[Redux](https://redux.js.org) provides a unified and central place for managing shared state.
It was designed after the [Elm architecture](https://guide.elm-lang.org/architecture/).

1. [State](https://redux.js.org/tutorials/fundamentals/part-3-state-actions-reducers)

    Redux is a tool for managing state. The first thing to define is the shape of that state.
    In Typescript, one would define an `Interface` or a `Type` describing it.
   
    For example:
    ```
    interface ApplicationState {
        session: SessionState
        todoItems: RemoteData<Todo[]>
    }
    ```

1. [Action](https://redux.js.org/tutorials/fundamentals/part-3-state-actions-reducers)

    Actions are any javascript with a type field that's a string. During the runtime
    of the application, Actions can be triggered to modify the state.

    For example:
    ```
    type ApplicationAction =
        | {type: 'session/sign in', user: User}
        | {type: 'session/sign out'}
        | {type: 'todo/load', todos: Todo[]}
        | {type: 'todo/add', todos: Todo}
    ```

1. [Reducer](https://redux.js.org/tutorials/fundamentals/part-3-state-actions-reducers)

    The reducer is the function that will take the current state, an action and produce
    a new state.

    For example:
    ```
    const reducer = (state: ApplicationState|undefined = initialState, action: AnyAction): ApplicationState => {
        if (!isApplicationAction(action)) {
            return state;
        }

        switch (action.type) {
            //...
        }
    }
    ```

1. [Store](https://redux.js.org/tutorials/fundamentals/part-4-store)

    All three of the above are combined into a `Store`. The store's purpose is:
    * Maintaining the state
    * Updating it using the reducer when an action is triggered 
    * Allowing subscription to state changes

## React + Redux

1. [Provider](https://redux.js.org/tutorials/fundamentals/part-5-ui-react#passing-the-store-with-provider)

    The `Provider` is a react context provider that holds a reference to the Redux store.
    It is used by the two following hooks.

1. [useSelector](https://redux.js.org/tutorials/fundamentals/part-5-ui-react#reading-state-from-the-store-with-useselector)

    `useSelector` is a React hook that allows subscribing to a part of the state.
    It takes a transform function that takes the Application State and returns the part
    of the state your component needs to render.

1. [useDispatch](https://redux.js.org/tutorials/fundamentals/part-5-ui-react#reading-state-from-the-store-with-useselector)

    `useDispatch` returns a `dispatch` function. This function can be invoked in order to
    trigger an `Action` on the Application Store.

That's a wrap!

While there are plenty other things you could try to learn about React and Redux
and the latest fancy library people use with them, the items above are all
you will need to build a production quality application.

I do recommend using Typescript and test driving your application,
but I cannot recommend pulling in external libraries to "solve" your development problems.

Additional libraries will tend to introduce more complexity,
more time for new comers to understand and learn how your codebase works,
more transitive dependencies that can introduce security issues...

Good luck out there.
