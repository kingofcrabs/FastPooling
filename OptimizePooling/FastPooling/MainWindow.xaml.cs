using OptimizePooling;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Threading;

namespace FastPooling
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class FastPoolingMainWindow : Window
    {
        worklist worklist = new worklist();
        public FastPoolingMainWindow()
        {
            InitializeComponent();
            txtRound.DataContext = GlobalVars.Instance;
            this.Loaded += MainWindow_Loaded;
            this.Closing += MainWindow_Closing;
            dataGridView.Visible = false;
        }

        void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Pipeserver.Close();
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            lblVersion.Content = "版本号：" + strings.version;
            chkUseTwoPlates.DataContext = this;
            CreateNamedPipeServer();
        }
       

        private int ParseValue(string sValue,string description, int min, int max)
        {
            int val = 0;
            if(sValue == "")
                throw new Exception(description + "不得为空！");
            bool bok = int.TryParse(sValue, out val);
            if (!bok)
                throw new Exception(description + "必须为数字！");
            if (val < min || val > max)
                throw new Exception(description + string.Format("必须在{0}~{1}之间", min, max));
            return val;

        }
        private void btnSetGrid_Click(object sender, RoutedEventArgs e)
        {
            //string sGridCnt = txtGridCnt.Text;
            try
            {
                onSetGrid();
            }
            catch(Exception ex)
            {
                AddErrorInfo(ex.Message);
                return;
            }
        }

        private void onSetGrid()
        {
            string sPoolingSampleCnt = txtPoolingSampleCnt.Text;
            int poolingSmpCnt = ParseValue(sPoolingSampleCnt, "Pooling样本数", 0, 96 * 6);
            string sNormalSampleCnt = txtNormalSampleCnt.Text;
            int normalSmpCnt = ParseValue(sNormalSampleCnt, "常规样本数", 0, 96);
            string sGridCnt = txtGridCnt.Text;

            if(poolingSmpCnt + normalSmpCnt == 0)
            {
                AddErrorInfo("常规样本数与Pooling样板数之和为0！");
                return;
            }

            int remainSmpCnt = poolingSmpCnt + normalSmpCnt - worklist.finishedSmpCnt;
            int nGridCnt = ParseValue(sGridCnt, "Grid数", 1, (remainSmpCnt + 15) / 16);
            int neededDstWell = worklist.CalculateNeededDstWell(poolingSmpCnt);
            if (normalSmpCnt + neededDstWell >= 96)
            {
                AddErrorInfo(string.Format("目标孔数：{0}超过96！", neededDstWell));
                return;
            }
            int dstNeeded = worklist.SetConfig(poolingSmpCnt, normalSmpCnt, UseTwoPlates);
            txtPlateNeeded.Text = worklist.bUseTwoPlates ? "2" : "1";
            txtDstWellCnt.Text = dstNeeded.ToString();
            var rCommands = worklist.GenerateRCommand();
            File.WriteAllLines(Folders.GetOutputFolder() + "rCommands.gwl", rCommands);
            Helper.WriteTotalDstWellCnt(dstNeeded);
            Helper.WriteThisBatchGrid(nGridCnt);
            Helper.CloseWaiter(strings.waiterName);
            EnableControls(false);
            InitDataGridView(12);
        }

        private void InitDataGridView(int gridCnt)
        {
            dataGridView.AllowUserToAddRows = false;
            dataGridView.EnableHeadersVisualStyles = false;
            dataGridView.Columns.Clear();
            int startGrid = GlobalVars.Instance.StartGridID;
            List<string> strs = new List<string>();

            for (int i = 0; i < gridCnt; i++)
            {
                DataGridViewTextBoxColumn column = new DataGridViewTextBoxColumn();
                column.HeaderText = string.Format("条{0}", startGrid + i);
                dataGridView.Columns.Add(column);
                dataGridView.Columns[i].SortMode = DataGridViewColumnSortMode.Programmatic;
                strs.Add("");
            }
            dataGridView.RowHeadersWidth = 80;
            for (int i = 0; i < 16; i++)
            {
                dataGridView.Rows.Add(strs.ToArray());
                dataGridView.Rows[i].HeaderCell.Value = string.Format("行{0}", i + 1);
            }
        }

        public void UpdateDataGridView(int gridID, List<string>barcode)
        {
            for (int i = 0; i < barcode.Count; i++ )
            {
                int col = gridID - GlobalVars.Instance.StartGridID;
                var cell = dataGridView.Rows[i].Cells[col];
                cell.Value = barcode[i];
                System.Drawing.Color foreColor = System.Drawing.Color.Green;
                string curBarcode = barcode[i];
                if( curBarcode == "***")
                {
                    foreColor = System.Drawing.Color.Red;
                }
                else if( curBarcode == "$$$")
                {
                    foreColor = System.Drawing.Color.Orange;
                }
                cell.Style.ForeColor = foreColor;
            }
        }

        private void AddWarning(string warning)
        {
            richTxtInfo.SelectionBrush = Brushes.Orange;
            richTxtInfo.AppendText(warning);

        }
       
        private void AddColorText(string txt, Brush brush)
        {
            TextRange tr = new TextRange(richTxtInfo.Document.ContentEnd,richTxtInfo.Document.ContentEnd);
            tr.Text = txt + "\r";
            tr.ApplyPropertyValue(TextElement.ForegroundProperty, brush);
            richTxtInfo.ScrollToEnd();
        }

       

        private void AddErrorInfo(string info)
        {
            AddColorText(info, Brushes.Red);
            richTxtInfo.Refresh();
        }

        public bool UseTwoPlates { get; set; }

        private void AddInfo(string info, bool success = true)
        {
            var brush =success ? Brushes.DarkGreen : Brushes.Blue;
            AddColorText(info, brush);
        }

        private void EnableControls(bool bEnable)
        {
            txtNormalSampleCnt.IsEnabled = bEnable;
            txtPoolingSampleCnt.IsEnabled = bEnable;
            btnSetGrid.IsEnabled = bEnable;
            chkUseTwoPlates.IsEnabled = bEnable;
        }

        #region namedpipe
        private void CreateNamedPipeServer()
        {
            try
            {
                Pipeserver.owner = this;
                Pipeserver.ownerInvoker = new Invoker(this);
                ThreadStart pipeThread = new ThreadStart(Pipeserver.createPipeServer);
                Thread listenerThread = new Thread(pipeThread);
                listenerThread.SetApartmentState(ApartmentState.STA);
                listenerThread.IsBackground = true;
                listenerThread.Start();
            }
            catch (Exception ex)
            {
                AddErrorInfo(ex.Message);
            }
        }

        internal void ExecuteCommand(string sCommand)
        {
            if (sCommand.Contains("shutdown"))
            {
                this.Close();
                return;
            }

            if (sCommand.Contains("NewBatch"))
            {
                GlobalVars.Instance.BatchID++;
                Folders.ClearOutPut();
                EnableControls(true);
                return;
            }

            bool bok = true;

            try
            {
                ExecuteCommandImpl(sCommand);
            }
            catch (Exception ex)
            {
                AddErrorInfo(ex.Message);
                bok = false;
            }

            Helper.WriteResult(bok);
            if (bok)
            {
                Thread.Sleep(1000);
                Helper.CloseWaiter(strings.waiterName);
            }
                
           
        }
        private void btnFix_Click(object sender, RoutedEventArgs e)
        {
            Fix();
        }

        private void Fix()
        {
            if (dataGridView.EditingControl is TextBox)
            {
                dataGridView.EditingControl.Text = dataGridView.CurrentCell.Value.ToString();
            }
            AddInfo("按下修复按钮", false);
            try
            {
                FixImpl();
            }
            catch(Exception ex)
            {
                AddErrorInfo(ex.Message);
                AddErrorInfo("修复失败！");
                return;
            }
            AddInfo("修复成功！", false);
            Helper.CloseWaiter(strings.waiterName);
            Helper.WriteResult(true);
            Helper.WriteRetry(false);
        }

        private void FixImpl()
        {

            if (GlobalVars.Instance.pos_BarcodeDict.Count == 0)
                throw new Exception("尚未扫描条码！");

            //检查当前列
            var lastPair = GlobalVars.Instance.pos_BarcodeDict.Last();
            int gridID = lastPair.Key.x + 1;
            int col = gridID - GlobalVars.Instance.StartGridID;
            List<string> barcodes = new List<string>();
            for(int row = 0; row< 16; row++)
            {
                var cell = dataGridView.Rows[row].Cells[col];
                barcodes.Add(cell.Value.ToString());
            }
            GlobalVars.Instance.SetBarcodes(gridID, barcodes);
            CheckBarcodes(gridID, barcodes);
            GlobalVars.Instance.CheckDuplicated();
        }


        private void ExecuteCommandImpl(string sCommand)
        {
           
            Helper.WriteRetry(false);
            if (sCommand.Contains("Gen"))
            {
                txtLog.AppendText(string.Format("Generate worklist, total sample count is:{0}!", GlobalVars.Instance.pos_BarcodeDict.Count));
                
                List<string> barcodesTrace = new List<string>();
                List<string> rCommands = new List<string>();
                string warningMsg = "";
                List<string> wklist = worklist.Generate(GlobalVars.Instance.pos_BarcodeDict.Count,
                    ref rCommands, ref barcodesTrace, ref warningMsg);
                GlobalVars.Instance.ResetPosBarcode();
                
                File.WriteAllLines(Folders.GetOutputFolder() + "pooling.gwl", wklist);
                File.WriteAllLines(Folders.GetOutputFolder() + string.Format("tracking_Batch{0}.csv",GlobalVars.Instance.BatchID), barcodesTrace);
                File.WriteAllText(Folders.GetOutputFolder() + "finished.txt", worklist.Finished.ToString());
                Folders.Backup();
                if (warningMsg != "")
                    AddWarning(warningMsg);
            }
            else
            {

                int grid = 0;
                List<string> barcodes = new List<string>();
                List<bool> results = new List<bool>();
                ReadBarcode(ref grid, barcodes);
                
                if(NeedGenerateWorklist(grid))
                {
                    //txtLog.AppendText(string.Format("need generate worklist, try to remove no need ones.original cnt is:{0}",barcodes.Count));
                    RemoveExtraBarcodes(ref barcodes);
                    //txtLog.AppendText(string.Format("after cnt is:{0}",barcodes.Count));
                }
                    
                UpdateDataGridView(grid, barcodes);
                GlobalVars.Instance.SetBarcodes(grid, barcodes);
                CheckBarcodes(grid, barcodes);
                GlobalVars.Instance.CheckDuplicated();
                AddInfo(string.Format("条{0}条码检查通过", grid));
              
            }
        }

        private void RemoveExtraBarcodes(ref List<string> barcodes)
        {
            int i = barcodes.Count -1;
            for(; i>0; i--)
            {
                if (barcodes[i] !="$$$")
                    break;
            }
            if(i != barcodes.Count -1) //some tube missing
            {
                barcodes = barcodes.Take(i + 1).ToList();
            }
        }

        private void UpdateDateGridView()
        {
            dataGridView.AllowUserToAddRows = false;
            dataGridView.EnableHeadersVisualStyles = false;
            dataGridView.Columns.Clear();
            List<string> strs = new List<string>();
        }

        private bool NeedGenerateWorklist(int curGrid)
        {
            return curGrid == GlobalVars.Instance.StartGridID + int.Parse(txtGridCnt.Text) - 1;
        }

        private void CheckBarcodes(int grid, List<string> barcodes)
        {
            if(barcodes.Contains("***") || barcodes.Contains(""))//barcode missing
            {
                throw new Exception(string.Format("条{0}上条码缺失！", grid));
            }

            if (barcodes.Contains("$$$"))//sample missing
            {
                int totalSample = worklist.totalNormalSmpCnt + worklist.totalPoolingSmpCnt;
                while (totalSample > 16)
                    totalSample -= 16;
                int firstMissingIndex = barcodes.IndexOf("$$$");

                if (NeedGenerateWorklist(grid) && firstMissingIndex >= totalSample) //if it is the last grid, and the count is not enough,ok, give warning
                {
                    AddWarning(string.Format("条{0}只有{1}个样品！", grid, barcodes.Count(x => x != "$$$")));
                }
                else
                {
                    throw new Exception(string.Format("条{0}上样品缺失！", grid));
                }
            }
        }

 
        private void ReadBarcode(ref int grid, List<string> barcodes)
        {
            string posIDFile = ConfigurationManager.AppSettings["posIDFile"];
            List<string> contents = File.ReadAllLines(posIDFile).ToList();
            contents = contents.Where(x => x != "").ToList();
            if (contents.Count() != 17)
                throw new Exception("条码文件行数不是17！");
            string firstLine = contents[1];
            string[] strs = firstLine.Split(';');
            grid = int.Parse(strs[0]);
            barcodes.Clear();
            contents.RemoveAt(0);
            foreach (string s in contents)
            {
                barcodes.Add(Parse(s));
            }
            txtLog.AppendText(string.Format("{0} 扫描grid: {1}\r\n", DateTime.Now.ToLongTimeString(), grid));
        }

        private string Parse(string s)
        {
            string[] strs = s.Split(';');
            return strs.Last();
        }
#endregion
        
        private void btnRetry_Click(object sender, RoutedEventArgs e)
        {
            AddInfo("按下重试按钮", false);
            Helper.CloseWaiter(strings.waiterName);
            Helper.WriteRetry(true);
        }

        //private void dataGridView_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        //{
        //    if (e.ColumnIndex == -1)
        //        return;
        //    if (e.ColumnIndex >= GlobalVars.Instance.pos_BarcodeDict.Keys.Count)
        //        return;
        //    int gridIndex = GlobalVars.Instance.pos_BarcodeDict.Keys.ElementAt(e.ColumnIndex).x;
        //    var cell = dataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex];
        //    string newVal = cell.Value.ToString();
        //    cell.ReadOnly = true;
        //    string hint = string.Format("Grid:{0} 第{1}个样品管的条码得到修复。"
        //        , gridIndex + 1, e.RowIndex + 1);
        //    GlobalVars.Instance.pos_BarcodeDict[new Position(gridIndex, e.RowIndex)] = newVal;
        //    AddInfo(hint);
        //}

      

     
    }

    public static class ExtensionMethods
    {

        private static Action EmptyDelegate = delegate() { };
        public static void Refresh(this UIElement uiElement)
        {

            uiElement.Dispatcher.Invoke(DispatcherPriority.Render, EmptyDelegate);

        }

    }
}
