namespace Mandabot.Tests


open Xunit
open System
open Amazon.Lambda.TestUtilities
open FSharp.Control.Tasks

open Mandabot
open Mandabot.Lib.Telegram
open System.Threading.Tasks

module FunctionTest =
    [<Fact>]
    let ``Fails If No Message is Present`` () =
        // Invoke the lambda function and confirm the string was upper cased.
        let lambdaFunction = Function()
        let context = TestLambdaContext()

        let update: Update =
            { update_id = 0L
              message = None
              callback_query = None
              channel_post = None
              chosen_inline_result = None
              edited_channel_post = None
              edited_message = None
              inline_query = None
              poll = None
              poll_answer = None
              pre_checkout_query = None
              shipping_query = None }


        Assert.ThrowsAsync<exn>(fun _ ->
            let tsk =
                task {
                    let! _ = lambdaFunction.FunctionHandler update context
                    return ()
                }

            tsk :> Task) :> Task

    [<EntryPoint>]
    let main _ = 0
