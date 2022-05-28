using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lottery_System.Common
{
    public class ConfigTool
    {
        public static string GetConnectionString(string connName)
        {
            return
                System.Configuration.ConfigurationManager.
                    ConnectionStrings[connName].ConnectionString.ToString();
        }
    }
}
