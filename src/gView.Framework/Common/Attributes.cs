using System;

namespace gView.Framework.Common
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
