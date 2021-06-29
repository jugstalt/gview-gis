using gView.Framework.Carto.UI;
using gView.Framework.Symbology.UI.Controls;
using gView.Framework.Sys.UI.Extensions;
using gView.Framework.UI.Symbology.Dialogs;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace gView.Framework.Symbology.UI
{
    internal class ColorTypeEditor : UITypeEditor 
	{
		public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
		{
			return UITypeEditorEditStyle.DropDown; 
		}

		public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
		{
			IWindowsFormsEditorService wfes=provider.GetService(
				typeof(IWindowsFormsEditorService)) as IWindowsFormsEditorService;

            if (wfes != null)
            {
                OfficeColorPicker picker = new OfficeColorPicker(wfes, (Color)value);
                picker.AllowNoColor = true;
                picker.Height = picker.PreferredHeight;
                wfes.DropDownControl(picker);
                return picker.Color;
            }

			return value;
		}

		public override void PaintValue(PaintValueEventArgs e)
		{
			using (SolidBrush brush = new SolidBrush((Color)e.Value))
			{
				e.Graphics.FillRectangle(brush, e.Bounds);
			}
		}

		public override bool GetPaintValueSupported(ITypeDescriptorContext context)
		{
			return true;
		}

	}

	internal class DashStyleTypeEditor : UITypeEditor 
	{
		public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
		{
			return UITypeEditorEditStyle.DropDown;
		}

		public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
		{
			IWindowsFormsEditorService wfes=provider.GetService(
				typeof(IWindowsFormsEditorService)) as IWindowsFormsEditorService;

			if(wfes!=null) 
			{
				Form_UITypeEditor_DashStyle dlg=new Form_UITypeEditor_DashStyle(wfes,(DashStyle)value);
				dlg.TopLevel=false;
				wfes.DropDownControl(dlg);
				value=dlg.DashStyle;
			}
			return value;
		}

		public override bool GetPaintValueSupported(ITypeDescriptorContext context)
		{
			return true;
		}

		public override void PaintValue(PaintValueEventArgs e)
		{
			using(Pen pen=new Pen(Color.Black,1)) 
			{
				pen.DashStyle=(DashStyle)e.Value;
				e.Graphics.DrawLine(pen,e.Bounds.Left+2,e.Bounds.Height/2,e.Bounds.Right-4,e.Bounds.Height/2);
			}
		}
	}

    internal class PenWidthTypeEditor : UITypeEditor
    {
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.DropDown;
        }

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            IWindowsFormsEditorService wfes = provider.GetService(
                typeof(IWindowsFormsEditorService)) as IWindowsFormsEditorService;

            if (wfes != null)
            {
                gView.Framework.Symbology.UI.Controls.OfficeLineWidthPicker picker = new gView.Framework.Symbology.UI.Controls.OfficeLineWidthPicker(wfes);
                picker.Height = picker.PreferredHeight;
                wfes.DropDownControl(picker);
                return picker.PenWidth != -1 ? picker.PenWidth : value;
            }
            return value;
        }

        public override bool GetPaintValueSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        public override void PaintValue(PaintValueEventArgs e)
        {
            
            using (Pen pen = new Pen(Color.Black, Math.Min(e.Bounds.Height,(float)e.Value)))
            {
                e.Graphics.DrawLine(pen, e.Bounds.Left + 1, e.Bounds.Height / 2, e.Bounds.Right - 1, e.Bounds.Height / 2);
            }
        }
    }

	internal class HatchStyleTypeEditor : UITypeEditor
	{
		public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
		{
			return UITypeEditorEditStyle.DropDown;
		}

		public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
		{
			IWindowsFormsEditorService wfes=provider.GetService(
				typeof(IWindowsFormsEditorService)) as IWindowsFormsEditorService;

			if(wfes!=null) 
			{
				Form_UITypeEditor_DashStyle dlg=new Form_UITypeEditor_DashStyle(wfes,(HatchStyle)value);
				dlg.TopLevel=false;
				wfes.DropDownControl(dlg);
				value=dlg.HatchStyle;
			}
			return value;
		}

	}

	internal class LineSymbolTypeEditor : UITypeEditor 
	{
		public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
		{
			return UITypeEditorEditStyle.DropDown;
		}

		public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
		{
			IWindowsFormsEditorService wfes=provider.GetService(
				typeof(IWindowsFormsEditorService)) as IWindowsFormsEditorService;

			if(wfes!=null) 
			{
				ISymbol symbol=(ISymbol)value;
				if(symbol==null) 
				{
					symbol=new SimpleLineSymbol();
				}

				Form_UITypeEditor_Color dlg=new Form_UITypeEditor_Color(wfes, symbol);
				wfes.DropDownControl(dlg.panelSymbol);
				return dlg.Symbol;
			}
			return value;
		}

		public override bool GetPaintValueSupported(ITypeDescriptorContext context)
		{
			return true;
		}

		public override void PaintValue(PaintValueEventArgs e)
		{
			if(!(e.Value is ISymbol))
            {
                return;
            }

			using (var bitmap = GraphicsEngine.Current.Engine.CreateBitmap(e.Bounds.Width, e.Bounds.Height))
			using (var canvas = bitmap.CreateCanvas())
			{
				new SymbolPreview(null).Draw(canvas, new GraphicsEngine.CanvasRectangle(0, 0, bitmap.Width, bitmap.Height), (ISymbol)e.Value);
				e.Graphics.DrawImage(bitmap.ToGdiBitmap(), new Point(e.Bounds.X, e.Bounds.Y));
			}
		}

	}

    internal class PointSymbolTypeEditor : UITypeEditor
    {
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.DropDown;
        }

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            IWindowsFormsEditorService wfes = provider.GetService(
                typeof(IWindowsFormsEditorService)) as IWindowsFormsEditorService;

            if (wfes != null)
            {
                ISymbol symbol = (ISymbol)value;
                if (symbol == null)
                {
                    symbol = new SimplePointSymbol();
                }

                Form_UITypeEditor_Color dlg = new Form_UITypeEditor_Color(wfes, symbol);
                wfes.DropDownControl(dlg.panelSymbol);
                return dlg.Symbol;
            }
            return value;
        }

        public override bool GetPaintValueSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        public override void PaintValue(PaintValueEventArgs e)
        {
            if (!(e.Value is ISymbol))
            {
                return;
            }

			using (var bitmap = GraphicsEngine.Current.Engine.CreateBitmap(e.Bounds.Width, e.Bounds.Height))
			using (var canvas = bitmap.CreateCanvas())
			{
				new SymbolPreview(null).Draw(canvas, new GraphicsEngine.CanvasRectangle(0, 0, bitmap.Width, bitmap.Height), (ISymbol)e.Value);
				e.Graphics.DrawImage(bitmap.ToGdiBitmap(), new Point(e.Bounds.X, e.Bounds.Y));
			}
        }
    }

	internal class CharacterTypeEditor : UITypeEditor
	{
		public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
		{
			return UITypeEditorEditStyle.DropDown;
		}
		public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
		{
			GraphicsEngine.Abstraction.IFont iFont = null;
			if (context.Instance is TrueTypeMarkerSymbol)
			{
				iFont = ((TrueTypeMarkerSymbol)context.Instance).Font;
			}
			if (context.Instance is CustomClass &&
				((CustomClass)context.Instance).ObjectInstance is TrueTypeMarkerSymbol)
			{
				iFont = ((TrueTypeMarkerSymbol)((CustomClass)context.Instance).ObjectInstance).Font;
			}

			if (iFont == null)
			{
				return value;
			}

			IWindowsFormsEditorService wfes = provider.GetService(
				typeof(IWindowsFormsEditorService)) as IWindowsFormsEditorService;

			if (wfes != null)
			{
				using (var font = new Font(iFont.Name, iFont.Size))
				{
					Form_UITypeEditor_Character dlg = new Form_UITypeEditor_Character(wfes, font, (byte)value);
					wfes.DropDownControl(dlg.panelChars);
					return dlg.Charakter;
				}
			}
			return value;
		}
	}

    internal class FileTypeEditor : UITypeEditor
    {
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
		{
			return UITypeEditorEditStyle.Modal;
		}
        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {

            string filename = String.Empty;
            if (context.Instance is RasterMarkerSymbol)
            {
                filename = ((RasterMarkerSymbol)context.Instance).Filename;
            }
            if (context.Instance is CustomClass &&
                ((CustomClass)context.Instance).ObjectInstance is RasterMarkerSymbol)
            {
                filename = ((RasterMarkerSymbol)((CustomClass)context.Instance).ObjectInstance).Filename;
            }

			#region Check Resources First

			var currentMap = SymbolPreview.CurrentMap;
			if(currentMap?.ResourceContainer!=null && currentMap.ResourceContainer.HasResources)
            {
				var resourcePicker = new FormMapResourcePicker(currentMap);
				resourcePicker.SetAbortButtonText("Or Select File...");

				switch(resourcePicker.ShowDialog())
                {
					case DialogResult.OK:
						return $"resource:{ resourcePicker.SelectedResourceName }";
					case DialogResult.Abort:
						// Open Filedialog
						break;
					default:
						return filename;
                }
            }

			#endregion

			OpenFileDialog dlg = new OpenFileDialog();
            dlg.FileName = filename;

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                return dlg.FileName;
            }
            return filename;
        }
    }

    internal class ColorGradientEditor : UITypeEditor
    {
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.Modal;
        }
        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            FormColorGradient dlg = new FormColorGradient();
            ColorGradient gradient = new ColorGradient(GraphicsEngine.ArgbColor.Red, GraphicsEngine.ArgbColor.Blue);

            if (context.Instance is GradientFillSymbol)
            {
                gradient =
                dlg.ColorGradient = ((GradientFillSymbol)context.Instance).ColorGradient;
            }
            if (context.Instance is CustomClass &&
                ((CustomClass)context.Instance).ObjectInstance is GradientFillSymbol)
            {
                gradient =
                dlg.ColorGradient = ((GradientFillSymbol)((CustomClass)context.Instance).ObjectInstance).ColorGradient;
            }

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                return dlg.ColorGradient;
            }
            return gradient;
        }
    }
}
