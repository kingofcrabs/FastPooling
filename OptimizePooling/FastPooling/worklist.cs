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
        public static int totalPoolingSmpCnt = 0;
        public static int totalNormalSmpCnt = 0;
        public static int finishedSmpCnt = 0;
        public static bool bUseTwoPlates = false;


        public List<string> GenerateRCommand()
        {

            /*R;AspirateParameters;DispenseParameters;Volume;LiquidClass;NoOfDitiRe
            uses;NoOfMultiDisp;Direction[;ExcludeDestWell]*
            where:
            AspirateParameters =
            SrcRackLabel;SrcRackID;SrcRackType;SrcPosStart;SrcPosEnd;
            and
            DispenseParameters =
            DestRackLabel;DestRackID;DestRackType;DestPosStart;DestPosEnd;
            R;T2;;Trough 100ml;1;8;MTP96-2;;96 Well Microplate;1;96;100;Water;
            2;5;0*/
            List<string> strs = new List<string>();
            string reagentVolume = ConfigurationManager.AppSettings["reagentVolume"];
            string sBufferTubeCnt = ConfigurationManager.AppSettings["bufferTubeCnt"];
            string aspParameters = string.Format("buffer;;;1;{0}", sBufferTubeCnt);
            int totalDstWellCnt = totalNormalSmpCnt + 2 + CalculateNeededDstWell(totalPoolingSmpCnt);
            string dispParameters = string.Format("{0};;;1;{1}", GlobalVars.Instance.DstLabware, totalDstWellCnt);
            string rCommand = string.Format("R;{0};{1};{2};;1;5;0", aspParameters, dispParameters, reagentVolume);
            strs.Add(rCommand);
            if (bUseTwoPlates)
            {
                dispParameters = string.Format("{0};;;1;{1}", GlobalVars.Instance.DstLabware2, totalDstWellCnt);
                rCommand = string.Format("R;{0};{1};{2};;1;5;0", aspParameters, dispParameters, reagentVolume);
                strs.Add(rCommand);
            }
            return strs;
        }

        public List<string> Generate(int sampleCount, ref List<string> barcodesTrace)
        {
            if( finishedSmpCnt + sampleCount > totalPoolingSmpCnt + totalNormalSmpCnt)
            {
                throw new Exception(string.Format("样品总数达到{0},超过设定值{1}",finishedSmpCnt + sampleCount,totalPoolingSmpCnt + totalNormalSmpCnt));
            }

            int poolingSampleCnt = sampleCount;
            int normalSampleCnt = 0; 
            if( finishedSmpCnt + sampleCount > totalPoolingSmpCnt && finishedSmpCnt < totalPoolingSmpCnt) //has normal sample, but not all normal sample
            {
                poolingSampleCnt = totalPoolingSmpCnt - finishedSmpCnt;
                normalSampleCnt = finishedSmpCnt + sampleCount - totalPoolingSmpCnt;
            }

            if( finishedSmpCnt >= totalPoolingSmpCnt)
            {
                poolingSampleCnt = 0;
                normalSampleCnt = sampleCount;
            }
            finishedSmpCnt += sampleCount;

            List<string> strs = new List<string>();
            //strs.AddRange(GenerateRCommand());
            List<string> poolingBarcodeTrace = new List<string>();
            List<string> normalBarcodeTrace = new List<string>();
            List<string> negBarcodeTrace = new List<string>();
            if (poolingSampleCnt > 0)
            {
                strs.AddRange(GeneratePooling(poolingSampleCnt,ref poolingBarcodeTrace));
                barcodesTrace.AddRange(poolingBarcodeTrace);
            }

            if( normalSampleCnt > 0)
            {
                strs.AddRange(GenerateNormal(poolingSampleCnt,normalSampleCnt,ref normalBarcodeTrace));
                barcodesTrace.AddRange(normalBarcodeTrace);
            }

            strs.AddRange(GenerateNegtive(ref negBarcodeTrace));
            barcodesTrace.AddRange(negBarcodeTrace);
            
            curBatchID++;
            return strs;
        }


        private List<string> GenerateNegtive(ref List<string> negTrace)
        {
            List<PipettingInfo> negPipettings = new List<PipettingInfo>();
            for( int i =0; i< 2; i++)
            {
                string sGrid = "neg";
                int srcWellID = i + 1;
                int dstWellID = curDstWellStartIndex + i + 1;
                negPipettings.Add(new PipettingInfo(
                    sGrid,
                    srcWellID,
                    GlobalVars.Instance.PipettingVolume * GlobalVars.Instance.PoolingCnt / 2,
                    GlobalVars.Instance.DstLabware,
                    MapDstWellID(dstWellID),
                    string.Format("neg{0}", i + 1)));

                negPipettings.Add(new PipettingInfo(
                 sGrid,
                 srcWellID,
                 GlobalVars.Instance.PipettingVolume * GlobalVars.Instance.PoolingCnt / 2,
                 GetSecondSliceDstLabware(),
                 MapDstWellID(dstWellID,false),
                 string.Format("neg{0}", i + 1)));
            }

            curDstWellStartIndex += 2;
            List<string> strs = new List<string>();
            strs.AddRange(Format(negPipettings));
            negTrace = GetBarcodesSourceInfo(negPipettings);
            return strs;
        }
        private List<string> GenerateNormal(int poolingSampleCnt, int normalSampleCnt, ref List<string> normalBarcodeTrace)
        {
            int startGridID = GlobalVars.Instance.StartGridID;// 
            List<PipettingInfo> normalPipettings = new List<PipettingInfo>();
            for (int i = 0; i < normalSampleCnt; i++)
            {
                int wellIndex = i + poolingSampleCnt;
                int srcGridID = startGridID + wellIndex / 16;
                int wellIndexInGrid = wellIndex - (wellIndex / 16) * 16;
                
                string sGrid = string.Format("grid{0}", srcGridID);
                int dstWellID = curDstWellStartIndex + i + 1;
                string barcode = GlobalVars.Instance.pos_BarcodeDict[new Position(srcGridID - 1, wellIndexInGrid)];
                double volume = Math.Round(GlobalVars.Instance.PipettingVolume / 2, 1);
                normalPipettings.Add(new PipettingInfo(
                    sGrid,
                    wellIndexInGrid + 1,
                    volume,
                    GlobalVars.Instance.DstLabware,
                    MapDstWellID(dstWellID),
                    barcode));
                normalPipettings.Add(new PipettingInfo(
                   sGrid,
                   wellIndexInGrid + 1,
                   volume,
                   GetSecondSliceDstLabware(),
                   MapDstWellID(dstWellID,false),
                   barcode));
            }
            curDstWellStartIndex += normalSampleCnt;
            List<string> strs = new List<string>();
            strs.AddRange(Format(normalPipettings));
            normalBarcodeTrace = GetBarcodesSourceInfo(normalPipettings);
            return strs;
        }


        public List<string> GeneratePooling(int sampleCount,ref List<string> barcodesTrace)
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
                string sGrid = string.Format("grid{0}", srcGridID);
                int dstWellID = curDstWellStartIndex + dstWellIndex + 1;
                string barcode = GlobalVars.Instance.pos_BarcodeDict[new Position(srcGridID - 1, wellIndexInGrid)];
                double volume = Math.Round(GlobalVars.Instance.PipettingVolume/2,1);
                fragmentsPipettingInfo.Add(new PipettingInfo(
                    sGrid,
                    wellIndexInGrid + 1,
                    volume,
                    GlobalVars.Instance.DstLabware,
                    MapDstWellID(dstWellID),
                    barcode ));
                fragmentsPipettingInfo.Add(new PipettingInfo(
                   sGrid,
                   wellIndexInGrid + 1,
                   volume,
                   GetSecondSliceDstLabware(),
                   MapDstWellID(dstWellID,false),
                   barcode));
            }

            curDstWellStartIndex += dstWellCntNeeded;
            strs.AddRange(Format(fragmentsPipettingInfo));
            batchPipettingInfos.AddRange(fragmentsPipettingInfo);
            barcodesTrace = GetBarcodesSourceInfo(batchPipettingInfos);
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
            if(bUseTwoPlates)
            {
                strs.Add(GlobalVars.Instance.DstLabware + ": ");
                foreach (int dstWellID in dstWellIDs)
                {
                    var sameDstPipettings = allPipettingInfo.Where(x => x.dstWellID == dstWellID && x.dstLabware == GlobalVars.Instance.DstLabware).ToList();
                    strs.Add( GetWellSourceBarcodes(dstWellID, sameDstPipettings));
                   
                }

                strs.Add(GlobalVars.Instance.DstLabware2 + ": ");
                foreach (int dstWellID in dstWellIDs)
                {
                    var sameDstPipettings = allPipettingInfo.Where(x => x.dstWellID == dstWellID && x.dstLabware == GlobalVars.Instance.DstLabware2).ToList();
                    strs.Add(GetWellSourceBarcodes(dstWellID, sameDstPipettings));
                }
            }
            else
            {
                foreach (int dstWellID in dstWellIDs)
                {
                    var sameDstPipettings = allPipettingInfo.Where(x => x.dstWellID == dstWellID).ToList();
                    strs.Add(GetWellSourceBarcodes(dstWellID, sameDstPipettings));
                }
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
            //pipettingInfos.ForEach(x => strs.AddRange(GenerateAspAndDisp(x)));
            List<PipettingInfo> tempPipettingInfos = new List<PipettingInfo>(pipettingInfos);
            while(tempPipettingInfos.Count > 0)
            {
                var first = tempPipettingInfos.First();
                var sameSrcWellPipettings = tempPipettingInfos.Where(x => x.srcLabware == first.srcLabware && x.srcWellID == first.srcWellID).ToList();
                strs.Add(GetAspirate(first.srcLabware, first.srcWellID, first.volume * sameSrcWellPipettings.Count));
                foreach (var pipetting in sameSrcWellPipettings)
                {
                    strs.Add(GetDispense(pipetting.dstLabware, pipetting.dstWellID, pipetting.volume));
                    tempPipettingInfos.Remove(pipetting);
                }
                strs.Add("W;");
            }
            return strs;
        }

        private List<string> GenerateAspAndDisp(PipettingInfo x)
        {
            string asp = GetAspirate(x.srcLabware, x.srcWellID, x.volume);
            string disp = GetDispense(x.dstLabware, x.dstWellID, x.volume);
            return new List<string>() { asp, disp,"W;" };
        }


        private string GetAspirate(string sLabware, int srcWellID, double vol)
        {
            string sAspirate = string.Format("A;{0};;;{1};;{2};;;",
                         sLabware,
                         srcWellID,
                         vol);
            return sAspirate;
        }

        private string GetDispense(string sLabware, int dstWellID, double vol)
        {
            string sDispense = string.Format("D;{0};;;{1};;{2};;;",
              sLabware,
              dstWellID,
              vol);
            return sDispense;
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
                    string sGrid = string.Format("grid{0}", srcGridID);
                    double volume = Math.Round(GlobalVars.Instance.PipettingVolume/2,1);
                    int dstWellID = curDstWellStartIndex + dstWellIndex + 1;
                    string barcode = GlobalVars.Instance.pos_BarcodeDict[new Position(srcGridID - 1, wellIndex)];
                    pipettingInfos.Add(new PipettingInfo(sGrid,
                        wellIndex + 1,
                        volume,
                        GlobalVars.Instance.DstLabware,
                        MapDstWellID(dstWellID), barcode));

                    pipettingInfos.Add(new PipettingInfo(sGrid,
                       wellIndex + 1,
                       volume,
                       GetSecondSliceDstLabware(),
                       MapDstWellID(dstWellID, false), barcode));
                }
            }
            return pipettingInfos;
        }
        
        private int MapDstWellID(int wellID, bool firstSlice = true)
        {
            //return bUseTwoPlates ? wellIndex + 8 : wellIndex;
            if(bUseTwoPlates)
                return wellID;
            int columnIndex = (wellID-1) / 8;
            int wellIDInColumn = wellID - columnIndex * 8;
            int mappedWellID = columnIndex * 16 + wellIDInColumn;
            if (!firstSlice)
                mappedWellID += 8;
            return mappedWellID;
            
        }

        private string GetSecondSliceDstLabware()
        {
            return bUseTwoPlates ? GlobalVars.Instance.DstLabware2 : GlobalVars.Instance.DstLabware;
        }
      

        public static int SetConfig(int nPoolingSmpCnt, int nNormalSmpCnt)
        {
            totalPoolingSmpCnt = nPoolingSmpCnt;
            totalNormalSmpCnt = nNormalSmpCnt;
            finishedSmpCnt = 0;
            int neededDstWellCnt = CalculateNeededDstWell(totalPoolingSmpCnt) + totalNormalSmpCnt + 2;
            bUseTwoPlates = neededDstWellCnt > 48;
            return neededDstWellCnt;
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
