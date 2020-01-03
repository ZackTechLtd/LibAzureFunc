using System;
namespace LibAzureFunc.Util
{
    public static class Helper
    {
        public static int GetIntValue(string strvalue)
        {
            if (int.TryParse(strvalue, out int intvalue))
            {
                return intvalue;
            }
            else
            {
                return 0;
            }
        }

        public static bool GetBoolValue(string strvalue)
        {
            if (bool.TryParse(strvalue, out bool boolvalue))
            {
                return boolvalue;
            }
            else
            {
                return false;
            }
        }
    }
}
