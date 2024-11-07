namespace gView.Framework.Core.MapServer
{
    public class InterpreterCapabilities
    {
        public InterpreterCapabilities(Capability[] capabilites)
        {
            Capabilities = capabilites;
        }

        public Capability[] Capabilities
        {
            get;
            private set;
        }

        #region Classes

        public enum Method { Get = 0, Post = 1 };

        public class Capability
        {
            public Capability(string name)
                : this(name, Method.Get, "1.0")
            {
            }
            public Capability(string name, Method method, string version)
            {
                Name = name;
                Method = method;
                Version = version;
            }

            public string Name { get; private set; }
            public Method Method { get; private set; }
            public string Version { get; private set; }
            public string RequestText { get; protected set; }
        }

        public class SimpleCapability : Capability
        {
            public SimpleCapability(string name, string link, string version)
                : this(name, Method.Get, link, version)
            {
            }
            public SimpleCapability(string name, Method method, string requestText, string version)
                : base(name, method, version)
            {
                RequestText = requestText;
            }
        }

        public class LinkCapability : Capability
        {
            public LinkCapability(string name, string requestLink, string version)
                : this(name, Method.Get, requestLink, version)
            {
            }
            public LinkCapability(string name, Method method, string requestLink, string version)
                : base(name, method, version)
            {
                RequestText = requestLink;
            }
        }

        #endregion
    }
}
