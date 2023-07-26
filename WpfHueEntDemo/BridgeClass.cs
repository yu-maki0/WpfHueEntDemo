using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfHueEntDemo
{
    public class BridgeClass
    {
        public string IP;
        public string APP_KEY;
        public string EN_KEY;
        public string NAME;
        public BridgeClass(string argip, string argappKey, string argEnkey, string argname)
        {
            IP = argip;
            APP_KEY = argappKey;
            EN_KEY = argEnkey; 
            NAME = argname;
        }
    }
}
