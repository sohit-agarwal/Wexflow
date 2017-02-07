
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

Wexflow is an open source workflow engine. Wexflow aims to make automations, workflow processes, long-running processes and interactions between systems, applications and folks easy, straitforward and clean.

# Get started

Browse [Wexflow wiki](https://github.com/aelassas/Wexflow/wiki/Wexflow-wiki) to get started.

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
