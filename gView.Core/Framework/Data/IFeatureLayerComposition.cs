using System;
using System.Collections.Generic;
using System.Text;

namespace gView.Framework.Data
{
    public interface IFeatureLayerComposition
    {
        FeatureLayerCompositionMode CompositionMode { get; set; } 
        float CompositionModeCopyTransparency { get; set; }
    }
}
