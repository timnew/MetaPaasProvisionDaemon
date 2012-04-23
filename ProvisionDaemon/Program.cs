using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;

namespace ProvisionDaemon
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static int Main(string[] args)
        {
            if (args.Length > 0)
            {
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
                catch (Exception)
                {
                    return -1;
                }
            }

            return LaunchService();
        }

        private static int LaunchService()
        {
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
            var stateSaver = new System.Collections.Hashtable();

            var installer = new ProvisionDaemonInstaller();

            try
            {
                installer.Uninstall(stateSaver);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);

                installer.Rollback(stateSaver);

                return -1;
            }

            installer.Commit(stateSaver);
            return 0;
        }

        private static int Install()
        {
            var stateSaver = new System.Collections.Hashtable();

            var installer = new ProvisionDaemonInstaller();

            try
            {
                installer.Install(stateSaver);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                installer.Rollback(stateSaver);
                return -1;
            }

            installer.Commit(stateSaver);
            return 0;
        }
    }
}
