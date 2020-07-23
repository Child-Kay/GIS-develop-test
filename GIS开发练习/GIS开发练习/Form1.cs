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
using ESRI.ArcGIS.Geoprocessing;
using ESRI.ArcGIS.esriSystem;
using System.Data.OleDb;



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

        private void button2_Click(object sender, EventArgs e)
        {
            object sev = null;      //标记处理的地理信息

            IGeoProcessor2 gp = new GeoProcessorClass();
            gp.OverwriteOutput = true;      //设置gp.OverwriteOutput是指可以用一个输出执行多次工具
            gp.AddToolbox(@"C:\Users\Administrator\Desktop\SlopExtract2\Process_Vital\ImportantFile\arcgis模型\工具箱.tbx");


            IGeoProcessorResult result = new GeoProcessorResultClass();


            //设置参数
            IVariantArray parameters = new VarArrayClass();      //using ESRI.ArcGIS.esriSystem;引用
            
            try
            {
                parameters.Add(@"C:\Users\Administrator\Desktop\SlopExtract2\Data\Dem8mYX_CDG.tif");
                parameters.Add("");
                parameters.Add(@"C:\Users\Administrator\Desktop\SlopExtract\Process\TEST_0719\resutlt\abc");

                 gp.Execute("模型", parameters, null);
                MessageBox.Show( gp.GetMessages(ref sev));
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            

            





        }

        /// <summary>
        /// 合并算法
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button3_Click(object sender, EventArgs e)
        {

            DataSet dataSet = ExcelToDS(@"C:\Users\Administrator\Desktop\SlopExtract\Process\TEST_0715\table_邻近关系表.xls");

            DataTable t = dataSet.Tables[0];
            string a = t.Rows[0][0].ToString();

            dataGridView1.DataSource = t;


            DataTable comElement= GetCombinedElement();

            DataSet ds = new DataSet();
            ds.Tables.Add(comElement);

            dataGridView1.DataSource = comElement;



      


        }



        /// <summary>
        /// 需要合并的邻近表记录（包括相邻面要素的ID）
        /// </summary>
        public struct MergeRecord
        {
            public int id1;
            public int id2;

        };



        /// <summary>
        /// 获取合并面要素集合（返回合并要素ID列表）
        /// </summary>
        public DataTable GetCombinedElement()
        {
            //定义主要变量
            List<List<int>> CombinedList = new List<List<int>>();       //合并列表
            List<MergeRecord> mRecords = new List<MergeRecord>();           //筛选出需要合并的相邻对象ID集合
            DataTable table_surface;        //面要素属性表
            DataTable table_near;               ///邻近表
            Double limit = 5;               //限差


            //获取要素属性表和邻近表
            DataSet dataSet = ExcelToDS(@"C:\Users\Administrator\Desktop\SlopExtract\Process\TEST_0715\table_邻近关系表.xls");
            table_near = dataSet.Tables[0];

            dataSet = ExcelToDS(@"C:\Users\Administrator\Desktop\SlopExtract\Process\TEST_0715\table_坡体单元属性表_r.xls");
            table_surface = dataSet.Tables[0];


            //邻近表中，提取所有满足限定条件（f(坡度,坡向)<limit）的记录
            foreach(DataRow row in table_near.Rows )
            {
                MergeRecord m;
                m.id1 = Int32.Parse(row[1].ToString());     //获取邻近表中一条记录的两个对象ID
                m.id2 = Int32.Parse(row[2].ToString());

                if (table_surface.Rows[m.id1][4].ToString() == "" || table_surface.Rows[m.id2][4].ToString() == "")       //当存在未定义值的记录是，跳过
                    continue;
                

                Double slop1 =Double.Parse( table_surface.Rows[m.id1][4].ToString());           //提取ID对应要素的坡度、坡向
                Double slop2 = Double.Parse(table_surface.Rows[m.id2][4].ToString());
                Double aspect1 = Double.Parse(table_surface.Rows[m.id1][5].ToString());
                Double aspect2 = Double.Parse(table_surface.Rows[m.id2][5].ToString());


                Double scale_factor = Math.Sqrt((slop1 - slop2) * (slop1 - slop2) + (aspect1 - aspect2) * (aspect1 - aspect2));     //计算尺度因子

                if(scale_factor<limit)          //当尺度因子小于限差
                {
                    if(m.id1>m.id2)     //保持 id2>id1
                    {
                        int id_int = m.id2;
                        m.id2 = m.id1;
                        m.id1 = id_int;
                    }

                    bool isExist = false;           //判断m在mRecords中是否存在
                    foreach(MergeRecord merge in mRecords)
                    {
                        if (m.id1 == merge.id1 && m.id2 == merge.id2)
                            isExist = true;

                    }

                    if(!isExist)            //当不存在时，增加记录
                    {
                        mRecords.Add(m);
                    }


                }


            }





            //递归算法，将满足条件的邻近表转换为合并要素ID列表
            List<int> element_ids = new List<int>();        //获取所有id1对象id集合
            foreach(MergeRecord m in mRecords)
            {
                if (element_ids.Count == 0)
                    element_ids.Add(m.id1);
                else if (m.id1 != element_ids[element_ids.Count - 1])
                    element_ids.Add(m.id1);

            }


            foreach(int id in element_ids)      //遍历element_ids中所有对象
            {
                bool isExit = false;

                foreach(List<int> i in CombinedList)        //查找对象是否存在
                {
                    if (isExit)
                        break;

                    foreach(int j in i)
                    {
                        if(id==j)
                        {
                            isExit = true;
                            break;
                        }
                    }
                }

                if (isExit)
                    continue;


                List<int> ids = new List<int>();        //合并对象集合
                GetList(id, ref ids, mRecords);         //获取对象
                CombinedList.Add(ids);
            }


            //转化成table中的列

            DataTable table = new DataTable();
            table.Columns.Add("合并");


            foreach(DataRow row  in table_surface.Rows)     //赋值id号的一列
            {
                DataRow r = table.NewRow();
                r[0] = row[0];
                table.Rows.Add(r);
            }

            foreach(List<int> elements in CombinedList)         //根据对象列表赋值
            {
                int value = elements[0];

                foreach(int id in elements)
                {
                    
                    table.Rows[id].BeginEdit();         //table赋值技巧
                    table.Rows[id][0] = value;
                    table.Rows[id].EndEdit();

                }

            }


            return table;
        }



        public void GetList (int id, ref List<int> ids,List<MergeRecord> mRecords)
        {
            if (!IsIn(id,ids)  )       //加自身
                ids.Add(id);

            foreach(MergeRecord r in mRecords)      //获取该对象所有相邻对象
            {
                if (id == r.id1&&!IsIn(r.id2,ids))
                {
                    ids.Add(r.id2);
                    GetList(r.id2, ref ids, mRecords);
                }
                    
                if(id==r.id2&&!IsIn(r.id1,ids))
                {
                    ids.Add(r.id1);
                    GetList(r.id1, ref ids, mRecords);
                }


            }



        }

        public bool IsIn(int id,List<int> ids)
        {
            bool isin = false;

            foreach(int i in ids)
            {
                if (i == id)
                    isin = true;
            }

            return isin;
        }
  
             

        /// <summary>
        /// 根据值获取索引(未使用)
        /// </summary>
        /// <returns></returns>
        public int GetIndexByValue(int value,DataColumn col)
        {
            int index=-1;
            

            return index;

        }




        /// <summary>
        /// 读取excle表格
        /// </summary>
        /// <param name="Path"></param>
        /// <returns></returns>
        public DataSet ExcelToDS(string Path)
        {
            string strConn = "Provider=Microsoft.Jet.OLEDB.4.0;" + "Data Source=" + Path + ";" + "Extended Properties=Excel 8.0;";
            OleDbConnection conn = new OleDbConnection(strConn);
            conn.Open();
            string strExcel = "";
            OleDbDataAdapter myCommand = null;
            DataSet ds = null;

            //获取表格的名称
            DataTable schemaTable = conn.GetOleDbSchemaTable(System.Data.OleDb.OleDbSchemaGuid.Tables, null);
            string tableName = schemaTable.Rows[0][2].ToString().Trim();


            strExcel = "select * from ["+tableName+"]";
            myCommand = new OleDbDataAdapter(strExcel, strConn);
            ds = new DataSet();
            myCommand.Fill(ds, "table1");
            return ds;
        }


        /// <summary>
        /// 写入Excel（未测试）
        /// </summary>
        /// <param name="Path"></param>
        /// <param name="oldds"></param>
        public void DSToExcel(string Path, DataSet oldds)
        {
            //先得到汇总EXCEL的DataSet 主要目的是获得EXCEL在DataSet中的结构 
            string strCon = " Provider = Microsoft.Jet.OLEDB.4.0 ; Data Source =" + Path + ";Extended Properties=Excel 8.0";
            OleDbConnection myConn = new OleDbConnection(strCon);
            string strCom = "select * from [Sheet1$]";
            myConn.Open();
            OleDbDataAdapter myCommand = new OleDbDataAdapter(strCom, myConn);
            System.Data.OleDb.OleDbCommandBuilder builder = new OleDbCommandBuilder(myCommand);
            //QuotePrefix和QuoteSuffix主要是对builder生成InsertComment命令时使用。 
            builder.QuotePrefix = "[";     //获取insert语句中保留字符（起始位置） 
            builder.QuoteSuffix = "]"; //获取insert语句中保留字符（结束位置） 
            DataSet newds = new DataSet();
            myCommand.Fill(newds, "Table1");
            for (int i = 0; i < oldds.Tables[0].Rows.Count; i++)
            {
                //在这里不能使用ImportRow方法将一行导入到news中，因为ImportRow将保留原来DataRow的所有设置(DataRowState状态不变)。在使用ImportRow后newds内有值，但不能更新到Excel中因为所有导入行的DataRowState != Added

                DataRow nrow =newds.Tables["Table1"].NewRow();
                for (int j = 0; j < newds.Tables[0].Columns.Count; j++)
                {
                    nrow[j] = oldds.Tables[0].Rows[i][j];
                }
                newds.Tables["Table1"].Rows.Add(nrow);
            }
            myCommand.Update(newds, "Table1");
            myConn.Close();
        }


        /// <summary>
        /// 读取shp文件
        /// </summary>
        /// <returns></returns>
        public string[] OpenShapeFile()
        {
            string[] ShpFile = new string[2];
            OpenFileDialog OpenShpFile = new OpenFileDialog();
            OpenShpFile.Title = "打开Shape文件";
            OpenShpFile.InitialDirectory = "E:";
            OpenShpFile.Filter = "Shape文件(*.shp)|*.shp";

            if (OpenShpFile.ShowDialog() == DialogResult.OK)
            {
                string ShapPath = OpenShpFile.FileName;
                //利用"\\"将文件路径分成两部分
                int Position = ShapPath.LastIndexOf("\\");

                string FilePath = ShapPath.Substring(0, Position);
                string ShpName = ShapPath.Substring(Position + 1);
                ShpFile[0] = FilePath;

                ShpFile[1] = ShpName;

            }
            return ShpFile;
        }



    }
}
