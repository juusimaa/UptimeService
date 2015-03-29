using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace UptimeService
{
    public static class Uptime
    {
        public static TimeSpan GetUptime()
        {
            using (var uptime = new PerformanceCounter("System", "System Up Time"))
            {
                uptime.NextValue();       //Call this an extra time before reading its value
                return TimeSpan.FromSeconds(uptime.NextValue());
            }
        }

        public static TimeSpan GetUptime2()
        {
            return TimeSpan.FromMilliseconds(GetTickCount64());
        }

        [DllImport("kernel32")]
        extern static UInt64 GetTickCount64();

        public static string GetFormattedUptime(this TimeSpan t)
        {
            return String.Format("{0} days, {1} hours, {2} minutes, {3} seconds",
                t.Days, t.Hours, t.Minutes, t.Seconds);
        }
    }
}
