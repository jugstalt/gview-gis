using System;
using System.Collections.Generic;
using System.Text;

namespace gView.Interoperability.GeoServices.Rest.Reflection
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class FormInputAttribute : Attribute
    {
        public FormInputAttribute(InputTypes inputType = InputTypes.Text)
        {
            this.InputType = inputType;
        }

        public InputTypes InputType { get; set; }

        public string[] Values { get; set; }

        public enum InputTypes
        {
            Ignore,
            Text,
            TextBox,
            TextBox10,
            Hidden,
            Password
        }
    }
}
