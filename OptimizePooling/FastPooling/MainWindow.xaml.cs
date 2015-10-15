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
    public partial class MainWindow : Window
    {
        public MainWindow()
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
            CreateNamedPipeServer();
        }

        private void btnSetGridCnt_Click(object sender, RoutedEventArgs e)
        {
            string sGridCnt = txtGridCnt.Text;
            
            if (sGridCnt == "")
            {
                AddErrorInfo("Grid数不得为空！");
                return;
            }
            int gridCnt = 0;
            bool bOk = int.TryParse(sGridCnt, out gridCnt);
            if (gridCnt < 1 || gridCnt > 10)
            {
                AddErrorInfo("Grid数必须在1~10之间！");
                return;
            }
            int neededDstWell = worklist.CalculateNeededDstWell(gridCnt * 16);
            if(worklist.curDstWellStartIndex + neededDstWell >= 96)
            {
                AddErrorInfo(string.Format("Grid数太多，一共需要{0}个目标孔做Pooling！", neededDstWell));
                return;
            }
            Helper.WriteGridCnt(gridCnt);
            Helper.CloseWaiter(strings.waiterName);
            EnableControls(false);
            InitDataGridView(gridCnt);
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
        }

       

        private void AddErrorInfo(string info)
        {
            AddColorText(info, Brushes.Red);
            richTxtInfo.Refresh();
        }

        private void AddInfo(string info, bool success = true)
        {
            var brush =success ? Brushes.DarkGreen : Brushes.Blue;
            AddColorText(info, brush);
        }

        private void EnableControls(bool bEnable)
        {
            txtGridCnt.IsEnabled = bEnable;
            btnSetGridCnt.IsEnabled = bEnable;
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

            if(sCommand.Contains("NewBatch"))
            {
                GlobalVars.Instance.BatchID++;
                EnableControls(true);
                return;
            }

            Helper.WriteRetry(false);
            int grid = 0;
            List<string> barcodes = new List<string>();
            List<bool> results = new List<bool>();
            ReadBarcode(ref grid, barcodes);
            bool bok = true;
            try
            {
                GlobalVars.Instance.SetBarcodes(grid, barcodes);
                UpdateDataGridView(grid,barcodes);
                CheckBarcodes(grid, barcodes);
                AddInfo(string.Format("Grid{0}条码检查通过", grid));
            }
            catch (Exception ex)
            {
                AddErrorInfo(ex.Message);
                bok = false;
            }
            
            if(NeedGenerateWorklist(grid))
            {
                txtLog.AppendText(string.Format("Generate worklist, total sample count is:{0}!",GlobalVars.Instance.pos_BarcodeDict.Count));
                worklist worklist = new worklist();
                List<string> barcodesTrace = new List<string>();
                List<string> wklist = worklist.Generate(GlobalVars.Instance.pos_BarcodeDict.Count,ref barcodesTrace);
                GlobalVars.Instance.ResetPosBarcode();
                File.WriteAllLines(Folders.GetOutputFolder() + "pooling.csv", wklist);
                File.WriteAllLines(Folders.GetOutputFolder() + "tracking.csv", barcodesTrace);
            }
            Helper.WriteResult(bok);
            if(bok)
                Helper.CloseWaiter(strings.waiterName);
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
            return curGrid == GlobalVars.Instance.StartGridID + int.Parse(txtGridCnt.Text) -1;
        }

        private void CheckBarcodes(int grid, List<string> barcodes)
        {
            if(barcodes.Contains("***"))//barcode missing
            {
                throw new Exception(string.Format("Grid{0}上条码缺失！", grid));
            }

            if(barcodes.Contains("$$$"))//sample missing
            {
                if(NeedGenerateWorklist(grid)) //if it is the last grid, ok, give warning
                {
                    AddWarning(string.Format("Grid{0}只有{1}个样品！", grid, barcodes.Count(x => x != "$$$")));
                }
                else
                {
                    throw new Exception(string.Format("Grid{0}上样品缺失！", grid));
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
