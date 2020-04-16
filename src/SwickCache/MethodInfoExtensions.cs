using System.Reflection;
using System.Text;

namespace Swick.Cache
{
    internal static class MethodInfoExtensions
    {
        public static string GetKey(this MethodInfo method, params object[] args)
        {
            var sb = new StringBuilder();

            sb.Append(method.DeclaringType.FullName);
            sb.Append('.');
            sb.Append(method.DeclaringType.GetType().FullName);
            sb.Append('.');
            sb.Append(method.Name);
            sb.Append('.');

            foreach (var a in args)
            {
                sb.Append(a);
                sb.Append(',');
            }

            return sb.ToString();
        }
    }
}
