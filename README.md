
![alt text](https://aelassas.github.io/wf_logo.png "Wexflow")

# Wexflow
Open source workflow engine written in pure C#.

Wexflow provides the following tasks:

- **File system tasks:** These tasks allow to create, copy, move or delete files and directories on a file system.
Compression tasks: These tasks allow to create a zip, a tar or a tar.gz from a collection of files.
- **MD5 task:** This task allows to generate MD5 sums of a collection of files.
- **FTP task:** This task allows to send files over FTP, FTPS or SFTP.
- **XML tasks:** These tasks allow to work with XML data. XSLT can be used along with XPath to generate XML documents. XSLT 1.0 and XSLT 2.0 are supported.
- **CSV tasks:** These tasks allow to work with CSV data. XML can be used along with XSLT to validate, compare and merge CSV data. The results of this can then be stored in CSV or XML format.
- **SQL task:** This task allows to execute SQL scripts. This task supports Microsoft Sql Server, Microsoft Access, Oracle, MySql, SQLite, PostGreSql and Teradata. This tasks can be used for bulk insert, for database updates, for database cleanup, for rebuilding indexes, for reorganizing indexes, for shrinking databases, for updating statistics, for transfering database data and so on.
- **WMI task:** This task allows to execute WMI queries. The results can be stored in XML format.
- **Image task:** This task allows to convert images to the following formats: Bmp, Emf, Exif, Gif, Icon, Jpeg, Png, Tiff and Wmf.
- **Audio and video tasks:** These tasks allow to convert, cut or edit audio and video files through FFMEG or VLC. These tasks can also be used to perform custom operations such as generating images and thumbnails from video files.
- **Email task:** This task allows to send a collection of emails.
- **Twitter task:** This task allows to send a collection of tweets.
- **Process task:** This task allows to launch any process on the computer.
- **Script tasks:** These tasks allows execute custom tasks written in C# or VB.

# How to use Wexflow
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
        - The enable option which allows to enable or disable a workflow.
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
-->
<Workflow id="$int" name="$string" description="$string">
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
</Workflow>
```

The name option of a Task must be one of the followings:
- **CsvToXml :** This task transforms a CSV file to an XML file. The format of the output XML file is described in the documentation of the task.
- **FilesCopier :** This task copies a collection of files to a destination folder. 
- **FilesLoader :** This task loads a collection of files located in folders or through the file option. 
- **FilesMover :** This task moves a collection of files to a destination folder.
- **FilesRemover :** This task deletes a collection of files.
- **FilesSender :** This task sends a collection of files to an FTP, FTPS or SFTP server.
- **ListEntities :** This task lists all the entities loaded by the workflow tasks in the logs. This task is useful for resolving issues.
- **ListFiles :** This task lists all the files loaded by the workflow tasks in the logs. This task is useful for resolving issues.
- **MailsSender :** This task sends a collection of emails from XML files. The format of the input XML files is described in the documentation of the task.
- **Md5 :** This task generates MD5 sums of a collection of files and writes the results in an XML file. The format of the output XML file is described in the documentation of the task.
- **Mkdir :** This task creates a collection of folders.
- **ProcessLauncher :** This task launches a process. If the process generates a file as output It is possible to pass a collection of files to the task so that for each file an output file will be generated through the process. Read the documentation of the task for further informations.
- **Rmdir :** This task deletes a collection of folders.
- **Touch :** This task creates a collection of empty files.
- **Twitter :** This task sends a collection of tweets from XML files. The format of input XML files is described in the documentation of the task.
- **XmlToCsv :** This task transforms an XML file to a CSV file. The format of the input XML file is described in the documentation of the task.
- **Xslt :** This task transforms an XML file. It is possible to use XSLT 1.0 processor or XSLT 2.0 processor.
- **Zip :** This task creates a zip archive from a collection of files.
- **Tar :** This task creates a tar archive from a collection of files.
- **Tgz :** This task creates a tar.gz archive from a collection of files.
- **Sql:** This task executes a colletion of SQL script files or a simple SQL script through sql settings option. It supports Microsoft Sql Server, Microsoft Access, Oracle, MySql, SQLite, PostGreSql and Teradata.
- **Wmi:** This task executes a WMI query and outputs the results in an XML file. The format of the output XML file is described in the documentation of the task.
- **ImagesTransformer:** This task transforms a collection of image files to a specified format. The output format can be one of the followings: Bmp, Emf, Exif, Gif, Icon, Jpeg, Png, Tiff or Wmf.

If a new workflow is created in C:\Wexflow\Workflows\ or if an existing workflow is deleted or modified, you have to restart Wexflow Windows Service so that these modifications take effect.

# Workflow samples

Below a workflow that waits for files to arrive in C:\WexflowTesting\Watchfolder1\ and C:\WexflowTesting\Watchfolder2\ then will send them to an FTP server then will move them to C:\WexflowTesting\Sent\ folder. This workflow starts every 2 minutes.

```xml
<Workflow id="6" name="Workflow_FilesSender" description="Workflow_FilesSender">
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
        <Task id="2" name="FilesSender" description="Sending files" enabled="true">
            <Setting name="protocol" value="ftp" /> <!-- ftp|ftps|sftp -->
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

Below a workflow that transcodes the WAV files located in C:\WexflowTesting\WAV\ to MP3 format through FFMPEG and put the transcoded files in C:\WexflowTesting\MP3\.

```xml
<Workflow id="12" name="Workflow_ffmpeg" description="Workflow_ffmpeg">
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

Below a workflow that waits for WAV files to arrive in C:\WexflowTesting\WAV\ then transcodes them to MP3 files through VLC then sends the MP3 files to an FTP server then moves the WAV files to C:\WexflowTesting\WAV_processed\. This workflow starts every 2 minutes.

```xml
<Workflow id="13" name="Workflow_vlc" description="Workflow_vlc">
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
        <Task id="3" name="FilesSender" description="Sending MP3 files" enabled="true">
            <Setting name="protocol" value="ftp" />
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

# How to create a custom task

To create a custom task MyTask for example you will need to proceed as follows:

1. Create a class library project in Visual Studio and name it Wexflow.Tasks.MyTask.
2. Reference the assemblies Wexflow.Core.dll and log4net.dll.These assemblies are located in the installation folder of Wexflow C:\Program Files\Wexflow\.
3. Create a public class MyTask that implements the abstract class Wexflow.Core.Task.

Wexflow.Tasks.MyTask code should look like as follows:

```cs
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
        }
        catch (ThreadAbortException)
        {
            throw;
        }
    }
}
```

To retrieve settings, you can use the following methods:

```cs
string settingValue = this.GetSetting("settingName");
string settingValue = this.GetSetting("settingName", defaultValue);
string[] settingValues = this.GetSettings("settingName");
```

To select files of other tasks through the selectFiles settings option, you can do it as follows:

```cs
FileInf[] files = this.SelectFiles();
```

To select entities of other tasks through the selectEntities settings option, you can do it as follows:

```cs
Entity[] entities = this.SelectEntities();
```

The Entity class could be very useful when working with custom tasks that manipulate objects from a database and Web Services for example.

To load a file in the task, you can do it as follows:

```cs
this.Files.Add(new FileInf(path, this.Id));
```

To load an entity in the task, you can do it as follows:

```cs
this.Entities.Add(myEntity);
```

Finally if you finished coding your custom task, compile the class library project and copy the assembly Wexflow.Tasks.MyTask.dll in C:\Program Files\Wexflow\. Your custom task is then ready to be used as follows:

```xml
<Task id="$int" name="MyTask" description="My task description" enabled="true">
    <Setting name="settingName" value="settingValue" />
</Task>
```

That's it. That's all the things you need to know to start coding your own custom tasks.

# More informations
More informations about Wexflow can be found on [CodeProject](https://www.codeproject.com/Articles/1164009/Wexflow-Open-source-workflow-engine-in-Csharp).

# License
MIT.
