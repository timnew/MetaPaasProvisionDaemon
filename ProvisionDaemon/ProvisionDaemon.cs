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

        private void Reboot()
        {
            logger.Info("Reboot");

            try
            {
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
            logger.Info("ComputerName: {0}", Environment.MachineName);
            logger.Debug("DesiredComputerName: {0}", DesiredComputerName);
            var result = Environment.MachineName == DesiredComputerName;
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
