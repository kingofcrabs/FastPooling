using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OptimizePooling;
using FastPooling;
using System.Collections.Generic;
using System.IO;

namespace UnitTests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void PoolingSample16()
        {
            //set barcodes
            GlobalVars.Instance.ClearBarcodes();

            SetBarcodes(16);
            worklist worklist = new worklist();
            worklist.SetConfig(16, 0);
            List<string> barcodeTrace = new List<string>();
            var strs = worklist.Generate(16, ref barcodeTrace);
            //File.WriteAllLines("F:\\test\\result.gwl",strs);
            //File.WriteAllLines("F:\\test\\barcodeTrace.txt", barcodeTrace);
        }

        [TestMethod]
        public void PoolingSample50()
        {
            //set barcodes
            GlobalVars.Instance.ClearBarcodes();

            SetBarcodes(50);
            worklist worklist = new worklist();
            worklist.SetConfig(50, 0);
            List<string> barcodeTrace = new List<string>();
            var strs = worklist.Generate(50, ref barcodeTrace);
            File.WriteAllLines("F:\\test\\result.gwl", strs);
            File.WriteAllLines("F:\\test\\barcodeTrace.txt", barcodeTrace);
        }

        private void SetBarcodes(int nBarcodesCnt)
        {
            Dictionary<int, List<string>> eachGridBarcodes = new Dictionary<int, List<string>>();
            for(int i = 0; i< nBarcodesCnt; i++)
            {
                int curGrid = i / 16;
                int curIDinGrid = i - curGrid * 16 + 1;
                curGrid += 5;
                string sBarcode = string.Format("{0}_{1:D2}", curGrid, curIDinGrid);
                if (!eachGridBarcodes.ContainsKey(curGrid))
                    eachGridBarcodes.Add(curGrid, new List<string>());
                eachGridBarcodes[curGrid].Add(sBarcode);
                //GlobalVars.Instance.SetBarcodes(curGrid,)
            }
            foreach (var thisGridBarcodes in eachGridBarcodes)
                GlobalVars.Instance.SetBarcodes(thisGridBarcodes.Key, thisGridBarcodes.Value);
        }
    }
}
