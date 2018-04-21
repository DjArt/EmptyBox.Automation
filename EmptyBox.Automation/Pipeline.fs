namespace EmptyBox.Automation

#nowarn "86"

open System
open System.Threading.Tasks

[<AllowNullLiteral>]
type public Pipeline<'TData>() =
    member val private AsyncInvoker : AsyncInvoker<'TData> = null with get, set

    static member (>>>) (x : IPipelineOutput<'TData>, y : IPipelineInput<'TData>) : bool =
        x.Output.AddHandler(y.Input)
        true

    static member (>) (x : IPipelineOutput<'TData>, y : IPipelineInput<'TData>) : bool =
        x.Output.RemoveHandler(y.Input)
        true

    static member (~~~) (x : IPipelineInput<'TData>) : AsyncInvoker<'TData> =
        let _x = (x :> obj) :?> Pipeline<'TData>
        match _x.AsyncInvoker with
        | null ->
            _x.AsyncInvoker <- AsyncInvoker<'TData>(x)
        | _ ->
            ()
        _x.AsyncInvoker

and [<AllowNullLiteral>] public AsyncInvoker<'TData>(pipe : IPipelineInput<'TData>) =
    
    inherit Pipeline<'TData>()
    
    let Task = EventHandler<'TData>(fun x y -> Task.Run(fun () -> pipe.Input.Invoke(x, y)) |> ignore)

    interface IPipelineInput<'TData> with
        member this.Input = Task

[<AllowNullLiteral>]
type public Pipeline<'TData0, 'TData1>() =
    inherit Pipeline<'TData0>()

    member val private AsyncInvoker : AsyncInvoker<'TData0, 'TData1> = null with get, set

    static member (>>>) (x : IPipelineOutput<'TData0>, y : IPipelineIO<'TData0, 'TData1>) : IPipelineOutput<'TData1> =
        x.Output.AddHandler(y.Input)
        y :> IPipelineOutput<'TData1>

    static member (>) (x : IPipelineOutput<'TData0>, y : IPipelineIO<'TData0, 'TData1>) : IPipelineOutput<'TData1> =
        x.Output.RemoveHandler(y.Input)
        y :> IPipelineOutput<'TData1>

    static member (~~~) (x : IPipelineIO<'TData0, 'TData1>) : AsyncInvoker<'TData0, 'TData1> =
        let _x = (x :> obj) :?> Pipeline<'TData0, 'TData1>
        match _x.AsyncInvoker with
        | null ->
            _x.AsyncInvoker <- AsyncInvoker<'TData0, 'TData1>(x)
        | _ ->
            ()
        _x.AsyncInvoker

and [<AllowNullLiteral>] public AsyncInvoker<'TData0, 'TData1>(pipe : IPipelineIO<'TData0, 'TData1>) =
    
    inherit Pipeline<'TData0, 'TData1>()

    let Pipe = pipe
    let Task = EventHandler<'TData0>(fun x y -> Task.Run(fun () -> Pipe.Input.Invoke(x, y)) |> ignore)

    interface IPipelineIO<'TData0, 'TData1> with
        member this.Input = Task

        [<CLIEvent>]
        member this.Output = Pipe.Output