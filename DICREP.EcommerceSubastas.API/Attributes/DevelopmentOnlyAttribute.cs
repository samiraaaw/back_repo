using System;

namespace DICREP.EcommerceSubastas.API.Attributes
{

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class DevelopmentOnlyAttribute : Attribute
    {
    }

}
