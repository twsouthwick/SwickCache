using System.Reflection;

namespace Swick.Cache
{
    public interface ICacheKeyProvider
    {
        string GetKey(MethodInfo method, params object[] args);
    }
}