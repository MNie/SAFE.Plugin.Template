module ExpectoTemplate

open Expecto

let config =
    let writeResults = TestResults.writeNUnitSummary ("TestResults.xml", "SAFE Plugin Tests")
    defaultConfig.appendSummaryHandler writeResults

[<EntryPoint>]
let main argv =
    Tests.runTestsInAssembly config argv
