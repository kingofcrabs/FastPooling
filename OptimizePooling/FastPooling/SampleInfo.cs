using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FastPooling
{
    class SampleInfo:BindableBase
    {
        private int rowNumber;
        private string barcode;
  
        public SampleInfo(int rowIndex, string barcode)
        {
            // TODO: Complete member initialization
            RowNumber = rowIndex + 1;
            Barcode = barcode;
        }

        public int RowNumber
        {
            get
            {
                return rowNumber;
            }
            set
            {
                SetProperty(ref rowNumber, value);
            }
        }
        public string Barcode
        {
            get
            {
                return barcode;
            }
            set
            {
                SetProperty(ref barcode, value);
            }
        }
    }
}
