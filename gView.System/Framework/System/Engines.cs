using gView.GraphicsEngine.Abstraction;
using System.Collections.Generic;
using System.Linq;

namespace gView.Framework.system
{
    static public class Engines
    {
        private static List<IGraphicsEngine> _registeredGraphicsEngines = new List<IGraphicsEngine>();
        static public IEnumerable<IGraphicsEngine> RegisteredGraphcisEngines()
        {
            return _registeredGraphicsEngines.ToArray();
        }

        static public IEnumerable<string> RegisteredGraphicsEngineNames()
        {
            return _registeredGraphicsEngines.Select(e => e.EngineName);
        }

        static public void RegisterGraphcisEngine(this IGraphicsEngine engine)
        {
            if (engine != null &&
                _registeredGraphicsEngines.Where(e => e.EngineName == engine.EngineName).Count() == 0)
            {
                _registeredGraphicsEngines.Add(engine);
            }
        }
    }
}
