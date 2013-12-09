using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Markup;
using RDotNet;
using RProvider;

namespace VizInt
{
	class RSymbolicExpressionConverter : MarkupExtension, IValueConverter
	{
		public override object ProvideValue(IServiceProvider serviceProvider)
		{
			return this;
		}

		public object Convert(object value, Type targetType,
			object parameter, CultureInfo culture)
		{
			var sexp = (SymbolicExpression) value;
			return RInterop.defaultFromR(sexp);
		}

		public object ConvertBack(object value, Type targetType,
			object parameter, CultureInfo culture)
		{
			throw new NotSupportedException();
		}
	}
}
