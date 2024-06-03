//using gView.Cmd.Core;
//using gView.Cmd.Core.Abstraction;
//using gView.Framework.Core.Common;
//using gView.Framework.Core.Data;

//namespace gView.Cmd.LuceneServer.Lib;

//public class RemoveCategoryCommand : ICommand
//{
//    public string Name => "LuceneServer.RemoveCategory";

//    public string Description => "Removes a category from a lucene server index";

//    public string ExecutableName => "";

//    public IEnumerable<ICommandParameterDescription> ParameterDescriptions =>
//        [
//            new RequiredCommandParameter<IFeatureClass>("source")
//            {
//                Description = "Source FeatureClass"
//            }
//        ];

//    public Task<bool> Run(IDictionary<string, object> parameters, ICancelTracker? cancelTracker = null, ICommandLogger? logger = null)
//    {
//        throw new NotImplementedException();
//    }
//}
