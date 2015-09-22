using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

namespace FastPooling
{
    class ScanInfo : BindableBase
    {
        ObservableCollection<string> barcodes = new ObservableCollection<string>();
        int gridID = GlobalVars.Instance.StartGridID;

        public int GridID
        {
            get
            {
                return gridID;
            }
            set
            {
                SetProperty(ref gridID, value);
            }
        }

        public ObservableCollection<string> Barcodes
        {
            get
            {
                return barcodes;
            }
            set
            {
                SetProperty(ref barcodes, value);
            }
        }
    }
}
