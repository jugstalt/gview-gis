using gView.Blazor.Models.Dialogs;

namespace gView.DataExplorer.Razor.Components.Dialogs.Models;

/// <summary>
/// Model for the "MxlUtil" explorer tool. Holds the selected utility name
/// (<see cref="Utility"/>, one of the <c>IMxlUtility.Name</c> values) and the
/// utility specific parameters. Only the parameters of the selected utility are
/// evaluated when the command is built.
/// </summary>
public class MxlUtilToolModel : IDialogResultItem
{
    public string Utility { get; set; } = "MxlDatasets";

    #region MxlDatasets

    public string MxlDatasets_Mxl { get; set; } = "";
    public string MxlDatasets_Command { get; set; } = "info";
    public string MxlDatasets_OutMxl { get; set; } = "";
    public int MxlDatasets_DatasetIndex { get; set; } = -1;
    public string MxlDatasets_ParameterName { get; set; } = "";
    public string MxlDatasets_NewValue { get; set; } = "";

    #endregion

    #region MxlToFdb

    public string MxlToFdb_Mxl { get; set; } = "";
    public string MxlToFdb_TargetConnectionString { get; set; } = "";
    public string MxlToFdb_TargetGuid { get; set; } = "sqlite";
    public string MxlToFdb_OutMxl { get; set; } = "";
    public string MxlToFdb_DontCopyFeaturesFrom { get; set; } = "";

    #endregion

    #region PublishService

    public string PublishService_Mxl { get; set; } = "";
    public string PublishService_Server { get; set; } = "";
    public string PublishService_Service { get; set; } = "";
    public string PublishService_Client { get; set; } = "";
    public string PublishService_Secret { get; set; } = "";

    #endregion

    #region ConvertAprx

    public string ConvertAprx_Input { get; set; } = "";
    public bool ConvertAprx_InputIsFolder { get; set; } = false;
    public string ConvertAprx_Output { get; set; } = "";
    public bool ConvertAprx_OutputIsFolder { get; set; } = false;
    public bool ConvertAprx_Silent { get; set; } = false;
    public string ConvertAprx_Dataset { get; set; } = "";
    public string ConvertAprx_DatasetConnectionString { get; set; } = "";
    public string ConvertAprx_AllowOverlappingLabelsPriority { get; set; } = "";
    public string ConvertAprx_CompositionModeCopyLayers { get; set; } = "";

    #endregion
}
