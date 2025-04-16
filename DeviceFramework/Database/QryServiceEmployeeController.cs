using System.Collections.Generic;
using System.Linq;
using TIM.Devices.Framework.Common;

namespace TIM.Devices.Framework.Database
{
    internal static class QryServiceEmployeeController
    {
        public static QryServiceEmployee GetByADLoginName(string adLoginName)
        {
            using (var ctx = DBContext.DataContextRead)
            {
                return ctx
                    .QryServiceEmployees
                    .Where(
                        e =>
                        e.ActivatedFlag &&
                        e.StatusID.HasValue &&
                        e.ADLoginName.Equals(adLoginName))
                    .ToList()
                    .FirstOrDefault(e => e.StatusID.IsEmployed() && !string.IsNullOrWhiteSpace(e.ADLoginName));
            }
        }
    }
}