using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace VizInt
{
    /// <summary>
    /// A template selector that iterates through all of the renderers to
    /// find the first that can display it.
    /// </summary>
    public class ComponentTemplateSelector : DataTemplateSelector
    {
        [ImportMany("Renderer", typeof(ICanRender))]
        public IEnumerable<ICanRender> Renderers { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var ctxt = (container as FrameworkElement);
            if (ctxt == null) return null;

            if (item == null)
            {
                //System.Diagnostics.Trace.WriteLine("Warning: Null item in data template selector");
                return ctxt.FindResource("Empty") as DataTemplate;
            }

            // Allow type-specific data templates to take precedence (should we)?
            var key = new DataTemplateKey(item.GetType());
            var typeTemplate = ctxt.TryFindResource(key) as DataTemplate;
            if (typeTemplate != null)
                return typeTemplate;

            // Common problem if the MEF import failed to find any suitable DLLs
            if (!Renderers.Any())
                System.Diagnostics.Trace.WriteLine("Warning: No visualizer components loaded");

            var template = "";
            var r = Renderers
                .OrderByDescending(i => i.Importance)
                .FirstOrDefault(i => (i.CanRender(item, ref template)));
            if (r == null || String.IsNullOrEmpty(template))
            {
                System.Diagnostics.Trace.WriteLine("Warning: No renderers that can handle object");
                return ctxt.FindResource("Default") as DataTemplate;
            }

            return ctxt.TryFindResource(template) as DataTemplate ?? ctxt.FindResource("Missing") as DataTemplate;
        }
    }
}