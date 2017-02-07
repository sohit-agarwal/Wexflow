<p align="center">
  <img src="https://aelassas.github.io/wexflow/images/logo.png" alt="Wexflow" />
  <h4 align="center">Workflows Made Easy</h4>
</p>

# Continuous integration

|  Server | Platform | Build status |
----------|--------|----------|
| AppVeyor (.NET) | Windows |[![Build Status](https://ci.appveyor.com/api/projects/status/github/aelassas/Wexflow?svg=true)](https://ci.appveyor.com/project/aelassas/wexflow)|
| Travis (Mono) | Linux |[![Build Status](https://travis-ci.org/aelassas/Wexflow.svg?branch=master)](https://travis-ci.org/aelassas/Wexflow)|
| Bitrise (Android)| Linux|[![Build Status](https://www.bitrise.io/app/63a806486aa95f7d.svg?token=iO5-oRJcLJ9JVF_Q1n1UPQ)](https://www.bitrise.io/app/63a806486aa95f7d)|

# Wexflow
<!--[![Release](http://img.shields.io/badge/release-v1.0.6-brightgreen.svg)](https://github.com/aelassas/Wexflow/releases/tag/v1.0.6)-->
[![License](http://img.shields.io/badge/license-MIT-blue.svg)](https://github.com/aelassas/Wexflow/blob/master/LICENSE.txt)
[![Gitter](https://badges.gitter.im/Join Chat.svg)](https://gitter.im/Wexflow/Lobby)
[![Twitter](https://img.shields.io/badge/twitter-@wexflow86-55acee.svg?style=flat-square)](https://twitter.com/wexflow86)

Wexflow is an open source multithreaded workflow engine written in C#. Wexflow aims to make automations, workflow processes, long-running processes and interactions between systems, applications and folks easy, straitforward and clean.

A workflow is a series of distinct steps or phases. Each step is modeled in Wexflow as a Task. Tasks can be assembled visually into workflows using XML.

Wexflow provides the following features:

- **Sequential workflows:** A sequential workflow executes a set of tasks in order, one by one. Tasks are executed in a sequential manner until the last task finishes. The order of the execution of the tasks can be altered by modifying the execution graph of the workflow.
- **Flowchart workflows:** A flowchart workflow is a workflow that contains at least one flowchart node (DoIf/DoWhile) in its execution graph. A flow chart node takes as input a flowchart task (A task that returns either true or false after performing its job) and a set of tasks to execute in order, one by one. The order of the execution of the tasks can be altered by modifying the execution graph of the flowchart node.
- **Workflow events:** After a workflow finishes its job, its final result is either success, or warning or error. If its final result is success, the OnSuccess event is triggered. If its final result is warning, the OnWarning event is triggered. If its final result is error, the OnError event is triggered. An event contains a set of tasks and/or flowchart nodes to execute in order, one by one. The order of the execution of the tasks and/or flowchart nodes can be altered by modifying the execution graph of the event.
- **Launch types:** Workflows can either be launched when Wexflow engine starts or triggered manually or launched periodically.
- **Wexflow Manager:** Wexflow provides a GUI for managing workflows that works on both Windows and Linux.
- **Wexflow Web Manager:** Wexflow can be hosted on any website through its lightweight JavaScript API (~6Kb). Wexflow can be hosted on ASP.NET, ASP.NET MVC, PHP, Ruby On Rails, Python websites and so on.
- **Wexflow Andriod Manager:** Wexflow provides an Android application for managing workflows.
- **Monitoring:** Everything that happens in Wexflow is traced in the log files.

In addition, Wexflow provides the following built-in tasks:

- **File system tasks:** These tasks allow to create, copy, move, rename or delete files and directories on a file system. These tasks allow also to check whether a collection of remote or local files and/or directories exists.
- **Sync task:** This task allows to synchronise the content of a local or a remote source directory to a local or a remote destination directory. This task makes use of Microsoft Sync Framework 2.1.
- **Compression tasks:** These tasks allow to create a zip, a tar or a tar.gz from a collection of files.
- **MD5 task:** This task allows to generate MD5 sums of a collection of files.
- **FTP task:** This task allows to list, upload, download or delete files over FTP, FTPS (explicit/implicit) or SFTP. This task makes use of open source libraries written in C#.
- **HTTP task:** This task allows to downoad files over HTTP or HTTPS.
- **XML tasks:** These tasks allow to work with XML data. XSLT can be used along with XPath to generate XML documents. XSLT 1.0 and XSLT 2.0 are supported.
- **CSV tasks:** These tasks allow to work with CSV data. XML can be used along with XSLT to validate, compare and merge CSV data. The results of this can then be stored in CSV or XML format.
- **SQL task:** This task allows to execute SQL scripts. This task supports Microsoft Sql Server, Microsoft Access, Oracle, MySql, SQLite, PostGreSql and Teradata. This task can be used for bulk insert, for database updates, for database cleanup, for rebuilding indexes, for reorganizing indexes, for shrinking databases, for updating statistics, for transfering database data and so on.
- **WMI task:** This task allows to execute WMI queries. The results can be stored in XML format.
- **Image task:** This task allows to convert images to the following formats:** Bmp, Emf, Exif, Gif, Icon, Jpeg, Png, Tiff and Wmf.
- **Audio and video tasks:** These tasks allow to convert, cut or edit audio and video files through FFMEG or VLC. These tasks can also be used to perform custom operations such as generating images and thumbnails from video files.
- **Email task:** This task allows to send a collection of emails.
- **Twitter task:** This task allows to send a collection of tweets.
- **Process task:** This task allows to launch any process on the computer.
- **Wait task:** This task allows to wait for a specified duration of time.
- **Script tasks:** These tasks allows execute custom tasks written in C# or VB.

At this time, Wexflow only supports sequential execution of tasks but more complex scenarios like DoWhile, DoIf, parallel tasks execution and so on are in the todo list and are comming soon.

# Is Wexflow an ETL system?

You may think that Wexflow is rather an ETL system than a workflow engine. Well, the answer is that you can do ETL with Wexflow and even more. The spirit of Wexflow is to offer generic functionalities in a way that you can do pretty much whatever you want. With Wexflow, you can do ETL and even more through custom tasks and sequential/flowchart workflows.

# Why not Workflow Foundation?

WF (Windows Workflow Foundation) is a Microsoft technology for defining, executing and managing workflows.

WF is a proprietary solution and comes with a limited number of built-in activities.

Here are the strengths of Wexflow vs WF:

- Open source.
- Comes with 29 built-in tasks.
- Worklow events (OnSuccess, OnWarning and OnError).
- Workflows can either be launched when Wexflow engine starts or triggered manually or launched periodically.
- Provides a GUI for managing workflows that works on both Windows and Linux.
- Provides an Android application for managing workflows.
- Provides a lightweight JavaScript API that allows to manage workflows in any website.
- Everything that happens in Wexflow is traced in the log files.

# Prerequisites

To use Wexflow, you'll need basic skills in:
- XML
- XPath
- XSL

To create a custom task, you'll need basic skills in:
- XML
- XPath
- XSL if necessary
- C# or VB

At this time, Wexflow only supports creating and editing workflows in XML. However, creating and editing workflows in design mode are in the todo list and are comming soon. Wexflow Designer aims to allow folks who are not familiar with XML to work with Wexflow so they can create and edit their workflows easily.

# How to install Wexflow

## Windows

Wexflow can be installed on Windows XP, Windows server 2003 and higher. Wexflow supports .NET Framework 4.0 and higher.

To install Wexflow, proceed as follows:

1. Install Microsoft .NET Framework 4.0 or higher.

2. Install Microsoft Sync Framework 2.1 Synchronization Redistributables (Synchronization-v2.1-x86-ENU.msi available in Wexflow_setup_windows.zip).

3. Install Microsoft Sync Framework 2.1 Provider Services Redistributables (ProviderServices-v2.1-x86-ENU.msi available in Wexflow_setup_windows.zip)

4. Install WexflowSetup.exe (available in Wexflow_setup_windows.zip):

![alt tag](https://aelassas.github.io/wexflow/images/setup-1.PNG)

You can choose to create a desktop shortcut:

![alt tag](https://aelassas.github.io/wexflow/images/setup-2.PNG)

Click on install to perform the installation:

![alt tag](https://aelassas.github.io/wexflow/images/setup-3.PNG)

Finally, click on finish to finish the installation:

![alt tag](https://aelassas.github.io/wexflow/images/setup-4.PNG)

The following menus are added in the start menu:

![alt tag](https://aelassas.github.io/wexflow/images/wsp2.png)

After Wexflow is installed a Windows Service named Wexflow is installed and starts automatically. To start Wexflow Manager, this Windows Service must be running. However, If you want to stop it you can do it from Windows Services console:

![alt tag](https://aelassas.github.io/wexflow/images/ws.PNG)

The "Manager" menu opens Wexflow Manager GUI. The "Web Manager" menu opens Wexflow Web Manager. The "Documentation" menu opens the documentation folder of Wexflow. The "Configuration" menu opens the configuration folder of Wexflow. The "Logs" menu opens the log file of the day.

## Linux

To run Wexflow on Linux, Wexflow Windows Service must be installed on a Windows machine. Wexflow Windows Service provides a self hosted web service that allows to query Wexflow Engine.

After Wexflow Windows Service is installed on a Windows Machine, proceed as follows to install Wexflow Manager on Linux:

- Download wexflow.tar.gz (available in Wexflow_setup_linux.zip)
- Install mono-complete:

```
sudo apt install mono-complete
```

- Install Wexflow Manager:

```
sudo mv wexflow.tar.gz /opt/
cd /opt/
sudo tar -zxvf wexflow.tar.gz
sudo chmod +x /opt/wexflow/wexflow.sh
sudo ln -s /opt/wexflow/wexflow.sh /usr/local/bin/wexflow
```

- Configure Wexflow web service uri by modifying the settings option WexflowWebServiceUri in Wexflow Manager configuration file:

```
sudo vim /opt/wexflow/Wexflow.Clients.Eto.Manager.exe.config 
```

Run Wexflow Manager:

```
wexflow
```

The following window will appear:

![alt tag](https://aelassas.github.io/wexflow/images/wml.png)

## Android

Wexflow provides a GUI for managing workflows that can be installed on an Android device.

To run Wexflow on Andoird, Wexflow Windows Service must be installed on a Windows machine. Wexflow Windows Service provides a self hosted web service that allows to query Wexflow Engine.

After Wexflow Windows Service is installed on a Windows Machine, proceed as follows to install Wexflow Manager on an Android device:

- Download wexflow.apk (available in Wexflow_setup_android.zip)

- Copy wexflow.apk into the Android device

- Install wexflow.apk

- Launch Wexflow application and open the application settings through the settings menu:

![alt tag](https://aelassas.github.io/wexflow/images/android-1.png)

-  Configure Wexflow Web Service Uri:

![alt tag](https://aelassas.github.io/wexflow/images/android-2.png)

That's it. Wexflow application is ready for work: 

![alt tag](https://aelassas.github.io/wexflow/images/android-3.png)

# How to uninstall Wexflow

## Windows

To uninstall Wexflow, simply click on "Uninstall" menu from "Windows Start menu > Wexflow".

Or go to "Configuration Panel > Add/remove programs" then select "Wexflow version 1.0.1" and click on uninstall:

![alt tag](https://aelassas.github.io/wexflow/images/wu.PNG)

After Wexflow is uninstalled, the folders C:\Wexflow\ and C:\WexflowTesting\ are not deleted to prevent user defined workflows and testing scenarios from being deleted. However, If you do not need them you can delete them manually.

The log file C:\Program Files\Wexflow\Wexflow.log is also not deleted to keep track of the last operations done by Wexflow. However, If you do not need the logs you can delete the log files.

## Linux

To uninstall Wexflow Manager from a Linux machine, proceed as follows:

```
sudo rm /usr/local/bin/wexflow
sudo rm -rf /opt/wexflow
```

## Android

To uninstall Wexflow from an Android device, simply open Settings>Applications>Wexflow then uninstall it.

# How to use Wexflow

## General

After installing Wexflow, the folders C:\Wexflow\ and C:\WexflowTesting\ are created. The folder C:\Wexflow\ contains the following elements:

- **Wexflow.xml** which is the main configuration file of Wexflow engine. Its path can be configured from C:\Program Files\Wexflow\Wexflow.Clients.WindowsService.exe.config
- **Workflows/** which contains the workflows in XML format.
- **Temp/** which is the temporary foler of Wexflow.

The folder C:\WexflowTesting\ contains data of testing scenarios.

The logs are written in C:\Program Files\Wexflow\Wexflow.log. There is one log file per day. The old log files are saved in this format C:\Program Files\Wexflow\Wexflow.logyyyyMMdd.

Below the configuration file of a workflow:

```xml
<?xml version="1.0" encoding="utf-8" ?>
<!--
    This is the configuration file of a workflow. 
    A workflow is composed of:
      - An id which is an integer that must be unique.
      - A name which is a string that must be unique.
      - A description wich is a string.
      - A settings section which is composed of the following elements:
        - A launchType which is one of the following options:
          - startup: The workflow is launched when Wexflow engine starts.
          - trigger: The workflow is launched manually from the Wexflow manager.
          - periodic: The workflow is lauched periodically.
        - A period which is necessary for the periodic launchType. It is 
          a timeSpan in this format dd.hh:mm:ss. For example the period
          00.00:02:00 will launch the workflow every 2 minutes.
        - The enabled option which allows to enable or disable a workflow.
          The possible values are true or false.
      - A Tasks section which contains the tasks that will be executed by
        the workflow one after the other.
        - A Task is composed of:
          - An id which is an integer that must be unique.
          - A name wich is one of the options described in the tasks documentation.
          - A description which is a string.
          - The enable option which allows to enable or disable a task. The possible 
            values are true or false.
          - A collection of settings.
        - An ExecutionGraph section which contains the execution graph of the workflow.
          This section is optional and described in the samples section.
-->
<Workflow xmlns="urn:wexflow-schema" id="$int" name="$string" description="$string">
  <Settings>
    <Setting name="launchType" value="startup|trigger|periodic" />
    <Setting name="period" value="dd.hh:mm:ss" />
    <Setting name="enabled" value="true|false" />
  </Settings>
  <Tasks>
    <Task id="$int" name="$string" description="$string" enabled="true|false">
      <Setting name="$string" value="$string" />
      <Setting name="$string" value="$string" />
      <!-- You can add as many settings as you want. -->
    </Task>
    <Task id="$int" name="$string" description="$string" enabled="true|false">
      <Setting name="$string" value="$string" />
      <Setting name="$string" value="$string" />
    </Task>
    <!-- You can add as many tasks as you want. -->
  </Tasks>
  <!-- This node is optional and described in the samples section. -->
  <ExecutionGraph />
</Workflow>
```

The name option of a Task must be one of the followings:
- **CsvToXml:** This task transforms a CSV file to an XML file. The format of the output XML file is described in the documentation of the task.
- **FilesCopier:** This task copies a collection of files to a destination folder. 
- **FilesLoader:** This task loads a collection of files located in folders or through the file option. 
- **FilesMover:** This task moves a collection of files to a destination folder.
- **FilesRemover:** This task deletes a collection of files.
- **Ftp:** This task allows to list, upload, download or delete files over FTP, FTPS (explicit/implicit) or SFTP. This task makes use of open source libraries written in C#.
- **ListEntities:** This task lists all the entities loaded by the workflow tasks in the logs. This task is useful for resolving issues.
- **ListFiles:** This task lists all the files loaded by the workflow tasks in the logs. This task is useful for resolving issues.
- **MailsSender:** This task sends a collection of emails from XML files. The format of the input XML files is described in the documentation of the task.
- **Md5:** This task generates MD5 sums of a collection of files and writes the results in an XML file. The format of the output XML file is described in the documentation of the task.
- **Mkdir:** This task creates a collection of folders.
- **ProcessLauncher:** This task launches a process. If the process generates a file as output It is possible to pass a collection of files to the task so that for each file an output file will be generated through the process. Read the documentation of the task for further informations.
- **Rmdir:** This task deletes a collection of folders.
- **Touch:** This task creates a collection of empty files.
- **Twitter:** This task sends a collection of tweets from XML files. The format of input XML files is described in the documentation of the task.
- **XmlToCsv:** This task transforms an XML file to a CSV file. The format of the input XML file is described in the documentation of the task.
- **Xslt:** This task transforms an XML file. It is possible to use XSLT 1.0 processor or XSLT 2.0 processor.
- **Zip:** This task creates a zip archive from a collection of files.
- **Tar:** This task creates a tar archive from a collection of files.
- **Tgz:** This task creates a tar.gz archive from a collection of files.
- **Sql:** This task executes a colletion of SQL script files or a simple SQL script through sql settings option. It supports Microsoft Sql Server, Microsoft Access, Oracle, MySql, SQLite, PostGreSql and Teradata.
- **Wmi:** This task executes a WMI query and outputs the results in an XML file. The format of the output XML file is described in the documentation of the task.
- **ImagesTransformer:** This task transforms a collection of image files to a specified format. The output format can be one of the followings: Bmp, Emf, Exif, Gif, Icon, Jpeg, Png, Tiff or Wmf.
- **Http:** This task allows to downoad files over HTTP or HTTPS.
- **Sync:** This task allows to synchronise the content of a local or remote source directory to a local or remote destination directory. This task makes use of Microsoft Sync Framework 2.1.
- **FilesRenamer:** This task allows to rename a collection of files on a file system. The Xslt task can be used along with the ListFiles task to create new file names. Check out the documentation of these tasks and the workflow sample Workflow_FilesRenamer.xml to see how this can be done.
- **Wait:** This task waits for a specified duration of time.
- **FilesExist:** This task checks whether a collection of files and/or directories exists.
- **FileExists:** This is a flowchart task that checks whether a given file exists on a file system or not.

To learn how to make your own workflows, you can check out the workflow samples availabe in C:\Wexflow\Workflows\ and read the tasks documentations available in the documentation folder C:\Program Files\Wexflow\Documentation.

If a new workflow is created in C:\Wexflow\Workflows\ or if an existing workflow is deleted or modified, you have to restart Wexflow Windows Service so that these modifications take effect.

To disable a workflow, you can set the enabled settings option of the workflow to false. If you want to make a workflow disappears from the list of the workflows loaded by Wexflow engine, you can create a directory named disabled within C:\Wexflow\Workflows\ and move that workflow to that directory then restart Wexflow Windows service.

## Wexflow Manager

![alt tag](https://aelassas.github.io/wexflow/images/wm.PNG)

Wexflow Manager is a simple application that allows the user to do the following things:

- See all the workflows loaded by Wexflow Engine.
- See the status of the selected workflow (running, suspended or disabled).
- Start a workflow.
- Stop a workflow.
- Suspend a workflow.
- Resume a workflow.

To see what's going on in Wexflow, open the log file C:\Program Files\Wexflow\Wexflow.log in a text editor like [Notepad++](https://notepad-plus-plus.org/download/v7.3.1.html). Notepad ++ will update the log file as it fills up.

## Wexflow Web Manager

![alt tag](https://aelassas.github.io/wexflow/images/wwm2.png)

Wexflow provides a lightweight JavaScript API (~6Kb) that allows Wexflow Manager to be hosted on any website.

Wexflow Web Manager allows the user to do the following things:

- See all the workflows loaded by Wexflow Engine.
- See the status of the selected workflow (running, suspended or disabled).
- Start a workflow.
- Stop a workflow.
- Suspend a workflow.
- Resume a workflow.

To host Wexflow Web Manager in a website, simply proceed as follows:

1. Reference wexflow.min.css and wexflow.min.js: These files are located in C:\Program Files\Wexfow\Web Manager\
2. Create an instance of Wexflow.

The HTML source code should look like as follows:

```html
<!DOCTYPE html>
<html>
<head>
    <title>Wexflow</title>

    <link rel="stylesheet" type="text/css" href="css/wexflow.min.css" />
    <script type="text/javascript" src="js/wexflow.min.js"></script>

    <script type="text/javascript">
        window.onload = function () {
            new Wexflow("wexflow", "http://localhost:8000/wexflow/");
        };
    </script>
</head>
<body>
    <div id="wexflow" style="position: absolute; top:0; right:0; bottom:0; left:0;"></div>
</body>
</html>
```

## Wexflow Android Manager

![alt tag](https://aelassas.github.io/wexflow/images/android-3.png)

Wexflow provides an Android application for managing workflows.

Wexflow Android Manager allows the user to do the following things:

- See all the workflows loaded by Wexflow Engine.
- See the status of the selected workflow (running, suspended or disabled).
- Start a workflow.
- Stop a workflow.
- Suspend a workflow.
- Resume a workflow.

# Workflow samples

In this section, few workflow samples will be presented in order to make the end user familiar with Wexflow workflows synthax.

## Sequential workflows

### Workflow 1

![alt tag](https://aelassas.github.io/wexflow/images/Workflow_Invoices.png)

This workflow uploads invoices to an SFTP server, then waits for 2 days and then notifies the customers.

```xml
<Workflow xmlns="urn:wexflow-schema" id="99" name="Workflow_Invoices" description="Workflow_Invoices">
    <Settings>
        <Setting name="launchType" value="trigger" />
        <Setting name="enabled" value="true" />
    </Settings>
    <Tasks>
        <Task id="1" name="FilesLoader" description="Loading invioces" enabled="true">
            <Setting name="folder" value="C:\WexflowTesting\Invoices\" />
        </Task>
        <Task id="2" name="Ftp" description="Uploading invoices" enabled="true">
            <Setting name="protocol" value="sftp" /> <!-- ftp|ftps|sftp -->
            <Setting name="command" value="upload" /> <!-- list|upload|download|delete -->
            <Setting name="server" value="127.0.1" />
            <Setting name="port" value="21" />
            <Setting name="user" value="user" />
            <Setting name="password" value="password" />
            <Setting name="path" value="/" />
            <Setting name="selectFiles" value="1" />
        </Task>
        <Task id="3" name="Wait" description="Waiting for 2 days" enabled="true">
            <Setting name="duration" value="2.00:00:00" />
        </Task>
        <Task id="4" name="FilesLoader" description="Loading emails" enabled="true">
            <Setting name="file" value="C:\WexflowTesting\Emails\Invoices.xml" />
        </Task>
       <Task id="5" name="MailsSender" description="Notifying customers" enabled="true">
            <Setting name="selectFiles" value="4" />
            <Setting name="host" value="127.0.0.1" />
            <Setting name="port" value="587" />
            <Setting name="enableSsl" value="true" />
            <Setting name="user" value="user" />
            <Setting name="password" value="password" />
        </Task>
        <Task id="6" name="FilesMover" description="Moving invoices" enabled="true">
            <Setting name="selectFiles" value="1" />
            <Setting name="destFolder" value="C:\WexflowTesting\Invoices_sent\" />
        </Task>
    </Tasks>
</Workflow>
```

First of all, the FilesLoader task loads all the invoices located in the folder C:\WexflowTesting\Invoices\, then the Ftp task uploads them to the SFTP server, then the Wait task waits for 2 days, then the FilesLoader task loads the emails in XML format and then the MailsSender task sends the emails. Finally, the FilesMover task moves the invoices to the folder C:\WexflowTesting\Invoices_sent\.

### Workflow 2

![alt tag](https://aelassas.github.io/wexflow/images/Workflow_FilesSender.png)

This workflow waits for files to arrive in C:\WexflowTesting\Watchfolder1\ and C:\WexflowTesting\Watchfolder2\ then uploads them to an FTP server then moves them to C:\WexflowTesting\Sent\ folder. This workflow starts every 2 minutes.

```xml
<Workflow xmlns="urn:wexflow-schema" id="6" name="Workflow_FilesSender" description="Workflow_FilesSender">
    <Settings>
        <Setting name="launchType" value="periodic" />
        <Setting name="period" value="00.00:02:00.00" />
        <Setting name="enabled" value="true" />
    </Settings>
    <Tasks>
        <Task id="1" name="FilesLoader" description="Loading files" enabled="true">
            <Setting name="folder" value="C:\WexflowTesting\Watchfolder1\" />
            <Setting name="folder" value="C:\WexflowTesting\Watchfolder2\" />
        </Task>
        <Task id="2" name="Ftp" description="Uploading files" enabled="true">
            <Setting name="protocol" value="ftp" /> <!-- ftp|ftps|sftp -->
            <Setting name="command" value="upload" /> <!-- list|upload|download|delete -->
            <Setting name="server" value="127.0.1" />
            <Setting name="port" value="21" />
            <Setting name="user" value="user" />
            <Setting name="password" value="password" />
            <Setting name="path" value="/" />
            <Setting name="selectFiles" value="1" />
        </Task>
        <Task id="3" name="FilesMover" description="Moving files to Sent folder" enabled="true">
            <Setting name="selectFiles" value="1" />
            <Setting name="destFolder" value="C:\WexflowTesting\Sent\" />
        </Task>
    </Tasks>
</Workflow>
```

First of all, the FilesLoader task loads all the files located in the folders C:\WexflowTesting\Watchfolder1\ and C:\WexflowTesting\Watchfolder2\ then the Ftp task loads the files and uploads them to the FTP server. Finally, the FilesMover task moves the files to the folder C:\WexflowTesting\Sent\.

### Workflow 3

![alt tag](https://aelassas.github.io/wexflow/images/Workflow_ffmpeg.png)

This workflow transcodes the WAV files located in C:\WexflowTesting\WAV\ to MP3 format through FFMPEG and moves the transcoded files to C:\WexflowTesting\MP3\.

```xml
<Workflow xmlns="urn:wexflow-schema" id="12" name="Workflow_ffmpeg" description="Workflow_ffmpeg">
    <Settings>
        <Setting name="launchType" value="trigger" />
        <Setting name="enabled" value="true" />
    </Settings>
    <Tasks>
        <Task id="1" name="FilesLoader" description="Loading WAV files" enabled="true">
            <Setting name="folder" value="C:\WexflowTesting\WAV\" />
        </Task>
        <Task id="2" name="ProcessLauncher" description="WAV to MP3" enabled="true">
            <Setting name="selectFiles" value="1" />
            <!-- You need to install FFMPEG -->
            <Setting name="processPath" value="C:\Program Files\ffmpeg\bin\ffmpeg.exe" />
            <!-- variables: {$filePath},{$fileName},{$fileNameWithoutExtension}-->
            <Setting name="processCmd" value="-i {$filePath} -codec:a libmp3lame -qscale:a 2 {$output:$fileNameWithoutExtension.mp3}" /> 
            <Setting name="hideGui" value="true" />
            <Setting name="generatesFiles" value="true" /> 
        </Task>
        <Task id="3" name="FilesMover" description="Moving MP3 files from temp folder" enabled="true">
            <Setting name="selectFiles" value="2" />
            <Setting name="destFolder" value="C:\WexflowTesting\MP3\" />
        </Task>
    </Tasks>
</Workflow>
```

First of all, the FilesLoader task loads all the files located in the folder C:\WexflowTesting\WAV\ then the ProcessLauncher task launches FFMPEG process on every file by specifying the right command in order to create the MP3 file. Finally, the FilesMover task moves the MP3 files to the folder C:\WexflowTesting\MP3\.

### Workflow 4

![alt tag](https://aelassas.github.io/wexflow/images/Workflow_vlc.png)

This workflow waits for WAV files to arrive in C:\WexflowTesting\WAV\ then transcodes them to MP3 files through VLC then uploads the MP3 files to an FTP server then moves the WAV files to C:\WexflowTesting\WAV_processed\. This workflow starts every 2 minutes.

```xml
<Workflow xmlns="urn:wexflow-schema" id="13" name="Workflow_vlc" description="Workflow_vlc">
    <Settings>
        <Setting name="launchType" value="periodic" />
        <Setting name="period" value="00.00:02:00.00" />
        <Setting name="enabled" value="true" />
    </Settings>
    <Tasks>
        <Task id="1" name="FilesLoader" description="Loading WAV files" enabled="true">
            <Setting name="folder" value="C:\WexflowTesting\WAV\" />
        </Task>
        <Task id="2" name="ProcessLauncher" description="WAV to MP3" enabled="true">
            <Setting name="selectFiles" value="1" />
            <!-- You need to install VLC-->
            <Setting name="processPath" value="C:\Program Files\VideoLAN\VLC\vlc.exe" />
            <!-- variables: {$filePath},{$fileName},{$fileNameWithoutExtension}-->
            <Setting name="processCmd" value="-I dummy {$filePath} :sout=#transcode{acodec=mpga}:std{dst={$output:$fileNameWithoutExtension.mp3},access=file} vlc://quit" />
            <Setting name="hideGui" value="true" />
            <Setting name="generatesFiles" value="true" />
        </Task>
        <Task id="3" name="Ftp" description="Uploading MP3 files" enabled="true">
            <Setting name="protocol" value="ftp" />
            <Setting name="command" value="upload" />
            <Setting name="server" value="127.0.1" />
            <Setting name="port" value="21" />
            <Setting name="user" value="user" />
            <Setting name="password" value="password" />
            <Setting name="path" value="/" />
            <Setting name="selectFiles" value="2" />
        </Task>
        <Task id="4" name="FilesMover" description="Moving WAV files" enabled="true">
            <Setting name="selectFiles" value="1" />
            <Setting name="destFolder" value="C:\WexflowTesting\WAV_processed\" />
        </Task>
    </Tasks>
</Workflow>
```

First of all, the FilesLoader task loads all the files located in the folder C:\WexflowTesting\WAV\ then the ProcessLauncher task launches VLC process on every file by specifying the right command in order to create the MP3 file. Then, the Ftp task loads the MP3 files generated by the ProcessLauncher task and then uploads them to the FTP server. Finally, the FilesMover task moves the processed WAV files to the folder C:\WexflowTesting\WAV_processed\.

### Workflow 5

![alt tag](https://aelassas.github.io/wexflow/images/Workflow_Ftp_download_tag.png)

This workflow downloads specific files from an FTP server. This workflow starts by listing all the files located at the root folder of the server, then the specific files that will be downloaded are tagged through an XSLT (LisFiles.xslt), then the files are downloaded by the Ftp task through todo="toDownload" and from="app4" tags, then the downloaded files are moved to the folder C:\WexflowTesting\Ftp_download\.

```xml
<Workflow xmlns="urn:wexflow-schema" id="40" name="Workflow_Ftp_download_tag" description="Workflow_Ftp_download_tag">
    <Settings>
        <Setting name="launchType" value="trigger" /> <!-- startup|trigger|periodic -->
        <Setting name="enabled" value="true" /> <!-- true|false -->
    </Settings>
    <Tasks>
        <Task id="1" name="Ftp" description="Listing files (FTP)" enabled="true">
            <Setting name="command" value="list" />
            <Setting name="protocol" value="ftp" /> <!-- ftp|ftps|sftp -->
            <Setting name="server" value="127.0.1" />
            <Setting name="port" value="21" />
            <Setting name="user" value="user" />
            <Setting name="password" value="password" />
            <Setting name="path" value="/" />
        </Task>
        <Task id="2" name="ListFiles" description="Listing files" enabled="true">
        </Task>
        <Task id="3" name="Xslt" description="Renaming and tagging files" enabled="true">
            <Setting name="selectFiles" value="2" />
            <Setting name="xsltPath" value="C:\Wexflow\Xslt\ListFiles.xslt" />
            <Setting name="version" value="2.0" /> <!-- 1.0|2.0 -->
            <Setting name="removeWexflowProcessingNodes" value="false" />
        </Task>
        <Task id="4" name="Ftp" description="Downloading files" enabled="true">
            <Setting name="command" value="download" />
            <Setting name="protocol" value="ftp" /> <!-- ftp|ftps|sftp -->
            <Setting name="server" value="127.0.1" />
            <Setting name="port" value="21" />
            <Setting name="user" value="user" />
            <Setting name="password" value="password" />
            <Setting name="path" value="/" />
            <Setting name="selectFiles" todo="toDownload" from="app4" />
        </Task>
        <Task id="5" name="FilesMover" description="Moving files to Ftp_download" enabled="true">
            <Setting name="selectFiles" value="4" />
            <Setting name="destFolder" value="C:\WexflowTesting\Ftp_download\" />
            <Setting name="overwrite" value="true" />
        </Task>
    </Tasks>
</Workflow>
```

Roughly speaking, the Ftp task loads the list of files located at the root folder of the FTP server in the running instance of the workflow, then the ListFiles task outputs and XML file that contains all the files loaded then the Xslt task takes as input this XML and generates an XML wich contains a system node called WexflowProcessing wich contains the list of files to be tagged and/or renamed.

To understand how tagging and renaming files work, refer to the documentation of the ListFiles and Xslt tasks.

Below is the XSLT ListFiles.xslt used for tagging files:

```xml
<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:output method="xml" indent="yes"/>

  <xsl:template match="/">
    <root>
      <WexflowProcessing>
        <xsl:for-each select="//WexflowProcessing/Workflow/Files//File">
          <xsl:choose>
            <xsl:when test="@name = 'file1.txt'">
              <File taskId="{@taskId}" name="{@name}" renameTo="file1_renamed.txt" 
                    todo="toRename" 
                    from="app1" />
            </xsl:when>
            <xsl:when test="@name = 'file2.txt'">
              <File taskId="{@taskId}" name="{@name}" renameTo="file2_renamed.txt" 
                    todo="toSend" 
                    from="app2" />
            </xsl:when>
            <xsl:when test="@name = 'file3.txt'">
              <File taskId="{@taskId}" name="{@name}" renameTo="file3_renamed.txt" 
                    todo="toDownload" 
                    from="app3" />
            </xsl:when>
            <xsl:when test="@name = 'file4.txt'">
              <File taskId="{@taskId}" name="{@name}" renameTo="file4_renamed.txt"
                    todo="toDownload" 
                    from="app4" />
            </xsl:when>
          </xsl:choose>
        </xsl:for-each>
      </WexflowProcessing>
    </root>
  </xsl:template>
</xsl:stylesheet>
```

## Execution graph

![alt tag](https://aelassas.github.io/wexflow/images/Workflow_FilesSender.png)

This workflow loads the file C:\WexflowTesting\file1.txt then uploads it to an FTP server then moves it to C:\WexflowTesting\Sent\ folder.

```xml
<Workflow xmlns="urn:wexflow-schema" id="6" name="Workflow_Ftp_upload" description="Workflow_Ftp_upload">
    <Settings>
        <Setting name="launchType" value="trigger" />
        <Setting name="enabled" value="true" />
    </Settings>
    <Tasks>
        <Task id="1" name="FilesLoader" description="Loading files" enabled="true">
            <Setting name="file" value="C:\WexflowTesting\file1.txt" />
        </Task>
        <Task id="2" name="Ftp" description="Uploading files" enabled="true">
            <Setting name="protocol" value="ftp" />
            <Setting name="command" value="upload" />
            <Setting name="server" value="127.0.1" />
            <Setting name="port" value="21" />
            <Setting name="user" value="user" />
            <Setting name="password" value="password" />
            <Setting name="path" value="/" />
            <Setting name="selectFiles" value="1" />
        </Task>
        <Task id="3" name="FilesMover" description="Moving files to Sent folder" enabled="true">
            <Setting name="selectFiles" value="1" />
            <Setting name="destFolder" value="C:\WexflowTesting\Sent\" />
        </Task>
    </Tasks>
    <ExecutionGraph>
      <Task id="1"><Parent id="-1" /></Task>
      <Task id="2"><Parent id="1"  /></Task>
      <Task id="3"><Parent id="2"  /></Task>
    </ExecutionGraph>
</Workflow>
```

First of all, the FilesLoader task loads the file C:\WexflowTesting\file1.txt then the Ftp task loads that file and uploads it to the FTP server. Finally, the FilesMover task moves that file to the folder C:\WexflowTesting\Sent\.

By convention, the parent task id of first task to be executed must always be -1. The execution graph of this workflow will execute the tasks in the following order:

![alt tag](https://aelassas.github.io/wexflow/images/Workflow_ExecutionGraph_upload_1.png)

However, if the execution graph is modified as follows:

```xml
<ExecutionGraph>
  <Task id="1"><Parent id="-1" /></Task>
  <Task id="3"><Parent id="1"  /></Task>
  <Task id="2"><Parent id="3"  /></Task>
</ExecutionGraph>
```

The tasks will be executed as follows:

![alt tag](https://aelassas.github.io/wexflow/images/Workflow_ExecutionGraph_upload_2.png)

If the execution graph is modified as follows:

```xml
<ExecutionGraph>
  <Task id="3"><Parent id="-1" /></Task>
  <Task id="2"><Parent id="3"  /></Task>
  <Task id="1"><Parent id="2"  /></Task>
</ExecutionGraph>
```

The tasks will be executed as follows:

![alt tag](https://aelassas.github.io/wexflow/images/Workflow_ExecutionGraph_upload_3.png)

Two things are forbidden in the execution graph:

- Infinite loops.
- Parallel tasks.

Here is an example of an infinite loop:

![alt tag](https://aelassas.github.io/wexflow/images/Workflow_ExecutionGraph_infinite_loop_2.png)

```xml
<ExecutionGraph>
  <Task id="1"><Parent id="-1" /></Task>
  <Task id="2"><Parent id="1"  /></Task>
  <Task id="1"><Parent id="2"  /></Task>
</ExecutionGraph>
```

Here is an example of parallel tasks:

![alt tag](https://aelassas.github.io/wexflow/images/Workflow_ExecutionGraph_parallel_tasks.png)

```xml
<ExecutionGraph>
  <Task id="1"><Parent id="-1" /></Task>
  <Task id="2"><Parent id="1"  /></Task>
  <Task id="3"><Parent id="1"  /></Task>
</ExecutionGraph>
```

## Flowchart workflows

### DoIf

![alt tag](https://aelassas.github.io/wexflow/images/Workflow_DoIf.png)

The following workflow is a flowchart workflow that is triggered by the file file.trigger. If the file file.trigger is found on the file system then this workflow will upload the file file1.txt to an FTP server then it will notify customers that the upload was successful. Otherwise, if the trigger file.trigger is not found on the file system then the workflow will notify customers that the upload failed.

```xml
<Workflow xmlns="urn:wexflow-schema" id="7" name="Workflow_DoIf" description="Workflow_DoIf">
    <Settings>
        <Setting name="launchType" value="trigger" />
        <Setting name="enabled" value="true" />
    </Settings>
    <Tasks>
        <Task id="1" name="FilesLoader" description="Loading files" enabled="true">
            <Setting name="file" value="C:\WexflowTesting\file1.txt" />
        </Task>
        <Task id="2" name="Ftp" description="Uploading files" enabled="true">
            <Setting name="protocol" value="ftp" />
            <Setting name="command" value="upload" />
            <Setting name="server" value="127.0.1" />
            <Setting name="port" value="21" />
            <Setting name="user" value="user" />
            <Setting name="password" value="password" />
            <Setting name="path" value="/" />
            <Setting name="selectFiles" value="1" />
        </Task>
        <Task id="3" name="FilesLoader" description="Loading emails (OK)" enabled="true">
            <Setting name="file" value="C:\WexflowTesting\Emails\Emails.xml" />
        </Task>
       <Task id="4" name="MailsSender" description="Notifying customers (OK)" enabled="true">
            <Setting name="selectFiles" value="3" />
            <Setting name="host" value="127.0.0.1" />
            <Setting name="port" value="587" />
            <Setting name="enableSsl" value="true" />
            <Setting name="user" value="user" />
            <Setting name="password" value="password" />
        </Task>
        <Task id="5" name="FilesLoader" description="Loading emails (KO)" enabled="true">
            <Setting name="file" value="C:\WexflowTesting\Emails\Emails.xml" />
        </Task>
       <Task id="6" name="MailsSender" description="Notifying customers (KO)" enabled="true">
            <Setting name="selectFiles" value="5" />
            <Setting name="host" value="127.0.0.1" />
            <Setting name="port" value="587" />
            <Setting name="enableSsl" value="true" />
            <Setting name="user" value="user" />
            <Setting name="password" value="password" />
        </Task>
        <Task id="99" name="FileExists" description="Checking trigger file" enabled="true">
            <Setting name="file" value="C:\WexflowTesting\file.trigger" />
        </Task>
    </Tasks>
    <ExecutionGraph>
      <DoIf id="100" if="99">
        <Parent id="-1" />
         <Do>
            <Task id="1"><Parent id="-1" /></Task>
            <Task id="2"><Parent id="1"  /></Task>
            <Task id="3"><Parent id="2"  /></Task>
            <Task id="4"><Parent id="3"  /></Task>
         </Do>
         <Otherwise>
            <Task id="5"><Parent id="-1" /></Task>
            <Task id="6"><Parent id="5"  /></Task>
         </Otherwise>
      </DoIf>
    </ExecutionGraph>
</Workflow>
```

By convention, the parent task id of the first task in Do and Otherwise nodes must always be -1.

You can add DoIf flowchart nodes pretty much wherever you want in the execution graph. Also, you can add as mush as you want. You can also add them in the event nodes OnSuccess, OnWarning and OnError. The only restrictions is that you cannot add a DoIf inside a DoIf and a DoIf inside a DoWhile.

### DoWhile

![alt tag](https://aelassas.github.io/wexflow/images/Workflow_DoWhile2.png)

This workflow is triggered by the file file.trigger. While the file file.trigger exists, this workflow will upload the file file1.txt to an FTP server then it will notify customers then it will wait for 2 days then it will start again.

```xml
<Workflow xmlns="urn:wexflow-schema" id="8" name="Workflow_DoWhile" description="Workflow_DoWhile">
    <Settings>
        <Setting name="launchType" value="trigger" />
        <Setting name="enabled" value="true" />
    </Settings>
    <Tasks>
        <Task id="1" name="FilesLoader" description="Loading files" enabled="true">
            <Setting name="file" value="C:\WexflowTesting\file1.txt" />
        </Task>
        <Task id="2" name="Ftp" description="Uploading files" enabled="true">
            <Setting name="protocol" value="ftp" />
            <Setting name="command" value="upload" />
            <Setting name="server" value="127.0.1" />
            <Setting name="port" value="21" />
            <Setting name="user" value="user" />
            <Setting name="password" value="password" />
            <Setting name="path" value="/" />
            <Setting name="selectFiles" value="1" />
        </Task>
        <Task id="3" name="FilesLoader" description="Loading emails" enabled="true">
            <Setting name="file" value="C:\WexflowTesting\Emails\Emails.xml" />
        </Task>
       <Task id="4" name="MailsSender" description="Notifying customers" enabled="true">
            <Setting name="selectFiles" value="3" />
            <Setting name="host" value="127.0.0.1" />
            <Setting name="port" value="587" />
            <Setting name="enableSsl" value="true" />
            <Setting name="user" value="user" />
            <Setting name="password" value="password" />
        </Task>
        <Task id="5" name="Wait" description="Waiting for 2 days..." enabled="true">
            <Setting name="duration" value="02.00:00:00" />
        </Task>
        <Task id="99" name="FileExists" description="Checking trigger file" enabled="true">
            <Setting name="file" value="C:\WexflowTesting\file.trigger" />
        </Task>
    </Tasks>
    <ExecutionGraph>
      <DoWhile id="100" while="99">
        <Parent id="-1" />
         <Do>
            <Task id="1"><Parent id="-1" /></Task>
            <Task id="2"><Parent id="1"  /></Task>
            <Task id="3"><Parent id="2"  /></Task>
            <Task id="4"><Parent id="3"  /></Task>
            <Task id="5"><Parent id="4"  /></Task>
         </Do>
      </DoWhile>
    </ExecutionGraph>
</Workflow>
```

By convention, the parent task id of the first task in the Do node must always be -1.

You can add DoWhile flowchart nodes pretty much wherever you want in the execution graph. Also, you can add as mush as you want. You can also add them in the event nodes OnSuccess, OnWarning and OnError. The only restrictions is that you cannot add a DoWhile inside a DoWhile and a DoWhile inside a DoIf.

## Workflow events

![alt tag](https://aelassas.github.io/wexflow/images/Workflow_Events.png)

This workflow uploads the file1.txt to an FTP server then notifies customers in case of success.

```xml
<Workflow xmlns="urn:wexflow-schema" id="9" name="Workflow_Events" description="Workflow_Events">
    <Settings>
        <Setting name="launchType" value="trigger" />
        <Setting name="enabled" value="true" />
    </Settings>
    <Tasks>
        <Task id="1" name="FilesLoader" description="Loading files" enabled="true">
            <Setting name="file" value="C:\WexflowTesting\file1.txt" />
        </Task>
        <Task id="2" name="Ftp" description="Uploading files" enabled="true">
            <Setting name="protocol" value="ftp" />
            <Setting name="command" value="upload" />
            <Setting name="server" value="127.0.1" />
            <Setting name="port" value="21" />
            <Setting name="user" value="user" />
            <Setting name="password" value="password" />
            <Setting name="path" value="/" />
            <Setting name="selectFiles" value="1" />
        </Task>
       <Task id="3" name="FilesLoader" description="Loading emails" enabled="true">
            <Setting name="file" value="C:\WexflowTesting\Emails\Emails.xml" />
        </Task>
       <Task id="4" name="MailsSender" description="Notifying customers" enabled="true">
            <Setting name="selectFiles" value="3" />
            <Setting name="host" value="127.0.0.1" />
            <Setting name="port" value="587" />
            <Setting name="enableSsl" value="true" />
            <Setting name="user" value="user" />
            <Setting name="password" value="password" />
        </Task>
    </Tasks>
    <ExecutionGraph>
      <Task id="1"><Parent id="-1" /></Task>
      <Task id="2"><Parent id="1"  /></Task>
      <OnSuccess>
        <Task id="3"><Parent id="-1" /></Task>
        <Task id="4"><Parent id="3"  /></Task>
      </OnSuccess>
    </ExecutionGraph>
</Workflow>
```

The flowchart event nodes OnWarning and OnError can be used in the same way. You can put DoIf and DoWhile flowchart nodes in event nodes.

These are simple and basic workflows to give an idea on how to make your own workflows. However, if you have multiple systems, applications and automations involved in a workflow, the workflow could be very interesting.

These are simple and basic workflows to give an idea on how to make your own workflows. However, if you have multiple systems, applications and automations involved in a workflow, the workflow could be very interesting.

# How to create a custom task

To create a custom task MyTask for example you will need to proceed as follows:

1. Create a class library project in Visual Studio and name it Wexflow.Tasks.MyTask.
2. Reference the assemblies Wexflow.Core.dll and log4net.dll.These assemblies are located in the installation folder of Wexflow C:\Program Files\Wexflow\.
3. Create a public class MyTask that implements the abstract class Wexflow.Core.Task.

Wexflow.Tasks.MyTask code should look like as follows:

```csharp
public class MyTask : Wexflow.Core.Task
{
    public MyTask(XElement xe, Workflow wf) : base(xe, wf)
    {
        // Task settings goes here
    }

    public override void Run()
    {
        try
        {
            // Task logic goes here

            return new TaskStatus(Status.Success, false);
        }
        catch (ThreadAbortException)
        {
            throw;
        }
    }
}
```

Each task returns a TaskStatus object when it finishes performing its job. TaskStatus is composed of the following elements:

```csharp
public Status Status { get; set; }
public bool Condition { get; set; }
```

The Status can be one of the followings:

```csharp
public enum Status
{
  Success,
  Warning,
  Error
}
```

For example, if a task performs an opetation on a collection of files and if this operation succeeds for all the files then its Status should be Success. Otherwise if this operation succeeds for some files and fails for others then its Status should be Warning. Otherwise if this operation fails for all the files then its Status should be Error.

The Condition property is designed for flowchart tasks. In addition to the Status of the task, a flowchart task returns either true or false after performing its operation.

The Condition property should always be set to false for sequential tasks.

To retrieve settings, you can use the following methods:

```csharp
string settingValue = this.GetSetting("settingName");
string settingValue = this.GetSetting("settingName", defaultValue);
string[] settingValues = this.GetSettings("settingName");
```

To select the files loaded by the running instance of a workflow through the selectFiles settings option, you can do it as follows:

```csharp
FileInf[] files = this.SelectFiles();
```

To select entities loaded by the running instance of a workflow through the selectEntities settings option, you can do it as follows:

```csharp
Entity[] entities = this.SelectEntities();
```

The Entity class could be very useful when working with custom tasks that manipulate objects from a database or Web Services for example.

To load a file within a task, you can do it as follows:

```csharp
this.Files.Add(new FileInf(path, this.Id));
```

To load an entity within a task, you can do it as follows:

```csharp
this.Entities.Add(myEntity);
```

Finally if you finished coding your custom task, compile the class library project and copy the assembly Wexflow.Tasks.MyTask.dll in C:\Program Files\Wexflow\. Your custom task is then ready to be used as follows:

```xml
<Task id="$int" name="MyTask" description="My task description" enabled="true">
    <Setting name="settingName" value="settingValue" />
</Task>
```

That's it. That's all the things you need to know to start coding your own custom tasks.

# How to debug Wexflow

How to debug Wexflow
To debug Wexflow, proceed as follows:

- Install Microsoft .NET Framework 4.0 or higher.
- Install Microsoft Sync Framework 2.1 SDK. You can download it from [here](https://www.microsoft.com/en-us/download/details.aspx?id=23217).
- Install Visual Studio 2010 or higher.
- Copy the folders "Wexflow" and "WexflowTesting" in C:\. You can download them from [here](https://aelassas.github.io/wexflow/images/Wexflow.zip).

## Wexflow Windows Service

To debug Wexflow Windows Service (Wexflow.Clients.WindowsService project), add "debug" command line argument in "Propject settings > Debug > Startup options":

![alt tag](https://aelassas.github.io/wexflow/images/wwsd.png)

## Wexflow Manager

To debug Wexflow Manager (Wexflow.Clients.Manager project), add "debug" command line argument in "Propject settings > Debug > Startup options":

![alt tag](https://aelassas.github.io/wexflow/images/wmd.png)

# Bugs and features

If you found any issues with Wexflow, please submit a bug report at the [Issue Tracker](https://github.com/aelassas/Wexflow/issues). Please include the following:

- The version of Wexflow you are using.
- How to reproduce the issue (a step-by-step description).
- Expected result.

If you'd like to add a feature request please add some details how it is supposed to work.

# Libraries used by Wexflow

Here is the list of the libraries used by Wexflow:

- [FluentFTP](https://github.com/hgupta9/FluentFTP): An FTP client supporting FTP and FTPS(exmplicit/implicit) written in C# and under MIT license.
- [SSH.NET](https://github.com/sshnet/SSH.NET): An SSH library for .NET written in C# and under MIT license.
- [SharpZipLib](https://github.com/icsharpcode/SharpZipLib): A Zip, GZip, Tar and BZip2 library written in C# and under MIT license.
- [Saxon-HE](http://saxon.sourceforge.net/): An XSLT and XQuery Processor that provides implementations of XSLT (2.0), XQuery (1.0, 3.0, and 3.1), and XPath (2.0, 3.0, and 3.1) at the basic level of conformance defined by W3C. It's an open source library available under the Mozilla Public License version 1.0.
- [log4net](https://logging.apache.org/log4net/): A port of the famous Apache log4j framework to the Microsoft .NET runtime. It's under the Apache license version 2.0.
- [TweetSharp](https://www.nuget.org/packages/TweetSharp/): A fast and clean wrapper around the Twitter AP written in C#.
- [Microsoft Sync Framework 2.1](https://msdn.microsoft.com/en-us/library/mt490616.aspx): A data synchronization platform that allows to synchronize data across multiple data stores.
- [Json.NET](https://github.com/JamesNK/Newtonsoft.Json): A high-performance JSON framework for .NET written in C# and under MIT license.
- [Hammock](https://www.nuget.org/packages/Hammock): an HTTP library that simplifies consuming and wrapping RESTful services.
- [Mono.Security](https://www.nuget.org/packages/Mono.Security/): A library that provides the missing pieces to .NET security.
- [Oracle Data Access Components (ODAC)](http://www.oracle.com/technetwork/topics/dotnet/utilsoft-086879.html): Oracle database client for .NET.
- [MySQL Connector/Net](https://dev.mysql.com/downloads/connector/net/1.0.html): A fully-managed ADO.NET driver for MySQL.
- [System.Data.SQLite](https://system.data.sqlite.org/): An ADO.NET provider for SQLite.
- [Npgsql](http://www.npgsql.org/): An open source ADO.NET Data Provider for PostgreSQL written in C# and under the PostgreSQL License, a liberal OSI-approved open source license.
- [.NET Data Provider for Teradata](http://downloads.teradata.com/download/connectivity/net-data-provider-for-teradata): An ADO.NET provider for Teradata.
- [Eto.Forms](https://github.com/picoe/Eto): A cross platform GUI framework for desktop and mobile applications in .NET.

# History

- 5 Jan 2017:
  - [Released version 1.0](https://github.com/aelassas/Wexflow/releases/tag/v1.0).
- 9 Jan 2017: 
  - [Released version 1.0.1](https://github.com/aelassas/Wexflow/releases/tag/v1.0.1).
  - Created Wexflow Windows Service.
  - Created Tar, Tgz and Sql tasks.
  - Updated Wexflow Manager.
  - Fixed some bugs.
- 16 Jan 2017:
  - [Released version 1.0.2](https://github.com/aelassas/Wexflow/releases/tag/v1.0.2).
  - Created Wmi and ImagesTransformer tasks.
  - Updated Wexflow Manager.
- 23 Jan 2017:
  - [Released version 1.0.3](https://github.com/aelassas/Wexflow/releases/tag/v1.0.3).
  - Created Http, Sync, FilesRenamer, FilesExist and Wait tasks.
  - Created file tags functionality.
  - Added list, download and delete commands to Ftp task (FTP/FTPS(explicit/implicit)/SFTP).
  - Added retryCount and retryTimeout setting options to Ftp task.
  - Updated Wexflow manager.
  - Updated Wexflow engine.
  - Fixed some bugs.
  - Updated setup file.
  - Updated README.md.
- 26 Jan 2017:
  - [Released version 1.0.4](https://github.com/aelassas/Wexflow/releases/tag/v1.0.4).
  - Created XSD validation of worklfow files before loading them.
  - Created tasks execution graph.
  - Created flowchart workflows (DoIf and DoWhile).
  - Created workflow events (OnSuccess, OnWarning and OnError).
  - Created FileExists flowchart task.
  - Updated setup.
- 30 Jan 2017:
  - [Released version 1.0.5](https://github.com/aelassas/Wexflow/releases/tag/v1.0.5).
  - Created Wexflow Web Manager: a lightweight JavaScript API (~6Kb) for managing workflows.
  - Created Wexflow Manager GUI for Linux.
  - Updated Wexflow Manager for Windows.
  - Updated setup for Windows.
  - Created a setup for Linux.
- 06 Feb 2017:
  - [Released version 1.0.6](https://github.com/aelassas/Wexflow/releases/tag/v1.0.6).
  - Created Wexflow Android Manager: an Android application for managing workflows.
  - Updated Wexflow Web Manager.
  - Updated Wexflow Manager (Windows and Linux).
  - Updated Wexflow Engine.
  

# More informations
More informations about Wexflow can be found on [CodeProject](https://www.codeproject.com/Articles/1164009/Wexflow-Open-source-workflow-engine-in-Csharp).

# License
Wexflow is licensed under the [MIT License](https://github.com/aelassas/Wexflow/blob/master/LICENSE.txt).
