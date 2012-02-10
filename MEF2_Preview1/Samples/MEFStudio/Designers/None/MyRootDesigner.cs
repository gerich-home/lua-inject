//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Windows.Forms;
using System.Data;
using System.Windows.Forms.Design;
using System.Drawing.Design;
using Microsoft.Tools.Graphs;
using Microsoft.Tools.Graphs.Bars;
using Microsoft.Tools.Graphs.Pies;
using Microsoft.Tools.Graphs.Legends;
using System.Drawing.Drawing2D;
using System.Diagnostics;

namespace Designers
{
    /// <summary>
    /// Demonstrates how to write a custom RootDesigner. This designer has a View
    /// of a Graph - it shows the number of components added to the designer
    /// in a pie/bar chart.
    /// </summary>
	public class MyRootDesigner : ComponentDesigner, IRootDesigner, IToolboxUser
	{
		MyRootControl _rootView;
	
		#region Implementation of IRootDesigner
		public object GetView(System.ComponentModel.Design.ViewTechnology technology)
		{
			_rootView = new MyRootControl(this);
			return _rootView;
		}
		public System.ComponentModel.Design.ViewTechnology[] SupportedTechnologies
		{
			get
			{
				return new ViewTechnology[]{ViewTechnology.Default};
			}
		}
		#endregion

		#region Implementation of IToolboxUser
		public void ToolPicked(System.Drawing.Design.ToolboxItem tool)
		{
			_rootView.InvokeToolboxItem(tool);
		}

		public bool GetToolSupported(System.Drawing.Design.ToolboxItem tool)
		{
			return true;
		}
		#endregion

		public new object GetService(Type type)
		{
			return base.GetService(type);
		}


		#region MyRootControl
        /// <summary>
        /// This is the View of the RootDesigner.
        /// </summary>
		public class MyRootControl : ScrollableControl
		{
			enum GraphStype
			{
				Pie = 1,
				Bar = 2
			}

			private string _displayString = "Pie Graph Representation of components added.";
			private MyRootDesigner _rootDesigner;
			private Hashtable _componentInfoTable;
			private int _totalComponents=0;
			private LinkLabel _linkLabel;
			private GraphStype _graphStyle = GraphStype.Pie;


			public MyRootControl(MyRootDesigner rootDesigner)
			{
				_rootDesigner = rootDesigner;
				_componentInfoTable = new Hashtable();
				_linkLabel = new LinkLabel();
				_linkLabel.Text = "Graph Style";
				_linkLabel.Location = new Point(10, 5);
				_linkLabel.Visible = false;
				_linkLabel.Click += new EventHandler(_linkLabel_Click);
				this.Controls.Add(_linkLabel);
				this.Resize += new EventHandler(MyRootControl_Resize);
				for(int i=1;i<_rootDesigner.Component.Site.Container.Components.Count;i++)
					UpdateTable(_rootDesigner.Component.Site.Container.Components[i].GetType());

				Invalidate();
			}
			protected override void OnPaint(PaintEventArgs pe)
			{
				try
				{
					if (_componentInfoTable.Count == 0)
					{
						_linkLabel.Visible = false;

						string s = "Add objects from the toolbox on to this custom Graphical RootDesigner";
						StringFormat sf = new StringFormat();
						sf.Alignment = StringAlignment.Center;
						sf.LineAlignment = StringAlignment.Center;

						pe.Graphics.FillRectangle(new LinearGradientBrush(this.Bounds, Color.White, Color.Orange, 45F), this.Bounds);
						pe.Graphics.DrawString(s, Font, new SolidBrush(Color.Black), this.Bounds, sf);
					}
					else
					{
						_linkLabel.Visible = true;

						pe.Graphics.FillRectangle(new SolidBrush(Color.White), this.Bounds);

						Image image = null;

						if (_graphStyle == GraphStype.Pie)
							image = GetPieGraph();
						else
							image = GetBarGraph();

						pe.Graphics.DrawImage(image, this.Bounds);
					}
				}
                catch (Exception ex)
                {
                    Trace.WriteLine(ex.ToString());
                }
            } // OnPaint


			public void InvokeToolboxItem(System.Drawing.Design.ToolboxItem tool)
			{
				IComponent[] newComponents = tool.CreateComponents(DesignerHost);
				_displayString = "Last Component added: " + newComponents[0].GetType().ToString();
				UpdateTable(newComponents[0].GetType());
				Invalidate();
			}
			private void UpdateTable(Type type)
			{
				if (_componentInfoTable[type] == null)
				{
					ComponentInfo ci = new ComponentInfo();
					RandomUtil ru = new RandomUtil();

					ci.Type = type;
					ci.Color = ru.GetColor();
					ci.Count = 1;
					_componentInfoTable.Add(type, ci);
					_totalComponents++;
				}
				else
				{
					ComponentInfo ci = (ComponentInfo)_componentInfoTable[type];
					_componentInfoTable.Remove(type);
					ci.Count++;
					_componentInfoTable.Add(type, ci);
				}
			}
			public IDesignerHost DesignerHost
			{
				get
				{
					return (IDesignerHost)_rootDesigner.GetService(typeof(IDesignerHost));
				}
			}

			public IToolboxService ToolboxService
			{
				get
				{
					return (IToolboxService)_rootDesigner.GetService(typeof(IToolboxService));
				}
			}
			private Image GetPieGraph()
			{
				RandomUtil ru = new RandomUtil();
				PieGraph pg = new PieGraph(this.Size);

				pg.Color = Color.White;
				pg.ColorGradient = Color.Orange; 

				Legend legend = new Legend(this.Width, 70);

				legend.Text = String.Empty;
				pg.Text = _displayString + " Total: " + _totalComponents.ToString();

				ICollection keys = _componentInfoTable.Keys;
				IEnumerator ie = keys.GetEnumerator();
				while (ie.MoveNext())
				{
					Type key = (Type)ie.Current;
					ComponentInfo ci = (ComponentInfo)_componentInfoTable[key];

					PieSlice ps = new PieSlice(ci.Count, ci.Color);
					pg.Slices.Add(ps);

					LegendEntry le = new LegendEntry(ci.Color, ci.Type.Name.ToString().Trim());
					legend.LegendEntryCollection.Add(le);
				}

				return GraphRenderer.DrawGraphAndLegend(pg, legend, this.Size);
			}
			private Image GetBarGraph()
			{
				RandomUtil ru = new RandomUtil();
				BarGraph bg = new BarGraph(this.Size);

				bg.Color = Color.White;
				bg.ColorGradient = Color.Orange;

				Legend legend = new Legend(this.Width, 70);

				legend.Text = String.Empty;
				bg.Text = _displayString + " Total: " + _totalComponents.ToString();

				ICollection keys = _componentInfoTable.Keys;
				IEnumerator ie = keys.GetEnumerator();

				while (ie.MoveNext())
				{
					Type key = (Type)ie.Current;
					ComponentInfo ci = (ComponentInfo)_componentInfoTable[key];
					BarSlice bs = new BarSlice(ci.Count, ci.Color);

					bg.BarSliceCollection.Add(bs);

					LegendEntry le = new LegendEntry(ci.Color, ci.Type.Name.ToString().Trim());

					legend.LegendEntryCollection.Add(le);
				}

				return GraphRenderer.DrawGraphAndLegend(bg, legend, this.Size);
			}
			private class ComponentInfo
			{
				public Type Type;
				public Color Color;
				public int Count;
			}
			private void MyRootControl_Resize(object sender, EventArgs e)
			{
				Invalidate();
			}
			private void _linkLabel_Click(object sender, EventArgs e)
			{
				if (_graphStyle == GraphStype.Pie)
					_graphStyle = GraphStype.Bar;
				else
					_graphStyle = GraphStype.Pie;

				Invalidate();
			}
		} // class MyRootControl
		#endregion

	}// class MyRootDesigner
}// namespace
