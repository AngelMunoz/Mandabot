namespace Mandabot

open System
open Amazon.Lambda
open Amazon.Lambda.Model
open FSharp.Control.Tasks
open System.Text.Json

open Mandabot.Lib
open Mandabot.Lib.Telegram
open System.Threading.Tasks


[<RequireQualifiedAccess>]
module Commands =
    let baseUrl =
        let baseUrl =
            Environment.GetEnvironmentVariable("MandabotBaseUrl")

        let apikey =
            Environment.GetEnvironmentVariable("MandabotApiKey")

        sprintf "%s%s" baseUrl apikey

    let getNotes (chatid: int64) (userid: int64) =
        task {
            let! result =
                Client.sendMessage
                    baseUrl
                    { chat_id = chatid
                      text = "Getting your notes\.\.\."
                      parse_mode = "MarkdownV2" }

            match result with
            | Ok _ -> ignore ()
            | Error err ->
                Amazon.Lambda.Core.LambdaLogger.Log(sprintf "Failed to Send Message to client \"%s\"" err.error)

            use lambda = new AmazonLambdaClient()

            let request = InvokeRequest()

            request.FunctionName <- "MandabotGetNotes"

            request.Payload <- JsonSerializer.Serialize({ userid = userid }, Http.JsonSerializerOpts.Value)

            let! response = lambda.InvokeAsync(request)

            if response.HttpStatusCode <> Net.HttpStatusCode.OK then
                return "We couldn't get your notes :( ..."
            else
                let! result =
                    JsonSerializer.DeserializeAsync<GetNotesLambdaResponse>
                        (response.Payload, Http.JsonSerializerOpts.Value)

                return sprintf "%A" result
        }

    let getNote chatid user index =
        task {

            return sprintf "note %i" index
        }

    let addNote chatid (user) =
        task {

            return "note n"
        }

    let updateNote chatid user index content =
        task {

            return sprintf "updated note %i" index
        }

    let deleteNote chatid user index =
        task {

            return sprintf "deleted %i" index
        }
