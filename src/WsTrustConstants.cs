namespace Abc.ServiceModel.Caching
{
    internal class WsTrustConstants
    {
        public sealed class Prefixes
        {
            public const string WsTrust2005 = "t";
            public const string WsTrust13 = "trust";
            public const string WsTrust14 = "tr";

            private Prefixes()
            {
            }
        }

        public sealed class Namespaces
        {
            public const string WsTrust2005 = "http://schemas.xmlsoap.org/ws/2005/02/trust";
            public const string WsTrust13 = "http://docs.oasis-open.org/ws-sx/ws-trust/200512";
            public const string WsTrust14 = "http://docs.oasis-open.org/ws-sx/ws-trust/200802";

            private Namespaces()
            {
            }
        }

        public sealed class ElementNames
        {
            public const string OnBehalfOf = "OnBehalfOf";
            public const string ActAs = "ActAs";

            private ElementNames()
            {
            }
        }
    }
}