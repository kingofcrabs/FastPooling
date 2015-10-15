using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SimulatePosID
{
    class Program
    {
        static void Main(string[] args)
        {
            string gridNum = args[0];
            //6;1;16;Tube Eppendorf 16 Pos;Labware4;968/002718;15P0000016-A
            List<string> strs = new List<string>() {"FC70413C" };
            if (gridNum != "8" || args.Length != 1)
            {
                for (int i = 0; i < 16; i++)
                {
                    string sBarcode = string.Format("{0}_{1:D3}", gridNum, i + 1);
                    string sLine = string.Format("{0};1;{1};Tube Eppendorf 16 Pos;Labware4;968/002718;{2}", gridNum, i + 1, sBarcode);
                    strs.Add(sLine);
                }
            }
            else
            {
                for (int i = 0; i < 13; i++)
                {
                    string sBarcode = string.Format("{0}_{1:D3}", gridNum, i + 1);
                    string sLine = string.Format("{0};1;{1};Tube Eppendorf 16 Pos;Labware4;968/002718;{2}", gridNum, i + 1, sBarcode);
                    strs.Add(sLine);
                }
                for(int i = 0; i< 3; i++)
                {
                    //string sBarcode = string.Format("{0}_{1:D3}", gridNum, i + 1);
                    string sLine = string.Format("{0};1;{1};Tube Eppendorf 16 Pos;Labware4;968/002718;{2}", gridNum, i + 1, "$$$");
                    strs.Add(sLine);
                }
            }
            
            File.WriteAllLines(@"C:\posID\scan.csv",strs);
        }
    }
}
