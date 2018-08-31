using System;
using System.Collections.Generic;

namespace Proj4Net.Utility
{
    public class CoordinateReferenceSystemCache
    {
        private static readonly IDictionary<String, CoordinateReferenceSystem> ProjCache = new Dictionary<String, CoordinateReferenceSystem>();
        private static readonly CoordinateReferenceSystemFactory CrsFactory = new CoordinateReferenceSystemFactory();

        // TODO: provide limit on number of items in cache (LRU)
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        ///<exception cref="UnsupportedParameterException"/>
        /// <exception cref="InvalidValueException"></exception>
        ///<exception cref="UnknownAuthorityCodeException"></exception>
        public CoordinateReferenceSystem CreateFromName(String name)
        {
            CoordinateReferenceSystem proj = null;//(CoordinateReferenceSystem) ProjCache.get(name);
            if (ProjCache.TryGetValue(name, out proj))
            {
                proj = CrsFactory.CreateFromName(name);
                ProjCache.Add(name, proj);
            }
            return proj;
        }

    }
}