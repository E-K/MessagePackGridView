using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessagePackGridView
{
    public class EnumerableReflectionHelper
    {
        public static IEnumerable<object> GetValues(object obj)
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
    }
}
