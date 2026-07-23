using System;
using System.Collections.Generic;
using System.Text;

namespace palo_ip_updater.Models
{
    internal class PaloConfigurationSettings
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string HostIp { get; set; }
        public string HostIpName { get; set; }
        public int UpdateInterval { get; set; }

        public PaloConfigurationSettings()
        {
            Username = string.Empty;
            Password = string.Empty;
            HostIp = string.Empty;
            HostIpName = string.Empty;
            UpdateInterval = 120;
        }
    }
}
