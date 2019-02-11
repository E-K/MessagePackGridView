using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessagePackGridView
{
    public class ReflectionHelper
    {
        public static IEnumerable<object> GetEnumerableValues(object obj)
        {
            var type = obj.GetType();
            var (isEnumerable, _) = type.IsEnumerable();
            if (!isEnumerable)
                throw new ArgumentException($"type {type.FullName} should be IEnumerable<T>");

            var getEnumerator = type.GetMethod("GetEnumerator");
            var enumerator = getEnumerator.Invoke(obj, Array.Empty<object>());
            var enumType = enumerator.GetType();
            var moveNext = enumType.GetMethod("MoveNext");
            var currentProp = enumType.GetProperty("Current");

            while((bool)moveNext.Invoke(enumerator, Array.Empty<object>()))
            {
                yield return currentProp.GetValue(enumerator);
            }
        }

        public static object GetKeyValuePairKey(object obj)
        {
            return obj.GetType().GetProperty("Key").GetValue(obj);
        }

        public static object GetKeyValuePairValue(object obj)
        {
            return obj.GetType().GetProperty("Value").GetValue(obj);
        }
    }
}
