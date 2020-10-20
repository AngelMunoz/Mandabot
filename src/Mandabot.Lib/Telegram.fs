namespace Mandabot.Lib.Telegram

type User =
    { id: int64
      is_bot: bool
      first_name: string
      last_name: Option<string>
      username: Option<string>
      language_code: Option<string>
      can_join_groups: Option<bool>
      can_read_all_group_messages: Option<bool>
      supports_inline_queries: Option<bool> }


type MessageEntity =
    { ``type``: string
      offset: int64
      length: int64
      url: Option<string>
      user: Option<User>
      language: Option<string> }


type Chat =
    { id: int64
      ``type``: string
      title: Option<string>
      username: Option<string>
      first_name: Option<string>
      last_name: Option<string>
      photo: Option<obj>
      description: Option<string>
      invite_link: Option<string>
      pinned_message: Option<Message>
      permissions: Option<obj>
      slow_mode_delay: Option<int64>
      sticker_set_name: Option<string>
      can_set_sticker: Option<bool> }

and Message =
    { message_id: int64
      from: Option<User>
      date: int64
      chat: Chat
      forward_fromL: Option<User>
      forward_from_chat: Option<Chat>
      forward_from_message_id: Option<int64>
      forward_signature: Option<string>
      forward_sender_name: Option<string>
      forward_date: Option<int64>
      reply_to_message: Option<Message>
      via_bot: Option<User>
      edit_date: Option<int64>
      media_group_id: Option<string>
      author_signature: Option<string>
      text: Option<string>
      entities: Option<array<MessageEntity>>
      animation: Option<obj>
      audio: Option<obj>
      document: Option<obj>
      photo: Option<array<obj>>
      sticker: Option<obj>
      video: Option<obj>
      video_note: Option<obj>
      voice: Option<obj>
      caption: Option<string>
      caption_entities: Option<array<MessageEntity>>
      contact: Option<obj>
      dice: Option<obj>
      game: Option<obj>
      poll: Option<obj>
      venue: Option<obj>
      location: Option<obj>
      new_chat_members: Option<array<User>>
      left_chat_member: Option<User>
      new_chat_title: Option<string>
      new_chat_photo: Option<array<obj>>
      delete_chat_photo: Option<bool>
      group_chat_created: Option<bool>
      supergroup_chat_created: Option<bool>
      channel_chat_created: Option<bool>
      migrate_to_chat_id: Option<int64>
      migrate_from_chat_id: Option<int64>
      pinned_message: Option<Message>
      invoice: Option<obj>
      successful_payment: Option<obj>
      connected_website: Option<string>
      passport_data: Option<obj>
      reply_markup: Option<obj> }


type Update =
    { update_id: int64
      message: Option<Message>
      edited_message: Option<Message>
      channel_post: Option<Message>
      edited_channel_post: Option<Message>
      inline_query: Option<obj>
      chosen_inline_result: Option<obj>
      callback_query: Option<obj>
      shipping_query: Option<obj>
      pre_checkout_query: Option<obj>
      poll: Option<obj>
      poll_answer: Option<obj> }

[<RequireQualifiedAccess>]
type SendChatActionType =
    | Typing
    | UploadPhoto
    | RecordVideo
    | UploadVideo
    | RecordAudio
    | UploadAudio
    | UploadDocument
    | FindLocation
    | RecordVideoNote
    | UpdateVideoNote

    member this.ToActionString() =
        match this with
        | Typing -> "typing"
        | UploadPhoto -> "upload_photo"
        | RecordVideo -> "record_video"
        | UploadVideo -> "upload_video"
        | RecordAudio -> "record_audio"
        | UploadAudio -> "upload_audio"
        | UploadDocument -> "upload_document"
        | FindLocation -> "find_location"
        | RecordVideoNote -> "record_video_note"
        | UpdateVideoNote -> "update_video_note"

type ApiResponse<'T> = { ok: bool; result: 'T }

[<RequireQualifiedAccess>]
module Client =
    open Mandabot.Lib
    open System.Threading.Tasks
    open FSharp.Control.Tasks

    let SendMessage (baseurl: string)
                    (payload: 'T)
                    : Task<Result<ApiResponse<Message>, {| ok: bool; error: string |}>> =
        task {
            let url = sprintf "%s/sendMessage" baseurl
            let! response = Http.sendPost<'T, ApiResponse<Message>> payload url

            match response with
            | Ok result -> return Ok result
            | Error err ->
                return Error
                           {| ok = false
                              error = err.description |}
        }

    let SendChatAction (baseUrl: string) (chatid: int64) (action: SendChatActionType) =
        task {
            let url = sprintf "%s/sendChatAction" baseUrl

            let payload =
                {| chat_id = chatid
                   action = action.ToActionString() |}

            let! response = Http.sendPost<{| chat_id: int64; action: string |}, ApiResponse<bool>> payload url

            return match response with
                   | Ok result -> Ok result
                   | Error err ->
                       Error
                           {| ok = false
                              error = err.description |}
        }
