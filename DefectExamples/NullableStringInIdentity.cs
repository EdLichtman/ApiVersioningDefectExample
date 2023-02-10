using System.Security.Principal;

namespace TestMinimalApiSwagger.DefectExamples
{
    public class NullableStringInIdentity
    {
        public string TestThisShouldBeNonNullable(IIdentity identity)
        {
            return identity.Name;
        }
    }
}
