using System;

namespace gView.Framework.Carto.Abstraction;

public interface IPropertyPageDefinition
{
    Type InterfaceType { get; }
    Type InstanceType { get; }
    Type PropertyPageType { get; }
}
