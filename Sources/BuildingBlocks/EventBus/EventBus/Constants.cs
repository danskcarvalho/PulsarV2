using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulsar.BuildingBlocks.EventBus
{
    class Constants
    {
        public const int MaxPolledEvents = 2000;
        public const int MaxLaterAttempts = 5;
        public const int InProgressTimeoutInHours = 48;
        public const int InProgressRestoreInHours = 3;
        public const int MaxEventsOnQueue = 500000;
    }
}
