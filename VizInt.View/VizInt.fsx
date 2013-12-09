
///////////////////
// Visualization //
///////////////////

// Install the WPF event loop
#load "WPFEventLoop.fsx"

#I @"bin\Debug"
#r "VizInt.View.dll"
#r "VizInt.Model.dll"

let model = VizInt.Model()
model.Dispatcher <- System.Windows.Application.Current.Dispatcher
fsi.AddPrintTransformer(model.Init())
let items = model.GetObservableCollection()
let w = VizInt.View(items)
w.Show()
