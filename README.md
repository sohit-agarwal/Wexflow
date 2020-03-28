[![Downloads](https://img.shields.io/github/downloads/aelassas/Wexflow/total.svg)](https://wexflow.github.io/stats) [![NuGet](https://buildstats.info/nuget/Wexflow)](https://www.nuget.org/packages/Wexflow) [![Build Status](https://aelassas.visualstudio.com/Wexflow/_apis/build/status/aelassas.Wexflow?branchName=master)](https://aelassas.visualstudio.com/Wexflow/_build/latest?definitionId=1&branchName=master)

[![Build History](https://buildstats.info/azurepipelines/chart/aelassas/Wexflow/1)](https://aelassas.visualstudio.com/Wexflow/_build?definitionId=1&_a=summary)

You can download the latest release from [here](https://github.com/aelassas/Wexflow/releases/latest).

You can find the documentation [here](https://github.com/aelassas/Wexflow/wiki).

# Wexflow

Wexflow is a free, open-source, high-performance, extensible, modular and cross-platform workflow engine and automation platform. The goal of Wexflow is to automate recurring tasks. With the help of Wexflow, building automation and workflow processes become easy. Wexflow also helps in making the long-running processes straightforward. The communication between systems or applications becomes easy through this powerful workflow engine and automation platform.

Wexflow comes with a cross-platform workflow server and a powerful backend. The workflow server exposes a [RESTful API](https://github.com/aelassas/Wexflow/wiki/RESTful-API) that allows Wexflow to be embeddable anywhere.

Wexflow makes use of [.NET Core](https://www.microsoft.com/net/download), a cross-platform version of .NET for building websites, services, and console apps. Thus, Wexflow provides a cross-platform workflow server and a powerful backend for managing, designing and tracking workflows with ease and flexibility. Wexflow runs on Windows, Linux and macOS.

Wexflow also makes use of [Quartz.NET](https://www.quartz-scheduler.net/) open-source job scheduling system that is used in large scale enterprise systems. Thus, Wexflow offers flexibility in planning jobs such as [cron jobs](https://github.com/aelassas/Wexflow/wiki/Cron-scheduling).

Since workflows are typically long running processes, they will need to be persisted to storage between tasks. There are several persistence providers available. Wexflow provides [LiteDB](http://www.litedb.org/), [MongoDB](https://github.com/aelassas/Wexflow/wiki/MongoDB), [RavenDB](https://github.com/aelassas/Wexflow/wiki/RavenDB), [CosmosDB](https://github.com/aelassas/Wexflow/wiki/CosmosDB), [PostgreSQL](https://github.com/aelassas/Wexflow/wiki/PostgreSQL), [SQL Server](https://github.com/aelassas/Wexflow/wiki/SQL-Server), [MySQL](https://github.com/aelassas/Wexflow/wiki/MySQL) and [SQLite](https://github.com/aelassas/Wexflow/wiki/SQLite) persistence providers which enhance and improve the performance of this automation platform. The user can choose the persistence provider of his choice at the installation.

Wexflow comes with a backend, so you can search and filter among all your workflows, have real-time statistics on your workflows, manage your workflows with ease, design your workflows with ease, and track your workflows with ease:

![Dashboard](https://aelassas.github.io/wexflow/images/wbo-dashboard-4.4-2.png)

Just to give you an idea of what Wexflow does, this is a screenshot from the "Designer" page. Using the "Designer" page, we get a nice visual overview of the dependency graph of the workflow. Each node represents a task which has to be run:

![Designer](https://aelassas.github.io/wexflow/images/wbo-designer-4.4-1.png)

Moreover, the "Designer" page allows to edit workflows through its JSON editor or its XML editor or its WYSIWYG form based editor:

![Designer](https://aelassas.github.io/wexflow/images/wbo-designer-5.3.png)

![Docker](https://aelassas.github.io/wexflow/images/small_h-trans.png)

You can deploy Wexflow using Docker containers on Windows, Linux and macOS distributions. [Here](https://github.com/aelassas/Wexflow/wiki/Docker) is the documentation for creating and building Docker images.

## Why Wexflow?

- [Free and open-source](https://github.com/aelassas/Wexflow/wiki/Free-and-open-source)
- [Easy to install and effortless configuration](https://github.com/aelassas/Wexflow/wiki/Installation)
- [Straightforward and easy to use](https://github.com/aelassas/Wexflow/wiki/Usage)
- [A cross-platform workflow server](https://github.com/aelassas/Wexflow/wiki/Workflow-server)
- [A powerful backend](https://github.com/aelassas/Wexflow/wiki/Usage#backend)
- [An Android app for managing workflows](https://github.com/aelassas/Wexflow/wiki/Usage#android-manager)
- [An iOS app for managing workflows](https://github.com/aelassas/Wexflow/wiki/Usage#ios-manager)
- [Sequential workflows](https://github.com/aelassas/Wexflow/wiki/Samples#sequential-workflows)
- [Flowchart workflows](https://github.com/aelassas/Wexflow/wiki/Samples#flowchart-workflows)
- [Approval workflows](https://github.com/aelassas/Wexflow/wiki/Samples#approval-workflows)
- [100+ built-in tasks](https://github.com/aelassas/Wexflow/wiki/Tasks-documentation)
- [User-driven](https://github.com/aelassas/Wexflow/wiki/User-driven)
- [Cron scheduling](https://github.com/aelassas/Wexflow/wiki/Cron-scheduling)
- [LiteDB, MongoDB, RavenDB and CosmosDB support](https://github.com/aelassas/Wexflow/wiki/Databases)
- [PostgreSQL, SQL Server, MySQL and SQLite support](https://github.com/aelassas/Wexflow/wiki/Databases)
- [Extensive logging and incident reporting](https://github.com/aelassas/Wexflow/wiki/Logging)
- [Real-time stats](https://github.com/aelassas/Wexflow/wiki/Usage#dashboard)
- [RESTful API](https://github.com/aelassas/Wexflow/wiki/RESTful-API)
- [Extensible](https://github.com/aelassas/Wexflow/wiki/Extensible)	

Discover more [features](https://github.com/aelassas/Wexflow/wiki).

## Examples

- Orchestration engine
- Form submission approval process
- Batch recording live video feeds
- Batch transcoding audio and video files
- Batch uploading videos and their metadata to YouTube SFTP dropbox
- Automatically upload videos to YouTube
- Automatically upload videos to Vimeo
- Automatically upload images and videos to Instagram
- Automatically send tweets
- Automatically send posts and links to Reddit
- Automatically send messages to Slack channels
- Automatically send SMS messages
- Batch encrypting and decrypting large files
- Batch converting, resizing and cropping images
- Creating and sending reports and invoices by email
- Connecting systems and applications via watch folders
- Batch downloading files over FTP/FTPS/SFTP/HTTP/HTTPS/Torrent
- Batch uploading files over FTP/FTPS/SFTP
- Database administration and maintenance
- Synchronizing the content of local or remote directories
- [Optimizing PDF files](https://blogs.datalogics.com/2018/11/26/wexflow-automating-datalogics-pdf-tools/)

Check out the available [built-in tasks](https://github.com/aelassas/Wexflow/wiki/Tasks-documentation) for more examples.

## JetBrains

This project is supported by [JetBrains](https://www.jetbrains.com/?from=Wexflow).

## Continuous Integration

|  Server | Platform | Status |
----------|--------|-------|
|Azure Pipelines (.NET and .NET Core)| Windows |[![Build Status](https://aelassas.visualstudio.com/Wexflow/_apis/build/status/aelassas.Wexflow?branchName=master)](https://aelassas.visualstudio.com/Wexflow/_build/latest?definitionId=1&branchName=master)|
|AppVeyor (.NET and .NET Core)| Windows |[![Build Status](https://ci.appveyor.com/api/projects/status/github/aelassas/Wexflow?svg=true)](https://ci.appveyor.com/project/aelassas/wexflow)|
|GitHub Actions (.NET Core)| Linux |[![Actions Status](https://github.com/aelassas/Wexflow/workflows/.NET%20Core/badge.svg)](https://github.com/aelassas/Wexflow/actions)|
|Bitrise (Android)|Linux| [![Build Status](https://app.bitrise.io/app/0fb832132f6afa6d/status.svg?token=j49g0Gx7rNWkl4s41xM_kA)](https://app.bitrise.io/app/0fb832132f6afa6d)|
|CircleCI (Android)|Linux | [![CircleCI](https://circleci.com/gh/aelassas/Wexflow.svg?style=shield)](https://circleci.com/gh/aelassas/Wexflow)|
|Bitrise (iOS)|macOS | [![Build Status](https://app.bitrise.io/app/f8006552bdd4ee80/status.svg?token=Yd_71TrG-cqFvEC1oV5teQ)](https://app.bitrise.io/app/f8006552bdd4ee80)|
|CircleCI (iOS)|macOS | [![CircleCI](https://circleci.com/gh/aelassas/Wexflow.svg?style=shield)](https://circleci.com/gh/aelassas/Wexflow)|
|FOSSA| Linux | [![FOSSA Status](https://app.fossa.com/api/projects/git%2Bgithub.com%2Faelassas%2FWexflow.svg?type=shield)](https://app.fossa.com/projects/git%2Bgithub.com%2Faelassas%2FWexflow?ref=badge_shield)|
