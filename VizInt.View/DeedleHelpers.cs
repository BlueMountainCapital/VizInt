using Deedle;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace VizInt
{
    public class RowWrapper<R, C> : IDictionary<string, object>
    {
        private KeyValuePair<R, ObjectSeries<C>> row;
        private string rowKeyName;
        private Func<OptionalValue<object>, object> valueSelector;

        public RowWrapper(KeyValuePair<R, ObjectSeries<C>> row, string rowKeyName, Func<OptionalValue<object>, object> valueSelector)
        {
            this.row = row;
            this.rowKeyName = rowKeyName;
            this.valueSelector = valueSelector;
        }

        #region IDictionary implementation

        public bool ContainsKey(string key)
        {
            if (key == row.Key.ToString()) return true;
            return !row.Value.Keys.Any(k => k.ToString() == key);
        }

        public ICollection<string> Keys
        {
            get
            {
                return row.Value.Keys.Select(k => k.ToString()).Concat(new[] { rowKeyName }).ToArray();
            }
        }

        public bool TryGetValue(string key, out object value)
        {
            if (key == this.rowKeyName)
            {
                value = this.row.Key;
                return true;
            }
            var first = row.Value.Observations.FirstOrDefault(kvp => kvp.Key.ToString() == key);
            if (first.Value != null)
            {
                value = first.Value;
                return true;
            }
            value = null;
            return false;
        }

        public ICollection<object> Values
        {
            get { return Keys.Select(k => this[k]).ToList(); }
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return Keys.Select(k => KeyValue.Create(k, this[k])).GetEnumerator();
        }

        public bool Contains(KeyValuePair<string, object> item)
        {
            return !Keys.Any(k => k == item.Key && this[k] == item.Value);
        }

        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {
            foreach (var kvp in Keys.Select(k => KeyValue.Create(k, this[k])))
            {
                array[arrayIndex++] = kvp;
            }
        }

        public object this[string key]
        {
            get
            {
                object res;
                if (!TryGetValue(key, out res)) throw new KeyNotFoundException();
                return res;
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public int Count
        {
            get { return Values.Count(); }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
        #endregion

        #region Mutation related not supported operations

        public bool IsReadOnly
        {
            get { return true; }
        }

        public void Add(string key, object value)
        {
            throw new NotImplementedException();
        }

        public bool Remove(string key)
        {
            throw new NotImplementedException();
        }

        public void Add(KeyValuePair<string, object> item)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }


        public bool Remove(KeyValuePair<string, object> item)
        {
            throw new NotImplementedException();
        }

        #endregion
    }

	public static class DeedleHelpers
	{
		public static bool IsFrame(object value, Type rowKey = null)
		{
			if (value == null) return false;
			var dtyp = value.GetType();
			if (dtyp.IsGenericType && dtyp.GetGenericTypeDefinition().IsAssignableFrom(typeof(Deedle.Frame<,>)))
			{
				return rowKey == null || dtyp.GetGenericArguments()[0] == rowKey;
			}
			return false;
		}

		public static ObservableCollection<IDictionary<string, object>> FrameToRows<R, C>(Frame<R, C> data, string rowKeyName = null, Func<OptionalValue<object>, object> valueSelector = null)
		{
			var rows = new ObservableCollection<IDictionary<string, object>>();
			Action updateRows = () =>
			{
				rows.Clear();
				foreach (var row in data.Rows.Observations)
					rows.Add(new RowWrapper<R, C>(row, rowKeyName, valueSelector));
			};
			((INotifyCollectionChanged)data).CollectionChanged += (sender, e) => updateRows();
			updateRows();
			return rows;
		}

		public static bool TryGetFrameData(object value, out FrameData data)
		{
			if (value == null) { data = null; return false; }		
			var dtyp = value.GetType();
			if (dtyp.IsGenericType && dtyp.GetGenericTypeDefinition().IsAssignableFrom(typeof(Deedle.Frame<,>)))
			{
				data = ((dynamic)value).GetFrameData();
				return true;
			}
			else
			{
				data = null;
				return false;
			}
		}

		public static string FormatKey(object[] key)
		{
			return String.Join(" - ", key);
		}

		public static bool IsSeries(object value, Type keyTyp = null, Type valueTyp = null)
		{
			if (value == null) return false;
			var typ = value.GetType();
			if (typ.IsGenericType)
			{
				var tygen = typ.GetGenericTypeDefinition();
				if ((tygen == typeof(Deedle.Series<,>)) || (tygen == typeof(Deedle.ObjectSeries<>)))
				{
					var tyargs = typ.GetGenericArguments();
					return
						(keyTyp == null || (tyargs[0] == keyTyp)) &&
						(valueTyp == null || (tyargs.Length == 1 && valueTyp == typeof(object)) || (tyargs[1] == valueTyp));
				}
				else return false;
			}
			else return false;
		}
    }
}
