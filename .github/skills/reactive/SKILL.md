name: Rx.NET Reactive Programming

description: Designs and implements event-driven, asynchronous, and time-aware C# applications using Rx.NET. Transforms callbacks, events, and async workflows into composable observable pipelines with strong guarantees around concurrency, resource lifetime, error handling, and testability.

# Rx.NET Agent Skills

This document defines the skills and competencies of an agent specialized in **Reactive Extensions for .NET (Rx.NET)**, used for building responsive, event-driven, and asynchronous C# applications.

---

## Overview

**Rx.NET** enables composable, declarative handling of asynchronous and event-based data streams using observable sequences and LINQ-style operators.

This agent is capable of:
- Designing reactive architectures
- Writing idiomatic Rx.NET code
- Refactoring imperative async/event code into reactive pipelines
- Debugging, testing, and optimizing Rx-based systems

Primary reference repository:  
https://github.com/dotnet/reactive

---

## Core Competencies

### 1. Reactive Programming Fundamentals
- Understand the **Observable / Observer** model
- Differentiate between **IObservable<T>** and **IEnumerable<T>**
- Apply **push-based** vs **pull-based** data flow concepts
- Use **cold** vs **hot** observables appropriately

---

### 2. Creating Observables
- Create observables using:
  - `Observable.Create`
  - `Observable.Return`, `Throw`, `Empty`, `Never`
  - `Observable.Interval`, `Timer`
  - `FromEvent`, `FromEventPattern`
  - `FromAsync` and `ToObservable`
- Convert existing .NET async/event APIs into observables

---

### 3. Subscribing and Lifetime Management
- Subscribe using `Subscribe(...)`
- Handle `OnNext`, `OnError`, `OnCompleted`
- Dispose subscriptions correctly
- Use:
  - `CompositeDisposable`
  - `SerialDisposable`
  - `RefCount`
- Prevent memory leaks in long-running applications

---

### 4. LINQ Operators for Streams
- Transform streams:
  - `Select`, `SelectMany`
- Filter streams:
  - `Where`, `DistinctUntilChanged`, `Skip`, `Take`
- Aggregate streams:
  - `Scan`, `Aggregate`, `Buffer`, `Window`
- Combine streams:
  - `Merge`, `Concat`, `Zip`, `CombineLatest`, `WithLatestFrom`

---

### 5. Time-Based and Concurrency Operators
- Use schedulers:
  - `ImmediateScheduler`
  - `CurrentThreadScheduler`
  - `TaskPoolScheduler`
  - `NewThreadScheduler`
- Apply time-based operators:
  - `Throttle`, `Debounce`
  - `Delay`, `Timeout`
  - `Sample`
- Control concurrency using `ObserveOn` and `SubscribeOn`

---

### 6. Error Handling and Resilience
- Handle errors using:
  - `Catch`
  - `Retry`
  - `RetryWhen`
  - `OnErrorResumeNext`
- Design fault-tolerant reactive pipelines
- Avoid stream termination when appropriate

---

### 7. Hot Observables and Multicasting
- Use:
  - `Subject<T>`
  - `BehaviorSubject<T>`
  - `ReplaySubject<T>`
  - `AsyncSubject<T>`
- Apply multicasting operators:
  - `Publish`
  - `Replay`
  - `Share`
- Understand when and when **not** to use `Subject<T>`

---

### 8. State Management with Rx
- Maintain state using `Scan`
- Model application state as observable streams
- Build reactive view-models (MVVM-friendly)
- Coordinate UI state changes declaratively

---

### 9. Testing Rx.NET Code
- Use `TestScheduler`
- Write deterministic time-based tests
- Assert observable sequences using recorded notifications
- Test concurrency and edge cases reliably

---

### 10. Performance and Best Practices
- Avoid unnecessary subscriptions
- Minimize allocations in hot paths
- Choose correct schedulers for UI vs background work
- Prefer operators over manual `Subscribe` logic
- Keep pipelines readable and composable

---

## Practical Use Cases

- UI event handling (WPF, WinForms, MAUI)
- Real-time data processing
- Streaming APIs
- Sensor / IoT data pipelines
- Background job orchestration
- Debounced user input
- Reactive state containers

---

