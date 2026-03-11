using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MinetaleConverter.Base.Logic.Extensions
{
    public static class ClassExtensions
    {
        public static T? GetAttribute<T>(this MemberInfo memberInfo) where T : Attribute =>
            (T?)Attribute.GetCustomAttribute(memberInfo, typeof(T));
    }
}
