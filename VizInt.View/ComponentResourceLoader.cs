using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Windows;

namespace VizInt
{
    /// <summary>
    /// Imports and aggregates resource dictionaries from each renderer.
    /// </summary>
    public class ComponentResourceLoader
    {
        [ImportMany("ApplicationResources", typeof(ResourceDictionary))]
        public IEnumerable<ResourceDictionary> Views { get; set; }

        /// <summary>
        /// Aggregates all of the component resources into the specified resource dictionary.
        /// </summary>
        internal void Load(ResourceDictionary root)
        {
            foreach (var r in Views)
            {
                root.MergedDictionaries.Add(r);
            }
        }
    }
}