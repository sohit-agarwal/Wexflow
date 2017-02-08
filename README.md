
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

# Getting started

Browse [Wexflow wiki](https://github.com/aelassas/Wexflow/wiki) to get started.

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

# More informations
More informations about Wexflow can be found on [CodeProject](https://www.codeproject.com/Articles/1164009/Wexflow-Open-source-workflow-engine-in-Csharp).

# License
Wexflow is licensed under the [MIT License](https://github.com/aelassas/Wexflow/blob/master/LICENSE.txt).
