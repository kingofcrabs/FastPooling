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
            lstBarcodes.ItemsSource = scanInfo.Barcodes;
        }

        private void btnSetGridCnt_Click(object sender, EventArgs e)
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
            EnableControls(false);
        }

        private void SetErrorInfo(string info)
        {
            txtInfo.Text = info;
            txtInfo.Foreground = new SolidColorBrush(Colors.Red);
        }

        private void EnableControls(bool bEnable)
        {
            txtGridCnt.IsEnabled = bEnable;
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
            if (sCommand != "")
            {
                txtLog.AppendText(sCommand + "\r\n");
            }

            Helper.WriteRetry(false);
            int grid = 0;
            List<string> barcodes = new List<string>();
            List<bool> results = new List<bool>();
            ReadBarcode(ref grid, barcodes);
            txtLog.AppendText(string.Format("读取Grid：{0}的条码。\r\n",grid));
            bool bok = true;
            try
            {
                CheckBarcodes(grid, barcodes);
                scanInfo.GridID = grid;
                scanInfo.Barcodes.Clear();
                barcodes.ForEach(x => scanInfo.Barcodes.Add(x));
            }
            catch (Exception ex)
            {
                SetErrorInfo(ex.Message);
                bok = false;
            }
            Helper.WriteResult(bok);
          
        }

        private void CheckBarcodes(int grid, List<string> barcodes)
        {
            if(barcodes.Contains("***"))//barcode missing
            {
                throw new Exception(string.Format("Grid{0}上条码缺失！", grid));
            }
        }
       
        private void ReadBarcode(ref int grid, List<string> barcodes)
        {
            string posIDFile =  ConfigurationManager.AppSettings["posIDFile"];
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
            txtLog.AppendText(string.Format("{0} Scan grid: {1}\r\n", DateTime.Now.ToLongTimeString(), grid));
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
            Helper.CloseWaiter(strings.NotifierName);
            Helper.WriteRetry(true);
        }
    }
}
