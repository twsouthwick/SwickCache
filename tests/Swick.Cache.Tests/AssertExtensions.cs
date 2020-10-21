using NSubstitute.Core;
using NSubstitute.Exceptions;
using Xunit;

namespace Swick.Cache.Tests
{
    public class AssertExtensions : Assert
    {
        public static void IsNSubstituteMock(object obj)
            => Assert.True(IsMock(obj), "Expected an NSubstitute mock");

        public static void IsNotNSubstituteMock(object obj)
            => Assert.False(IsMock(obj), "Did not expect an NSubstitute mock");

        private static bool IsMock(object obj)
        {
            try
            {
                var router = SubstitutionContext.Current.GetCallRouterFor(obj);

                return router != null;
            }
            catch (NotASubstituteException)
            {
                return false;
            }
        }
    }
}
