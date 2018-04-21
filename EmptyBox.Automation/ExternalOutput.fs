namespace EmptyBox.Automation

open System

type ExternalOutput<'TData>(action : EventHandler<'TData>) = 
    inherit Pipeline<'TData>()

    interface IPipelineInput<'TData> with
        member this.Input = action