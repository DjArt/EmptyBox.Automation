namespace EmptyBox.Automation

open System

[<AllowNullLiteral>]
type public IPipelineInput<'TData> =
    abstract member Input : EventHandler<'TData>

[<AllowNullLiteral>]
type public IPipelineOutput<'TData> =
    [<CLIEvent>]
    abstract member Output : IEvent<EventHandler<'TData>, 'TData>

[<AllowNullLiteral>]
type public IPipelineIO<'TData0, 'TData1> =
    inherit IPipelineInput<'TData0>
    inherit IPipelineOutput<'TData1>