using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MinetaleConverter.Base
{
    public static class Extensions
    {
        public static T[] ToArray<T>(this IEnumerable<char> str, Func<string, T> convert) =>
            new string(str.ToArray()).Replace(" ", "").Split(",").Select(x => convert(x)).ToArray();

        public static T? GetAttribute<T>(this MemberInfo memberInfo) where T : Attribute =>
            (T?)Attribute.GetCustomAttribute(memberInfo, typeof(T));
    }
}
