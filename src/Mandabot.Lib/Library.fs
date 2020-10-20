namespace Mandabot.Lib


open System
open Amazon.Lambda.Serialization.SystemTextJson
open System.Text.Json
open System.Text.Json.Serialization

type CustomSerializer() as this =
    inherit DefaultLambdaJsonSerializer()

    do
        this.SerializerOptions.AllowTrailingCommas <- true
        this.SerializerOptions.IgnoreNullValues <- true
        this.SerializerOptions.Converters.Add(JsonFSharpConverter())
        this.SerializerOptions.ReadCommentHandling <- JsonCommentHandling.Skip
