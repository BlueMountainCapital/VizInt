using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Markup;

namespace VizInt
{
	class ValueToSummaryConverter : MarkupExtension, IValueConverter
	{
		public override object ProvideValue(IServiceProvider serviceProvider)
		{
			return this;
		}

		public object Convert(object value, Type targetType,
			object parameter, CultureInfo culture) {
			if (value == null)
				return "null";

			var type = value.GetType();
			var typeStr = typeToShortString(type);

			var valueStr = "...";
			if (type.IsPrimitive || value is string || value is DateTime)
				valueStr = value.ToString();

			if (DeedleHelpers.IsFrame(value)) {
				valueStr = FormatDeedleFrameInfo((dynamic)value);
			}
			return typeStr + " = " + valueStr;
		}

		static string FormatDeedleFrameInfo<R, C>(Deedle.Frame<R, C> frame)
		{
			return String.Format("[{0} rows x ({1})]", Deedle.FrameModule.CountRows(frame), 
				String.Join(",", frame.ColumnKeys.Select(k => k.ToString())));
		}

		private object typeToShortString(Type type) {
			return  TypeUtils.GetTypeName(type);
		}

		public object ConvertBack(object value, Type targetType,
			object parameter, CultureInfo culture)
		{
			throw new NotSupportedException();
		}
	}
}
