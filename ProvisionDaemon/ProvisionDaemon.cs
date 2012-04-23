using System;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.ServiceProcess;
using System.Text;
using NLog;
using ProvisionDaemon.ROOT.CIMV2;

namespace ProvisionDaemon
{
    [DisplayName("Provision Daemon")]
    [ServiceProcessDescription("MetaPaas Provision Daemon")]
    public partial class ProvisionDaemon : ServiceBase
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public ProvisionDaemon()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            logger.Info("Service Start");

            if (!CheckComputerName())
            {
                RenameComputerName();
                Reboot();
            }

            logger.Info("All job done, shutting down...");
            this.Stop();
        }

        private bool CheckRebootInterval()
        {
            var now = DateTime.Now;
            var lastReboot = Properties.Settings.Default.LastReboot;
            var timespan = now - lastReboot;
            var minInterval = Properties.Settings.Default.MinRebootInterval;
            var canReboot = timespan >= minInterval;
            logger.Info("Time: {0} from Last Reboot: {1} is {2}, which is {3} than threadhold {4}", now, lastReboot, timespan, canReboot ? "longer" : "shorter", minInterval);
            
            Properties.Settings.Default.LastReboot = now;
            Properties.Settings.Default.Save();
            logger.Info("Log Reboot Time: {0}", now);
            
            return canReboot;
        }

        private void Reboot()
        {
            logger.Info("Reboot");

            if (!CheckRebootInterval())
            {
                logger.Error("Reboot Canceled, Now is less than threadhold from last reboot");
                return;
            }

            try
            {
                logger.Info("Trigger Reboot");
                var os = WinOperatingSystem.CreateInstance();
                var result = os.Reboot();
                if (result == 0)
                {
                    logger.Info("Reboot triggered");
                }
                else
                {
                    logger.Error("Reboot failed");
                }
            }
            catch (Exception ex)
            {
                logger.ErrorException("OperatingSystem.Reboot threw exception", ex);
            }

        }

        private string desiredComputerName;
        public string DesiredComputerName
        {
            get
            {
                if (desiredComputerName == null)
                {
                    logger.Info("Build DesireComputerName");

                    var hostname = Dns.GetHostName();
                    logger.Debug("Hostname: {0}", hostname);

                    var addresses = Dns.GetHostAddresses(hostname).Where(a => a.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);
                    logger.Debug(() =>
                    {
                        StringBuilder sb = new StringBuilder();
                        sb.Append("Host Addresses:");
                        foreach (var fo in addresses)
                        {
                            sb.Append("\n    ");
                            sb.Append(fo.ToString());
                        }
                        return sb.ToString();
                    });

                    var ipAddress = addresses.First(); // TODO Use a better and reasonable algorithm
                    logger.Info("Use Ip: {0}", ipAddress);

                    desiredComputerName =
                        Properties.Settings.Default.ComputerNamePrefix +
                        ipAddress.ToString().Replace(".", Properties.Settings.Default.ComputerNameSeperator);
                    logger.Debug("Desired Computer Name: {0}", desiredComputerName);
                }

                return desiredComputerName;
            }
        }

        private bool CheckComputerName()
        {
            //var currentName = Environment.MachineName;
            var currentName = Dns.GetHostName(); // use Dns.GetHostName to solve the truncated problem.
            var desireName = DesiredComputerName; // local copy makes log more clear.

            logger.Info("ComputerName: {0}", currentName);
            logger.Debug("DesiredComputerName: {0}", desireName);
            var result = currentName == desireName;
            logger.Info(() => result ? "Computer-Name is valid" : "Computer need to be renamed");

            return result;
        }

        private void RenameComputerName()
        {
            logger.Info("Rename Computer");
            try
            {
                var computer = ComputerSystem.GetInstances().OfType<ComputerSystem>().First();

                var result = computer.Rename(DesiredComputerName, null, null);

                if (result != 0)
                {
                    logger.Error("Failed to rename due to error code {0}", result);
                }
                else
                {
                    logger.Info("Rename Succeeded");
                }
            }
            catch (Exception ex)
            {
                logger.FatalException("ComputerSystem.Rename threw exception.", ex);
            }
        }

        protected override void OnStop()
        {
            logger.Info("Stop Service");
        }
    }
}
