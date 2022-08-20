using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulsar.BuildingBlocks.EventBus
{
    class Constants
    {
        public const int MAX_POLLED_EVENTS = 2000;
        public const int MAX_LATER_ATTEMPTS = 5;
        public const int IN_PROGRESS_TIMEOUT_IN_HOURS = 48;
        public const int IN_PROGRESS_RESTORE_IN_HOURS = 3;
        public const int MAX_EVENTS_ON_QUEUE = 500000;
    }
}
