using NSubstitute.Core;
using NSubstitute.Exceptions;

namespace Swick.Cache.Tests
{
    public class Assert: Xunit.Assert
    {
        public static void IsNSubstituteMock(object obj)
            => True(IsMock(obj), "Expected an NSubstitute mock");

        public static void IsNotNSubstituteMock(object obj)
            => False(IsMock(obj), "Did not expect an NSubstitute mock");

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
