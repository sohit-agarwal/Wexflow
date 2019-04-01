<!--
[![Logo](https://aelassas.github.io/wexflow/images/wexflow.png "Logo")](https://wexflow.github.io/)
-->
<!--
# <a href="https://wexflow.github.io/"><img alt="Wexflow" src="https://aelassas.github.io/wexflow/images/wexflow-3.6-2.jpg" width="100%"></a>
-->

# [![Logo](https://aelassas.github.io/wexflow/images/wexflow-logo-2.png)](https://wexflow.github.io/)

[![Release](http://img.shields.io/badge/release-v3.8-brightgreen.svg)](https://github.com/aelassas/Wexflow/releases/latest) [![Nuget](http://img.shields.io/badge/nuget-v3.8.0-blue.svg)](https://www.nuget.org/packages/Wexflow) [![GitHub Downloads](https://img.shields.io/github/downloads/aelassas/Wexflow/total.svg)](https://www.somsubhra.com/github-release-stats/?username=aelassas&repository=Wexflow) [![Gitter](https://badges.gitter.im/Join%20Chat.svg)](https://gitter.im/Wexflow/Lobby) [![Release](http://img.shields.io/badge/prs-welcome-brightgreen.svg)](https://github.com/aelassas/Wexflow#contribute) [![License](http://img.shields.io/badge/license-MIT-blue.svg)](https://github.com/aelassas/Wexflow/blob/master/LICENSE.txt) [![Tweet](https://img.shields.io/twitter/url/http/shields.io.svg?style=social)](https://twitter.com/intent/tweet?text=Wexflow%20-%20Open%20source%20workflow%20engine&url=https://wexflow.github.io&via=aelassas_dev&hashtags=wexflow,workflow) [![Twitter URL](https://img.shields.io/twitter/url/https/twitter.com/fold_left.svg?style=social&label=Follow)](https://twitter.com/aelassas_dev)

<!--
[![NuGet Downloads](https://img.shields.io/nuget/dt/Wexflow.svg)](https://www.nuget.org/packages/Wexflow/)
-->

<!--
[![forthebadge](http://aelassas.github.io/wexflow/images/made-with-c-sharp.svg)](http://forthebadge.com)
[![forthebadge](http://aelassas.github.io/wexflow/images/built-with-love.svg)](http://forthebadge.com)
-->

Wexflow is a high-performance, extensible, modular and cross-platform workflow engine. The goal of Wexflow is to automate recurring tasks without user intervention. With the help of Wexflow, building automation and workflow processes become easy. Wexflow also helps in making the long-running processes straightforward. 

Wexflow aims to make automations, workflow processes, long-running processes and interactions between systems, applications and folks easy, straightforward and clean. The communication between systems or applications becomes easy through this powerful workflow engine.

Wexflow makes use of [.NET Core](https://www.microsoft.com/net/download), a cross-platform version of .NET for building websites, services, and console apps. Thus, Wexflow provides a cross-platform workflow server and a cross-platform backend for managing, designing and tracking workflows with ease and flexibility. Wexflow server and its backend run on Windows, Linux and macOS.

Wexflow also makes use of [Quartz.NET](https://www.quartz-scheduler.net/) open source job scheduling system that is used in large scale enterprise systems. Thus, Wexflow offers flexibility in planning workflow jobs such as [cron workflows](https://github.com/aelassas/Wexflow/wiki/Cron-scheduling).

Furthermore, Wexflow makes use of [LiteDB](http://www.litedb.org/) NoSQL Document Store database in its server and backend which enhance and improve the performance of this workflow engine.

# Features

Wexflow provides the following features:

- [x] [Free and open-source](https://github.com/aelassas/Wexflow/wiki/Free-and-open-source).
- [x] [Easy to install and effortless configuration](https://github.com/aelassas/Wexflow/wiki/Installation).
- [x] [Straightforward and easy to use](https://github.com/aelassas/Wexflow/wiki/Usage).
- [x] [Modular](https://github.com/aelassas/Wexflow/wiki/Modular).
- [x] [Well documented](https://github.com/aelassas/Wexflow/wiki/).
- [x] [User driven](https://github.com/aelassas/Wexflow/wiki/User-driven).
- [x] [A cross-platform workflow server](https://github.com/aelassas/Wexflow/wiki/Workflow-server).
- [x] [A cross-platform backend](https://github.com/aelassas/Wexflow/wiki/Usage#backend).
- [x] [A cross-platform application for managing workflows](https://github.com/aelassas/Wexflow/wiki/Usage#manager).
- [x] [A cross-platform application for designing workflows](https://github.com/aelassas/Wexflow/wiki/Usage#designer).
- [x] [An Android application for managing workflows](https://github.com/aelassas/Wexflow/wiki/Usage#android-manager).
- [x] [User management](https://github.com/aelassas/Wexflow/wiki/Usage#users).
- [x] [Sequential workflows](https://github.com/aelassas/Wexflow/wiki/Samples#sequential-workflows).
- [x] [Flowchart workflows](https://github.com/aelassas/Wexflow/wiki/Samples#flowchart-workflows).
- [x] [Workflow events](https://github.com/aelassas/Wexflow/wiki/Samples#workflow-events).
- [x] [Cron scheduling](https://github.com/aelassas/Wexflow/wiki/Cron-scheduling).
- [x] [Extensive logging and incident reporting](https://github.com/aelassas/Wexflow/wiki/Logging).
- [x] [Real-time statistics on workflows](https://github.com/aelassas/Wexflow/wiki/Usage#dashboard).
- [x] [92 built-in tasks](https://github.com/aelassas/Wexflow/wiki/Tasks-documentation).
- [x] [RESTful API](https://github.com/aelassas/Wexflow/wiki/RESTful-API).
- [x] [Hot reloading](https://github.com/aelassas/Wexflow/wiki/Hot-reloading).
- [x] [Automation](https://github.com/aelassas/Wexflow/wiki/Automation).
- [x] [Monitoring](https://github.com/aelassas/Wexflow/wiki/Monitoring).
- [x] [Extensible](https://github.com/aelassas/Wexflow/wiki/Extensible).

Wexflow comes with a backend too, so you can search and filter among all your workflows, have real-time statistics on your workflows, manage your workflows with ease, design your workflows with ease, and track your workflows with ease:

![Dashboard](https://aelassas.github.io/wexflow/images/wbo-dashboard-3.2.png)

Just to give you an idea of what Wexflow does, this is a screenshot from the "Designer" page. Using the "Designer" page, we get a nice visual overview of the dependency graph of the workflow. Each node represents a task which has to be run:

![Designer](https://aelassas.github.io/wexflow/images/wbo-designer-3.2.png)

Discover more about the features in [details](https://github.com/aelassas/Wexflow/wiki).

# Examples

Here are some examples of using Wexflow:

- Orchestration engine.
- Batch recording live video feeds.
- Batch transcoding audio and video files.
- Batch uploading videos and their metadata to YouTube SFTP dropbox.
- Batch encrypting and decrypting large files.
- Batch converting, resizing and cropping images.
- Creating and sending reports and invoices by email.
- Connecting systems and applications via watch folders.
- Batch downloading files over FTP/FTPS/SFTP/HTTP/HTTPS/Torrent.
- Batch uploading files over FTP/FTPS/SFTP.
- Database administration and maintenance.
- Synchronizing the content of local or remote directories.
- Batch sending tweets.
- etc.

# Benefits

Here are the benefits of using Wexflow:

- [x] Gain time by automating repetitive tasks.
- [x] Save money by avoiding re-work and corrections.
- [x] Reduce human error.
- [x] Become more efficient and effective in completing your tasks.
- [x] Become more productive in what you do.
- [x] Become consistent in what you do.

# Continuous integration

|  Server | Platform | Status |
----------|--------|----------|
| [AppVeyor](https://www.appveyor.com/) (.NET)| Windows |[![Build Status](https://ci.appveyor.com/api/projects/status/github/aelassas/Wexflow?svg=true)](https://ci.appveyor.com/project/aelassas/wexflow)|
|[Bitrise](https://www.bitrise.io/) (Android)| Linux|[![Build Status](https://app.bitrise.io/app/dba3b2d20b9fa08f/status.svg?token=y-XB39RvGk5hta1p-YS2NA&branch=master)](https://app.bitrise.io/app/dba3b2d20b9fa08f)|
|[codecov.io](https://codecov.io)|Windows|[![codecov](https://codecov.io/gh/aelassas/Wexflow/branch/master/graph/badge.svg)](https://codecov.io/gh/aelassas/Wexflow)|

# Get started

- [Features](https://github.com/aelassas/Wexflow/wiki)
- [Installation guide](https://github.com/aelassas/Wexflow/wiki/Installation)
- [Quick start](https://github.com/aelassas/Wexflow/wiki/Usage)
- [Workflow samples](https://github.com/aelassas/Wexflow/wiki/Samples)
- [Tasks documentation](https://github.com/aelassas/Wexflow/wiki/Tasks-documentation)

# Download

Download the latest release [here](https://github.com/aelassas/Wexflow/releases/latest).

# Awards

- [Best C# Article of January 2017 : Second Prize on CodeProject](https://www.codeproject.com/Articles/1164009/Wexflow-Open-source-workflow-engine-in-Csharp).

# Contribute

Contributions are very welcome!

To contribute to this project, proceed as follows:
- Read the documentation on how to [debug](https://github.com/aelassas/Wexflow/wiki/Debug) Wexflow.
- [Fork](https://guides.github.com/activities/forking/) this repository.
- Clone your fork.
- Branch.
- Make and push your changes.
- Create a [pull request](https://help.github.com/articles/creating-a-pull-request/).
- After your pull request has been reviewed, it can be merged into the repository.
- To run unit tests, follow these [guidelines](https://github.com/aelassas/Wexflow/wiki/How-to-run-unit-tests%3F).

# Bugs and features
  
 If you found any issues with Wexflow, please submit a bug report at the [Issue Tracker](https://github.com/aelassas/Wexflow/issues). Please include the following:
 
  - The version of Wexflow you are using.
  - How to reproduce the issue (a step-by-step description).
  - Expected result.
 
If you'd like to add a feature request please add some details how it is supposed to work.

# Changelog

The changelog is available in the [release history](https://github.com/aelassas/Wexflow/wiki/History).

# License

Wexflow is licensed under the [MIT License](https://github.com/aelassas/Wexflow/blob/master/LICENSE.txt). 

Wexflow contains other libraries with their individual licenses. More details about these licenses can be found in the [wiki](https://github.com/aelassas/Wexflow/wiki/License).

# Developers
- [Akram El Assas](https://github.com/aelassas) (Project founder and maintainer)
- [Hans Meyer](https://github.com/HaMster21) 
- [Jan Borup Coyle](https://github.com/janborup) 
- [Alex Higgins](https://github.com/alexhiggins732)
- [Igor Quirino](https://github.com/iquirino)

# Credits

Thanks to [JetBrains](https://www.jetbrains.com) for the free open source license of Resharper Ultimate.

Improved and optimized using:

<a href="https://www.jetbrains.com/resharper/"><img src="https://aelassas.github.io/wexflow/images/logo_resharper.gif" alt="Resharper" width="100" /></a>
