using System;
using System.Text;

namespace CustomPlayBmsUtils
{
    static class BmsUtils
    {
        public static int HexToDec(string hexCode)
        {
            const string hexSheet = "0123456789ABCDEF";
            return hexSheet.IndexOf(hexCode[0]) * 16 + hexSheet.IndexOf(hexCode[1]);
        }
    }
}
