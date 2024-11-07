using System;

namespace gView.Framework.Common.Reflection
{
    public class HtmlLinkAttribute : Attribute
    {
        public HtmlLinkAttribute(string template)
        {
            LinkTemplate = template;
        }

        public string LinkTemplate { get; set; }
    }
}
