using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Xml;

namespace DinamapN
{
    class Tool
    {
        //Declaration of Dinawin DLL functions.
        [DllImport("DinaWin.dll")] 
        internal static extern bool checkReadiness();

        [DllImport("DinaWin.dll")]
        internal static extern string getState();

        [DllImport("DinaWin.dll")]
        internal static extern int getBufferLength();

        [DllImport("DinaWin.dll")]
        internal static extern bool resetMonitor();

        //Public Variables to the Class
        public static string szTitulo_APP = "(C)Dinamap - Monitoring Program";

        //Member of the Class
        public static string GetConfigurationRegistry(string key)
        {
            string strValue = "";
            try
            {
                strValue = Application.UserAppDataRegistry.GetValue(key).ToString();
            }
            catch
            {
                ;
            }
            return strValue;
        }

        public static bool SetConfigurationRegistry(string key, string strValue)
        {
            try
            {
                Application.UserAppDataRegistry.SetValue(key, strValue);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool Dina_CheckReadiness()
        {
            bool bRet = false;

            try
            {
                bRet = checkReadiness();
            }
            catch (System.DllNotFoundException ex)
            {
                MessageBox.Show("DinaWin.dll could not load.");
            }

            //return true;
            return bRet;
        }

        public static XmlDocument Dina_GetStateOn()
        {
            try
            {
                XmlDocument xmlData = new XmlDocument();
                xmlData.LoadXml(getState());
                return xmlData;
            }
            catch (System.DllNotFoundException ex)
            {
                return new XmlDocument();
            }

        }

        public static XmlDocument Dina_GetStateOff()
        {
            try
            {
                XmlDocument xmlData = new XmlDocument();
                xmlData.Load("C:\\2.xml");
                return xmlData;
            }
            catch (System.DllNotFoundException ex)
            {
                return new XmlDocument();
            }

        }

        public static int Dina_GetBufferLength()
        {
            int nLon;

            nLon = getBufferLength();

            return nLon;
        }

        public static bool Dina_ResetMonitor()
        {
            bool bRet = false;
           
            try
            {
                bRet = resetMonitor();
            }
            catch (System.DllNotFoundException ex)
            {
                MessageBox.Show("DinaWin.dll could not load.");
            }

            return bRet;
        }


    } //End of Class
}  //End of Namespace
