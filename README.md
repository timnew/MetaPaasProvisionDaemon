MetaPaasProvisionDaemon
=======================

A windows service that rename the computer according to its ipaddresses. It is designed for MetaPaas KVM based vitural machines,
which have problem with its computer name. All instances' computer name are the same, which cause problems to services who identify
hosts based on computer name, such as SQL Server and MSMQ, etc.

The service rename the computer based the computer's ip address with the service is started. And rename the computer if needed, then
reboot. The reboot is required by Windows, and only happened after renaming occurs.

The Computer Name will be renamed according to its ip address as following rule:
IP Address: 10.18.9.222 ==> Computer Name: MetaPaas-10-18-9-222

Prefix "MetaPaas-" and splitter "-" are configurable, which can be changed in .settings configuration file.

Known Issues:
1. Service might work inproperly if the computer have multiple network interfaces.
2. Service might work inpropery if the computer is not connected to network.
3. Service might work inproperly if big delay happed to DHCP response.