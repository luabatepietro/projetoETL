module Program

open Expecto

[<EntryPoint>]
let main argv =
    runTestsWithCLIArgs [] argv TransformTests.allTests
