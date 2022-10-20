using System.Diagnostics;
using System.Reflection;
using BasicConcepts;
using QuikLuaApiWrapper.Extensions;

namespace QuikLuaApi.Entities
{
    internal class QuikTable
    {
        public string Name { get; }
        public string this[string key]
        {
            get => _contents[key];
            set => _contents[key] = value;
        }

        public QuikTable(string name)
        {
            Name = name;
        }

        public T Deserialize<T>()
        {
            if (typeof(T).GetCustomAttribute<QuikTableAttribute>() 
                is not QuikTableAttribute tableAttribute || tableAttribute.TableName != Name)
            {
                throw new InvalidOperationException("");
            }

            var objtype = typeof(T);

            var properties = objtype
                .GetProperties()
                .Where(p =>
                    p.GetCustomAttribute<QuikTableValueAttribute>()
                        is QuikTableValueAttribute valueAttribute
                            && _contents.ContainsKey(valueAttribute.ColumnName));

            var obj = (T)Activator.CreateInstance(objtype);

            foreach (var property in properties)
            {
                var attribute = property.GetCustomAttribute<QuikTableValueAttribute>();

                if (property.PropertyType != attribute.Type)
                {
                    throw new InvalidOperationException($"QuikTableValueAttribute.Type mismatch in {objtype.Name}.{property.Name}");
                }

                property.SetValue(obj, Parse(property.PropertyType, _contents[attribute.ColumnName]));
            }

            return obj;
        }

        private static object Parse(Type type, string value)
        {
            if (type == typeof(Decimal5))
            {
                return !string.IsNullOrWhiteSpace(value) && Decimal5.TryParse(value, out Decimal5 result)
                    ? result
                    : default(Decimal5);
            }
            if (type == typeof(Currencies))
            {
                return !string.IsNullOrWhiteSpace(value) 
                    ? value.CodeToCurrency()
                    : Currencies.RUB;
            }
            if (type == typeof(long))
            {
                return !string.IsNullOrWhiteSpace(value) && long.TryParse(value, out long result)
                    ? result
                    : default(long);
            }
            if (type == typeof(DateTimeOffset))
            {
                return !string.IsNullOrWhiteSpace(value) && value.TryConvertToMoexExpiry(out DateTimeOffset result)
                    ? result
                    : default(DateTimeOffset);
            }

            return value ?? string.Empty;
        }

        private readonly Dictionary<string, string> _contents = new();
    }

    [System.AttributeUsage(AttributeTargets.All, Inherited = true, AllowMultiple = true)]
    sealed class QuikTableValueAttribute : Attribute
    {
        public string ColumnName { get; }
        public Type Type { get; }

        public QuikTableValueAttribute(string columnName, Type type)
        {
            ColumnName = columnName;
            Type = type;
        }
    }

    [System.AttributeUsage(AttributeTargets.All, Inherited = true, AllowMultiple = true)]
    sealed class QuikTableAttribute : Attribute
    {
        public string TableName { get; }

        public QuikTableAttribute(string tableName)
        {
            TableName = tableName;
        }
    }
}