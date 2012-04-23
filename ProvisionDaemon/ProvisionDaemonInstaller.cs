using System.ComponentModel;
using System.Configuration.Install;
using System.ServiceProcess;

namespace ProvisionDaemon
{
    [RunInstaller(true)]
    public partial class ProvisionDaemonInstaller : Installer
    {
        public ProvisionDaemonInstaller()
        {
            InitializeComponent();
            InjectSubInstallers();
        }

        private void InjectSubInstallers()
        {
            Installers.AddRange(new Installer[] {
                new ServiceProcessInstaller(){
                     Account= ServiceAccount.LocalSystem
                },
                new ServiceInstaller() {
                     StartType= ServiceStartMode.Automatic,
                     ServiceName="ProvisionDaemon",
                     DisplayName="Provision Daemon",
                     Description="Provision Daemon"
                }
            });
        }
    }
}
