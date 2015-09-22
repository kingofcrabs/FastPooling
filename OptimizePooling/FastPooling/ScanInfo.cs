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
        ObservableCollection<SampleInfo> samplesInfo = new ObservableCollection<SampleInfo>();
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

        public ObservableCollection<SampleInfo> SamplesInfo
        {
            get
            {
                return samplesInfo;
            }
            set
            {
                SetProperty(ref samplesInfo, value);
            }
        }
    }
}
