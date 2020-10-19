namespace Mandabot.GetNotes


open Amazon.Lambda.Core
open Mandabot.Lib

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
    member __.FunctionHandler (input: GetNotesLambdaParams) (_: ILambdaContext): GetNotesLambdaResponse =
        // do the actual note search
        { notes =
              [| {| id = 0L; text = "Text 1" |}
                 {| id = 1L; text = "Text 2" |} |] }
