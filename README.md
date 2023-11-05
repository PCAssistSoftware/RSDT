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
* (Optionally) Checks to see if a process is running (useful in cases where install will fail if app already open or another related process running)
* Then it will deploy the MSI
* Finally (optionally) shows a toast notification on the remote computer(s)

***

![Screenshot](https://github.com/PCAssistSoftware/RSDT/blob/master/screenshot.png)

***

## FUTURE

* Add support for EXE - change browse button code so it allows selection of EXE or MSI and automatically detects extension of file chosen to then amend ScriptBlock in Public Async Function DeploySoftware to add correct command and switches depending on whether EXE or MSI
* Show error code for MSI failures e.g. 1602, 1603 etc - either in separate column or as a tooltip in Deploy Software column for any Deploy Failed entries
* Optionally add switch to MSIEXEC to create log and find way to pull back log content on MSI failure
* Finish adding cancellation to rest of the stages, only working for some of them so far, need to find way to do it for port test and to get that and Get-ADComputers working in same ways as other stages
* Add 

