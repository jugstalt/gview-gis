using System;
using System.IO;
using System.Data;
using System.Collections;
using System.Collections.Generic;
using gView.Framework.Geometry;
using gView.Framework.Data;
using gView.Framework.FDB;
using gView.Framework.UI;
using gView.Framework.system;

namespace gView.Framework.Data
{
	internal class SHPTreeNode
	{
		/* region covered by this node */
		public Envelope Bounds;
		//public double [] adfBoundsMin=new double[4];
		//public double [] adfBoundsMax=new double[4];

		/* list of shapes stored at this node.  The papsShapeObj pointers
		   or the whole list can be NULL */
		public int	nShapeCount 
		{
			get { return panShapeIds.Count; }
		}
		//int		*panShapeIds;
		//SHPObject   **papsShapeObj;
		public ArrayList panShapeIds=new ArrayList();
		//public ArrayList papsShapeObj=new ArrayList();

		public int	nSubNodes 
		{
			get { return apsSubNode.Count; }
		}
		public ArrayList apsSubNode=new ArrayList();
		//struct shape_tree_node *apsSubNode[MAX_SUBNODE];
	}
	
	internal class SHPTree
	{
		public int	nMaxDepth;
		public int	nDimension;
    
		public SHPTreeNode	psRoot;
	}

	public class SHPObject 
	{
		public SHPObject(int id,IEnvelope e) 
		{
			ID=id;
			env=e;
		}
		public int ID;
		public IEnvelope env;
	}
	/// <summary>
	/// Zusammenfassung für QuadTree.
	/// </summary>
	public class QuadTree : IIndexTree
	{
		private int _nShapeCount=0;
		private const double SHP_SPLIT_RATIO=0.55;
		private SHPTree	psTree=null;

		public QuadTree()
		{
		}
		public QuadTree(int nShapeCount) 
		{
			_nShapeCount=nShapeCount;
		}

		private SHPTreeNode SHPTreeNodeCreate( IEnvelope env )

		{
			SHPTreeNode	psTreeNode=new SHPTreeNode();

			//psTreeNode.nShapeCount = 0;
			//psTreeNode.panShapeIds = null;
			//psTreeNode.papsShapeObj = null;

			//psTreeNode.nSubNodes = 0;

			psTreeNode.Bounds=new Envelope(env);
			//memcpy(psTreeNode.adfBoundsMin,padfBoundsMin,4);
			//memcpy(psTreeNode.adfBoundsMax,padfBoundsMax,4);

			return psTreeNode;
		}

		public bool SHPCreateTree(int nDimension, int nMaxDepth, IEnvelope Bounds )

		{
			nDimension=2;
			psTree=new SHPTree();

			if( Bounds==null )
				return false;

			/* -------------------------------------------------------------------- */
			/*      Allocate the tree object                                        */
			/* -------------------------------------------------------------------- */
			psTree.nMaxDepth = nMaxDepth;
			psTree.nDimension = nDimension;

			/* -------------------------------------------------------------------- */
			/*      If no max depth was defined, try to select a reasonable one     */
			/*      that implies approximately 8 shapes per node.                   */
			/* -------------------------------------------------------------------- */
			if( psTree.nMaxDepth == 0 && _nShapeCount>0)
			{
				int	nMaxNodeCount = 1;

				while( nMaxNodeCount*4 < _nShapeCount )
				{
					psTree.nMaxDepth += 1;
					nMaxNodeCount = nMaxNodeCount * 2;
				}
			}

			/* -------------------------------------------------------------------- */
			/*      Allocate the root node.                                         */
			/* -------------------------------------------------------------------- */
			psTree.psRoot = SHPTreeNodeCreate( Bounds );

			/* -------------------------------------------------------------------- */
			/*      If we have a file, insert all it's shapes into the tree.        */
			/* -------------------------------------------------------------------- */
			/*
			if( hSHP != NULL )
			{
				int	iShape, nShapeCount;
        
				SHPGetInfo( hSHP, &nShapeCount, NULL, NULL, NULL );

				for( iShape = 0; iShape < nShapeCount; iShape++ )
				{
					SHPObject	*psShape;
            
					psShape = SHPReadObject( hSHP, iShape );
					SHPTreeAddShapeId( psTree, psShape );
					SHPDestroyObject( psShape );
				}
			}        
			*/
			return true;
		}

		//private bool SHPCheckBoundsOverlap( 
		//  double [] padfBox1Min, double [] padfBox1Max,
		//	double [] padfBox2Min, double [] padfBox2Max,
		//	int nDimension )
		private bool SHPCheckBoundsOverlap( 
			IEnvelope Box1,
			IEnvelope Box2 )
		{
			if ( Box2.maxx < Box1.minx ) return false;
			if ( Box2.maxy < Box2.miny ) return false;
			
			if ( Box1.maxx < Box2.minx ) return false;
			if ( Box1.maxy < Box2.miny ) return false;

			/*
			for( iDim = 0; iDim < nDimension; iDim++ )
			{
				if( padfBox2Max[iDim] < padfBox1Min[iDim] )
					return false;
        
				if( padfBox1Max[iDim] < padfBox2Min[iDim] )
					return false;
			}
			*/

			return true;
		}

		private bool SHPCheckObjectContained( SHPObject psObject,
			IEnvelope Bounds )
		{
			if(  psObject.env.minx < Bounds.minx
				|| psObject.env.maxx > Bounds.maxx )
				return false;
    
			if( psObject.env.miny < Bounds.miny 
				|| psObject.env.maxy > Bounds.maxy )
				return false;

			return true;

			//if( nDimension == 2 )
			//	return true;
    
			/*
			if( psObject->dfZMin < padfBoundsMin[2]
				|| psObject->dfZMax < padfBoundsMax[2] )
				return false;
        
			if( nDimension == 3 )
				return true;

			if( psObject->dfMMin < padfBoundsMin[3]
				|| psObject->dfMMax < padfBoundsMax[3] )
				return FALSE;
			*/
			//return false;
		}

		private void memcpy(double [] to,double [] from,int len) 
		{
			for(int i=0;i<len;i++)
				to[i]=from[i];
		}
		//private void SHPTreeSplitBounds( double [] padfBoundsMinIn, double [] padfBoundsMaxIn,
		//	double [] padfBoundsMin1, double [] padfBoundsMax1,
		//	double [] padfBoundsMin2, double [] padfBoundsMax2 )
		private void SHPTreeSplitBounds( IEnvelope BoundsIn, out IEnvelope Bounds1, out IEnvelope Bounds2)
		{
			/* -------------------------------------------------------------------- */
			/*      The output bounds will be very similar to the input bounds,     */
			/*      so just copy over to start.                                     */
			/* -------------------------------------------------------------------- */
			Bounds1=new Envelope(BoundsIn);
			Bounds2=new Envelope(BoundsIn);
    
			/* -------------------------------------------------------------------- */
			/*      Split in X direction.                                           */
			/* -------------------------------------------------------------------- */
			//if( (padfBoundsMaxIn[0] - padfBoundsMinIn[0])
			//	> (padfBoundsMaxIn[1] - padfBoundsMinIn[1]) )
			if( (BoundsIn.maxx - BoundsIn.minx)
				> (BoundsIn.maxy - BoundsIn.miny) )
			{
				double dfRange = BoundsIn.maxx - BoundsIn.minx;
				//double	dfRange = padfBoundsMaxIn[0] - padfBoundsMinIn[0];

				Bounds1.maxx = BoundsIn.minx + dfRange * SHP_SPLIT_RATIO;
				Bounds2.minx = BoundsIn.maxx - dfRange * SHP_SPLIT_RATIO;
				//padfBoundsMax1[0] = padfBoundsMinIn[0] + dfRange * SHP_SPLIT_RATIO;
				//padfBoundsMin2[0] = padfBoundsMaxIn[0] - dfRange * SHP_SPLIT_RATIO;
			}

				/* -------------------------------------------------------------------- */
				/*      Otherwise split in Y direction.                                 */
				/* -------------------------------------------------------------------- */
			else
			{
				double dfRange = BoundsIn.maxy - BoundsIn.miny; 
				//double	dfRange = padfBoundsMaxIn[1] - padfBoundsMinIn[1];

				Bounds1.maxy = BoundsIn.miny + dfRange * SHP_SPLIT_RATIO;
				Bounds2.miny = BoundsIn.maxy - dfRange * SHP_SPLIT_RATIO;
				//padfBoundsMax1[1] = padfBoundsMinIn[1] + dfRange * SHP_SPLIT_RATIO;
				//padfBoundsMin2[1] = padfBoundsMaxIn[1] - dfRange * SHP_SPLIT_RATIO;
			}
		}

		private bool SHPTreeNodeAddShapeId( SHPTreeNode psTreeNode, SHPObject psObject,
			int nMaxDepth, int nDimension )

		{
			int		i;
    
			/* -------------------------------------------------------------------- */
			/*      If there are subnodes, then consider wiether this object        */
			/*      will fit in them.                                               */
			/* -------------------------------------------------------------------- */
			if( nMaxDepth > 1 && psTreeNode.nSubNodes > 0 )
			{
				for( i = 0; i < psTreeNode.nSubNodes; i++ )
				{
					if( SHPCheckObjectContained(psObject,((SHPTreeNode)psTreeNode.apsSubNode[i]).Bounds))
					{
						return SHPTreeNodeAddShapeId( (SHPTreeNode)psTreeNode.apsSubNode[i],
							psObject, nMaxDepth-1,
							nDimension );
					}
				}
			}
			else if( nMaxDepth > 1 && psTreeNode.nSubNodes == 0 )
			{
				//double [] adfBoundsMinH1=new double[4], adfBoundsMaxH1=new double[4];
				//double [] adfBoundsMinH2=new double[4], adfBoundsMaxH2=new double[4];
				//				double [] adfBoundsMin1=new double[4], adfBoundsMax1=new double[4];
				//				double [] adfBoundsMin2=new double[4], adfBoundsMax2=new double[4];
				//				double [] adfBoundsMin3=new double[4], adfBoundsMax3=new double[4];
				//				double [] adfBoundsMin4=new double[4], adfBoundsMax4=new double[4];

				IEnvelope BoundsH1,BoundsH2,Bounds1,Bounds2,Bounds3,Bounds4;

				SHPTreeSplitBounds( psTreeNode.Bounds,
					out BoundsH1, out BoundsH2 );

				SHPTreeSplitBounds( BoundsH1,
					out Bounds1, out Bounds2);

				SHPTreeSplitBounds( BoundsH2,
					out Bounds3, out Bounds4); 

				bool b1=false,b2=false,b3=false,b4=false;
				if( (b1=SHPCheckObjectContained(psObject, Bounds1 )) ||
					(b2=SHPCheckObjectContained(psObject, Bounds2 )) ||
					(b3=SHPCheckObjectContained(psObject, Bounds3 )) || 
					(b4=SHPCheckObjectContained(psObject, Bounds4 )) )
				{
					//psTreeNode->nSubNodes = 4;
					//psTreeNode.apsSubNode.Clear();
					psTreeNode.apsSubNode.Add(SHPTreeNodeCreate( Bounds1 ));
					psTreeNode.apsSubNode.Add(SHPTreeNodeCreate( Bounds2 ));
					psTreeNode.apsSubNode.Add(SHPTreeNodeCreate( Bounds3 ));
					psTreeNode.apsSubNode.Add(SHPTreeNodeCreate( Bounds4 ));

					/* recurse back on this node now that it has subnodes */
					return( SHPTreeNodeAddShapeId( psTreeNode, psObject,
						nMaxDepth, nDimension ) );
				}
			}

				/* -------------------------------------------------------------------- */
				/*      Otherwise, consider creating two subnodes if could fit into     */
				/*      them, and adding to the appropriate subnode.                    */
				/* -------------------------------------------------------------------- */
			else if( nMaxDepth > 1 && psTreeNode.nSubNodes == 0 )
			{
				//double [] adfBoundsMin1=new double[4], adfBoundsMax1=new double[4];
				//double [] adfBoundsMin2=new double[4], adfBoundsMax2=new double[4];
				IEnvelope Bounds1,Bounds2;

				SHPTreeSplitBounds( psTreeNode.Bounds,
					out Bounds1, out Bounds2 );

				if( SHPCheckObjectContained(psObject, Bounds1) )
				{
					//psTreeNode.nSubNodes = 2;
					psTreeNode.apsSubNode.Clear();
					psTreeNode.apsSubNode.Add(SHPTreeNodeCreate( Bounds1 ));
					psTreeNode.apsSubNode.Add(SHPTreeNodeCreate( Bounds2 ));

					return( SHPTreeNodeAddShapeId( (SHPTreeNode)psTreeNode.apsSubNode[0], psObject,
						nMaxDepth - 1, nDimension ) );
				}
				else if( SHPCheckObjectContained(psObject, Bounds2 ) )
				{
					//psTreeNode.nSubNodes = 2;
					psTreeNode.apsSubNode.Clear();
					psTreeNode.apsSubNode.Add(SHPTreeNodeCreate( Bounds1 ));
					psTreeNode.apsSubNode.Add(SHPTreeNodeCreate( Bounds2 ));

					return( SHPTreeNodeAddShapeId( (SHPTreeNode)psTreeNode.apsSubNode[1], psObject,
						nMaxDepth - 1, nDimension ) );
				}
			}

			/* -------------------------------------------------------------------- */
			/*      If none of that worked, just add it to this nodes list.         */
			/* -------------------------------------------------------------------- */
			psTreeNode.panShapeIds.Add(psObject.ID);
			return true;
		}

		public bool SHPTreeAddShapeId( SHPObject psObject )

		{
			if(psTree==null) return false;
			return( SHPTreeNodeAddShapeId( psTree.psRoot, psObject,
				psTree.nMaxDepth, psTree.nDimension ) );
		}


		public void DestroyEmptyNodes(int minIDsPerNode) 
		{
				
		}

		private void DestroyEmptyNodes(SHPTreeNode node,int nimIDsPerNode) 
		{
			
		}


		

		private void SHPTreeCollectShapeIds( SHPTree hTree, SHPTreeNode psTreeNode,
			IEnvelope Bounds,
			//double [] padfBoundsMin, double [] padfBoundsMax,
			ref int pnShapeCount, ref int pnMaxShapes,
			List<int> ppanShapeList )

		{
			int		i;
    
			/* -------------------------------------------------------------------- */
			/*      Does this node overlap the area of interest at all?  If not,    */
			/*      return without adding to the list at all.                       */
			/* -------------------------------------------------------------------- */
			if( !SHPCheckBoundsOverlap(psTreeNode.Bounds,Bounds) )
				return;

			/* -------------------------------------------------------------------- */
			/*      Grow the list to hold the shapes on this node.                  */
			/* -------------------------------------------------------------------- */
			/*
			if( pnShapeCount + psTreeNode.nShapeCount > pnMaxShapes )
			{
				pnMaxShapes = (pnShapeCount + psTreeNode.nShapeCount) * 2 + 20;
				*ppanShapeList = (int *)
					SfRealloc(*ppanShapeList,sizeof(int) * *pnMaxShapes);
			}
			*/

			/* -------------------------------------------------------------------- */
			/*      Add the local nodes shapeids to the list.                       */
			/* -------------------------------------------------------------------- */
			for( i = 0; i < psTreeNode.nShapeCount; i++ )
			{
				ppanShapeList.Add(Convert.ToInt32(psTreeNode.panShapeIds[i]));
				pnShapeCount++;
				//(*ppanShapeList)[(*pnShapeCount)++] = psTreeNode->panShapeIds[i];
			}
    
			/* -------------------------------------------------------------------- */
			/*      Recurse to subnodes if they exist.                              */
			/* -------------------------------------------------------------------- */
			for( i = 0; i < psTreeNode.nSubNodes; i++ )
			{
				if( psTreeNode.apsSubNode[i] != null )
					SHPTreeCollectShapeIds( hTree, (SHPTreeNode)psTreeNode.apsSubNode[i],
						Bounds,
						ref pnShapeCount, ref pnMaxShapes,
						ppanShapeList );
			}
		}

		public List<int> FindShapeIds( IEnvelope Bounds )
		{
			List<int> panShapeList=new List<int>();
			if(psTree==null) return panShapeList;

			int	nMaxShapes = 0;

			/* -------------------------------------------------------------------- */
			/*      Perform the search by recursive descent.                        */
			/* -------------------------------------------------------------------- */
			int pnShapeCount = 0;

			SHPTreeCollectShapeIds( psTree, psTree.psRoot,
				Bounds,
				ref pnShapeCount, ref nMaxShapes,
				panShapeList );

			/* -------------------------------------------------------------------- */
			/*      Sort the id array                                               */
			/* -------------------------------------------------------------------- */

			//qsort(panShapeList, *pnShapeCount, sizeof(int), compare_ints);

			return panShapeList;
		}

		
		private int _nodenr=1;
		public void write(string path) 
		{
			System.IO.StreamWriter sw=new System.IO.StreamWriter(path,false);
			write(sw,this.psTree.psRoot,0);
			sw.Close();

		}
		private void write(System.IO.StreamWriter sw,SHPTreeNode node,int level) 
		{
			sw.WriteLine("\n\n\n"+(_nodenr++).ToString()+" node (level="+level.ToString()+")");
			sw.WriteLine(node.Bounds.minx.ToString()+"\t"+node.Bounds.miny.ToString()+"\t"+node.Bounds.maxx.ToString()+"\t"+node.Bounds.maxy.ToString());
			foreach(object obj in node.panShapeIds) 
			{
				sw.Write(obj.ToString()+", ");
			}
			foreach(SHPTreeNode n in node.apsSubNode) 
			{
				write(sw,n,level+1);
			}
		}

		public void writeSIX(string path) 
		{
			FileStream fs=new FileStream(path,FileMode.CreateNew,FileAccess.Write);
			BinaryWriter bw=new BinaryWriter(fs);
			writeSIXNode(bw,psTree.psRoot);
			bw.Close();
			fs.Close();
		}

		private void writeSIXNode(BinaryWriter bw,SHPTreeNode node) 
		{
			// BoundingBox
			bw.Write((double)node.Bounds.minx);
			bw.Write((double)node.Bounds.miny);
			bw.Write((double)node.Bounds.maxx);
			bw.Write((double)node.Bounds.maxy);

			// IDs
			bw.Write((System.Int32)node.panShapeIds.Count);
			for(int i=0;i<node.panShapeIds.Count;i++) 
			{
				bw.Write((System.Int32)node.panShapeIds[i]);
			}

			// Subnodes
			bw.Write((System.Int16)node.apsSubNode.Count);
			for(int i=0;i<node.apsSubNode.Count;i++) 
			{
				writeSIXNode(bw,(SHPTreeNode)node.apsSubNode[i]);
			}
		}
	}

	internal class SpatialIndexNodeComparer : System.Collections.Generic.IComparer<SpatialIndexNode>
	{
		double _x0=0,_y0=0;
		public SpatialIndexNodeComparer(double x0,double y0) 
		{
			_x0=x0;
			_y0=y0;
		}
		#region IComparer Member

		public int Compare(SpatialIndexNode x, SpatialIndexNode y)
		{	
			IEnvelope env1=((SpatialIndexNode)x).Rectangle.Envelope;
			IEnvelope env2=((SpatialIndexNode)y).Rectangle.Envelope;
			
			double s1=Math.Sqrt((env1.minx-_x0)*(env1.minx-_x0) + (env1.maxy-_y0)*(env1.maxy-_y0));
			double s2=Math.Sqrt((env2.minx-_x0)*(env2.minx-_x0) + (env2.maxy-_y0)*(env2.maxy-_y0));

			if(s1==s2) 
			{
				return ((SpatialIndexNode)x).CompareTo(y);	
			}
			return ((s1 < s2) ?  -1 : 1);
		}

		#endregion
	}

	internal class DualTreeNode
	{
		public Envelope Bounds;
		public short page=0;
		private static double SPLIT_RATIO=0.55;
		public static int maxPerNode=1000;

		public int	ShapeCount 
		{
			get { return ShapeIds.Count; }
		}
		
		public List<int> ShapeIds=new List<int>();
		public ArrayList Shapes=new ArrayList();

		public int	SubNodesCount 
		{
			get { return SubNodes.Count; }
		}
		public ArrayList SubNodes=new ArrayList();

		public void SplitTreeNode() 
		{
			// Keine Knoten mit Unterknoten nochmals splitten...
			if(this.SubNodes.Count>0) return;

			double w=Bounds.maxx-Bounds.minx;
			double h=Bounds.maxy-Bounds.miny;
			double minx,miny,maxx,maxy;

			if(w>h) 
			{
				minx=Bounds.minx;  maxx=minx+w*SPLIT_RATIO;
				miny=Bounds.miny;  maxy=miny+h;
				SubNodes.Add(DualTreeNode.CreateNode(new Envelope(minx,miny,maxx,maxy),0));
				minx=Bounds.maxx-w*SPLIT_RATIO;  maxx=Bounds.maxx;
				SubNodes.Add(DualTreeNode.CreateNode(new Envelope(minx,miny,maxx,maxy),1));
			} 
			else 
			{
				minx=Bounds.minx;  maxx=minx+w;
				miny=Bounds.miny;  maxy=miny+h*SPLIT_RATIO;
				SubNodes.Add(DualTreeNode.CreateNode(new Envelope(minx,miny,maxx,maxy),0));
				miny=Bounds.maxy-h*SPLIT_RATIO; maxy=Bounds.maxy;
				SubNodes.Add(DualTreeNode.CreateNode(new Envelope(minx,miny,maxx,maxy),1));
			}

			ArrayList myShapes=this.Shapes;
			this.Shapes=new ArrayList();

			foreach(SHPObject shape in myShapes) 
			{
				AddShape(shape);
			}
			myShapes=null;
		}


		private bool CheckObjectContained( SHPObject psObject  )
		{
			if(  psObject.env.minx < Bounds.minx
				|| psObject.env.maxx > Bounds.maxx )
				return false;
    
			if( psObject.env.miny < Bounds.miny 
				|| psObject.env.maxy > Bounds.maxy )
				return false;

			return true;
		}

		private bool CheckBoundsOverlap( 
			IEnvelope Box1,
			IEnvelope Box2 )
		{
			Envelope env=new Envelope(Box1);
			return env.Intersects(Box2);

			/*
			if ( Box2.maxx < Box1.minx ) return false;
			if ( Box2.maxy < Box2.miny ) return false;
			
			if ( Box1.maxx < Box2.minx ) return false;
			if ( Box1.maxy < Box2.miny ) return false;
			*/
			return true;
		}

		public bool AddShape(SHPObject sObject) 
		{
            if (sObject.env == null)
            {
                Shapes.Add(sObject);
                return true;
            }
			if(SubNodes.Count>0) 
			{
				foreach(DualTreeNode subNode in SubNodes) 
				{
					if(subNode.CheckObjectContained(sObject)) 
					{
						return subNode.AddShape(sObject);
					}
					// wenn in keines der Subnudes passt -> in eigenes übernehmen (ohne Kontrolle auf überlauf)
					//Shapes.Add(sObject);
					//return true;
				}
			}
			//else 
			{
				Shapes.Add(sObject);
				if(Shapes.Count>DualTreeNode.maxPerNode) 
				{
					this.SplitTreeNode();
				}
				return true;
			}
			return false;
		}

		public void Thin() 
		{
			foreach(SHPObject shape in Shapes) 
			{
				ShapeIds.Add(shape.ID);
			}
			Shapes.Clear();
			foreach(DualTreeNode subNode in SubNodes) 
			{
				subNode.Thin();
			}
		}


		public void CollectIDs( IEnvelope QueryBounds, List<int> Ids ) 
		{
			if( !CheckBoundsOverlap(QueryBounds,this.Bounds) )
				return;

			foreach(int id in ShapeIds) 
			{
				Ids.Add(id);
			}

			foreach(DualTreeNode subNode in SubNodes) 
			{
				subNode.CollectIDs(QueryBounds,Ids);
			}
		}

		static public DualTreeNode CreateNode(IEnvelope Bounds,short page) 
		{
			DualTreeNode node=new DualTreeNode();
			node.Bounds=new Envelope(Bounds);
			node.page=page;

			return node;
		} 
	}

	public class DualTree : IIndexTree, IProgressReporter
	{
		private int _maxPerNode=1000,_featureCount=0;
		private DualTreeNode _root;
        private ProgressReport _report = null;

		public DualTree() 
		{
			DualTreeNode.maxPerNode=_maxPerNode;
		}
		public DualTree(int maxPerNode) 
		{
			DualTreeNode.maxPerNode=_maxPerNode=maxPerNode;
		}
		public void CreateTree(IEnvelope Bounds) 
		{
			_root=DualTreeNode.CreateNode(Bounds,0);
            _featureCount = 0;
		}

		public bool AddShape(SHPObject shape) 
		{
            _featureCount++;
            if ((_featureCount % 100) == 0 && ReportProgress != null)
            {
                if (_report == null) _report = new ProgressReport();
                _report.Message = "Add Features...";
                _report.featureMax = -1;
                _report.featurePos = _featureCount;
                ReportProgress(_report);
            }

			return _root.AddShape(shape);
		}
		public void FinishIt() 
		{
			_root.Thin();
		}

        public IEnvelope Bounds
        {
            get
            {
                if (_root == null) return null;
                return _root.Bounds;
            }
        }
		private int _nodenr=1;
		public void write(string path) 
		{
			System.IO.StreamWriter sw=new System.IO.StreamWriter(path,false);
			write(sw,_root,0);
			sw.Close();

		}
		private void write(System.IO.StreamWriter sw,DualTreeNode node,int level) 
		{
			sw.WriteLine("\n\n\n"+(_nodenr++).ToString()+" node (level="+level.ToString()+")");
			sw.WriteLine(node.Bounds.minx.ToString()+"\t"+node.Bounds.miny.ToString()+"\t"+node.Bounds.maxx.ToString()+"\t"+node.Bounds.maxy.ToString());
			foreach(object obj in node.ShapeIds) 
			{
				sw.Write(obj.ToString()+", ");
			}
			foreach(DualTreeNode n in node.SubNodes) 
			{
				write(sw,n,level+1);
			}
		}


		public void writeMDB(string path) 
		{
			/*
			FDB.AccessFDB fdb=new gView.FDB.AccessFDB();
			fdb.Create(path);
			fdb.Open(path);
			int DSid=fdb.CreateDataset("TREEDS",null);

			gView.Framework.Data.Field field=new gView.Framework.Data.Field();
			field.type=FieldType.integer;
			field.name=field.aliasname="ID_COUNT";
			ArrayList fields=new ArrayList();
			fields.Add(field);
			int FCid=fdb.CreateFeatureClass(DSid,"TREE",geometryType.Polygon,fields);

			ArrayList features=new ArrayList();
			writeMDB(_root,features);
			fdb.Insert("TREE",features);

			fdb.Dispose();
			*/
		}

		private void writeMDB(DualTreeNode node,ArrayList features) 
		{
			/*
			Feature feat=new Feature();
			Polygon p=new Polygon();
			Ring ring=new Ring();
			ring.AddPoint(new Point(node.Bounds.minx,node.Bounds.miny));
			ring.AddPoint(new Point(node.Bounds.maxx,node.Bounds.miny));
			ring.AddPoint(new Point(node.Bounds.maxx,node.Bounds.maxy));
			ring.AddPoint(new Point(node.Bounds.minx,node.Bounds.maxy));
			//ring.AddPoint(new Point(node.Bounds.minx,node.Bounds.miny));
			p.AddRing(ring);
			feat.Shape=p;

			FieldValue fv=new FieldValue("ID_COUNT",node.ShapeIds.Count);
			feat.Fields.Add(fv);

			features.Add(feat);

			foreach(DualTreeNode subNode in node.SubNodes) 
			{
				writeMDB(subNode,features);
			}
			*/
		}

		public int _NID;
        public bool writeIDXIndex(string filename)
        {
            if (ReportProgress != null) _report = new ProgressReport();

            StreamWriter sw = new StreamWriter(filename, false);
            BinaryWriter bw = new BinaryWriter(sw.BaseStream);

            _NID = 0;
            List<SpatialIndexNode> nodes = new List<SpatialIndexNode>();
            if (ReportProgress != null)
            {
                _report.Message = "Collect Nodes...";
                ReportProgress(_report);
            }

            writeFDB_FC_Index_nodes(_root, nodes, _NID++, -1);

            if (ReportProgress != null)
            {
                _report.Message = "Write IDX...";
                _report.featurePos = 0;
                _report.featureMax = nodes.Count / 100;
                ReportProgress(_report);
            }

            int counter = 0;
            bw.Write((int)-999);
            foreach (SpatialIndexNode node in nodes)
            {
                bw.Write((int)node.NID);
                bw.Write((int)node.PID);
                bw.Write((double)node.Rectangle.Envelope.minx);
                bw.Write((double)node.Rectangle.Envelope.miny);
                bw.Write((double)node.Rectangle.Envelope.maxx);
                bw.Write((double)node.Rectangle.Envelope.maxy);
                bw.Write((int)node.IDs.Count);
                foreach (uint id in node.IDs)
                {
                    bw.Write((int)id);
                }
                counter++;
                if ((counter % 100)==0 && ReportProgress!=null)
                {
                    _report.featurePos++;
                    ReportProgress(_report);
                }
            }
            bw.Write((int)-999);
            sw.Close();

            _report = null;
            return true;
        }

		public List<SpatialIndexNode> Nodes
		{
			get 
			{
				_NID=0;
				List<SpatialIndexNode> nodes=new List<SpatialIndexNode>();
				writeFDB_FC_Index_nodes(_root,nodes,_NID++,-1);
				
				nodes.Sort(new SpatialIndexNodeComparer(_root.Bounds.minx,_root.Bounds.maxy));
				ReorderNodes(nodes);

				return nodes;
				/*
				return ReorderNodes(nodes,
					((SpatialIndexNode)nodes[0]).Rectangle.Envelope.minx,
					((SpatialIndexNode)nodes[0]).Rectangle.Envelope.maxy);
				*/
			}
		}

		// Von Nord-Westen aus sortieren
		private void ReorderNodes(List<SpatialIndexNode> nodes) 
		{
			int count=0,off=nodes.Count+100;

			foreach(SpatialIndexNode node in nodes) 
			{
				int oNID=node.NID;
				node.NID=count;

				foreach(SpatialIndexNode n in nodes)
				{
					if(n.PID<0) continue;
					if(n.PID==oNID) n.PID=off+count;
				}
				count++;
			}
			foreach(SpatialIndexNode n in nodes) 
			{
				if(n.PID<0) continue;
				n.PID-=off;
			}
		}
		
		private SpatialIndexNode getNodePerNID(ArrayList nodes,int nid) 
		{
			foreach(SpatialIndexNode node in nodes) 
			{
				if(node.NID==nid) return node;
			}
			return null;
		}

		private SpatialIndexNode getNodePerPID(ArrayList nodes,int pid) 
		{
			foreach(SpatialIndexNode node in nodes) 
			{
				if(node.PID==pid) return node;
			}
			return null;
		}

		private void writeFDB_FC_Index_nodes(DualTreeNode dnode,List<SpatialIndexNode> nodes,int NID,int PID) 
		{
			SpatialIndexNode node=new SpatialIndexNode();
			Polygon p=new Polygon();
			Ring ring=new Ring();
			ring.AddPoint(new Point(dnode.Bounds.minx,dnode.Bounds.miny));
			//ring.AddPoint(new Point(node.Bounds.maxx,node.Bounds.miny));
			ring.AddPoint(new Point(dnode.Bounds.maxx,dnode.Bounds.maxy));
			//ring.AddPoint(new Point(node.Bounds.minx,node.Bounds.maxy));
			//ring.AddPoint(new Point(node.Bounds.minx,node.Bounds.miny));
			p.AddRing(ring);
			node.Rectangle=p;

			node.NID=NID;
			node.PID=PID;
			node.Page=dnode.page;
			node.IDs=dnode.ShapeIds;
			nodes.Add(node);
	
			foreach(DualTreeNode subNode in dnode.SubNodes) 
			{
				writeFDB_FC_Index_nodes(subNode,nodes,_NID++,NID);
			}
		}
		
		#region IIndexTree Member

		public List<int> FindShapeIds(IEnvelope Bounds)
		{
			List<int> Ids=new List<int>();
			_root.CollectIDs(Bounds,Ids);

			return Ids;
		}

		#endregion

        #region IProgressReporter Members

        public event ProgressReporterEvent ReportProgress;

        public ICancelTracker CancelTracker
        {
            get { return null; }
        }

        #endregion
    }

	public class BinarySearchTreeNode 
	{
		public List<BinarySearchTreeNode> ChildNodes=null; 
		public int NID=0;
		public short Page=0;
	}
	public class BinarySearchTree : ISearchTree
	{
		protected IEnvelope _bounds;
		protected double _spatialRatio;
		protected BinarySearchTreeNode _root=null;

        protected BinarySearchTree() { }
		public BinarySearchTree(IEnvelope Bounds,double SpatialRatio) 
		{
			_bounds=Bounds=Bounds;
			_spatialRatio =SpatialRatio;
		}
		public BinarySearchTreeNode Root 
		{
			get 
			{
				return _root;
			}
			set 
			{
				_root=value;
			}
		}

        public List<long> CollectNIDs(IGeometry geometry)
        {
            if (geometry != null)
                return CollectNIDs(geometry.Envelope);

            return null;
        }
        private List<long> CollectNIDs(IEnvelope bounds) 
		{
			List<long> nids=new List<long>();
			nids.Add(_root.NID);
			CollectNIDs(_root,bounds,_bounds,nids);
			nids.Sort();

			return nids;
		}

		private void CollectNIDs(BinarySearchTreeNode parent,IEnvelope Bounds,IEnvelope parentBounds,List<long> nids) 
		{
			if(parent.ChildNodes==null) return;
			if(parent.ChildNodes.Count==0) return;

			double w=parentBounds.maxx-parentBounds.minx;
			double h=parentBounds.maxy-parentBounds.miny;
			double minx0,miny0,maxx0,maxy0;
			double minx1,miny1,maxx1,maxy1;

			if(w>h) 
			{
				minx0=parentBounds.minx;  maxx0=minx0+w*_spatialRatio;
				miny0=parentBounds.miny;  maxy0=miny0+h;
				//SubNodes.Add(DualTreeNode.CreateNode(new Envelope(minx,miny,maxx,maxy),0));
				minx1=parentBounds.maxx-w*_spatialRatio;  maxx1=parentBounds.maxx;
				miny1=miny0;                              maxy1=maxy0;
				//SubNodes.Add(DualTreeNode.CreateNode(new Envelope(minx,miny,maxx,maxy),1));
			} 
			else 
			{
				minx0=parentBounds.minx;  maxx0=minx0+w;
				miny0=parentBounds.miny;  maxy0=miny0+h*_spatialRatio;
				//SubNodes.Add(DualTreeNode.CreateNode(new Envelope(minx,miny,maxx,maxy),0));
				minx1=minx0;                             maxx1=maxx0;
				miny1=parentBounds.maxy-h*_spatialRatio; maxy1=parentBounds.maxy;
				//SubNodes.Add(DualTreeNode.CreateNode(new Envelope(minx,miny,maxx,maxy),1));
			}

			foreach(BinarySearchTreeNode node in parent.ChildNodes) 
			{
				Envelope nodeBounds=null;
				switch(node.Page) 
				{
					case 0:
						nodeBounds=new Envelope(minx0,miny0,maxx0,maxy0);
						break;
					case 1:
						nodeBounds=new Envelope(minx1,miny1,maxx1,maxy1);
						break;
				}
				if(nodeBounds==null) continue;
				if(nodeBounds.Intersects(Bounds)) 
				{
					nids.Add(node.NID);
					CollectNIDs(node,Bounds,nodeBounds,nids);
				} 
				else 
				{
					nodeBounds=null;
				}
			}
		}

        public List<long> CollectNIDsPlus(IEnvelope envelope)
        {
            return CollectNIDs(envelope);
        }
	}

    internal class IDXIndexTreeNode
    {
        public int NID;
        public int pos;
        public Envelope Bounds;
        public List<IDXIndexTreeNode> SubNodes = new List<IDXIndexTreeNode>();

        private bool CheckBoundsOverlap(
            IEnvelope Box1,
            IEnvelope Box2)
        {
            Envelope env = new Envelope(Box1);
            return env.Intersects(Box2);
        }

        public void CollectIDs(BinaryReader br,IEnvelope QueryBounds, List<int> Ids)
        {
            if (!CheckBoundsOverlap(QueryBounds, this.Bounds))
                return;

            br.BaseStream.Position = pos;
            int count = br.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                Ids.Add(br.ReadInt32());
            }

            foreach (IDXIndexTreeNode subNode in SubNodes)
            {
                subNode.CollectIDs(br, QueryBounds, Ids);
            }
        }
    }

    public class IDXIndexTree : IIndexTree
    {
        private string _filename="";
        private IDXIndexTreeNode _root = null;

        public IDXIndexTree(string filename)
        {
            FileInfo fi = new FileInfo(filename);
            if (!fi.Exists) return;

            _filename = filename;

            StreamReader sr = new StreamReader(filename);
            BinaryReader br = new BinaryReader(sr.BaseStream);

            if (br.ReadInt32() != -999)
            {
                sr.Close();
                return;
            }

            DataTable tab = new DataTable();
            tab.Columns.Add("NID", typeof(int));
            tab.Columns.Add("PID", typeof(int));
            tab.Columns.Add("minx", typeof(double));
            tab.Columns.Add("miny", typeof(double));
            tab.Columns.Add("maxx", typeof(double));
            tab.Columns.Add("maxy", typeof(double));
            tab.Columns.Add("POS", typeof(int));

            int NID;
            while ((NID = br.ReadInt32()) != null)
            {
                if (NID == -999) break;

                DataRow row=tab.NewRow();
                row["NID"] = NID;
                row["PID"] = br.ReadInt32();
                row["minx"] = br.ReadDouble();
                row["miny"] = br.ReadDouble();
                row["maxx"] = br.ReadDouble();
                row["maxy"] = br.ReadDouble();
                row["POS"] = br.BaseStream.Position;
               
                if ((int)row["NID"] == 0)  // Root;
                {
                    _root = new IDXIndexTreeNode();
                    _root.NID = (int)row["NID"];
                    _root.Bounds = new Envelope((double)row["minx"], (double)row["miny"], (double)row["maxx"], (double)row["maxy"]);
                    _root.pos = (int)row["POS"];
                }
                tab.Rows.Add(row);

                int count = (int)br.ReadInt32();
                br.BaseStream.Position += count * sizeof(int);
            }

            if (_root != null)
            {
                InsertBinarySearchTreeNodes(_root, tab);
            }

            sr.Close();
        }

        private void InsertBinarySearchTreeNodes(IDXIndexTreeNode parent, DataTable tab)
        {
            DataRow[] rows = tab.Select("PID=" + parent.NID);
            foreach (DataRow row in rows)
            {
                IDXIndexTreeNode node = new IDXIndexTreeNode();
                node.NID = (int)row["NID"];
                node.pos = (int)row["POS"];
                node.Bounds = new Envelope((double)row["minx"], (double)row["miny"], (double)row["maxx"], (double)row["maxy"]);
                InsertBinarySearchTreeNodes(node, tab);

                if (parent.SubNodes == null) parent.SubNodes = new List<IDXIndexTreeNode>();
                parent.SubNodes.Add(node);
            }
        }

        #region IIndexTree Members

        public List<int> FindShapeIds(IEnvelope Bounds)
        {
            StreamReader sr = new StreamReader(_filename);

            try
            {
                BinaryReader br = new BinaryReader(sr.BaseStream);
                List<int> Ids = new List<int>();
                _root.CollectIDs(br, Bounds, Ids);

                sr.Close();
                return Ids;
            }
            catch
            {
                sr.Close();
                return null;
            }
        }

        #endregion
    }

}
