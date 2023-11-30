//using System;
//using System.Collections.Generic;
//using System.Text;

//namespace gView.Framework.Carto.Rendering
//{
//    //[RegisterPlugIn("2C8BF397-D96B-47B6-803F-1190E1808A51")]
//    public class FilterDependentLabelRenderer
//    {
//        private RendererList _renderers;

//        public FilterDependentLabelRenderer()
//        {
//            _renderers = new RendererList();
//        }

//        #region IGroupRenderer Member

//        public ILabelRendererGroup Renderers
//        {
//            get { return _renderers; }
//        }

//        #endregion

//        #region ILabelRenderer Member

//        public void PrepareQueryFilter(IDisplay display, gView.Framework.Data.IFeatureLayer layer, gView.Framework.Data.IQueryFilter filter)
//        {
//            foreach (ILabelRenderer renderer in _renderers)
//            {
//                if (renderer == null) continue;

//                renderer.PrepareQueryFilter(display, layer, filter);
//            }
//        }

//        public bool CanRender(gView.Framework.Data.IFeatureLayer layer, IMap map)
//        {
//            return true;
//        }

//        public string Name
//        {
//            get { return "Scale Dependent/Group Labelrenderer"; }
//        }

//        public LabelRenderMode RenderMode
//        {
//            get
//            {
//                return LabelRenderMode.UseRenderPriority;
//            }
//        }

//        public int RenderPriority
//        {
//            get { return 0; }
//        }

//        public void Draw(IDisplay disp, gView.Framework.Data.IFeature feature)
//        {
//            foreach (ILabelRenderer renderer in _renderers)
//            {
//                if (renderer == null) continue;
//                renderer.Draw(disp, feature);
//            }
//        }

//        #endregion

//        #region Classes

//        private class RendererList : List<ILabelRenderer>, ILabelRendererGroup
//        {
//            public new void Add(ILabelRenderer renderer)
//            {
//                if (renderer == null) return;

//                if (renderer is ScaleRenderer)
//                {
//                    base.Add(renderer);
//                }
//                else
//                {
//                    base.Add(new ScaleRenderer(renderer));
//                }
//            }
//        }

//        #endregion
//    }
//}
