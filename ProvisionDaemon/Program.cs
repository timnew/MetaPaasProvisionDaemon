using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using NLog;

namespace ProvisionDaemon
{
    static class Program
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static int Main(string[] args)
        {
            logger.Info("Launch from CLI");

            if (args.Length > 0)
            {
                logger.Info("Arguments: {0}", args[0]);
                try
                {
                    switch (args[0].ToLowerInvariant())
                    {
                        case "install":
                            return Install();
                        case "uninstall":
                            return Uninstall();
                        case "run":
                        case "launch":
                            return LaunchService();
                    }

                }
                catch (Exception ex)
                {
                    logger.FatalException("Launch Error.", ex);
                    return -1;
                }
            }

            return LaunchService();
        }

        private static int LaunchService()
        {
            logger.Info("Launch Service From CLI");

            try
            {
                var ServicesToRun = new ServiceBase[] 
			    { 
				    new ProvisionDaemon() 
			    };

                ServiceBase.Run(ServicesToRun);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return -1;
            }
            return 0;
        }

        private static int Uninstall()
        {
            logger.Info("Uninstalling...");

            var stateSaver = new System.Collections.Hashtable();
            var installer = new ProvisionDaemonInstaller();

            try
            {
                installer.Uninstall(stateSaver);
            }
            catch (Exception ex)
            {
                logger.FatalException("Uninstall Failed", ex);
                logger.Info("Rolling Back...");
                installer.Rollback(stateSaver);

                return -1;
            }

            logger.Info("Commiting...");
            installer.Commit(stateSaver);
            return 0;
        }

        private static int Install()
        {
            logger.Info("Installing...");
            
            var stateSaver = new System.Collections.Hashtable();
            var installer = new ProvisionDaemonInstaller();

            try
            {
                installer.Install(stateSaver);
            }
            catch (Exception ex)
            {
                logger.FatalException("Install Failed", ex);
                logger.Info("Rolling Back...");
                installer.Rollback(stateSaver);
                return -1;
            }

            logger.Info("Commiting...");
            installer.Commit(stateSaver);
            return 0;
        }
    }
}
