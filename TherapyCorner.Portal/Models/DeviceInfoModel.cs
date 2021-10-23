using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace TherapyCorner.Portal.Models
{
    public class DeviceInfoModel
    {
        public string DeviceCode { get; set; }
        public string DeviceIP { get; set; }

        public string HashedCode
        {
            get
            {
                string ConvertedString = BitConverter.ToString(Encoding.Default.GetBytes(DeviceCode ));
                return ConvertedString;
            }
            set
            {
                string zHex = value.Replace("-", "");
                byte[] ba = new byte[zHex.Length / 2];  //One byte for each two chars in zHex
                for (int ZZ = 0; ZZ < ba.Length; ZZ++)
                {
                    ba[ZZ] = Convert.ToByte(zHex.Substring(ZZ * 2, 2), 16);
                }
                string zBackIn = Encoding.ASCII.GetString(ba);  //The original string
                DeviceCode = zBackIn;
            }
        }
    }
}