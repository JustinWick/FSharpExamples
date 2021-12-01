module FSharpExamples.AsyncExamples

open System
open System.Threading
open System.Threading.Tasks


//async {
//    try
//        let! html = System.Uri("http://www.example.com/")  |> System.Net.Http.HttpClient().
//        System.IO.File.WriteAllText("example.html", html )
//    with
//        | ex -> printfn "%s" (ex.Message);
//}
//|> Async.RunSynchronously

//// DONE!
let runSynchronouslyExample() =
    // Prints "A", "B" immediately, then "C", "D" in 1 second
    printfn "A"

    async {
        printfn "B"
        do! Async.Sleep(1000)
        printfn "C"
    } |> Async.RunSynchronously

    printfn "D"


//// DONE!
let startExample() =
    // Prints "A", "D" immediately, then "B" quickly, and then "C" in 1 second.
    printfn "A"

    async {
        printfn "B"
        do! Async.Sleep(1000)
        printfn "C"
    } |> Async.Start

    printfn "D"



//// DONE!
let startAsTaskExample() =
    // Prints "A", "D" immediately, then "B" quickly, then "C", "E" in 1 second
    printfn "A"

    let t =
        async {
            printfn "B"
            do! Async.Sleep(1000)
            printfn "C"
        } |> Async.StartAsTask

    printfn "D"
    t.Wait()
    printfn "E"


//// DONE!
let startImmediateExample() =
    // Prints "A", "B", "D" immediately, then "C" in 1 second
    printfn "A"

    async {
        printfn "B"
        do! Async.Sleep(1000)
        printfn "C"
    } |> Async.StartImmediate

    printfn "D"


//// DONE!
let startImmediateAsTaskExample() =
    // Prints "A", "B", "D" immediately, then "C", "E" in 1 second
    printfn "A"

    let t =
        async {
            printfn "B"
            do! Async.Sleep(1000)
            printfn "C"
        } |> Async.StartImmediateAsTask

    printfn "D"
    t.Wait()
    printfn "E"


//// DONE!
let choiceExample1 () =
    printfn "Starting"
    // Prints a randomly selected odd number in 1-2 seconds, never 2
    // If the list is changed to all even numbers, it will instead print "No Result"
    [ 2; 3; 5; 7 ]
    |> List.map
        (fun i ->
            async {
                do! Async.Sleep(System.Random().Next(1000, 2000))
                return if i % 2 > 0 then Some(i) else None
            })
    |> Async.Choice
    |> Async.RunSynchronously
    |> function
        | Some (i) -> printfn $"{i}"
        | None -> printfn "No Result"

//// DONE!
let choiceExample2 () =
    // <<<JUSTIN NOTE: The docs on this function don't quite match behavior--the docs make me think that the exception is always thrown, but child computations are apparently cancelled when the first computation returns

    // Will sometimes print a randomly selected odd number, sometimes throw System.Exception("Even numbers not supported: 2")
    [ 2; 3; 5; 7 ]
    |> List.map
        (fun i ->
            async {
                do! Async.Sleep(System.Random().Next(1000, 2000))

                return
                    if i % 2 > 0 then
                        Some(i)
                    else
                        failwith $"Even numbers not supported: {i}"
            })
    |> Async.Choice
    |> Async.RunSynchronously
    |> function
        | Some (i) -> printfn $"{i}"
        | None -> printfn "No Result"


let someRiskyBusiness () =
    match System.Random().Next(1, 2) with
    | 1 -> 1
    | _ -> failwith "No, not The One"

//// DONE!
let fromContinuationsExample() =
    //
    // This anonymous function will call someRiskyBusiness() and be a good citizen with regards to the continuations
    // defined to report the outcome.
    //
    let computation =
        (fun (successCont, exceptionCont, cancellationCont) ->
            try
                someRiskyBusiness () |> successCont
            with
            | :? OperationCanceledException as oce -> cancellationCont oce
            | e -> exceptionCont e)
        |> Async.FromContinuations

    Async.StartWithContinuations(
        computation,
        (fun result -> printfn $"Result: {result}"),
        (fun e -> printfn $"Exception: {e}"),
        (fun oce -> printfn $"Cancelled: {oce}")
    )

//// DONE!
let ignoreExample() =
    // <<<JW NOTE>>> - Is this any good???

    // From https://github.com/MicrosoftDocs/visualfsharpdocs/blob/master/docs/conceptual/snippets/fsasyncapis/snippet34.fs

    // Reads bytes from a given file asynchronously and then ignores the result, allowing the do! to be used with functions
    // that return an unwanted value.
    let readFile filename numBytes =
        async {
            use file = System.IO.File.OpenRead(filename)
            printfn "Reading from file %s." filename
            // Throw away the data being read.
            do! file.AsyncRead(numBytes) |> Async.Ignore
        }
    readFile "example.txt" 42 |> Async.Start
    
//// DONE!
let parallelExample1 () =
    // This will print "3", "5", "7", "11" (in any order) in 1-2 seconds and then
    // [| false; true; true; true; false; true |]
    let t =
        [ 2; 3; 5; 7; 10; 11 ]
        |> List.map
            (fun i ->
                async {
                    do! Async.Sleep(System.Random().Next(1000, 2000))

                    if i % 2 > 0 then
                        printfn $"{i}"
                        return true
                    else
                        return false
                })
        |> Async.Parallel
        |> Async.StartAsTask

    t.Wait()
    printfn $"%A{t.Result}"
    
//// DONE!
let parallelExample2 () =
    // This will print "3", "5" (in any order) in 1-2 seconds, and then "7", "11" (in any order) in 1-2 more seconds and then
    // [| false; true; true; true; false; true |]
    let computations =
        [ 2; 3; 5; 7; 10; 11 ]
        |> List.map
            (fun i ->
                async {
                    do! Async.Sleep(System.Random().Next(1000, 2000))

                    return
                        if i % 2 > 0 then
                            printfn $"{i}"
                            true
                        else
                            false
                })

    let t =
        Async.Parallel(computations, 3)
        |> Async.StartAsTask

    t.Wait()
    printfn $"%A{t.Result}"

//// DONE!
let sequentialExample() =
    // This will print "3", "5", "7", "11" with ~1-2 seconds between them except for pauses where even numbers would be
    // and then prints [| false; true; true; true; false; true |]
    let computations =
        [ 2; 3; 5; 7; 10; 11 ]
        |> List.map
            (fun i ->
                async {
                    do! Async.Sleep(System.Random().Next(1000, 2000))

                    if i % 2 > 0 then
                        printfn $"{i}"
                        return true
                    else
                        return false
                })

    let t =
        Async.Sequential(computations)
        |> Async.StartAsTask

    t.Wait()
    printfn $"%A{t.Result}"

//// DONE!
let sleepExample1 () =
    // Prints "C", then "A" quickly, and then "B" 1 second later
    async {
        printfn "A"
        do! Async.Sleep(1000)
        printfn "B"
    } |> Async.Start

    printfn "C"

//// DONE!
let sleepExample2 () =
    // Prints "C", then "A" quickly, and then "B" 1 second later
    async {
        printfn "A"
        do! Async.Sleep(TimeSpan(0, 0, 1))
        printfn "B"
    } |> Async.Start
    printfn "C"


//// DONE!
let cancelDefaultTokenExample() =
    // This will print "2" 2 seconds from start, "3" 3 seconds from start, "5" 5 seconds from start, cease computation
    // and then print "Tasks Not Finished: One or more errors occurred. (A task was canceled.)".
    try
        let computations =
            [ 2; 3; 5; 7; 11 ]
            |> List.map
                (fun i ->
                    async {
                        do! Async.Sleep(i * 1000)
                        printfn $"{i}"
                    })

        let t =
            Async.Parallel(computations, 3)
            |> Async.StartAsTask

        Thread.Sleep(6000)
        Async.CancelDefaultToken()
        printfn $"Tasks Finished: %A{t.Result}"
    with
    | :? System.AggregateException as ae -> printfn $"Tasks Not Finished: {ae.Message}"

//// DONE!
let catchExample() =
    // Prints the returned value of the computation or the exception if there is one
    async { return someRiskyBusiness() }
    |> Async.Catch
    |> Async.RunSynchronously
    |> function
        | Choice1Of2 result -> printfn $"Result: {result}"
        | Choice2Of2 e -> printfn $"Exception: {e}"

//// DONE!
let defaultCancellationTokenExample() =
    // This will print "2" 2 seconds from start, "3" 3 seconds from start, "5" 5 seconds from start, cease computation
    // and then print "Computation Cancelled", followed by "Tasks Finished".
    Async.DefaultCancellationToken.Register(fun () -> printfn "Computation Cancelled") |> ignore
    [ 2; 3; 5; 7; 11 ]
    |> List.map
        (fun i ->
            async {
                do! Async.Sleep(i * 1000)
                printfn $"{i}"
            })
    |> List.iter Async.Start

    Thread.Sleep(6000)
    Async.CancelDefaultToken()
    printfn "Tasks Finished"

//// DONE!
let onCancelExample() =
    // This will print "2" 2 seconds from start, "3" 3 seconds from start, "5" 5 seconds from start, cease computation
    // and then print "Computation Cancelled: 7", "Computation Cancelled: 11" and "Tasks Finished" in any order.
    [ 2; 3; 5; 7; 11 ]
    |> List.iter
        (fun i ->
            async {
                use! holder = Async.OnCancel(fun () -> printfn $"Computation Cancelled: {i}")
                do! Async.Sleep(i * 1000)
                printfn $"{i}"
            }
            |> Async.Start)

    Thread.Sleep(6000)
    Async.CancelDefaultToken()
    printfn "Tasks Finished"

//// DONE!
let startChildExample() =
    // Will throw a System.TimeoutException if called with a timeout < 2000, otherwise will print "Result: 3"
    let computeWithTimeout timeout =
        async {
            let! completor1 =
                Async.StartChild(
                    (async {
                        do! Async.Sleep(1000)
                        return 1
                     }),
                    millisecondsTimeout = timeout
                )

            let! completor2 =
                Async.StartChild(
                    (async {
                        do! Async.Sleep(2000)
                        return 2
                     }),
                    millisecondsTimeout = timeout
                )

            let! v1 = completor1
            let! v2 = completor2
            printfn $"Result: {v1 + v2}"
        } |> Async.RunSynchronously

    // The below is test code not example code
    computeWithTimeout 2000
    computeWithTimeout 1000

//// DONE!
let tryCancelledExample() =
    // This will print "2" 2 seconds from start, "3" 3 seconds from start, "5" 5 seconds from start, cease computation
    // and then print "Computation Cancelled: 7", "Computation Cancelled: 11" and "Tasks Finished" in any order.
    [ 2; 3; 5; 7; 11 ]
    |> List.map
        (fun i ->
            Async.TryCancelled(
                async {
                    do! Async.Sleep(i * 1000)
                    printfn $"{i}"
                },
                fun oce -> printfn $"Computation Cancelled: {i}"
            ))
    |> List.iter Async.Start

    Thread.Sleep(6000)
    Async.CancelDefaultToken()
    printfn "Tasks Finished"

let someLongRunningComputation () = async { Thread.Sleep(5000) }

let someShortRunningComputation () = async { Thread.Sleep(1) }

//// DONE!
let switchToNewThreadExample() =
    // This will run someLongRunningComputation() without blocking the threads in the threadpool
    async {
        do! Async.SwitchToNewThread()
        do! someLongRunningComputation()
    } |> Async.StartImmediate

//// DONE!
let switchToThreadPoolExample() =
    // This will run someLongRunningComputation() without blocking the threads in the threadpool, and then switch to the
    // threadpool for shorter computations.
    async {
        do! Async.SwitchToNewThread()
        do! someLongRunningComputation()
        do! Async.SwitchToThreadPool()

        for i in 1 .. 10 do
            do! someShortRunningComputation()
    } |> Async.StartImmediate



// MISSING:
//
// StartChildAsTask
// AwaitEvent
// AwaitIAsyncResult
// AwaitTask
// AwaitTask
// AwaitWaitHandle
// CancellationToken
// SwitchToContext - This seems mostly useful for GUI-specific code, do we really need it here? Maybe show it generically using
//                   let ctx = SynchronizationContext.Current ; do! Async.SwitchToThreadPool() ; something() ; do! Async.SwitchToContext(ctx)
//                   ???
// OnCancel
// 


// DELETEME
let sumOfSquares1 l : float =
    l |> Seq.map (fun x -> x * x) |> Seq.sum

let sumOfSquares : seq<float> -> float = (Seq.map (fun x -> x * x)) >> Seq.sum
