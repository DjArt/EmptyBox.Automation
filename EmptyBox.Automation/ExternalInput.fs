namespace EmptyBox.Automation

open System

type ExternalInput<'TData>() = 
    inherit Pipeline<'TData>()

    let InternalEvent = Event<EventHandler<'TData>, 'TData>()

    interface IPipelineOutput<'TData> with
        [<CLIEvent>]
        member this.Output = InternalEvent.Publish

    member public this.Send(input : 'TData) = InternalEvent.Trigger(this, input)