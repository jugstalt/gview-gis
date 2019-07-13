using System;
using System.Collections.Generic;
using System.Text;
using gView.Framework.UI;
using gView.Framework.system;
using gView.Framework.Network;
using gView.Framework.Data;
using gView.Framework.Geometry;
using gView.Plugins.Network.Graphic;
using gView.Framework.Carto;
using gView.Framework.UI.Events;
using System.Threading;
using gView.Framework.UI.Dialogs;
using System.Threading.Tasks;

namespace gView.Plugins.Network
{
    [RegisterPlugIn("158C5F28-B987-4d16-8C9D-A1FC6E70EB56")]
    public class TraceNetwork : ITool
    {
        private IMapDocument _doc = null;
        private Module _module = null;

        #region ITool Member

        public string Name
        {
            get { return "Trace Network"; }
        }

        public bool Enabled
        {
            get
            {
                if (_module != null && _module.SelectedNetworkTracer != null)
                {
                    return _module.SelectedNetworkTracer.CanTrace(TracerInput());
                }
                return false;
            }
        }

        public string ToolTip
        {
            get { return "Trace Network"; }
        }

        public ToolType toolType
        {
            get { return ToolType.command; }
        }

        public object Image
        {
            get { return global::gView.Win.Plugins.Network.Properties.Resources.path3; }
        }

        public void OnCreate(object hook)
        {
            if (hook is IMapDocument)
            {
                _doc = (IMapDocument)hook;
                _module = Module.GetModule(_doc);
            }
        }

        async public Task<bool> OnEvent(object MapEvent)
        {
            if (_module != null && _module.SelectedNetworkTracer != null)
            {
                if (_module.SelectedNetworkTracer is INetworkTracerProperties &&
                   await ((INetworkTracerProperties)_module.SelectedNetworkTracer).NetworkTracerProperties(_module.SelectedNetworkFeatureClass, TracerInput()) != null)
                {
                    FormTracerProperties dlg = new FormTracerProperties(
                        await ((INetworkTracerProperties)_module.SelectedNetworkTracer).NetworkTracerProperties(_module.SelectedNetworkFeatureClass, TracerInput()));
                    if (dlg.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                        return false;
                }
                if (_module.SelectedNetworkTracer is IProgressReporterEvent)
                {
                    NetworkProgressReporter reporter = new NetworkProgressReporter(_doc);

                    FormTaskProgress dlg = new FormTaskProgress();
                    dlg.ShowProgressDialog(reporter, this.Trace(reporter));
                }
                else
                {
                    await Trace(null);
                }
                ((MapEvent)MapEvent).drawPhase = DrawPhase.Graphics;
                ((MapEvent)MapEvent).refreshMap = true;
            }

            return true;
        }

        #endregion

        async private Task Trace(object arg)
        {
            NetworkProgressReporter reporter = arg as NetworkProgressReporter;
            if (_doc == null || _module == null)
                return;

            if (reporter != null && _module.SelectedNetworkTracer is IProgressReporterEvent)
                ((IProgressReporterEvent)_module.SelectedNetworkTracer).ReportProgress += reporter.FireProgressReporter;


            NetworkTracerOutputCollection outputCollection = await _module.SelectedNetworkTracer.Trace(
                                 _module.SelectedNetworkFeatureClass,
                                 TracerInput(),
                                 reporter != null ? reporter.CancelTracker : null);

            IDisplay display = (IDisplay)_doc.FocusMap;
            _module.RemoveAllNetworkGraphicElements(display);

            if (outputCollection != null)
            {
                foreach (INetworkTracerOutput output in outputCollection)
                {
                    if (output is NetworkEdgeCollectionOutput)
                    {
                        IFeatureCursor cursor = await _module.NetworkPathEdges((NetworkEdgeCollectionOutput)output);
                        if (cursor == null)
                            return;
                        IFeature feature;
                        while ((feature = await cursor.NextFeature()) != null)
                        {
                            if (!(feature.Shape is IPolyline))
                                continue;
                            display.GraphicsContainer.Elements.Add(new GraphicNetworkPathEdge((IPolyline)feature.Shape));
                        }
                    }
                    else if (output is NetworkPolylineOutput)
                    {
                        display.GraphicsContainer.Elements.Add(new GraphicNetworkPathEdge(((NetworkPolylineOutput)output).Polyline));
                    }
                    else if (output is NetworkFlagOutput)
                    {
                        NetworkFlagOutput flag = (NetworkFlagOutput)output;
                        string text = flag.UserData != null ? flag.UserData.ToString() : "Flag";
                        display.GraphicsContainer.Elements.Add(new GraphicFlagPoint(flag.Location, text));
                    }
                }
            }
        }

        private NetworkTracerInputCollection TracerInput()
        {
            if (_module == null)
                return null;

            NetworkTracerInputCollection input = new NetworkTracerInputCollection();
            if (_module.StartNodeIndex >= 0)
                input.Add(new NetworkSourceInput(_module.StartNodeIndex));
            if (_module.EndNodeIndex >= 0)
                input.Add(new NetworkSinkInput(_module.EndNodeIndex));
            if (_module.GraphWeight != null)
                input.Add(new NetworkWeighInput(_module.GraphWeight, _module.WeightApplying));
            if (_module.StartEdgeIndex >= 0)
                input.Add(new NetworkSourceEdgeInput(_module.StartEdgeIndex, _module.StartPoint));

            return input;
        }
    }
}
