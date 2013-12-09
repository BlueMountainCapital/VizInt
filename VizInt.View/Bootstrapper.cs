using System;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace VizInt
{
    public class Bootstrapper
    {
        /// <summary>
        /// Used to create and initialise the template selector and the
        /// resources that it references.
        /// </summary>
        /// <param name="dictionary"></param>
        static public void Prepare(ResourceDictionary dictionary)        
        {
            // TODO: Need to find path to load Visualizer plugins
            // Fsi "shadow copy" visualizer hack: it sets this env var for us to use
            var basePath = Environment.GetEnvironmentVariable("FSI_SHADOW_PATH");
            if (String.IsNullOrEmpty(basePath))
                basePath = AppDomain.CurrentDomain.BaseDirectory;

            // Different path for this?
            var catalog = new DirectoryCatalog(basePath, @"*Visualizer*.dll");
            var container = new CompositionContainer(catalog);

            // Resources are merged into the passed dictionary, and which point
            // we no longer require a reference to the loader.
            var loader = new ComponentResourceLoader();
            container.ComposeParts(loader);
            loader.Load(dictionary);

            // Template selector is initialised and added to resources.
            var selector = new ComponentTemplateSelector();
            container.ComposeParts(selector);
            dictionary.Add("componentSelector", selector);            
        }
    }
}
