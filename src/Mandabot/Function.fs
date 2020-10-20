namespace Mandabot

open Amazon.Lambda.Core

open System
open System.IO
open System.Text.Json
open System.Text.Json.Serialization
open System.Threading.Tasks

open FSharp.Control.Tasks

open Mandabot.Lib
open Mandabot.Lib.Telegram

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[<assembly:LambdaSerializer(typeof<CustomSerializer>)>]
()

/// <summary>
/// This module handles any database interaction
/// and should focus only in checking data either to be saved or validated
/// </summary>
[<RequireQualifiedAccess>]
module Commands =

    let GetNotes (userid: int64) (chatid: int64): Task<string> =
        task {
            // TODO: query against an actual database
            let notes = [| "Note 1"; "Note 2"; "Note 3" |]
            return (sprintf "%A" notes).Replace("|", "\|").Replace(";", "\;").Replace(".", "\.")
        }

    let GetNote (chatid: int64) (userid: int64) (query: Option<string>): Task<string> =
        task {
            // TODO: query against an actual database
            let note =
                defaultArg query "We couldn't find your note..."

            return (sprintf "%s" note)
        }

    let SaveNote (chatid: int64) (userid: int64) (note: string * string) =
        task {
            // TODO: query against an actual database
            // TODO: Check that the title doesn't exist for the user in the chat yet
            let (title, content) = note
            return sprintf "Saved Note:\n<b>%s</b>\n%s" title content
        }




/// <summary>
/// This module contains the functions that handle the commands given by the telegram bot
/// telegram also provides us with language in case it's set so we should be also able to
/// provide the correct translation (from.language_code)
/// </summary>
[<RequireQualifiedAccess>]
module Handlers =

    let LetsGetStarted (baseUrl: string) (msg: Message) (from: User) =
        task {
            let text =
                sprintf "Hey %s I'm Mandadinbot\n" from.first_name
                + "I'll be assisting you with simple tasks, if you need help just type \"/\" in the chat\n"
                + "and there you will see what can I do for you\n"
                + "I hope to be useful in any way to you 😁"

            let! result = Client.SendMessage baseUrl {| chat_id = msg.chat.id; text = text |}

            return match result with
                   | Error err -> eprintfn "Failed to send Message - [%i;%i - %s]" msg.chat.id from.id err.error
                   | _ -> ()
        }

    let StartGetNotes (baseUrl: string) (msg: Message) (from: User) =
        task {
            Client.SendChatAction baseUrl msg.chat.id SendChatActionType.Typing
            |> Async.AwaitTask
            |> Async.StartImmediateAsTask
            |> ignore
            /// in case we need to do some queries to another API/Database
            let! result = Commands.GetNotes from.id msg.chat.id

            let! result =
                Client.SendMessage
                    baseUrl
                    {| chat_id = msg.chat.id
                       text = result |}

            return match result with
                   | Error err -> eprintfn "Failed to send Message - [%i;%i - %s]" msg.chat.id from.id err.error
                   | _ -> ()
        }

    let GetNote (baseUrl: string) (chat: int64) (from: int64) (query: Option<string>) =
        task {
            Client.SendChatAction baseUrl chat SendChatActionType.Typing
            |> Async.AwaitTask
            |> Async.StartImmediateAsTask
            |> ignore
            let! result = Commands.GetNote from chat query

            let! result = Client.SendMessage baseUrl {| chat_id = chat; text = result |}

            return match result with
                   | Error err -> eprintfn "Failed to send Message - [%i;%i - %s]" chat from err.error
                   | _ -> ()
        }

    let AddNote (baseUrl: string) (chat: int64) (from: int64) (note: string * string) =
        task {
            Client.SendChatAction baseUrl chat SendChatActionType.Typing
            |> Async.AwaitTask
            |> Async.StartImmediateAsTask
            |> ignore
            let! result = Commands.SaveNote chat from note

            let! result =
                Client.SendMessage
                    baseUrl
                    {| chat_id = chat
                       text = result
                       parse_mode = "HTML" |}

            return match result with
                   | Error err -> eprintfn "Failed to send Message - [%i;%i - %s]" chat from err.error
                   | _ -> ()
        }


type Function() =

    let baseUrl =
        let baseUrl =
            Environment.GetEnvironmentVariable("MandabotBaseUrl")

        let apikey =
            Environment.GetEnvironmentVariable("MandabotApiKey")

        sprintf "%s%s" baseUrl apikey

    /// <summary>
    /// This is the entry point of a telegram bot, basically depending on the command given by the telegram bot
    /// here we decide what kind of action to take if we need to trigger an email, a database query, data processing
    /// you name it, in this case we're just echoing for demonstration purposes
    /// </summary>
    /// <param name="update">Update from the telegram API</param>
    /// <param name="context"></param>
    /// <returns></returns>
    member __.FunctionHandler (update: Update) (ctx: ILambdaContext)
                              : Task<{| method: string
                                        chat_id: int64
                                        action: string |}> =
        task {
            match update.message with
            | Some msg ->
                let entity =
                    msg.entities
                    |> Option.map Seq.tryHead
                    |> Option.flatten

                match entity, msg.text, msg.from with
                | Some entity, Some text, Some from when entity.``type`` = "bot_command" ->
                    if text.StartsWith "/getnotes" then
                        do! Handlers.StartGetNotes baseUrl msg from
                    else

                    if text.StartsWith "/getnote" then
                        let text =
                            if text.Length > 8 then Some(text.Substring(8).Trim()) else None

                        do! Handlers.GetNote baseUrl msg.chat.id from.id text
                    else

                    if text.StartsWith "/addnote" then
                        let (title, content) =
                            let lines =
                                if text.Length > 8 then text.Substring(8).Trim() else ""

                            let split = lines.Split('\n')
                            if split.Length > 1 then
                                let title = split |> Seq.tryHead
                                let content = String.Join('\n', split |> Array.tail)
                                title, content
                            else
                                (None, "")

                        if title.IsNone || String.IsNullOrWhiteSpace content then
                            let! _ =
                                Client.SendMessage
                                    baseUrl
                                    {| chat_id = msg.chat.id
                                       text =
                                           "I Can't save that\, I need the note in the following format\:\n"
                                           + "```\nTitle\nContent... (content\ncan\nspan as many lines as you want)\n```"
                                       parse_mode = "MarkdownV2" |}

                            ()
                        else
                            do! Handlers.AddNote baseUrl msg.chat.id from.id (title.Value, content)
                    else if text.StartsWith "/updatenote" then
                        ()
                    else if text.StartsWith "/deletenote" then
                        ()
                    else if text.StartsWith "/start" then
                        do! Handlers.LetsGetStarted baseUrl msg from
                    else
                        Client.SendMessage
                            baseUrl
                            {| chat_id = msg.chat.id
                               text = "Hmm... weird I don't know what am I supposed to do with that" |}
                        |> Async.AwaitTask
                        |> Async.StartImmediateAsTask
                        |> ignore

                | None, Some text, Some from ->
                    // non command update, eg. added to a room, etc
                    printfn "%s - [%s - bot:%b]" text from.first_name from.is_bot
                    Client.SendMessage
                        baseUrl
                        {| chat_id = msg.chat.id
                           text = "I see, I'm not trained to answer that though..." |}
                    |> Async.AwaitTask
                    |> Async.StartImmediateAsTask
                    |> ignore
                | unknown ->
                    printfn "Unknown Payload: %A" unknown
                    Client.SendMessage
                        baseUrl
                        {| chat_id = msg.chat.id
                           text = "Hey there! it's nice being here :)" |}
                    |> Async.AwaitTask
                    |> Async.StartImmediateAsTask
                    |> ignore

                return {| method = "sendChatAction"
                          chat_id = msg.chat.id
                          action = SendChatActionType.Typing.ToActionString() |}

            | None -> return failwith "Missing Message in the update"
        }
