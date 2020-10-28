using System.Reflection;
using System.Text;

namespace Swick.Cache
{
    public class CacheKeyProvider : ICacheKeyProvider
    {
        protected virtual void BuildKey(KeyBuilder builder)
        {
        }

        public string GetKey(MethodInfo method, params object[] args)
        {
            var sb = new StringBuilder();
            var builder = new KeyBuilder(sb);

            BuildKey(builder);

            builder.Append(method.DeclaringType.FullName);
            builder.Append(method.Name);

            foreach (var a in args)
            {
                builder.Append(a);
            }

            return builder.ToString();
        }

        protected readonly struct KeyBuilder
        {
            private readonly StringBuilder _builder;

            internal KeyBuilder(StringBuilder builder)
            {
                _builder = builder;
            }

            public void Append(object o)
            {
                _builder.Append(o);
                _builder.Append('*');
            }

            public override string ToString() => _builder.ToString();
        }
    }
}
