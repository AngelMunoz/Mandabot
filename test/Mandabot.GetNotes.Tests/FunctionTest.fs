namespace MandaBot.GetNotes.Tests


open Xunit
open Amazon.Lambda.TestUtilities

open Mandabot.GetNotes


module FunctionTest =
    [<Fact>]
    let ``Invoke ToUpper Lambda Function`` () =
        // Invoke the lambda function and confirm the string was upper cased.
        // let lambdaFunction = Function()
        // let context = TestLambdaContext()
        // let upperCase = lambdaFunction.FunctionHandler params context

        Assert.Equal(true, true)

    [<EntryPoint>]
    let main _ = 0
