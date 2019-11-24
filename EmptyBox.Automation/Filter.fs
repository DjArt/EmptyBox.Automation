namespace EmptyBox.Automation

open System

type public Filter<'TData>(predicate : Predicate<'TData>) =
    inherit Pipeline<'TData, 'TData>()

    let InternalEvent = Event<EventHandler<'TData>, 'TData>()

    interface IPipelineIO<'TData, 'TData> with
        member this.Input = EventHandler<'TData>(this.Input)
        [<CLIEvent>]
        member this.Output = InternalEvent.Publish

    member private this.Input _ x =
        match predicate.Invoke x with
        | true ->
            InternalEvent.Trigger(this, x)
        | _ -> ()