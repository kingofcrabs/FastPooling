using FastPooling;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OptimizePooling
{
    class worklist
    {
        public static int curDstWellStartIndex = 0;
        public static int curBatchID = 1;
        public List<string> Generate(int sampleCount,ref List<string> barcodesTrace)
        {

            //first process batchs,48 per batch for 6 pooling into 1, 64 for 8 pooling into 1,etc.
            int sampleCntPerBatch = 8 * GlobalVars.Instance.PoolingCnt;
            int batchCnt = sampleCount / sampleCntPerBatch;
            List<PipettingInfo> batchPipettingInfos = new List<PipettingInfo>();
            int startGridID = GlobalVars.Instance.StartGridID;// 
            for (int i = 0; i < batchCnt; i++)
            {
                batchPipettingInfos.AddRange(GenerateBatch(startGridID));
                curDstWellStartIndex += 8;
                startGridID += 3;
            }
            List<string> strs = Format(batchPipettingInfos);
           
            //process remaining
            List<PipettingInfo> fragmentsPipettingInfo = new List<PipettingInfo>();
            int remainingCnt = sampleCount - batchCnt * sampleCntPerBatch;
            int dstWellCntNeeded = CalculateNeededDstWell(remainingCnt);
            int additionalWellCnt = dstWellCntNeeded * GlobalVars.Instance.PoolingCnt - remainingCnt;
            for (int wellIndex = 0; wellIndex < remainingCnt; wellIndex++ )
            {
                int srcGridID = startGridID + wellIndex / 16;
                int wellIndexInGrid = wellIndex - (wellIndex / 16) * 16;
                int dstWellIndex = wellIndex - wellIndex / dstWellCntNeeded * dstWellCntNeeded;
                
                fragmentsPipettingInfo.Add(new PipettingInfo(
                    string.Format("grid{0}", srcGridID),
                    wellIndexInGrid + 1,
                    GlobalVars.Instance.PipettingVolume,
                    GlobalVars.Instance.DstLabware,
                    curDstWellStartIndex + dstWellIndex + 1,
                     GlobalVars.Instance.pos_BarcodeDict[new Position(srcGridID - 1, wellIndexInGrid)]));

                if(wellIndex == remainingCnt -1) //add additional wells
                {
                    for(int i = 0; i< additionalWellCnt; i++)
                    {
                        int addtionalDstWellIndex = wellIndex + i;
                        addtionalDstWellIndex = addtionalDstWellIndex - addtionalDstWellIndex / dstWellCntNeeded * dstWellCntNeeded;
                        fragmentsPipettingInfo.Add(new PipettingInfo(
                            GlobalVars.Instance.NegtiveLabware,
                            GlobalVars.Instance.NegtiveStartWellID + i,
                            GlobalVars.Instance.PipettingVolume,
                            GlobalVars.Instance.DstLabware,
                            curDstWellStartIndex + addtionalDstWellIndex + 1,
                            "negtive"));
                    }
                }
            }

            curDstWellStartIndex += dstWellCntNeeded;
            strs.AddRange(Format(fragmentsPipettingInfo));
            batchPipettingInfos.AddRange(fragmentsPipettingInfo);
            barcodesTrace = GetBarcodesSourceInfo(batchPipettingInfos);
            curBatchID++;
            return strs;
        }

        static public int CalculateNeededDstWell(int cnt)
        {
            return (cnt + GlobalVars.Instance.PoolingCnt -1)/ GlobalVars.Instance.PoolingCnt;
        }
        //get source info for all the pipetting infos.
        private List<string> GetBarcodesSourceInfo(List<PipettingInfo> allPipettingInfo)
        {
            List<string> strs = new List<string>();
            List<int> dstWellIDs = allPipettingInfo.Select(x=>x.dstWellID).Distinct().ToList();
            foreach(int dstWellID in dstWellIDs)
            {
                var sameDstPipettings = allPipettingInfo.Where(x=>x.dstWellID == dstWellID).ToList();
                strs.Add(GetWellSourceBarcodes(dstWellID,sameDstPipettings));
            }
            return strs;
            
        }

        private string GetWellSourceBarcodes(int dstWellID,List<PipettingInfo> sameDstPipettings)
        {
 	        string s = dstWellID.ToString();
            foreach(var pipettingInfo in sameDstPipettings)
            {
                s += "," + pipettingInfo.barcode;
            }
            return s;
        }

        private List<string> Format(List<PipettingInfo> pipettingInfos)
        {
            List<string> strs = new List<string>();
            pipettingInfos.ForEach(x => strs.Add(Format(x)));
            return strs;
        }

        private string Format(PipettingInfo x)
        {
            return string.Format("{0},{1},{2},{3},{4},{5}",
                x.srcLabware,
                x.srcWellID,
                x.volume,
                x.dstLabware,
                x.dstWellID,
                x.barcode);
        }

        private IEnumerable<PipettingInfo> GenerateBatch(int startGridID)
        {
            List<PipettingInfo> pipettingInfos = new List<PipettingInfo>();
           
            for(int iGrid = 0; iGrid < 3; iGrid++)
            {
                int srcGridID = startGridID + iGrid;
                for( int wellIndex = 0; wellIndex < 16 ; wellIndex++)
                {
                    int dstWellIndex = wellIndex;
                    if (dstWellIndex >= 8)
                        dstWellIndex -= 8;
                    pipettingInfos.Add(new PipettingInfo(string.Format("grid{0}", srcGridID),
                        wellIndex + 1,
                        GlobalVars.Instance.PipettingVolume,
                        GlobalVars.Instance.DstLabware,
                        curDstWellStartIndex + dstWellIndex + 1, GlobalVars.Instance.pos_BarcodeDict[new Position(srcGridID - 1, wellIndex)]));
                }
            }
            return pipettingInfos;
        }


      
    }
    struct Position
    {
        public int x;
        public int y;
        public Position(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }
   

    class PipettingInfo
    {
        public string srcLabware;
        public int srcWellID;
        public double volume;
        public string dstLabware;
        public int dstWellID;
        public string barcode;

        public PipettingInfo(string srcLabware, 
            int srcWellID, 
            double volume, 
            string dstLabware,
            int dstWellID,
            string barcode)
        {
            this.srcLabware = srcLabware;
            this.srcWellID = srcWellID;
            this.volume = volume;
            this.dstLabware = dstLabware;
            this.dstWellID = dstWellID;
            this.barcode = barcode;
        }
    }
}
