using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VizInt
{
    /// <summary>
    /// Can we render the specific object? If so, provide the name of
    /// a DataTemplate that renders the view.
    /// </summary>
    public interface ICanRender
    {
        bool CanRender(object o, ref string templateName);
        int Importance { get;  }
    }
}
