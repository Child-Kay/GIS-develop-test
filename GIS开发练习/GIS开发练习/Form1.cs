using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.SystemUI;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Carto;

namespace GIS开发练习
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            ESRI.ArcGIS.RuntimeManager.Bind(ESRI.ArcGIS.ProductCode.EngineOrDesktop);       //绑定ESRI组件
            ESRI.ArcGIS.RuntimeManager.BindLicense(ESRI.ArcGIS.ProductCode.EngineOrDesktop);

            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            ICommand cmd = new ControlsMapZoomPanToolClass();
            cmd.OnCreate(axMapControl1.Object);
            axMapControl1.CurrentTool = cmd as ITool;
        }

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {

        }

        private ILayer mLayer = null;

        private void 缩放至图层ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (mLayer != null)     //判断图层是否为空
            {
                (axMapControl1.Map as IActiveView).Extent = mLayer.AreaOfInterest;      //将当前视图范围赋予mLayer的值
                (axMapControl1.Map as IActiveView).PartialRefresh(esriViewDrawPhase.esriViewGeography, null, null);     //视图局部刷新
            }
        }

        private void axTOCControl1_OnMouseDown(object sender, ITOCControlEvents_OnMouseDownEvent e)
        {
            if (e.button == 2)         //为右键时
            {
                esriTOCControlItem item = esriTOCControlItem.esriTOCControlItemNone;
                IBasicMap imap = null;
                ILayer il = null;
                Object o1 = null;
                Object o2 = null;

                axTOCControl1.HitTest(e.x, e.y, ref item, ref imap, ref il, ref o1, ref o2);      //赋值当前鼠标点击图层到item

                mLayer = il;
                
                if (item == esriTOCControlItem.esriTOCControlItemLayer)     //当选择为图层时
                {
                   
                    contextMenuStrip1.Show(Control.MousePosition);      //在当前鼠标位置显示右键菜单
                }
            }

        }
    }
}
