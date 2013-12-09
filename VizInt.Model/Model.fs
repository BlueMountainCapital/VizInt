namespace VizInt

open System
open System.Collections.Generic
open System.Collections.ObjectModel
open System.ComponentModel
open System.Linq
open System.Windows
open System.Reflection

type private ExpanderState = Collapsed = 1 | Expanded = 2 | Automatically = 3

type ScopeEntry(name: string, ty: Type, value: obj) =
    let mutable isExpanded = ExpanderState.Collapsed
    let propertyChanged = Event<PropertyChangedEventHandler, PropertyChangedEventArgs>()

    member this.Name = name
    member this.DataType = ty
    member this.Value = value
    member this.IsExpanded
        with get() = isExpanded <> ExpanderState.Collapsed
        and set(newVal) = 
            isExpanded <- if newVal then ExpanderState.Expanded else ExpanderState.Collapsed                
            propertyChanged.Trigger(this, new PropertyChangedEventArgs("IsExpanded"))

    [<CLIEvent>]
    member this.PropertyChanged = propertyChanged.Publish

    // Deal with automatic expanding/collapsing.
    // We do not collapse if the entry was explicitly expanded by the user
    member internal this.SetAutoExpanded(expand: bool) = 
        match expand, isExpanded with
        | true, ExpanderState.Collapsed -> 
            // We cannot use the property here because we want to set the "auto-expanded" state
            isExpanded <- ExpanderState.Automatically
            propertyChanged.Trigger(this, new PropertyChangedEventArgs("IsExpanded"))
        | false, ExpanderState.Automatically -> this.IsExpanded <- false
        | _ -> ()

    // Define the add and remove methods to implement this interface.
    interface INotifyPropertyChanged with
        member this.add_PropertyChanged(handler) = this.PropertyChanged.AddHandler(handler)
        member this.remove_PropertyChanged(handler) = this.PropertyChanged.RemoveHandler(handler)


type Model() =

    // Find the FSI dynamic assembly
    let fsiAssembly = 
        System.AppDomain.CurrentDomain.GetAssemblies() 
        |> Array.tryFind (fun assm -> assm.GetName().Name = "FSI-ASSEMBLY")

    let getNewVariables (knownFSITypes: System.Collections.Generic.ICollection<Type>) =
        dict [|
            let types = match fsiAssembly with 
                        | Some fsia -> fsia.GetTypes()
                        | None -> Array.empty
            types |> Array.sortInPlaceBy (fun t -> t.Name)

            for t in types do
                if t.Name.StartsWith("FSI_") && not(knownFSITypes.Contains(t)) then
                    yield t, [| let flags = BindingFlags.Static ||| BindingFlags.NonPublic ||| BindingFlags.Public
                                for pi in t.GetProperties(flags) do //i checked, nothing interesting in GetFields
                                    if not(pi.Name.Contains("@")) && pi.GetIndexParameters().Length = 0 && 
                                        (pi.PropertyType <> typeof<Unit>) then
                                        yield ScopeEntry(pi.Name, pi.PropertyType, pi.GetValue(null, Array.empty))
                                |]
        |]

    // Get the variables that exist at startup
    let vars = getNewVariables(Array.empty)

    // This is the collection that is exposed from the model
    let coll = new ObservableCollection<ScopeEntry>(Seq.concat vars.Values)        

    // This is updated to reflect which FSI types have already been reflected on
    let knownFsiTypes = new System.Collections.Generic.List<_>(vars.Keys)

    let showMany (dispatcher : System.Windows.Threading.Dispatcher) (entries: ScopeEntry[]) = 
        dispatcher.BeginInvoke(Action(fun () -> 
            // Soft-collapse the last item
            if coll.Count > 0 then coll.Last().SetAutoExpanded(false)
          
            entries |> Seq.iter (fun entry -> ignore <| coll.Add entry)

            // Soft-expand the last item
            if coll.Count > 0 then coll.Last().SetAutoExpanded(true)))

    let mutable _dispatcher : Threading.Dispatcher = null
    
    // Start listening
    let mutable listen = true
    
    let listener =        
        fun (_:obj) ->
            if listen && _dispatcher <> null then
                listen <- false //we are going to try to not fire every time a 100 item list for example is entered into FSI
                try
                    try
                        // New variables since the last time we looked
                        let newVars = getNewVariables(knownFsiTypes)
                                             
                        showMany _dispatcher ([| for entry in Seq.concat newVars.Values do
                                                    // Don't allow UI elements in the view, for now
                                                    // TODO: in the view, filter Windows and ancestors/descendents of the view itself
                                                    if not(entry.Value :? System.Windows.UIElement) then yield entry |])
                                                                        
                        knownFsiTypes.AddRange(newVars.Keys)
                        listen <- true
                    with e ->
                        printfn "%A" (e.InnerException)
                finally
                    listen <- true //want to make sure we don't leave this as false if there is a problem!
                null
            else
                null

    // Initialize the listener and return it
    member this.Init () = listener

    /// Explicitly add a value to the window
    member this.Show(o: obj, name: string) =
        showMany _dispatcher ([| ScopeEntry(name, o.GetType(), o) |])

    /// Get the collection to watch
    member this.GetObservableCollection() = coll

    member this.Dispatcher 
        with set v  = _dispatcher <- v
        and  get () = _dispatcher

