using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Microsoft.Win32;
using gView.Framework;
using gView.Framework.Carto;
using gView.Framework.system;
using gView.Framework.UI;
using gView.Framework.Data;
using gView.Framework.IO;
using gView.Framework.Symbology;

namespace gView.Framework.Carto.UI
{
    public enum TOCModifier { Public, Private }
	/// <summary>
	/// Zusammenfassung für TOCCoClass.
	/// </summary>
	public class TOC : gView.Framework.UI.ITOC,gView.Framework.IO.IPersistable
	{
        public event EventHandler TocChanged = null;

		private List<ITOCElement> _elements;
		private int _pos;
        private TOCModifier _modifier = TOCModifier.Public;

		public IMap _map;

		public TOC(IMap map)
		{
			_elements=new List<ITOCElement>();
			_pos=0;
			_map=map;
		}
        public void Dispose()
        {
        }

        public TOCModifier Modifier
        {
            get { return _modifier; }
            set { _modifier = value; }
        }
        
		#region ITOC Member

		public void Reset()
		{
			_pos=0;
		}

		public gView.Framework.UI.ITOCElement NextVisibleElement
		{
			get
			{
				if(_pos>=_elements.Count) return null;

				ITOCElement element=(ITOCElement)_elements[_pos];
				if(element.ElementType==TOCElementType.ClosedGroup) 
				{
					for(int i=_pos+1;i<_elements.Count;i++) 
					{	
						ITOCElement elemParent=((ITOCElement)_elements[i]).ParentGroup;
						ITOCElement parent=element.ParentGroup;
						while(true) 
						{
							if(parent==elemParent || elemParent==null) 
							{
								_pos=i;
								return element;
							}
							if(parent==null) break;
							parent=parent.ParentGroup;
						}
					}
					_pos=_elements.Count;
				} 
				else 
				{
					_pos++;
				}
				return element;
			}
		}

        public List<ITOCElement> GroupedElements(ITOCElement group)
        {
            List<ITOCElement> elements = new List<ITOCElement>();
            if (group == null || group.ElementType != TOCElementType.OpenedGroup) return elements;

            int index = _elements.IndexOf(group);
            if (index == -1) return elements;

            while (++index < _elements.Count)
            {
                ITOCElement element = _elements[index];
                if (!IsChild(group, element)) break;

                if (element.ParentGroup != group) continue;

                elements.Add(element);
                if (element.ElementType == TOCElementType.OpenedGroup)
                {
                    foreach (ITOCElement childElement in GroupedElements(element))
                    {
                        elements.Add(childElement);
                    }
                }
            }
            return elements;
        }

        public List<ITOCElement> Elements
        {
            get
            {
                List<ITOCElement> elements = new List<ITOCElement>();

                foreach (ITOCElement element in _elements)
                    elements.Add(element);

                return elements;
            }
        }

        public System.Drawing.Bitmap Legend()
        {
            List<ITOCElement> list = new List<ITOCElement>();
            foreach (ITOCElement element in _elements)
            {
                if (element.Layers == null) 
                    continue;
                foreach (ILayer layer in element.Layers)
                {
                    if (layer == null) continue;
                    if (_map != null && _map.Display != null)
                    {
                        if (layer.MinimumScale > 1 && layer.MinimumScale > _map.Display.mapScale) continue;
                        if (layer.MaximumScale > 1 && layer.MaximumScale < _map.Display.mapScale) continue;
                    }
                    if (layer.Visible)
                    {
                        list.Add(element);
                        break;
                    }
                }
            }

            return Legend(list);
        }
        public System.Drawing.Bitmap Legend(List<ITOCElement> elements)
        {
            List<System.Drawing.Bitmap> bitmaps = new List<System.Drawing.Bitmap>();
            System.Drawing.Bitmap bitmap = null;
            System.Drawing.Graphics gr = null;
            try
            {
                foreach (ITOCElement element in elements)
                {
                    System.Drawing.Bitmap bm = Legend(element);
                    if (bm != null) bitmaps.Add(bm);
                }

                if (bitmaps.Count == 0) return new System.Drawing.Bitmap(1, 1);

                int width = 0, height = 0;
                foreach (System.Drawing.Bitmap bm in bitmaps)
                {
                    width = (int)Math.Max(width, bm.Width);
                    height += bm.Height;
                }

                bitmap = new System.Drawing.Bitmap(width, height);
                gr = System.Drawing.Graphics.FromImage(bitmap);
                gr.FillRectangle(System.Drawing.Brushes.White, 0, 0, bitmap.Width, bitmap.Height);


                int y = 0;
                foreach (System.Drawing.Bitmap bm in bitmaps)
                {
                    gr.DrawImage(bm, new System.Drawing.Point(0, y));
                    y += bm.Height;
                }
                return bitmap;
            }
            catch
            {
                if (bitmap != null) bitmap.Dispose();  
                return null;
            }
            finally
            {
                if (gr != null) gr.Dispose();

                foreach (System.Drawing.Bitmap bm in bitmaps)
                {
                    bm.Dispose();
                }
                bitmaps.Clear();
            }
        }
        public System.Drawing.Bitmap Legend(ITOCElement element)
        {
            if (element==null || element.Layers==null || !_elements.Contains(element)) return null;

            System.Drawing.Bitmap bm = new System.Drawing.Bitmap(1,1);
            System.Drawing.Graphics gr = System.Drawing.Graphics.FromImage(bm);
            System.Drawing.Font font1 = new System.Drawing.Font("Arial", 8, System.Drawing.FontStyle.Bold);
            System.Drawing.Font font2 = new System.Drawing.Font("Arial", 8);
            try
            {

                int height = 0, width = 130;
                List<object> items = new List<object>();
                foreach (ILayer layer in element.Layers)
                {
                    if (layer is IWebServiceLayer && layer.Class is IWebServiceClass)
                    {
                        IWebServiceClass wClass = layer.Class as IWebServiceClass;
                        if (wClass.LegendRequest(_map.Display))
                        {
                            System.Drawing.Bitmap lBm = wClass.Legend;
                            if (lBm == null) continue;
                            height += lBm.Height;
                            items.Add(lBm);
                        }
                    }
                    else if (layer is IFeatureLayer && ((IFeatureLayer)layer).FeatureRenderer is ILegendGroup)
                    {
                        IFeatureLayer fLayer = layer as IFeatureLayer;
                        ILegendGroup lGroup = fLayer.FeatureRenderer as ILegendGroup;

                        width = (int)Math.Max(40 + gr.MeasureString(element.Name, font1).Width, width);
                        for (int i = 0; i < lGroup.LegendItemCount; i++)
                        {
                            ILegendItem lItem = lGroup.LegendItem(i);
                            if (lItem is ISymbol)
                            {
                                height += 22;
                                width = (int)Math.Max(40 + gr.MeasureString(lItem.LegendLabel, font1).Width, width);
                                items.Add(lItem);
                            }
                        }
                        break;
                    }
                }
                bm.Dispose();
                gr.Dispose();
                bm = null;
                gr = null;
                if (items.Count == 1)
                {
                    bm = new System.Drawing.Bitmap(width, height);
                    gr = System.Drawing.Graphics.FromImage(bm);
                    gr.FillRectangle(System.Drawing.Brushes.White, 0, 0, bm.Width, bm.Height);

                    if (items[0] is System.Drawing.Bitmap)
                    {
                        System.Drawing.Bitmap lBm = (System.Drawing.Bitmap)items[0];
                        gr.DrawImage(lBm, new System.Drawing.Point(0, 0));
                        lBm.Dispose();
                    }
                    else if (items[0] is ILegendItem)
                    {
                        ISymbol symbol = items[0] as ISymbol;
                        SymbolPreview.Draw(gr,
                            new System.Drawing.Rectangle(2, 1, 30, 20),
                            symbol);
                        gr.DrawString(element.Name, font1, System.Drawing.Brushes.Black, 32, 3);
                    }
                }
                else if (items.Count > 1)
                {
                    bm = new System.Drawing.Bitmap(width, height + 15);
                    gr = System.Drawing.Graphics.FromImage(bm);
                    gr.FillRectangle(System.Drawing.Brushes.White, 0, 0, bm.Width, bm.Height);

                    int y = 12;
                    foreach (object item in items)
                    {
                        if (item is System.Drawing.Image)
                        {
                            System.Drawing.Bitmap lBm = (System.Drawing.Bitmap)item;
                            gr.DrawImage(lBm, new System.Drawing.Point(0, y));
                            y += lBm.Height;
                            lBm.Dispose();
                        }
                        else if(item is ILegendItem)
                        {
                            ISymbol symbol = item as ISymbol;
                            SymbolPreview.Draw(gr,
                                new System.Drawing.Rectangle(4, 1 + y, 30, 20),
                                symbol);
                            gr.DrawString(((ILegendItem)item).LegendLabel, font2, System.Drawing.Brushes.Black, 34, y + 3);
                            y += 22;
                        }
                    } 
                    gr.DrawString(element.Name, font1, System.Drawing.Brushes.Black, 2, 2);
                }
            }
            catch
            {
                if (bm != null)
                {
                    bm.Dispose();
                    bm = null;
                }
            }
            finally
            {
                font1.Dispose();
                font2.Dispose();
                if (gr != null) gr.Dispose();
            }

            return bm;
        }

        /*
        public List<ITOCElement> VisibleElements
        {
            get
            {
                List<ITOCElement> visElements = new List<ITOCElement>();

                for (int i = 0; i < _elements.Count; i++)
                {
                    ITOCElement element = (ITOCElement)_elements[i];
                    if (element.ElementType == TOCElementType.ClosedGroup)
                    {
                        for (int j = i + 1; j < _elements.Count; j++)
                        {
                            ITOCElement elemParent = ((ITOCElement)_elements[j]).ParentGroup;
                            ITOCElement parent = element.ParentGroup;
                            bool found = false;
                            while (true)
                            {
                                if (parent == elemParent || elemParent == null)
                                {
                                    i = j;
                                    found = true;
                                    break;
                                    //return element;
                                }
                                if (parent == null) break;
                                parent = parent.ParentGroup;
                            }
                            if (found) break;
                        }
                    }
                    visElements.Add(element);
                }
                return visElements;
            }
        }
        */

        /*
        public void AddGroup(string GroupName, ITOCElement parent)
        {
            string name = GroupName;
            bool found = true;
            int c = 2;

            while (found)
            {
                found = false;
                foreach (ITOCElement elem in _elements)
                {
                    if (elem.ParentGroup == parent && elem.name == name)
                    {
                        found = true;
                        name = GroupName + " " + c.ToString();
                        c++;
                        break;
                    }
                }
            }
            GroupLayer gLayer = new GroupLayer();
            gLayer.Title = name;
            if(_map!=null) _map.AddLayer(gLayer);

            if (parent == null)
            {
                _elements.Add(new TOCElement(gLayer, name, parent, this, TOCElementType.OpenedGroup));
            }
            else
            {
                _elements.Insert(this.lastGroupItemIndex(parent) + 1,
                    new TOCElement(gLayer,name , parent, this, TOCElementType.OpenedGroup));
            }
        }
        */

		public void Add2Group(ITOCElement element, ITOCElement Group)
		{
			if(element==null) return;
			if(_elements.IndexOf(element)==-1) return;
			if(element==Group) return;

            if (Group == null || Group.Layers.Count != 1 ||
                !(Group.Layers[0] is GroupLayer)) return;

            ITOCElement pElement = element.ParentGroup;
            IGroupLayer pLayer = null;
            if (pElement != null && pElement.Layers.Count == 1 &&
                (pElement.Layers[0] is GroupLayer))
            {
                pLayer = pElement.Layers[0] as IGroupLayer;
            }
            
            
            IGroupLayer gLayer = Group.Layers[0] as IGroupLayer;

			if(element.ElementType==TOCElementType.Layer) 
			{
				_elements.Remove(element);
				_elements.Insert(this.lastGroupItemIndex(Group)+1,element);
			} 
			else if(element.ElementType==TOCElementType.OpenedGroup ||
				element.ElementType==TOCElementType.ClosedGroup) 
			{
				// Zirkulationen vermeiden
				ITOCElement parent=Group;
				while(parent!=null) 
				{
					if(parent.ParentGroup==element) 
						((TOCElement)parent).ParentGroup=element.ParentGroup;

					parent=parent.ParentGroup;
				}

				_elements.Remove(element);
				_elements.Insert(this.lastGroupItemIndex(Group)+1,element);

				MoveGroup(element);
			}

			((TOCElement)element).ParentGroup=Group;
            foreach (ILayer layer in element.Layers)
            {
                if (pLayer is GroupLayer)
                {
                    ((GroupLayer)pLayer).Remove(layer as Layer);
                }
                if(gLayer is GroupLayer) 
                {
                    ((GroupLayer)gLayer).Add(layer as Layer);
                }
            }

            if (TocChanged != null) TocChanged(this, new EventArgs());
		}

		public ITOCElement GetTOCElement(string name,ITOCElement parent) 
		{
			foreach(ITOCElement element in _elements) 
			{
				if(element.Name==name && element.ParentGroup==parent) return element;
			}
			return null;
		}

        public ITOCElement GetTOCElement(ILayer layer)
        {
            foreach (ITOCElement tocElement in _elements)
            {
                if (tocElement.Layers == null) continue;
                foreach(ILayer e in tocElement.Layers)
                {
                    if (e == layer) 
                        return tocElement;
                }
            }
            return null;
        }

        public ITOCElement GetTOCElement(IClass Class)
        {
            if (Class == null) return null;
            foreach (ITOCElement tocElement in _elements)
            {
                if (tocElement.Layers == null) continue;
                foreach (ILayer e in tocElement.Layers)
                {
                    if (e == null) continue;
                    if (e.Class == Class) return tocElement;
                }
            }
            return null;
        }

		public List<ITOCElement> GroupElements 
		{
			get 
			{
				List<ITOCElement> e=new List<ITOCElement>();
				foreach(ITOCElement elem in _elements) 
				{
					if( elem.ElementType==TOCElementType.ClosedGroup ||
					    elem.ElementType==TOCElementType.OpenedGroup ) e.Add(elem);
				}
				return e;
			}
		}

		public void RenameElement(ITOCElement element,string newName) 
		{
			if(element.ElementType==TOCElementType.Layer) 
			{
				if(_elements.IndexOf(element)==-1) return;

				// Zwei Layer mit selben Namen und gleicher ParentGroup
				// vereinigen...
				foreach(ITOCElement e in _elements) 
				{
					if(e==element) continue;
					if(e.Name==newName && e.ParentGroup==element.ParentGroup) 
					{
						if(e.ElementType!=TOCElementType.Layer) return;  // Nur mit Layer vereinigbar...

                        foreach(ILayer layer in element.Layers)
                        {
							((TOCElement)e).LayersList.Add(layer);
						}
						_elements.Remove(element);
						return;
					}
				}
				((TOCElement)element).rename(newName);
			} 
			else if( element.ElementType==TOCElementType.OpenedGroup ||
					 element.ElementType==TOCElementType.ClosedGroup ) 
			{
				// Zwei Gruppen mit selben Namen und gleicher ParentGroup
				// vereinigen...
				foreach(ITOCElement e in _elements) 
				{
					if(e==element) continue;
					if( e.ElementType!=TOCElementType.ClosedGroup &&
						e.ElementType!=TOCElementType.OpenedGroup ) continue;
					if(	e.ParentGroup==element.ParentGroup &&
						e.Name==newName) 
					{
                        foreach (ITOCElement e2 in ListOperations<ITOCElement>.Clone(_elements))
                        {
                            if (e2.ParentGroup == element)
                            {
                                ((TOCElement)e2).ParentGroup = e;
                                this.RenameElement(e2, e2.Name);
                            }
                        }
						_elements.Remove(element);
						this.MoveGroup(e);

                        // Groupelement können auch Layer enthalten (zB WebserviceLayer...)
                        foreach (ILayer layer in element.Layers)
                        {
                            ((TOCElement)e).LayersList.Add(layer);
                        }
						break;
					}
				}
				if(_elements.IndexOf(element)==-1) return;
				((TOCElement)element).rename(newName);
			}

            if (TocChanged != null) TocChanged(this, new EventArgs());
		}

        public void MoveElement(ITOCElement element, ITOCElement insertBefore, bool insertAfter)
        {
            if (element == insertBefore) return;

            if (_elements.IndexOf(element) == -1 ||
                _elements.IndexOf(insertBefore) == -1) return;

            int sum = (insertAfter) ? 1 : 0;

            if (element.ElementType == TOCElementType.Layer)
            {
                _elements.Remove(element);
                ((TOCElement)element).ParentGroup = insertBefore.ParentGroup;
                _elements.Insert(_elements.IndexOf(insertBefore) + sum, element);
            }
            else if (element.ElementType == TOCElementType.OpenedGroup ||
                    element.ElementType == TOCElementType.ClosedGroup)
            {
                _elements.Remove(element);
                ((TOCElement)element).ParentGroup = insertBefore.ParentGroup;
                _elements.Insert(_elements.IndexOf(insertBefore) + sum, element);
                this.MoveGroup(element);
            }

            if (TocChanged != null) TocChanged(this, new EventArgs());
        }

		public int CountGroupLayers(ITOCElement Group,bool subGroups)
		{
			if( Group.ElementType!=TOCElementType.OpenedGroup &&
				Group.ElementType!=TOCElementType.ClosedGroup ) return 0;
			if(_elements.IndexOf(Group)==-1) return 0;

			int count=0;
			foreach(ITOCElement element in _elements) 
			{
				if(element.ParentGroup==Group) 
				{
					if(element.ElementType==TOCElementType.Layer)
						count++;
					if((element.ElementType==TOCElementType.ClosedGroup ||
						element.ElementType==TOCElementType.OpenedGroup ) &&
						subGroups) 
					{
						count+=CountGroupLayers(element,true);
					}
				}
			}
			return count;
		}
		public int CountVisibleGroupLayers(ITOCElement Group,bool subGroups) 
		{
			if( Group.ElementType!=TOCElementType.OpenedGroup &&
				Group.ElementType!=TOCElementType.ClosedGroup ) return 0;
			if(_elements.IndexOf(Group)==-1) return 0;

			int count=0;
			foreach(ITOCElement element in _elements) 
			{
				if(element.ParentGroup==Group) 
				{
					if(element.ElementType==TOCElementType.Layer) 
					{
						if(element.LayerVisible) count++;
					}
					if((element.ElementType==TOCElementType.ClosedGroup ||
						element.ElementType==TOCElementType.OpenedGroup ) &&
						subGroups) 
					{
						count+=CountVisibleGroupLayers(element,true);
					}
				}
			}
			return count;
		}
		public void SplitMultiLayer(ITOCElement element) 
		{
			int index=_elements.IndexOf(element);
			if(index==-1) return;
			if(element.ElementType!=TOCElementType.Layer) return;

			for(int i=((TOCElement)element).LayersList.Count-1;i>0;i--) 
			{
				ILayer layer=(ILayer)((TOCElement)element).LayersList[i];
				((TOCElement)element).LayersList.Remove(layer);

				AddLayer(layer,element.ParentGroup,++index);
			}
		}
		#endregion

		public void AddLayer(ILayer layer,ITOCElement parent) 
		{
			AddLayer(layer,parent,-1);
		}
		internal void AddLayer(ILayer layer,ITOCElement parent,int pos) 
		{
			int c=1;
			string alias=layer.Title,alias2=alias;
		
			while(GetTOCElement(alias2,parent)!=null) 
			{
				alias2=alias+"_"+c.ToString();
				c++;
			}
            alias = alias2;

            InsertLayer(pos, layer, alias, parent);
		}
        public void RemoveLayer(ILayer layer)
        {
            foreach (ITOCElement element in ListOperations<ITOCElement>.Clone(_elements))
            {
                foreach (ILayer l in ListOperations<ILayer>.Clone(element.Layers))
                {
                    if (l == layer)
                    {
                        element.RemoveLayer(l);
                        if (element.Layers.Count == 0)
                        {
                            _elements.Remove(element);
                            if (TocChanged != null) TocChanged(this, new EventArgs());
                            break;
                        }
                    }
                }
            }
        }
        private void InsertLayer(int pos, ILayer layer, string alias, ITOCElement parent)
        {
            TOCElement element =
                (layer is IWebServiceLayer) ?
                new TOCElement(layer, alias, parent, this, TOCElementType.OpenedGroup) :
                new TOCElement(layer, alias, parent, this, (layer is IGroupLayer) ? TOCElementType.OpenedGroup : TOCElementType.Layer);

            if (pos < 0 || pos >= _elements.Count)
            {
                _elements.Add(element);
            }
            else
            {
                _elements.Insert(pos, element);
            }

            if (layer is IWebServiceLayer && layer.Class is IWebServiceClass && ((IWebServiceClass)layer.Class).Themes != null)
            {
                IWebServiceClass wc = layer.Class as IWebServiceClass;
                foreach (IWebServiceTheme theme in wc.Themes)
                {
                    theme.DatasetID = layer.DatasetID;
                    if (theme.Locked) continue;
                    InsertLayer(pos > 0 ? pos + 1 : -1, theme, theme.Title, element);
                }
            }

            if (TocChanged != null) TocChanged(this, new EventArgs());
        }

		private int lastGroupItemIndex(ITOCElement parent) 
		{
			int index=0,i=0;
			foreach(ITOCElement elem in _elements) 
			{
				if(elem==parent) 
					index=i;
				else if(elem.ParentGroup==parent && elem.ElementType==TOCElementType.Layer)
					index=i;
				i++;
			}
			return index;
		}
		private void MoveGroup(ITOCElement Group) 
		{
			List<ITOCElement> collect=new List<ITOCElement>();
			foreach(ITOCElement elem in _elements) 
			{
				if(elem.ParentGroup==Group) 
				{
					collect.Add(elem);
				}
			}
			foreach(ITOCElement elem in collect) 
			{
				_elements.Remove(elem);
			}
			int index=_elements.IndexOf(Group)+1;
			foreach(ITOCElement elem in collect) 
			{
				_elements.Insert(index++,elem);
				if( elem.ElementType==TOCElementType.ClosedGroup ||
					elem.ElementType==TOCElementType.OpenedGroup ) 
					MoveGroup(elem);
			}

            if (TocChanged != null) TocChanged(this, new EventArgs());
		}

		public void SetGroupVisibility(ITOCElement Group,bool visible) 
		{
			foreach(ITOCElement element in _elements) 
			{
				if(element.ParentGroup==Group) 
				{
					if( element.ElementType==TOCElementType.Layer       ||
						element.ElementType==TOCElementType.ClosedGroup ||
						element.ElementType==TOCElementType.OpenedGroup )
						
					{
						element.LayerVisible=visible;
					}
				}
			}
		}

		public List<ILayer> VisibleLayers
		{
			get 
			{
				List<ILayer> layers=new List<ILayer>();

				foreach(ITOCElement tocElement in _elements) 
				{
                    if (tocElement == null || tocElement.ElementType != TOCElementType.Layer ||
                        tocElement.Layers == null) continue;

					//if(!tocElement.LayerVisible) continue;

                    if (this.Modifier == TOCModifier.Public)
                    {
                        foreach (ILayer layer in tocElement.Layers)
                        {
                            if (layer == null || !layer.Visible) continue;
                            layers.Add(layer);
                        }
                    }
                    else
                    {
                        ITOCElement e=tocElement;
                        bool visible = e.LayerVisible;
                        while (visible && e.ParentGroup != null)
                        {
                            visible = e.ParentGroup.LayerVisible;
                            e = e.ParentGroup;
                        }
                        if (visible)
                        {
                            foreach (ILayer layer in tocElement.Layers)
                                layers.Add(layer);
                        }
                    }
				}

				return layers;
			}
		}

        public List<IWebServiceLayer> VisibleWebServiceLayers
        {
            get
            {
                List<IWebServiceLayer> layers = new List<IWebServiceLayer>();

                foreach (ITOCElement tocElement in _elements)
                {
                    if (tocElement == null || tocElement.ElementType == TOCElementType.Layer ||
                        tocElement.Layers == null) continue;
                    
                    if (this.Modifier == TOCModifier.Public)
                    {
                        foreach (ILayer layer in tocElement.Layers)
                        {
                            if (!(layer is IWebServiceLayer) || !layer.Visible) continue;
                            layers.Add(layer as IWebServiceLayer);
                        }
                    }
                    else
                    {
                        ITOCElement e=tocElement;
                        bool visible = e.LayerVisible;
                        while (visible && e.ParentGroup != null)
                        {
                            visible = e.ParentGroup.LayerVisible;
                            e = e.ParentGroup;
                        }
                        if (visible)
                        {
                            foreach (ILayer layer in tocElement.Layers)
                            {
                                if (!(layer is IWebServiceLayer)) continue;
                                layers.Add(layer as IWebServiceLayer);
                            }
                        }
                    }
				}

                return layers;
            }
        }

        public List<ILayer> Layers
        {
            get
            {
                List<ILayer> layers = new List<ILayer>();

                foreach (ITOCElement tocElement in _elements)
                {
                    if (tocElement.ElementType != TOCElementType.Layer) continue;

                    foreach(ILayer layer in tocElement.Layers)
                    {
                        layers.Add(layer);
                    }
                }

                return layers;
            }
        }

        public void RemoveAllElements()
        {
            _elements.Clear();
            if (TocChanged != null) TocChanged(this, new EventArgs());
        }

        private bool IsChild(ITOCElement parent, ITOCElement child)
        {
            if (child == null) return false;
            if (parent == null) return true; // null ist immer Parent

            while (child.ParentGroup != null)
            {
                if (child.ParentGroup == parent) return true;
                child = child.ParentGroup;
            }
            return false;
        }

		#region IPersistable Member

		public void Load(gView.Framework.IO.IPersistStream stream)
		{
			_elements.Clear();

			TOCElement element=null;
			while((element=(TOCElement)stream.Load("ITOCElement",null,new TOCElement(this)))!=null) 
			{
				_elements.Add(element);
			}
		}

		public void Save(gView.Framework.IO.IPersistStream stream)
		{
			foreach(TOCElement element in _elements) 
			{
				stream.Save("ITOCElement",element);
			}
		}

		#endregion

        #region IClone
        public object Clone()
        {
            return Clone(_map);
        }
        #endregion

        #region IClone3 Member

        public object Clone(IMap map)
        {
            TOC toc = new TOC(map);

            foreach (ITOCElement element in _elements)
            {
                int parentIndex = -1;
                if (element.ParentGroup != null)
                {
                    parentIndex = _elements.IndexOf(element.ParentGroup);
                    if (parentIndex >= toc._elements.Count) parentIndex = -1;
                }

                toc._elements.Add(((TOCElement)element).Copy(toc, (parentIndex == -1) ? null : toc._elements[parentIndex]));
            }
            return toc;
        }

        #endregion
    }

	internal class TOCElement : gView.Framework.UI.ITOCElement,gView.Framework.IO.IPersistable
	{
		private string _name;
		private TOCElementType _type;
		ITOCElement _parent;
        List<IDatasetElement> _layers = new List<IDatasetElement>();
		private TOC _toc;
        private bool _showLegend=true;
        private bool _visible=false;
        private bool _locked = false;

		public TOCElement(TOC parentTOC) 
		{
			_toc=parentTOC;
		}
		public TOCElement(ILayer layer,string name,ITOCElement parent,TOC parentTOC) 
		{
			_layers.Add(layer);
			_name=name;
			_type=TOCElementType.Layer;
			_parent=parent;
			_toc=parentTOC;
            if (layer is ILayer)
            {
                _visible = ((ILayer)layer).Visible;
            }
		}
        public TOCElement(ILayer layer, string name, ITOCElement parent, TOC parentTOC, TOCElementType type)
            : this(layer, name, parent, parentTOC)
        {
            _type = type;
        }
		public TOCElement(string name,TOC parentTOC) 
		{
			_name=name;
			_type=TOCElementType.Layer;
			_parent=null;
			_toc=parentTOC;
		}
		public TOCElement(string name,ITOCElement parent,TOC parentTOC) 
		{
			_name=name;
			_type=TOCElementType.Layer;
			_parent=parent;
			_toc=parentTOC;
		}
		public TOCElement(string name,ITOCElement parent,TOCElementType type,TOC parentTOC) 
		{
			_name=name;
			_type=type;
			_parent=parent;
			_toc=parentTOC;
		}

        internal TOCElement Copy(TOC toc,ITOCElement parent)
        {
            TOCElement elem = new TOCElement(toc);
            elem._name = _name;
            elem._type = _type;
            elem._parent = parent;
            elem._layers = ListOperations<IDatasetElement>.Clone(_layers);
            elem._showLegend = _showLegend;
            elem._visible = LayerVisible;
            elem._locked = _locked;

            return elem;
        }

        private static string RecursiveName(TOCElement element)
        {
            string name = "";
            RecursiveName(element, ref name);
            return name;
        }
        private static void RecursiveName(TOCElement element, ref string name)
        {
            if (element == null) return;
            name = ((name != "") ? element._name + "|" + name : element._name);
            RecursiveName(element.ParentGroup as TOCElement, ref name);
        }

        internal static bool layerVisible(ILayer layer)
        {
            if (layer is Layer)
            {
                // Sollte nicht Recursive über Grouplayer bestimmt werden
                IGroupLayer gLayer = layer.GroupLayer;
                ((Layer)layer).GroupLayer = null;
                bool visible = layer.Visible;
                ((Layer)layer).GroupLayer = gLayer;

                return visible;
            }
            else if (layer != null)
            {
                return layer.Visible;
            }
            return false;
        }
		#region ITOCElement Member

		public string Name
		{
			get
			{
                    return _name;
			}
            set
            {
                if (value == null || value == "" || _toc == null) return;
                _toc.RenameElement(this, value);
            }
		}

		public gView.Framework.UI.TOCElementType ElementType
		{
			get
			{
				return _type;
			}
		}

		public List<ILayer> Layers
		{
			get
			{
				List<ILayer> e=new List<ILayer>();
				foreach(ILayer layer in _layers) 
				{
					e.Add(layer);
				}
				return e;
			}
		}
        public void RemoveLayer(ILayer layer)
        {
            if (!_layers.Contains(layer)) return;
            _layers.Remove(layer);
        }
        public void AddLayer(ILayer layer)
        {
            if (_layers.Contains(layer)) return;
            _layers.Add(layer);
        }
        public ITOCElement ParentGroup
        {
            get
            {
                return _parent;
            }
            set
            {
                if (_parent == value) return;
                _parent = value;

                if (_layers != null)
                {
                    foreach (ILayer layer in _layers)
                    {
                        if (!(layer is Layer)) continue;

                        if (layer.GroupLayer is GroupLayer)
                        {
                            ((GroupLayer)layer.GroupLayer).Remove(layer as Layer);
                        }

                        ((Layer)layer).GroupLayer =
                            (_parent != null && _parent.Layers.Count == 1) ? _parent.Layers[0] as IGroupLayer : null;
                    }
                }
            }
        }

		public bool LayerVisible 
		{
			get 
			{
                if (_toc.Modifier == TOCModifier.Public)
                {
                    if (_type == TOCElementType.Layer)
                    {
                        foreach (ILayer layer in _layers)
                        {
                            if (layer == null) continue;

                            if (TOCElement.layerVisible(layer))
                            {
                                return _visible = true;
                            }
                        }
                        return _visible = false;
                    }
                    if (_type == TOCElementType.OpenedGroup ||
                        _type == TOCElementType.ClosedGroup)
                    {
                        //return _visible = (_toc.CountVisibleGroupLayers(this, true) > 0);
                        if (_layers.Count == 1 && _layers[0] is IGroupLayer)
                        {
                            return _visible = TOCElement.layerVisible(_layers[0] as ILayer);
                        }
                        else if (_layers.Count == 1 && _layers[0] is IWebServiceLayer)
                        {
                            return _visible = TOCElement.layerVisible(_layers[0] as ILayer);
                        }
                    }
                    return _visible = false;
                }
                else
                {
                    return _visible;
                }
			}
			set 
			{
				if(_type==TOCElementType.Layer)
				{
                    if (_toc.Modifier == TOCModifier.Public)
                    {
                        foreach (ILayer layer in _layers)
                        {
                            if (layer == null) continue;
                            layer.Visible = _visible = value;
                        }
                    }
                    else
                    {
                        _visible = value;
                    }
				}
				if( _type==TOCElementType.OpenedGroup ||
					_type==TOCElementType.ClosedGroup ) 
				{
                    if (_toc.Modifier == TOCModifier.Public)
                    {
                        if (_layers.Count == 1 && _layers[0] is IGroupLayer)
                        {
                            _visible = ((IGroupLayer)_layers[0]).Visible = value;
                        }
                        else if (_layers.Count == 1 && _layers[0] is IWebServiceLayer)
                        {
                            _visible = ((IWebServiceLayer)_layers[0]).Visible = value;
                        }
                    }
                    else
                    {
                        _visible = value;
                    }
                    //if(_toc is TOC) 
                    //{
                    //    ((TOC)_toc).SetGroupVisibility(this,value);
                    //}
				}
			}
		}

        public bool LayerLocked
        {
            get { return _locked; }
            set { _locked = value; }
        }
        public bool LegendVisible
        {
            get { return _showLegend; }
            set { _showLegend = value; }
        }

		public void OpenCloseGroup(bool open) 
		{
			if( _type==TOCElementType.OpenedGroup ||
				_type==TOCElementType.ClosedGroup ) 
			{
				if(open)
					_type=TOCElementType.OpenedGroup;
				else
					_type=TOCElementType.ClosedGroup;
			}
		}

        public ITOC TOC
        {
            get { return _toc; }
        }
		#endregion

		public void rename(string newName) 
		{
			_name=newName;
            foreach (ILayer layer in _layers)
            {
                if (layer is IGroupLayer)
                    ((IGroupLayer)layer).Title = newName;
            }
		}
		public List<IDatasetElement> LayersList 
		{
			get { return _layers; }
		}

		#region IPersistable Member

		public void Load(gView.Framework.IO.IPersistStream stream)
		{
			_name=(string)stream.Load("Name");
			_type=(TOCElementType)stream.Load("Type");
            _showLegend = (bool)stream.Load("legend", false);
            _locked = (bool)stream.Load("locked", false);

            if ((_type == TOCElementType.ClosedGroup || _type == TOCElementType.OpenedGroup) &&
                _name.IndexOf("|") != -1)
            {
                int pos = _name.LastIndexOf("|");
                _name = _name.Substring(pos + 1, _name.Length - pos - 1);
            }

            if (_toc.Modifier == TOCModifier.Private)
            {
                _visible = (bool)stream.Load("visible", false);
            }

			string parentName=(string)stream.Load("Parent");
			if(parentName!=null) 
			{
                foreach(ITOCElement group in _toc.GroupElements)
                {
                    if (RecursiveName(group as TOCElement) == parentName)
                    {
                        _parent = group;
                        break;
                    }
				}
			}

            _layers.Clear();
            if (_type == TOCElementType.ClosedGroup || _type == TOCElementType.OpenedGroup)
            {
                PersistLayer pElement = null;

                pElement = (PersistLayer)stream.Load("DatasetElement", null, new PersistLayer(_toc._map));

                if (pElement != null && pElement.DatasetElement != null)
                {
                    ILayer gLayer = null;
                    foreach (IDatasetElement dsElement in _toc._map.MapElements)
                    {
                        if (dsElement is IGroupLayer &&
                            dsElement.ID == pElement.DatasetElement.ID/*RecursiveName(this)*/)
                        {
                            gLayer = dsElement as IGroupLayer;
                            break;
                        }
                    }

                    if (gLayer == null)
                    {
                        // wenn Gruppe ein WebServiceLayer ist....
                        gLayer = pElement.DatasetElement as ILayer;

                        //
                        // Für alte Versionen: Suchen, ob Grouplayer in Karte vorhanden
                        // Wenn nein: einfügen
                        //
                        if (gLayer == null) _toc._map.AddLayer(gLayer = new GroupLayer(RecursiveName(this)));
                    }

                    if (gLayer != null)
                    {
                        _layers.Add(gLayer);
                        if (_parent != null && _parent.Layers.Count == 1 && _parent.Layers[0] is GroupLayer)
                            ((GroupLayer)_parent.Layers[0]).Add(gLayer as Layer);
                    }
                }
            }
            else
            {
                PersistLayer pElement = null;

                while ((pElement = (PersistLayer)stream.Load("DatasetElement", null, new PersistLayer(_toc._map))) != null)
                {
                    _layers.Add(pElement.DatasetElement);  // DatasetElement kann auch null sein, wenn (vorübergehend) nicht mehr im Dataset...
                    if (_parent != null && _parent.Layers.Count == 1 && _parent.Layers[0] is GroupLayer)
                        ((GroupLayer)_parent.Layers[0]).Add(pElement.DatasetElement as Layer);
                }
            }
		}

		public void Save(gView.Framework.IO.IPersistStream stream)
		{
            stream.Save("Name", (_type == TOCElementType.OpenedGroup || _type == TOCElementType.ClosedGroup) ? RecursiveName(this) : _name);
			stream.Save("Type",(int)_type);
            stream.Save("legend", _showLegend);
            stream.Save("locked", _locked);
            
            if (_toc.Modifier == TOCModifier.Private)
            {
                stream.Save("visible", _visible);
            }
			if(_parent!=null) 
			{
                stream.Save("Parent", RecursiveName(_parent as TOCElement));
			}
			foreach(ILayer layer in _layers) 
			{
				PersistLayer pElement=new PersistLayer(_toc._map, layer);
				stream.Save("DatasetElement",pElement);
			}
		}

		#endregion
    }

	internal class PersistLayer : gView.Framework.IO.IPersistable
	{
        private IMap _map = null;
        private IDatasetElement _element = new NullLayer();

        public PersistLayer(IMap map)
        {
            _map = map;
        }
		public PersistLayer(IMap map,IDatasetElement element) : this(map) 
		{
            _element = element;
		}

		public IDatasetElement DatasetElement 
		{
			get { return _element; }
		}

		#region IPersistable Member

		public void Load(gView.Framework.IO.IPersistStream stream)
		{
			if(_map==null) return;

			int datasetIndex=(int)stream.Load("DatasetIndex",-1);

			IDataset dataset=_map[datasetIndex];
			// dataset ist bei Grouplayern immer null, darum kein abbruch
            //if(dataset==null) return;

            bool isWebTheme = (bool)stream.Load("IsWebTheme", false);

            string webThemeId = String.Empty;
            string webClassName = String.Empty;
            if (isWebTheme && dataset != null)
            {
                webThemeId = (string)stream.Load("ID", "");
                webClassName = (string)stream.Load("ClassName", "");

                IDatasetElement wElement = dataset[webClassName];
                if (wElement == null || !(wElement.Class is IWebServiceClass)) return;

                IWebServiceClass wc = wElement.Class as IWebServiceClass;
                if (wc == null || wc.Themes == null) return;

                foreach (IWebServiceTheme theme in wc.Themes)
                {
                    if (theme.LayerID == webThemeId)
                    {
                        _element = theme;
                        return;
                    }
                }
                return;
            }

            string name = (string)stream.Load("Name", "");
            int _id_ = (int)stream.Load("_ID_", -1);

            if (_id_ == -1)  // Old Version
            {
                foreach (IDatasetElement layer in _map.MapElements)
                {
                    if (layer.Class != null && layer.Class.Dataset == dataset && layer.Title == name)
                    {
                        _element = layer;
                        return;
                    }
                }
            }
            else
            {
                foreach (IDatasetElement layer in _map.MapElements)
                {
                    // Grouplayer
                    if (dataset == null &&
                        layer is IGroupLayer &&
                        layer.ID == _id_ &&
                        layer.Title == name)
                    {
                        _element = layer;
                        return;
                    }
                    // Layer
                    if (layer.Class != null && layer.Class.Dataset == dataset && 
                        layer.Title == name && layer.ID==_id_)
                    {
                        _element = layer;
                        return;
                    }
                }
            }

            // für ein späters speichern des projektes die werte merken
            if (_element is NullLayer)
            {
                ((NullLayer)_element).PersistLayerID = _id_;
                ((NullLayer)_element).PersistDatasetID = datasetIndex;
                ((NullLayer)_element).PersistIsWebTheme = isWebTheme;
                ((NullLayer)_element).PersistWebThemeID = webThemeId;
                ((NullLayer)_element).PersistClassName = webClassName;
                ((NullLayer)_element).Title = name;
            }
		}

		public void Save(gView.Framework.IO.IPersistStream stream)
		{
            if (_element == null || _map == null) return;

            if (_element is NullLayer)
            {
                NullLayer nLayer = (NullLayer)_element;

                stream.Save("DatasetIndex", nLayer.PersistDatasetID);

                if (nLayer.PersistIsWebTheme)
                {
                    stream.Save("ID", nLayer.PersistWebThemeID);
                    stream.Save("IsWebTheme", true);
                    stream.Save("ClassName", nLayer.PersistClassName);
                }
                else
                {
                    stream.Save("_ID_", nLayer.PersistLayerID);
                    stream.Save("Name", nLayer.Title);
                }
            }
            else
            {
                //IDataset dataset = _map[_element.DatasetID];

                stream.Save("DatasetIndex", _element.DatasetID);

                if (_element is IWebServiceTheme)
                {
                    stream.Save("ID", ((IWebServiceTheme)_element).LayerID);
                    stream.Save("IsWebTheme", true);

                    //if (dataset.Elements.Count==1 && dataset.Elements[0].Class != null)
                    if (((IWebServiceTheme)_element).ServiceClass != null)
                    {
                        //stream.Save("ClassName", dataset.Elements[0].Class.Name);
                        stream.Save("ClassName", ((IWebServiceTheme)_element).ServiceClass.Name);
                    }
                }
                else
                {
                    stream.Save("_ID_", _element.ID);
                    stream.Save("Name", _element.Title);
                }
            }
		}

		#endregion
	}
}
