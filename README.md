# Remote Software Deployment Tool

This was a project I was writing for a client which was never completely finished as in the end they decided to move to using Microsoft Intune instead.

Over my years of becoming a developer I have used GitHub a lot to learn so rather than the project gathering dust on one of my backup drives I thought I would put it on GitHub for others to benefit, it still has some rough edges and may not be the cleanest programming but it worked.

Hopefully it will be of use to someone, or at least worthy of some comments / feedback as to how I could have done things better :)

***


It uses PowerShell via the Microsoft.PowerShell.SDK and System.Management.Automation NuGet packages and live updates a DevExpress GridControl to show progress of the MSI deployment across the selected computers.

You can either scan and deploy to all computers in AD (obviously a filter could be added to exclude as required), or provide a list of computers names (currently just hard coded at this stage - but could be added to form by way of listview / listbox etc)

The steps it goes through once you have selected an MSI and entered other parameters are as follows:


* Firstly checks the computer(s) are online
* Then checks they can be accessed remotely (requires psremoting to be enabled on client computers)
* Next checks to see if software you are deploying is already installed
* Optionally checks to see if a process is running (useful in cases where install will fail if app already open or another related process running)
* Then it will deploy the MSI
* Finally optionally shows a toast notification on the computer


