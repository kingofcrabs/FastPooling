using OptimizePooling;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FastPooling
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ScanInfo scanInfo = new ScanInfo();
        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += MainWindow_Loaded;
            this.Closing += MainWindow_Closing;
            
        }

        void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Pipeserver.Close();
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            CreateNamedPipeServer();
            txtRound.DataContext = GlobalVars.Instance;
            txtCurGridNumber.DataContext = scanInfo;
            lstBarcodes.ItemsSource = scanInfo.SamplesInfo;
        }

        private void btnSetGridCnt_Click(object sender, RoutedEventArgs e)
        {
            string sGridCnt = txtGridCnt.Text;
            if (sGridCnt == "")
            {
                SetErrorInfo("Grid数不得为空！");
                return;
            }
            int gridCnt = 0;
            bool bOk = int.TryParse(sGridCnt, out gridCnt);
            if (gridCnt < 1 || gridCnt > 10)
            {
                SetErrorInfo("Grid数必须在1~10之间！");
                return;
            }
            Helper.WriteGridCnt(gridCnt);
            Helper.CloseWaiter("Feed");
            EnableControls(false);
        }

        private void SetErrorInfo(string info)
        {
            txtInfo.Text = info;
            txtInfo.Foreground = new SolidColorBrush(Colors.Red);
            txtInfo.Background = new SolidColorBrush(Colors.White);
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
                SetErrorInfo(ex.Message);
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
                scanInfo.GridID = grid;
                scanInfo.SamplesInfo.Clear();
                for (int i = 0; i < barcodes.Count;i++ )
                {
                    scanInfo.SamplesInfo.Add(new SampleInfo(i, barcodes[i]));
                }
                CheckBarcodes(grid, barcodes);
                GlobalVars.Instance.SetBarcodes(grid, barcodes);
            }
            catch (Exception ex)
            {
                SetErrorInfo(ex.Message);
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
                Helper.CloseWaiter("Feed");
          
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
                    ShowWarning(string.Format("Grid{0}只有{1}个样品！", grid, barcodes.Count(x => x != "$$$")));
                }
                else
                {
                    throw new Exception(string.Format("Grid{0}上样品缺失！", grid));
                }
            }
        }

        private void ShowWarning(string warning)
        {
            txtInfo.Text = warning;
            txtInfo.Foreground = Brushes.Orange;
            txtInfo.Background = Brushes.White;
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
            txtLog.AppendText("retry pressed!\r\n");
            Helper.CloseWaiter(strings.WaiterName);
            Helper.WriteRetry(true);
        }

      

       
    }

    
}
