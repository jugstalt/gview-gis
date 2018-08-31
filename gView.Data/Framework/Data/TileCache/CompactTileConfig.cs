using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace gView.Data.Framework.Data.TileCache
{
    public class CompactTileConfig
    {
        public int Epsg { get; set; }
        public double Dpi { get; set; }
        public double[] Origin { get; set; }

        public double[] Extent { get; set; }

        public string Orientation { get; set; }

        public int[] TileSize { get; set; }

        public string Format { get; set; }

        public IEnumerable<LevelConfig> Levels { get; set; }

        public class LevelConfig
        {
            public int Level{get;set;}
            public double Scale { get; set; }
        } 
    }
}
