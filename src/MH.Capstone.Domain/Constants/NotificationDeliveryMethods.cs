using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MH.Capstone.Domain.Constants
{
    [Flags]
    public enum NotificationDeliveryMethods
    {
        None = 1 << 0,
        InApp = 1 << 1,
        Email = 1 << 2,
    }
}
