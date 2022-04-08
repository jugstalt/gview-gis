using System;
using System.Drawing;
using System.Windows.Forms;

namespace gView.Framework.system.UI
{
    public class VistaStyleTreeView : TreeView
    {
        public VistaStyleTreeView()
        {
            base.DrawMode = TreeViewDrawMode.OwnerDrawAll;
            base.DrawNode += new DrawTreeNodeEventHandler(VistaStyleTreeView_DrawNode);
        }

        void VistaStyleTreeView_DrawNode(object sender, DrawTreeNodeEventArgs e)
        {
            System.Drawing.Graphics g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            TreeView tv = (TreeView)sender;
            if (e.Node.IsVisible == false)
            {
                return;
            }

            int indent = tv.Indent;
            int depth = e.Node.Level + 1;
            int selectedDepth = 1;
            if (e.Node.TreeView.SelectedNode != null)
            {
                selectedDepth = e.Node.TreeView.SelectedNode.Level + 1;
            }

            int nodeIndent = indent * depth;

            Font font = tv.Font;
            if (e.Node.NodeFont != null)
            {
                font = e.Node.NodeFont;
            }

            Brush fgBrush = Brushes.Black;
            Brush bgBrush = Brushes.White;
            //if (e.State == TreeNodeStates.Selected)
            //{
            //    fgBrush = SystemBrushes.HighlightText;
            //}

            g.FillRectangle(bgBrush, e.Bounds);
            int imageWidth = 0;
            if (tv.ImageList != null)
            {
                imageWidth = tv.ImageList.ImageSize.Width;
            }

            if (e.Node.IsSelected || e.State == TreeNodeStates.Marked)
            {
                SizeF size = g.MeasureString(e.Node.Text, font);
                RectangleF rect = new RectangleF(e.Bounds.X, e.Bounds.Y + 1, e.Bounds.Width - 1, e.Bounds.Height - 2); //new RectangleF(e.Bounds.Left + nodeIndent + imageWidth, e.Bounds.Top, size.Width, size.Height);
                using (System.Drawing.Drawing2D.GraphicsPath path = this.GetRoundedRect(rect, 3f))
                using (System.Drawing.Drawing2D.LinearGradientBrush brush = new System.Drawing.Drawing2D.LinearGradientBrush(
                    rect, Color.FromArgb(234, 243, 252), Color.FromArgb(206, 228, 254), 90f))
                using (Pen pen = new Pen(Color.FromArgb(148, 183, 226)))
                {
                    g.FillPath(brush, path);
                    g.DrawPath(pen, path);
                }
            }
            else if (e.State == TreeNodeStates.Hot ||
                e.State == TreeNodeStates.Selected)
            {
                SizeF size = g.MeasureString(e.Node.Text, font);
                RectangleF rect = new RectangleF(e.Bounds.X, e.Bounds.Y + 1, e.Bounds.Width - 1, e.Bounds.Height - 2); //new RectangleF(e.Bounds.Left + nodeIndent + imageWidth, e.Bounds.Top, size.Width, size.Height);
                using (System.Drawing.Drawing2D.GraphicsPath path = this.GetRoundedRect(rect, 3f))
                using (System.Drawing.Drawing2D.LinearGradientBrush brush = new System.Drawing.Drawing2D.LinearGradientBrush(
                    rect, Color.FromArgb(254, 254, 254), Color.FromArgb(235, 243, 253), 90f))
                using (Pen pen = new Pen(Color.FromArgb(201, 224, 252)))
                {
                    g.FillPath(brush, path);
                    g.DrawPath(pen, path);
                }
            }

            g.DrawString(e.Node.Text, font, fgBrush, e.Bounds.Left + nodeIndent + imageWidth, e.Bounds.Top + 1);

            int expanderSize = 9;

            Rectangle expanderBounds = new Rectangle(e.Bounds.Left + nodeIndent - indent / 2 - expanderSize / 2, e.Bounds.Top + tv.ItemHeight / 2 - expanderSize / 2, expanderSize, expanderSize);
            if (e.Node.Nodes.Count > 0)
            {
                if (e.Node.IsExpanded)
                {
                    g.DrawImage(gView.Win.Sys.UI.Properties.Resources.tree_minus, expanderBounds, new Rectangle(0, 0, 9, 9), GraphicsUnit.Pixel);
                }
                else
                {
                    g.DrawImage(gView.Win.Sys.UI.Properties.Resources.tree_plus, expanderBounds, new Rectangle(0, 0, 9, 9), GraphicsUnit.Pixel);
                }
            }

            if (this.UIImageList != null)
            {
                Image img = null;
                int imageIndex = tv.ImageIndex;
                if (e.Node.ImageIndex != -1)
                {
                    imageIndex = e.Node.ImageIndex;
                }

                img = this.UIImageList[imageIndex];
                g.DrawImageUnscaled(img, e.Bounds.Left + nodeIndent, e.Bounds.Top + e.Bounds.Height / 2 - img.Height / 2);
            }
        }

        public IUIImageList UIImageList
        {
            get;
            set;
        }

        #region Get the desired Rounded Rectangle path.
        private System.Drawing.Drawing2D.GraphicsPath GetRoundedRect(RectangleF baseRect,
           float radius)
        {
            // if corner radius is less than or equal to zero, 
            // return the original rectangle 
            if (radius <= 0.0F)
            {
                System.Drawing.Drawing2D.GraphicsPath mPath = new System.Drawing.Drawing2D.GraphicsPath();
                mPath.AddRectangle(baseRect);
                mPath.CloseFigure();
                return mPath;
            }

            // if the corner radius is greater than or equal to 
            // half the width, or height (whichever is shorter) 
            // then return a capsule instead of a lozenge 
            if (radius >= (Math.Min(baseRect.Width, baseRect.Height)) / 2.0)
            {
                return GetCapsule(baseRect);
            }

            // create the arc for the rectangle sides and declare 
            // a graphics path object for the drawing 
            float diameter = radius * 2.0F;
            SizeF sizeF = new SizeF(diameter, diameter);
            RectangleF arc = new RectangleF(baseRect.Location, sizeF);
            System.Drawing.Drawing2D.GraphicsPath path = new System.Drawing.Drawing2D.GraphicsPath();

            // top left arc 
            path.AddArc(arc, 180, 90);

            // top right arc 
            arc.X = baseRect.Right - diameter;
            path.AddArc(arc, 270, 90);

            // bottom right arc 
            arc.Y = baseRect.Bottom - diameter;
            path.AddArc(arc, 0, 90);

            // bottom left arc
            arc.X = baseRect.Left;
            path.AddArc(arc, 90, 90);

            path.CloseFigure();
            return path;
        }
        #endregion

        #region Gets the desired Capsular path.
        private System.Drawing.Drawing2D.GraphicsPath GetCapsule(RectangleF baseRect)
        {
            float diameter;
            RectangleF arc;
            System.Drawing.Drawing2D.GraphicsPath path = new System.Drawing.Drawing2D.GraphicsPath();
            try
            {
                if (baseRect.Width > baseRect.Height)
                {
                    // return horizontal capsule 
                    diameter = baseRect.Height;
                    SizeF sizeF = new SizeF(diameter, diameter);
                    arc = new RectangleF(baseRect.Location, sizeF);
                    path.AddArc(arc, 90, 180);
                    arc.X = baseRect.Right - diameter;
                    path.AddArc(arc, 270, 180);
                }
                else if (baseRect.Width < baseRect.Height)
                {
                    // return vertical capsule 
                    diameter = baseRect.Width;
                    SizeF sizeF = new SizeF(diameter, diameter);
                    arc = new RectangleF(baseRect.Location, sizeF);
                    path.AddArc(arc, 180, 180);
                    arc.Y = baseRect.Bottom - diameter;
                    path.AddArc(arc, 0, 180);
                }
                else
                {
                    // return circle 
                    path.AddEllipse(baseRect);
                }
            }
            catch (Exception /*ex*/)
            {
                path.AddEllipse(baseRect);
            }
            finally
            {
                path.CloseFigure();
            }
            return path;
        }
        #endregion
    }
}
