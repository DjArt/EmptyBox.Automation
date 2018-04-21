namespace EmptyBox.Automation

open System
open System.Collections.Generic

type public BufferControlMessage() =
    let x = null

type public BufferControl() =
    inherit Pipeline<BufferControlMessage, BufferControlMessage>()

    let InternalEvent = Event<EventHandler<BufferControlMessage>, BufferControlMessage>()
    let BufferClearEvent = Event<EventHandler>()
    let BufferReleaseEvent = Event<EventHandler>()
    let BufferResizeEvent = Event<EventHandler<uint32>, uint32>()

    interface IPipelineIO<BufferControlMessage, BufferControlMessage> with
        member this.Input = null
        [<CLIEvent>]
        member this.Output = InternalEvent.Publish
    
    [<CLIEvent>]
    member public this.BufferClear = BufferClearEvent.Publish
    [<CLIEvent>]
    member public this.BufferRelease = BufferReleaseEvent.Publish
    [<CLIEvent>]
    member public this.BufferResize = BufferResizeEvent.Publish
    

type public Buffer<'TData>(size : uint32) =
    inherit Pipeline<'TData, 'TData[]>()

    let InternalEvent = Event<EventHandler<'TData[]>, 'TData[]>()
    let mutable Store = Array.zeroCreate<'TData>(size :> obj :?> int)
    let mutable Count = 0u
    let mutable Size = size

    interface IPipelineIO<'TData, 'TData[]> with
        member this.Input = EventHandler<'TData>(this.Input)
        [<CLIEvent>]
        member this.Output = InternalEvent.Publish

    member private this.Input _ x =
        Store.[Count :> obj :?> int] <- x
        Count <- Count + 1u
        if Count = Size then
            this.Release()
        ()

    member private this.Clear() =
        Count <- 0u
        Store <- Array.zeroCreate<'TData>(size :> obj :?> int)

    member private this.Release() = 
        if Count < (Store.Length :> obj :?> uint32) then
            System.Array.Resize(ref Store, Count :> obj :?> int)
        Count <- 0u
        let tmpStore = Store
        Store <- Array.zeroCreate<'TData>(Size :> obj :?> int)
        InternalEvent.Trigger(this, tmpStore)

    member private this.Resize(size : uint32) =
        Size <- size
        if Size >= Count then
            this.Release()
        else
            System.Array.Resize(ref Store, Size :> obj :?> int)

    
