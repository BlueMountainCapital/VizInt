using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Reflection;
using System.ComponentModel;

namespace VizInt
{
    public static class TypeUtils
    {
        /// <summary> Similar to Type.GetType() but also searchs the current AppDomain. </summary>
        public static Type FindType(string typeName)
        {
            if (typeName == string.Empty) return null;

            Type type = Type.GetType(typeName, false);
            if (type != null) return type;
            foreach (System.Reflection.Assembly ass in AppDomain.CurrentDomain.GetAssemblies())
            {
                type = ass.GetType(typeName, false);
                if (type != null) return type;
            }
            return null;
        }

        /// <summary>
        /// Given a CLR Type, returns a C#-like type name, without namespaces, converting 
        /// generic types and system types appropriately. Useful for debugging and logging purposes.
        /// </summary>
        /// <example>
        /// <![CDATA[
        /// 
        /// string result = TypeUtils.GetTypeName(typeof(Cell<double>)); // returns "Cell<double>"
        /// 
        /// ]]>
        /// </example>
        /// <param name="t">Type to obtain string on</param>
        /// <returns>type name</returns>
        /// 
        public static string GetTypeName(Type t)
        {
            StringBuilder sb = new StringBuilder();
            AppendTypeString(t, sb);
            return sb.ToString();
        }

        static Dictionary<Type, string> standardTypes;

        static TypeUtils()
        {
            standardTypes = new Dictionary<Type, string>();
            standardTypes[typeof(sbyte)] = "sbyte";
            standardTypes[typeof(byte)] = "byte";
            standardTypes[typeof(short)] = "short";
            standardTypes[typeof(ushort)] = "ushort";
            standardTypes[typeof(int)] = "int";
            standardTypes[typeof(uint)] = "uint";
            standardTypes[typeof(long)] = "long";
            standardTypes[typeof(ulong)] = "ulong";
            standardTypes[typeof(float)] = "float";
            standardTypes[typeof(double)] = "double";
            standardTypes[typeof(bool)] = "bool";
            standardTypes[typeof(char)] = "char";
            standardTypes[typeof(string)] = "string";
            standardTypes[typeof(object)] = "object";

        }

        static void AppendTypeString(Type t, StringBuilder sb)
        {
            string name;
            if (t.IsNested)
            {
                AppendTypeString(t.DeclaringType, sb);
                sb.Append('.');
            }
            if (t.IsArray)
            {
                AppendTypeString(t.GetElementType(), sb);
                sb.Append('[');
                for (int i = 1; i < t.GetArrayRank(); i++) sb.Append(',');
                sb.Append(']');
            }
            else if (t.IsGenericTypeDefinition)
            {
                // Instead of Cell`, returns Cell<>
                AppendGenericName(t, sb);
                sb.Append("<>");
            }
            else if (t.IsGenericType)
            {
                // Instead of Cell`1, returns Cell<double>
                bool isNullable = (t.GetGenericTypeDefinition() == typeof(Nullable<>));

                if (!isNullable)
                {
                    AppendGenericName(t, sb);
                    sb.Append('<');
                }

                int startPos = sb.Length;
                foreach (Type param in t.GetGenericArguments())
                {
                    if (sb.Length != startPos) sb.Append(',');
                    AppendTypeString(param, sb);
                }
                sb.Append(isNullable ? '?' : '>');
            }
            else if (standardTypes.TryGetValue(t, out name))
            {
                // Int32 -> int etc
                sb.Append(name);
            }
            else
            {
                // Just use the standard name
                sb.Append(t.Name);
            }
        }

        static void AppendGenericName(Type t, StringBuilder sb)
        {
            string name = t.Name;
            int offset = name.IndexOf('`');
            if (offset > 0)
            {
                sb.Append(name.Substring(0, offset));
            }
            else
            {
                sb.Append(name);
            }
        }

        /// <summary>
        /// Like object.ToString() but for collections it returns the length of the collection, 
        /// and for Types, returns the C# type name.
        /// </summary>
        public static string ToString(object obj)
        {
            if (obj == null)
            {
                return "null";
            }
            else if (obj is Type)
            {
                return GetTypeName((Type)obj);
            }
            else if (obj is IEnumerable)
            {
                Type type = obj.GetType();
                if (type.IsValueType || type == typeof(string))
                {
                    return obj.ToString();
                }
                else if (type.IsArray)
                {
                    string arrName = TypeUtils.GetTypeName(type);
                    int bracket = arrName.IndexOf('[');
                    return arrName.Substring(0, bracket + 1) + ((Array)obj).Length + arrName.Substring(bracket + 1);
                }
                else if (obj is ICollection)
                {
                    // Any Collection using object.ToString() instead shows the length of the collection
                    MethodInfo toString = type.GetMethod("ToString");
                    if (toString.DeclaringType == typeof(object))
                    {
                        return TypeUtils.GetTypeName(type) + "(" + ((ICollection)obj).Count + ")";
                    }
                }
            }

            // Default to the object's ToString
            return obj.ToString();
        }

        public static bool IsNumeric(object o)
        {
            var type = o as Type ?? o.GetType();
            return type.IsPrimitive && type != typeof(bool);
        }
    }
}
