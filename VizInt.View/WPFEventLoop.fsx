
#r "PresentationCore.dll"
#r "PresentationFramework.dll"
#r "System.Xaml.dll"
#r "WindowsBase.dll"

open System
open System.Collections.Generic
open System.Windows

module WPFEventLoop =
    open System.Windows.Threading
    open Microsoft.FSharp.Compiler.Interactive
    open Microsoft.FSharp.Compiler.Interactive.Settings

    type RunDelegate<'b> = delegate of unit -> 'b
    
    let mutable _dispatcher : Dispatcher = null

    let Create() =

        let app = 
            let app = Application() 
            app.DispatcherUnhandledException.AddHandler (fun o args -> 
                Console.WriteLine (sprintf "Unhandled: %s" (args.Exception.ToString()))
                // Remove windows, otherwise attempting to draw them may cause further
                // exceptions (so may Closing them, but it's worth a try...)
                Application.Current.Windows |> Seq.cast |> Seq.iter (fun (w : Window) -> 
                    if w <> Application.Current.MainWindow then w.Close())
                args.Handled <- true)
            let w = new Window()
            _dispatcher <- w.Dispatcher
            app

        { new IEventLoop with
            member x.Run() : bool =
                app.Run() |> ignore
                false

            member x.Invoke(f) =
                try  _dispatcher.Invoke(DispatcherPriority.Send, new RunDelegate<_>(fun () -> box(f ()))) |> unbox
                with e -> eprintf "\n\n ERROR: %O\n" e; reraise()

            member x.ScheduleRestart() = 
                ()
         }

    let Install() = fsi.EventLoop <- Create()

WPFEventLoop.Install()

