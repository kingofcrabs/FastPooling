using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OptimizePooling;
using FastPooling;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace UnitTests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void PoolingSample16()
        {
            TestPoolingSample(16);
        }

        [TestMethod]
        public void PoolingSample24()
        {
            TestPoolingSample(24);
        }

        [TestMethod]
        public void PoolingSample15()
        {
            TestPoolingSample(15);
        }

        [TestMethod]
        public void PoolingSample50()
        {
            TestPoolingSample(50);
        }

        [TestMethod]
        public void PoolingSample300()
        {
            TestPoolingSample(300);
        }

        [TestMethod]
        public void PoolingSample26()
        {
            TestPoolingSample(26);
        }

        [TestMethod]
        public void Pooling51Normal16()
        {
            TestGeneric(51, 16);
        }

        [TestMethod]
        public void Normal13()
        {
            TestGeneric(0, 13);
        }


        [TestMethod]
        public void Normal16()
        {
            TestGeneric(0,16);
        }

        private void TestGeneric(int poolingCnt, int normalCnt)
        {
            GlobalVars.Instance.ResetPosBarcode();
            int totalCnt = poolingCnt + normalCnt;
            SetBarcodes(totalCnt);
            worklist worklist = new worklist();
            worklist.SetConfig(poolingCnt, normalCnt,false);
            List<string> barcodeTrace = new List<string>();
            List<string> rCommands = new List<string>();
            string warnMsg = "";
            var strs = worklist.Generate(totalCnt,ref rCommands, ref barcodeTrace, ref warnMsg);
            string poolingOrNormal = normalCnt == 0 ? "Pooling" : "Normal";
            string sGwl = GetTestResultFolder() + string.Format("result{0}{1}.gwl", totalCnt, poolingOrNormal);
            string sBarcodeTrace = GetTestResultFolder() + string.Format("barcodeTrace{0}{1}.txt", totalCnt, poolingOrNormal);
            string sRCommandGwl = GetTestResultFolder() + string.Format("reagent{0}{1}.gwl", totalCnt, poolingOrNormal);
          
            File.WriteAllLines(sRCommandGwl, worklist.GenerateRCommand());
            File.WriteAllLines(sGwl, strs);
            File.WriteAllLines(sBarcodeTrace, barcodeTrace);
            rCommands = worklist.GenerateRCommand();
            var expectedRCommands = File.ReadAllLines(sRCommandGwl);
            var expectedGwl = File.ReadAllLines(sGwl);
            var expectedBarcodeTrace = File.ReadAllLines(sBarcodeTrace);
            //CheckEqual(rCommands.ToArray(), expectedRCommands);
            //CheckEqual(expectedGwl, strs.ToArray());
            //CheckEqual(expectedBarcodeTrace, barcodeTrace.ToArray());

        }

        private void CheckEqual(string[] strs, string[] expectedStrs)
        {

            Assert.AreEqual(strs.Length, expectedStrs.Length);
            for(int i = 0; i< strs.Length; i++)
            {
                Assert.AreEqual(strs[i], expectedStrs[i]);
            }

        }

        private void TestPoolingSample(int cnt)
        {
            TestGeneric(cnt, 0);
        }

        static public string GetTestResultFolder()
        {
            string s = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            int index = s.LastIndexOf("\\");
            s = s.Substring(0, index);
            index = s.LastIndexOf("\\");
            return s.Substring(0, index) + "\\testResults\\";
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
