namespace Mandabot


open Amazon.Lambda.Core

open System
open System.IO
open System.Threading.Tasks
open FSharp.Control.Tasks
open Mandabot.Lib
open Mandabot.Lib.Telegram

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[<assembly:LambdaSerializer(typeof<CustomSerializer>)>]
()


type Function() =
    /// <summary>
    /// A simple function that takes a string and does a ToUpper
    /// </summary>
    /// <param name="input"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    member __.FunctionHandler (update: Update) (ctx: ILambdaContext)
                              : Task<{| method: string
                                        chat_id: int64
                                        text: string |}> =
        task {
            match update.message with
            | Some msg ->
                let! response =
                    match msg.entities
                          |> Option.map Seq.tryHead
                          |> Option.flatten,
                          msg.text,
                          msg.from with
                    | Some entity, Some text, Some from when entity.``type`` = "bot_command" ->
                        if text.StartsWith "/getnotes" then
                            Commands.getNotes msg.chat.id from.id
                        else if text.StartsWith "/getnote" then
                            Commands.getNote msg.chat.id (sprintf "%s %i" from.first_name from.id) 0
                        else if text.StartsWith "/addnote" then
                            Commands.addNote msg.chat.id (sprintf "%s %i" from.first_name from.id)
                        else if text.StartsWith "/updatenote" then
                            Commands.updateNote msg.chat.id (sprintf "%s %i" from.first_name from.id) 0 "updated"
                        else if text.StartsWith "/deletenote" then
                            Commands.deleteNote msg.chat.id (sprintf "%s %i" from.first_name from.id) 0
                        else
                            Task.FromResult("That isn't a valid command, please try again")
                    | None, Some text, Some from ->
                        let result =
                            sprintf
                                """got **"%s"** from "%s %i", but not sure how to handle it"""
                                text
                                from.first_name
                                from.id

                        Task.FromResult(result)
                    | _ -> Task.FromResult("No idea how to handle that")

                return {| method = "sendMessage"
                          chat_id = msg.chat.id
                          text = response |}
            | None -> return failwith "Missing Message in the update"
        }
