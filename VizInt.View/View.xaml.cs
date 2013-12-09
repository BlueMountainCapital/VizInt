using System.Collections.ObjectModel;
using System.Windows;

namespace VizInt
{
    /// <summary>
    /// Interaction logic for View.xaml
    /// </summary>
    public partial class View : Window
    {
        public View(ObservableCollection<ScopeEntry> collection) {
	        this.DataContext = collection;
	        
            InitializeComponent();
			
            // Scrolling to bottom is not strictly always correct, but the model happens to only ever
			// add to the end
			collection.CollectionChanged += (sender, args) => this.Scroller.ScrollToBottom();

            // This finds plugins and adds them in to the resource dictionary
            Bootstrapper.Prepare(Resources);
        }

    }
}
