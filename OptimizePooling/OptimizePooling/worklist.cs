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
        int curPlateColumnIndex = 0;
        public List<string> Generate(int sampleCount)
        {
            int sampleCntPerBatch = 8 * GlobalVars.Instance.PoolingCnt;
            int batchCnt = sampleCount / sampleCntPerBatch;
            List<PipettingInfo> pipettingInfos = new List<PipettingInfo>();
            int startGridID = int.Parse(ConfigurationManager.AppSettings["startGrid"]);
            for (int i = 0; i < batchCnt; i++)
            {
                pipettingInfos.AddRange(GenerateBatch(startGridID));
                curPlateColumnIndex++;
                startGridID += 3;
            }
            List<string> strs = Format(pipettingInfos);
            strs.Add("B");
            int remainingCnt = sampleCount - batchCnt * sampleCntPerBatch;
            int dstWellCntNeeded = remainingCnt / GlobalVars.Instance.PoolingCnt;


            return strs;
        }

        private List<string> Format(List<PipettingInfo> pipettingInfos)
        {
            throw new NotImplementedException();
        }

        private IEnumerable<PipettingInfo> GenerateBatch(int startGridID)
        {
            List<PipettingInfo> pipettingInfos = new List<PipettingInfo>();
           
            for(int iGrid = 0; iGrid < 3; iGrid++)
            {
                int srcGrid = startGridID + iGrid;
                for( int wellIndex = 0; wellIndex < 16 ; wellIndex++)
                {
                    int dstWellIndex = wellIndex;
                    if (dstWellIndex >= 8)
                        dstWellIndex -= 8;
                    pipettingInfos.Add(new PipettingInfo(string.Format("grid{0}", srcGrid),
                        wellIndex + 1,
                        GlobalVars.Instance.PipettingVolume,
                        GlobalVars.Instance.DstLabware,
                        curPlateColumnIndex * 8 + dstWellIndex + 1, GlobalVars.Instance.pos_BarcodeDict[new Position(srcGrid - 1, wellIndex)]));
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
    class GlobalVars
    {
        static GlobalVars instance = null;
        public Dictionary<Position, string> pos_BarcodeDict;
        public static GlobalVars Instance
        {
            get
            {
                if (instance == null)
                    instance = new GlobalVars();
                return instance;
            }
        }

        public  string DstLabware
        {
            get{
                return GetSetting("dstLabware");
            }
        }

        public int PoolingCnt
        {
            get
            {
                return int.Parse(GetSetting("poolingCnt"));
            }
        }



        public double PipettingVolume
        {
            get
            {
                return double.Parse(GetSetting("volumeUL"));
            }
        }


        private string GetSetting(string key)
        {
            return ConfigurationManager.AppSettings[key];
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
