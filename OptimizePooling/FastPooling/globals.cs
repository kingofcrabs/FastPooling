﻿using OptimizePooling;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FastPooling
{
    class GlobalVars : BindableBase
    {
        static GlobalVars instance = null;
        public Dictionary<Position, string> pos_BarcodeDict;

        #region configures
        private double volume = double.Parse(GetSetting("volumeUL"));
        private string dstLabware = GetSetting("dstLabware");
        private int poolingCnt = int.Parse(GetSetting("poolingCnt"));
        private int startGridID = int.Parse(ConfigurationManager.AppSettings["startGrid"]);
        private string ngetiveLabware = ConfigurationManager.AppSettings["negtiveLabware"];
        private int negtiveStartWellID = int.Parse(ConfigurationManager.AppSettings["negtiveStartWell"]);
        #endregion

        private int batchID = 0;
        public static GlobalVars Instance
        {
            get
            {
                if (instance == null)
                    instance = new GlobalVars();
                return instance;
            }
        }

        public string NegtiveLabware
        {
            get
            {
                return NegtiveLabware;
            }
        }

        public int NegtiveStartWellID
        {
            get
            {
                return negtiveStartWellID;
            }
        }

        public string WaiterName
        {
            get
            {
                return "FeedMe";
            }
             
        }

        public GlobalVars()
        {
            pos_BarcodeDict = new Dictionary<Position, string>();
        }

        public string DstLabware
        {
            get
            {
                return dstLabware;
            }
        }

        public int PoolingCnt
        {
            get
            {
                return poolingCnt;
            }
        }

        public int BatchID
        {
            get
            {
                return batchID;
            }
            set
            {
                SetProperty(ref batchID, value);
            }
        }

        public double PipettingVolume
        {
            get
            {
                return volume;
            }
        }

        private static string GetSetting(string key)
        {
            return ConfigurationManager.AppSettings[key];
        }

        public int StartGridID
        {
            get
            {
                return startGridID;
            }
        }

        internal void SetBarcodes(int gridID, List<string> barcodes)
        {
            int gridIndex = gridID - 1;
            for(int i = 0; i< barcodes.Count; i++)
            {
                if (barcodes[i] == "$$$")
                    continue;
                Position pos = new Position(gridIndex, i);
                if (pos_BarcodeDict.ContainsKey(pos))
                    pos_BarcodeDict[pos] = barcodes[i];
                else
                    pos_BarcodeDict.Add(pos, barcodes[i]);
            }
        }

        internal void ResetPosBarcode()
        {
            pos_BarcodeDict.Clear();
        }
    }
}