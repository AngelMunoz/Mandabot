namespace Mandabot.Lib

open System.Net.Http
open System.Text.Json
open System.Text.Json.Serialization
open System.Threading.Tasks
open FSharp.Control.Tasks
open System.Text

[<RequireQualifiedAccess>]
module Http =
    let JsonSerializerOpts =
        let opts = JsonSerializerOptions()
        opts.AllowTrailingCommas <- true
        opts.IgnoreNullValues <- true
        opts.Converters.Add(JsonFSharpConverter())
        opts.ReadCommentHandling <- JsonCommentHandling.Skip
        lazy (opts)

    let sendPost<'T, 'U> (payload: 'T)
                         (url: string)
                         : Task<Result<'U, {| ok: bool
                                              error_code: int
                                              description: string |}>> =
        task {
            let content = JsonSerializer.Serialize(payload)
            use http = new HttpClient()

            use content =
                new StringContent(content, Encoding.UTF8, "application/json")

            let! response = http.PostAsync(url, content)
            let! json = response.Content.ReadAsStringAsync()

            if response.IsSuccessStatusCode then
                return Ok(JsonSerializer.Deserialize<'U>(json, JsonSerializerOpts.Value))
            else
                return Error
                           (JsonSerializer.Deserialize<{| ok: bool
                                                          error_code: int
                                                          description: string |}>(json, JsonSerializerOpts.Value))
        }
