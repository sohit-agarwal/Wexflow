#define MyAppName "Wexflow"
#define MyAppVersion "5.4"
#define MyAppPublisher "Akram El Assas"
#define MyAppPublisherURL "https://wexflow.github.io/"
#define MyAppExeName "Wexflow.Clients.Manager.exe"

[Setup]
;SignTool=signtool
PrivilegesRequired=admin
; NOTE: The value of AppId uniquely identifies this application.
; Do not use the same AppId value in installers for other applications.
; (To generate a new GUID, click Tools | Generate GUID inside the IDE.)
AppId={{36E7859C-FD7F-47E1-91C6-41B5F522E2F7}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
;AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppPublisherURL}
DefaultDirName={pf}\{#MyAppName}
DisableProgramGroupPage=yes
OutputDir=.
OutputBaseFilename=wexflow-{#MyAppVersion}-windows-x64
SetupIconFile="..\src\dotnet\Wexflow.Clients.Manager\Wexflow.ico"
Compression=lzma
SolidCompression=yes
; "ArchitecturesInstallIn64BitMode=x64" requests that the install be
; done in "64-bit mode" on x64, meaning it should use the native
; 64-bit Program Files directory and the 64-bit view of the registry.
; On all other architectures it will install in "32-bit mode".
ArchitecturesInstallIn64BitMode=x64
LicenseFile="..\LICENSE.txt"

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked
;Name: "quicklaunchicon"; Description: "{cm:CreateQuickLaunchIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked; OnlyBelowVersion: 0,6.1

[Types]
Name: "full"; Description: "Full installation"
Name: "custom"; Description: "Custom installation"; Flags: iscustom

[Components]
Name: "program"; Description: "Program Files"; Types: full custom; Flags: fixed
Name: "samples"; Description: "Workflow samples"; Types: full

[Files]
; Wexflow server
Source: "..\src\dotnet\Wexflow.Server\bin\x64\Release\Wexflow.Server.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\src\dotnet\Wexflow.Server\bin\x64\Release\Wexflow.Server.exe.config"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\src\dotnet\Wexflow.Server\bin\x64\Release\*.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\src\dotnet\Wexflow.Server\bin\x64\Release\swagger-ui\*"; DestDir: "{app}\swagger-ui"; Flags: ignoreversion recursesubdirs
Source: "..\src\dotnet\Wexflow.Core.SQLite\bin\x64\Release\x64\SQLite.Interop.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\libs\chromedriver.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\libs\7z.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\libs\x64\MediaInfo.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\libs\x64\libsass.dll"; DestDir: "{app}"; Flags: ignoreversion

; Wexflow Manager
Source: "..\src\dotnet\Wexflow.Clients.Manager\bin\x64\Release\Wexflow.Clients.Manager.exe"; DestDir: "{app}\Manager"; Flags: ignoreversion
Source: "..\src\dotnet\Wexflow.Clients.Manager\bin\x64\Release\Wexflow.Clients.Manager.exe.config"; DestDir: "{app}\Manager"; Flags: ignoreversion
Source: "..\src\dotnet\Wexflow.Clients.Manager\bin\x64\Release\Wexflow.Core.Service.Client.dll"; DestDir: "{app}\Manager"; Flags: ignoreversion
Source: "..\src\dotnet\Wexflow.Clients.Manager\bin\x64\Release\Newtonsoft.Json.dll"; DestDir: "{app}\Manager"; Flags: ignoreversion
Source: "..\src\dotnet\Wexflow.Clients.Manager\bin\x64\Release\Wexflow.Core.Service.Contracts.dll"; DestDir: "{app}\Manager"; Flags: ignoreversion

; Wexflow.Clients.CommandLine
Source: "..\src\dotnet\Wexflow.Clients.CommandLine\bin\x64\Release\Wexflow.Clients.CommandLine.exe"; DestDir: "{app}\Wexflow.Clients.CommandLine"; Flags: ignoreversion
Source: "..\src\dotnet\Wexflow.Clients.CommandLine\bin\x64\Release\Wexflow.Clients.CommandLine.exe.config"; DestDir: "{app}\Wexflow.Clients.CommandLine"; Flags: ignoreversion
Source: "..\src\dotnet\Wexflow.Clients.CommandLine\bin\x64\Release\*.dll"; DestDir: "{app}\Wexflow.Clients.CommandLine"; Flags: ignoreversion

; Wexflow Backend
Source: "..\src\backend\Wexflow.Backend\index.html"; DestDir: "{app}\Backend"; Flags: ignoreversion
Source: "..\src\backend\Wexflow.Backend\forgot-password.html"; DestDir: "{app}\Backend"; Flags: ignoreversion
Source: "..\src\backend\Wexflow.Backend\dashboard.html"; DestDir: "{app}\Backend"; Flags: ignoreversion
Source: "..\src\backend\Wexflow.Backend\manager.html"; DestDir: "{app}\Backend"; Flags: ignoreversion
Source: "..\src\backend\Wexflow.Backend\designer.html"; DestDir: "{app}\Backend"; Flags: ignoreversion
Source: "..\src\backend\Wexflow.Backend\editor.html"; DestDir: "{app}\Backend"; Flags: ignoreversion
Source: "..\src\backend\Wexflow.Backend\approval.html"; DestDir: "{app}\Backend"; Flags: ignoreversion
Source: "..\src\backend\Wexflow.Backend\history.html"; DestDir: "{app}\Backend"; Flags: ignoreversion
Source: "..\src\backend\Wexflow.Backend\users.html"; DestDir: "{app}\Backend"; Flags: ignoreversion
Source: "..\src\backend\Wexflow.Backend\profiles.html"; DestDir: "{app}\Backend"; Flags: ignoreversion

Source: "..\src\backend\Wexflow.Backend\images\*"; DestDir: "{app}\Backend\images"; Flags: ignoreversion
Source: "..\src\backend\Wexflow.Backend\assets\*"; DestDir: "{app}\Backend\assets"; Flags: ignoreversion

Source: "..\src\backend\Wexflow.Backend\css\images\*"; DestDir: "{app}\Backend\css\images"; Flags: ignoreversion
Source: "..\src\backend\Wexflow.Backend\css\login.min.css"; DestDir: "{app}\Backend\css"; Flags: ignoreversion
Source: "..\src\backend\Wexflow.Backend\css\forgot-password.min.css"; DestDir: "{app}\Backend\css"; Flags: ignoreversion
Source: "..\src\backend\Wexflow.Backend\css\dashboard.min.css"; DestDir: "{app}\Backend\css"; Flags: ignoreversion
Source: "..\src\backend\Wexflow.Backend\css\manager.min.css"; DestDir: "{app}\Backend\css"; Flags: ignoreversion
Source: "..\src\backend\Wexflow.Backend\css\designer.min.css"; DestDir: "{app}\Backend\css"; Flags: ignoreversion
Source: "..\src\backend\Wexflow.Backend\css\editor.min.css"; DestDir: "{app}\Backend\css"; Flags: ignoreversion
Source: "..\src\backend\Wexflow.Backend\css\approval.min.css"; DestDir: "{app}\Backend\css"; Flags: ignoreversion
Source: "..\src\backend\Wexflow.Backend\css\history.min.css"; DestDir: "{app}\Backend\css"; Flags: ignoreversion
Source: "..\src\backend\Wexflow.Backend\css\users.min.css"; DestDir: "{app}\Backend\css"; Flags: ignoreversion
Source: "..\src\backend\Wexflow.Backend\css\profiles.min.css"; DestDir: "{app}\Backend\css"; Flags: ignoreversion

Source: "..\src\backend\Wexflow.Backend\js\settings.js"; DestDir: "{app}\Backend\js"; Flags: ignoreversion
Source: "..\src\backend\Wexflow.Backend\js\login.min.js"; DestDir: "{app}\Backend\js"; Flags: ignoreversion
Source: "..\src\backend\Wexflow.Backend\js\forgot-password.min.js"; DestDir: "{app}\Backend\js"; Flags: ignoreversion
Source: "..\src\backend\Wexflow.Backend\js\dashboard.min.js"; DestDir: "{app}\Backend\js"; Flags: ignoreversion
Source: "..\src\backend\Wexflow.Backend\js\manager.min.js"; DestDir: "{app}\Backend\js"; Flags: ignoreversion

Source: "..\src\backend\Wexflow.Backend\js\ace.js"; DestDir: "{app}\Backend\js"; Flags: ignoreversion
Source: "..\src\backend\Wexflow.Backend\js\worker-xml.js"; DestDir: "{app}\Backend\js"; Flags: ignoreversion
Source: "..\src\backend\Wexflow.Backend\js\mode-xml.js"; DestDir: "{app}\Backend\js"; Flags: ignoreversion
Source: "..\src\backend\Wexflow.Backend\js\worker-json.js"; DestDir: "{app}\Backend\js"; Flags: ignoreversion
Source: "..\src\backend\Wexflow.Backend\js\mode-json.js"; DestDir: "{app}\Backend\js"; Flags: ignoreversion
Source: "..\src\backend\Wexflow.Backend\js\ext-searchbox.js"; DestDir: "{app}\Backend\js"; Flags: ignoreversion
Source: "..\src\backend\Wexflow.Backend\js\ext-prompt.js"; DestDir: "{app}\Backend\js"; Flags: ignoreversion
Source: "..\src\backend\Wexflow.Backend\js\ext-keybinding_menu.js"; DestDir: "{app}\Backend\js"; Flags: ignoreversion
Source: "..\src\backend\Wexflow.Backend\js\ext-settings_menu.js"; DestDir: "{app}\Backend\js"; Flags: ignoreversion
Source: "..\src\backend\Wexflow.Backend\js\theme-*.js"; DestDir: "{app}\Backend\js"; Flags: ignoreversion
Source: "..\src\backend\Wexflow.Backend\js\editor.min.js"; DestDir: "{app}\Backend\js"; Flags: ignoreversion

Source: "..\src\backend\Wexflow.Backend\js\designer.min.js"; DestDir: "{app}\Backend\js"; Flags: ignoreversion

Source: "..\src\backend\Wexflow.Backend\js\approval.min.js"; DestDir: "{app}\Backend\js"; Flags: ignoreversion
Source: "..\src\backend\Wexflow.Backend\js\history.min.js"; DestDir: "{app}\Backend\js"; Flags: ignoreversion
Source: "..\src\backend\Wexflow.Backend\js\users.min.js"; DestDir: "{app}\Backend\js"; Flags: ignoreversion
Source: "..\src\backend\Wexflow.Backend\js\profiles.min.js"; DestDir: "{app}\Backend\js"; Flags: ignoreversion

; Wexflow's Documentation
Source: "..\src\dotnet\Wexflow.Core\Workflow.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\dotnet\Wexflow.Tasks.CsvToXml\CsvToXml.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\dotnet\Wexflow.Tasks.FileExists\FileExists.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\dotnet\Wexflow.Tasks.FilesConcat\FilesConcat.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\dotnet\Wexflow.Tasks.FilesCopier\FilesCopier.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\dotnet\Wexflow.Tasks.FilesExist\FilesExist.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\dotnet\Wexflow.Tasks.FilesInfo\FilesInfo.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\dotnet\Wexflow.Tasks.FilesLoader\FilesLoader.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\dotnet\Wexflow.Tasks.FilesMover\FilesMover.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\dotnet\Wexflow.Tasks.FilesRemover\FilesRemover.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\dotnet\Wexflow.Tasks.FilesRenamer\FilesRenamer.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\dotnet\Wexflow.Tasks.Ftp\Ftp.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\dotnet\Wexflow.Tasks.Http\Http.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\dotnet\Wexflow.Tasks.ImagesTransformer\ImagesTransformer.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\dotnet\Wexflow.Tasks.ListEntities\ListEntities.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\dotnet\Wexflow.Tasks.ListFiles\ListFiles.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\dotnet\Wexflow.Tasks.MailsReceiver\MailsReceiver.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\dotnet\Wexflow.Tasks.MailsSender\MailsSender.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\dotnet\Wexflow.Tasks.Md5\Md5.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\dotnet\Wexflow.Tasks.MediaInfo\MediaInfo.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\dotnet\Wexflow.Tasks.Mkdir\Mkdir.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\dotnet\Wexflow.Tasks.Movedir\Movedir.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\dotnet\Wexflow.Tasks.ProcessLauncher\ProcessLauncher.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\dotnet\Wexflow.Tasks.Rmdir\Rmdir.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\dotnet\Wexflow.Tasks.Sha1\Sha1.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\dotnet\Wexflow.Tasks.Sha256\Sha256.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\dotnet\Wexflow.Tasks.Sha512\Sha512.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\dotnet\Wexflow.Tasks.Sql\Sql.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\dotnet\Wexflow.Tasks.Sync\Sync.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\dotnet\Wexflow.Tasks.Tar\Tar.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\dotnet\Wexflow.Tasks.Template\Template.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\dotnet\Wexflow.Tasks.Tgz\Tgz.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\dotnet\Wexflow.Tasks.Touch\Touch.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\dotnet\Wexflow.Tasks.Twitter\Twitter.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\dotnet\Wexflow.Tasks.Wait\Wait.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\dotnet\Wexflow.Tasks.Wmi\Wmi.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\dotnet\Wexflow.Tasks.XmlToCsv\XmlToCsv.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\dotnet\Wexflow.Tasks.Xslt\Xslt.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\dotnet\Wexflow.Tasks.Zip\Zip.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\dotnet\Wexflow.Tasks.Now\Now.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\dotnet\Wexflow.Tasks.Workflow\Workflow.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\dotnet\Wexflow.Tasks.FilesSplitter\FilesSplitter.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\dotnet\Wexflow.Tasks.ProcessKiller\ProcessKiller.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\dotnet\Wexflow.Tasks.Unzip\Unzip.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\dotnet\Wexflow.Tasks.Untar\Untar.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\dotnet\Wexflow.Tasks.Untgz\Untgz.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\dotnet\Wexflow.Tasks.ProcessInfo\ProcessInfo.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\dotnet\Wexflow.Tasks.TextToPdf\TextToPdf.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\dotnet\Wexflow.Tasks.HtmlToPdf\HtmlToPdf.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\dotnet\Wexflow.Tasks.SqlToXml\SqlToXml.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\dotnet\Wexflow.Tasks.SqlToCsv\SqlToCsv.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\dotnet\Wexflow.Tasks.Guid\Guid.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\dotnet\Wexflow.Tasks.FilesEqual\FilesEqual.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\dotnet\Wexflow.Tasks.FilesDiff\FilesDiff.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\dotnet\Wexflow.Tasks.Torrent\Torrent.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\dotnet\Wexflow.Tasks.ImagesResizer\ImagesResizer.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\dotnet\Wexflow.Tasks.ImagesCropper\ImagesCropper.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\dotnet\Wexflow.Tasks.CsvToSql\CsvToSql.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\dotnet\Wexflow.Tasks.ImagesConcat\ImagesConcat.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\dotnet\Wexflow.Tasks.ImagesOverlay\ImagesOverlay.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\dotnet\Wexflow.Tasks.Unrar\Unrar.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\dotnet\Wexflow.Tasks.UnSevenZip\UnSevenZip.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\dotnet\Wexflow.Tasks.FilesEncryptor\FilesEncryptor.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\dotnet\Wexflow.Tasks.FilesDecryptor\FilesDecryptor.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\dotnet\Wexflow.Tasks.TextsEncryptor\TextsEncryptor.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\dotnet\Wexflow.Tasks.TextsDecryptor\TextsDecryptor.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\dotnet\Wexflow.Tasks.DatabaseBackup\DatabaseBackup.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\dotnet\Wexflow.Tasks.DatabaseRestore\DatabaseRestore.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\dotnet\Wexflow.Tasks.IsoCreator\IsoCreator.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\dotnet\Wexflow.Tasks.IsoExtractor\IsoExtractor.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\dotnet\Wexflow.Tasks.SevenZip\SevenZip.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\dotnet\Wexflow.Tasks.TextToSpeech\TextToSpeech.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\dotnet\Wexflow.Tasks.SpeechToText\SpeechToText.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\dotnet\Wexflow.Tasks.FileMatch\FileMatch.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\dotnet\Wexflow.Tasks.Ping\Ping.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\dotnet\Wexflow.Tasks.WebToScreenshot\WebToScreenshot.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\dotnet\Wexflow.Tasks.WebToHtml\WebToHtml.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\dotnet\Wexflow.Tasks.ExecCs\ExecCs.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\dotnet\Wexflow.Tasks.ExecVb\ExecVb.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\dotnet\Wexflow.Tasks.HttpPost\HttpPost.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\dotnet\Wexflow.Tasks.HttpPut\HttpPut.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\dotnet\Wexflow.Tasks.HttpPatch\HttpPatch.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\dotnet\Wexflow.Tasks.HttpDelete\HttpDelete.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\dotnet\Wexflow.Tasks.UglifyJs\UglifyJs.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\dotnet\Wexflow.Tasks.UglifyCss\UglifyCss.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\dotnet\Wexflow.Tasks.UglifyHtml\UglifyHtml.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\dotnet\Wexflow.Tasks.HtmlToText\HtmlToText.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\dotnet\Wexflow.Tasks.HttpGet\HttpGet.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\dotnet\Wexflow.Tasks.ScssToCss\ScssToCss.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\dotnet\Wexflow.Tasks.YamlToJson\YamlToJson.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\dotnet\Wexflow.Tasks.JsonToYaml\JsonToYaml.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\dotnet\Wexflow.Tasks.CsvToJson\CsvToJson.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\dotnet\Wexflow.Tasks.CsvToYaml\CsvToYaml.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\dotnet\Wexflow.Tasks.EnvironmentVariable\EnvironmentVariable.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\dotnet\Wexflow.Tasks.MessageCorrect\MessageCorrect.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\dotnet\Wexflow.Tasks.InstagramUploadImage\InstagramUploadImage.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\dotnet\Wexflow.Tasks.InstagramUploadVideo\InstagramUploadVideo.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\dotnet\Wexflow.Tasks.FolderExists\FolderExists.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\dotnet\Wexflow.Tasks.FileContentMatch\FileContentMatch.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\dotnet\Wexflow.Tasks.Approval\Approval.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\dotnet\Wexflow.Tasks.VimeoListUploads\VimeoListUploads.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\dotnet\Wexflow.Tasks.Vimeo\Vimeo.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\dotnet\Wexflow.Tasks.Slack\Slack.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\dotnet\Wexflow.Tasks.FilesLoaderEx\FilesLoaderEx.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\dotnet\Wexflow.Tasks.FilesJoiner\FilesJoiner.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\dotnet\Wexflow.Tasks.Twilio\Twilio.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion

Source: "..\LICENSE"; DestDir: "{app}\"; Flags: ignoreversion

; Wexflow's configuration
Source: "..\samples\dotnet\Wexflow\*"; DestDir: "C:\Wexflow\"; Components: samples; Flags: ignoreversion recursesubdirs uninsneveruninstall
Source: "..\samples\dotnet\Wexflow\TasksNames.json"; DestDir: "C:\Wexflow\"; Flags: ignoreversion recursesubdirs uninsneveruninstall
Source: "..\samples\dotnet\Wexflow\TasksSettings.json"; DestDir: "C:\Wexflow\"; Flags: ignoreversion recursesubdirs uninsneveruninstall
Source: "..\src\dotnet\Wexflow.Core\Wexflow.xml"; DestDir: "C:\Wexflow\"; Flags: ignoreversion recursesubdirs uninsneveruninstall
Source: "..\src\dotnet\Wexflow.Core\Workflow.xsd"; DestDir: "C:\Wexflow\"; Flags: ignoreversion recursesubdirs uninsneveruninstall
Source: "..\src\dotnet\Wexflow.Core\GlobalVariables.xml"; DestDir: "C:\Wexflow\"; Flags: ignoreversion recursesubdirs uninsneveruninstall
Source: "..\samples\WexflowTesting\*"; DestDir: "C:\WexflowTesting\"; Components: samples; Flags: ignoreversion recursesubdirs uninsneveruninstall

; Wexflow.Scripts.MongoDB
Source: "..\src\dotnet\Wexflow.Scripts.MongoDB\bin\x64\Release\Wexflow.Scripts.MongoDB.exe"; DestDir: "{app}\Wexflow.Scripts.MongoDB"; Flags: ignoreversion recursesubdirs
Source: ".\dotnet\Wexflow.Scripts.MongoDB\Wexflow.Scripts.MongoDB.exe.config"; DestDir: "{app}\Wexflow.Scripts.MongoDB"; Flags: ignoreversion recursesubdirs
Source: "..\src\dotnet\Wexflow.Scripts.MongoDB\bin\x64\Release\*.dll"; DestDir: "{app}\Wexflow.Scripts.MongoDB"; Flags: ignoreversion recursesubdirs
Source: "..\samples\workflows\dotnet\*.xml"; DestDir: "{app}\Wexflow.Scripts.MongoDB\Workflows"; Flags: ignoreversion recursesubdirs
Source: "..\src\dotnet\Wexflow.Server\bin\x64\Release\System.Net.Http.dll"; DestDir: "{app}\Wexflow.Scripts.MongoDB"; Flags: ignoreversion
Source: "..\src\dotnet\Wexflow.Server\bin\x64\Release\System.Buffers.dll"; DestDir: "{app}\Wexflow.Scripts.MongoDB"; Flags: ignoreversion
Source: "..\src\dotnet\Wexflow.Server\bin\x64\Release\System.Runtime.InteropServices.RuntimeInformation.dll"; DestDir: "{app}\Wexflow.Scripts.MongoDB"; Flags: ignoreversion
Source: "..\src\dotnet\Wexflow.Server\bin\x64\Release\System.Runtime.CompilerServices.Unsafe.dll"; DestDir: "{app}\Wexflow.Scripts.MongoDB"; Flags: ignoreversion
Source: "..\src\dotnet\Wexflow.Server\bin\x64\Release\System.Threading.Tasks.Extensions.dll"; DestDir: "{app}\Wexflow.Scripts.MongoDB"; Flags: ignoreversion
Source: "..\src\dotnet\Wexflow.Server\bin\x64\Release\System.Numerics.Vectors.dll"; DestDir: "{app}\Wexflow.Scripts.MongoDB"; Flags: ignoreversion
Source: "..\src\dotnet\Wexflow.Server\bin\x64\Release\System.Memory.dll"; DestDir: "{app}\Wexflow.Scripts.MongoDB"; Flags: ignoreversion
Source: "..\src\dotnet\Wexflow.Server\bin\x64\Release\Microsoft.Bcl.AsyncInterfaces.dll"; DestDir: "{app}\Wexflow.Scripts.MongoDB"; Flags: ignoreversion
Source: "..\src\dotnet\Wexflow.Server\bin\x64\Release\System.Net.Sockets.dll"; DestDir: "{app}\Wexflow.Scripts.MongoDB"; Flags: ignoreversion
Source: "..\src\dotnet\Wexflow.Server\bin\x64\Release\System.Text.Encodings.Web.dll"; DestDir: "{app}\Wexflow.Scripts.MongoDB"; Flags: ignoreversion
Source: "..\src\dotnet\Wexflow.Server\bin\x64\Release\System.Text.Json.dll"; DestDir: "{app}\Wexflow.Scripts.MongoDB"; Flags: ignoreversion
Source: "..\src\dotnet\Wexflow.Server\bin\x64\Release\System.ValueTuple.dll"; DestDir: "{app}\Wexflow.Scripts.MongoDB"; Flags: ignoreversion

; Wexflow.Scripts.RavenDB
Source: "..\src\dotnet\Wexflow.Scripts.RavenDB\bin\x64\Release\Wexflow.Scripts.RavenDB.exe"; DestDir: "{app}\Wexflow.Scripts.RavenDB"; Flags: ignoreversion recursesubdirs
Source: ".\dotnet\Wexflow.Scripts.RavenDB\Wexflow.Scripts.RavenDB.exe.config"; DestDir: "{app}\Wexflow.Scripts.RavenDB"; Flags: ignoreversion recursesubdirs
Source: "..\src\dotnet\Wexflow.Scripts.RavenDB\bin\x64\Release\*.dll"; DestDir: "{app}\Wexflow.Scripts.RavenDB"; Flags: ignoreversion recursesubdirs
Source: "..\src\dotnet\Wexflow.Server\bin\x64\Release\System.Net.Http.dll"; DestDir: "{app}\Wexflow.Scripts.RavenDB"; Flags: ignoreversion
Source: "..\src\dotnet\Wexflow.Server\bin\x64\Release\System.Buffers.dll"; DestDir: "{app}\Wexflow.Scripts.RavenDB"; Flags: ignoreversion
Source: "..\src\dotnet\Wexflow.Server\bin\x64\Release\System.Runtime.InteropServices.RuntimeInformation.dll"; DestDir: "{app}\Wexflow.Scripts.RavenDB"; Flags: ignoreversion
Source: "..\src\dotnet\Wexflow.Server\bin\x64\Release\System.Runtime.CompilerServices.Unsafe.dll"; DestDir: "{app}\Wexflow.Scripts.RavenDB"; Flags: ignoreversion
Source: "..\src\dotnet\Wexflow.Server\bin\x64\Release\System.Threading.Tasks.Extensions.dll"; DestDir: "{app}\Wexflow.Scripts.RavenDB"; Flags: ignoreversion
Source: "..\src\dotnet\Wexflow.Server\bin\x64\Release\System.Numerics.Vectors.dll"; DestDir: "{app}\Wexflow.Scripts.RavenDB"; Flags: ignoreversion
Source: "..\src\dotnet\Wexflow.Server\bin\x64\Release\System.Memory.dll"; DestDir: "{app}\Wexflow.Scripts.RavenDB"; Flags: ignoreversion
Source: "..\src\dotnet\Wexflow.Server\bin\x64\Release\Microsoft.Bcl.AsyncInterfaces.dll"; DestDir: "{app}\Wexflow.Scripts.RavenDB"; Flags: ignoreversion
Source: "..\src\dotnet\Wexflow.Server\bin\x64\Release\System.Net.Sockets.dll"; DestDir: "{app}\Wexflow.Scripts.RavenDB"; Flags: ignoreversion
Source: "..\src\dotnet\Wexflow.Server\bin\x64\Release\System.Text.Encodings.Web.dll"; DestDir: "{app}\Wexflow.Scripts.RavenDB"; Flags: ignoreversion
Source: "..\src\dotnet\Wexflow.Server\bin\x64\Release\System.Text.Json.dll"; DestDir: "{app}\Wexflow.Scripts.RavenDB"; Flags: ignoreversion
Source: "..\src\dotnet\Wexflow.Server\bin\x64\Release\System.ValueTuple.dll"; DestDir: "{app}\Wexflow.Scripts.RavenDB"; Flags: ignoreversion

; Wexflow.Scripts.CosmosDB
Source: "..\src\dotnet\Wexflow.Scripts.CosmosDB\bin\x64\Release\Wexflow.Scripts.CosmosDB.exe"; DestDir: "{app}\Wexflow.Scripts.CosmosDB"; Flags: ignoreversion recursesubdirs
Source: ".\dotnet\Wexflow.Scripts.CosmosDB\Wexflow.Scripts.CosmosDB.exe.config"; DestDir: "{app}\Wexflow.Scripts.CosmosDB"; Flags: ignoreversion recursesubdirs
Source: "..\src\dotnet\Wexflow.Core.CosmosDB\bin\x64\Release\*.dll"; DestDir: "{app}\Wexflow.Scripts.CosmosDB"; Flags: ignoreversion
Source: "..\src\dotnet\Wexflow.Scripts.CosmosDB\bin\x64\Release\*.dll"; DestDir: "{app}\Wexflow.Scripts.CosmosDB"; Flags: ignoreversion recursesubdirs

; Wexflow.Scripts.PostgreSQL
Source: "..\src\dotnet\Wexflow.Scripts.PostgreSQL\bin\x64\Release\Wexflow.Scripts.PostgreSQL.exe"; DestDir: "{app}\Wexflow.Scripts.PostgreSQL"; Flags: ignoreversion recursesubdirs
Source: ".\dotnet\Wexflow.Scripts.PostgreSQL\Wexflow.Scripts.PostgreSQL.exe.config"; DestDir: "{app}\Wexflow.Scripts.PostgreSQL"; Flags: ignoreversion recursesubdirs
Source: "..\src\dotnet\Wexflow.Scripts.PostgreSQL\bin\x64\Release\*.dll"; DestDir: "{app}\Wexflow.Scripts.PostgreSQL"; Flags: ignoreversion recursesubdirs
Source: "..\src\dotnet\Wexflow.Server\bin\x64\Release\System.Net.Http.dll"; DestDir: "{app}\Wexflow.Scripts.PostgreSQL"; Flags: ignoreversion
Source: "..\src\dotnet\Wexflow.Server\bin\x64\Release\System.Buffers.dll"; DestDir: "{app}\Wexflow.Scripts.PostgreSQL"; Flags: ignoreversion
Source: "..\src\dotnet\Wexflow.Server\bin\x64\Release\System.Runtime.InteropServices.RuntimeInformation.dll"; DestDir: "{app}\Wexflow.Scripts.PostgreSQL"; Flags: ignoreversion
Source: "..\src\dotnet\Wexflow.Server\bin\x64\Release\System.Runtime.CompilerServices.Unsafe.dll"; DestDir: "{app}\Wexflow.Scripts.PostgreSQL"; Flags: ignoreversion
Source: "..\src\dotnet\Wexflow.Server\bin\x64\Release\System.Threading.Tasks.Extensions.dll"; DestDir: "{app}\Wexflow.Scripts.PostgreSQL"; Flags: ignoreversion
Source: "..\src\dotnet\Wexflow.Server\bin\x64\Release\System.Numerics.Vectors.dll"; DestDir: "{app}\Wexflow.Scripts.PostgreSQL"; Flags: ignoreversion
Source: "..\src\dotnet\Wexflow.Server\bin\x64\Release\System.Memory.dll"; DestDir: "{app}\Wexflow.Scripts.PostgreSQL"; Flags: ignoreversion
Source: "..\src\dotnet\Wexflow.Server\bin\x64\Release\Microsoft.Bcl.AsyncInterfaces.dll"; DestDir: "{app}\Wexflow.Scripts.PostgreSQL"; Flags: ignoreversion
Source: "..\src\dotnet\Wexflow.Server\bin\x64\Release\System.Net.Sockets.dll"; DestDir: "{app}\Wexflow.Scripts.PostgreSQL"; Flags: ignoreversion
Source: "..\src\dotnet\Wexflow.Server\bin\x64\Release\System.Text.Encodings.Web.dll"; DestDir: "{app}\Wexflow.Scripts.PostgreSQL"; Flags: ignoreversion
Source: "..\src\dotnet\Wexflow.Server\bin\x64\Release\System.Text.Json.dll"; DestDir: "{app}\Wexflow.Scripts.PostgreSQL"; Flags: ignoreversion
Source: "..\src\dotnet\Wexflow.Server\bin\x64\Release\System.ValueTuple.dll"; DestDir: "{app}\Wexflow.Scripts.PostgreSQL"; Flags: ignoreversion

; Wexflow.Scripts.SQLServer
Source: "..\src\dotnet\Wexflow.Scripts.SQLServer\bin\x64\Release\Wexflow.Scripts.SQLServer.exe"; DestDir: "{app}\Wexflow.Scripts.SQLServer"; Flags: ignoreversion recursesubdirs
Source: ".\dotnet\Wexflow.Scripts.SQLServer\Wexflow.Scripts.SQLServer.exe.config"; DestDir: "{app}\Wexflow.Scripts.SQLServer"; Flags: ignoreversion recursesubdirs
Source: "..\src\dotnet\Wexflow.Scripts.SQLServer\bin\x64\Release\*.dll"; DestDir: "{app}\Wexflow.Scripts.SQLServer"; Flags: ignoreversion recursesubdirs

; Wexflow.Scripts.MySQL
Source: "..\src\dotnet\Wexflow.Scripts.MySQL\bin\x64\Release\Wexflow.Scripts.MySQL.exe"; DestDir: "{app}\Wexflow.Scripts.MySQL"; Flags: ignoreversion recursesubdirs
Source: ".\dotnet\Wexflow.Scripts.MySQL\Wexflow.Scripts.MySQL.exe.config"; DestDir: "{app}\Wexflow.Scripts.MySQL"; Flags: ignoreversion recursesubdirs
Source: "..\src\dotnet\Wexflow.Scripts.MySQL\bin\x64\Release\*.dll"; DestDir: "{app}\Wexflow.Scripts.MySQL"; Flags: ignoreversion recursesubdirs

; Wexflow.Scripts.SQLite
Source: "..\src\dotnet\Wexflow.Scripts.SQLite\bin\x64\Release\Wexflow.Scripts.SQLite.exe"; DestDir: "{app}\Wexflow.Scripts.SQLite"; Flags: ignoreversion recursesubdirs
Source: ".\dotnet\Wexflow.Scripts.SQLite\Wexflow.Scripts.SQLite.exe.config"; DestDir: "{app}\Wexflow.Scripts.SQLite"; Flags: ignoreversion recursesubdirs
Source: "..\src\dotnet\Wexflow.Scripts.SQLite\bin\x64\Release\*.dll"; DestDir: "{app}\Wexflow.Scripts.SQLite"; Flags: ignoreversion recursesubdirs
Source: "..\src\dotnet\Wexflow.Scripts.SQLite\bin\x64\Release\x64\SQLite.Interop.dll"; DestDir: "{app}\Wexflow.Scripts.SQLite"; Flags: ignoreversion recursesubdirs

; NOTE: Don't use "Flags: ignoreversion" on any shared system files

[Icons]
Name: "{commonprograms}\{#MyAppName}\Manager"; Filename: "{app}\Manager\{#MyAppExeName}";
Name: "{commonprograms}\{#MyAppName}\Backend"; Filename: "{app}\Backend\index.html";
Name: "{commonprograms}\{#MyAppName}\Swagger"; Filename: "http://localhost:8000";
;Name: "{commonprograms}\{#MyAppName}\Start Wexflow Windows Service"; Filename: {sys}\sc.exe; Parameters: "start Wexflow" ; IconFilename: "{app}\Wexflow.ico";
;Name: "{commonprograms}\{#MyAppName}\Stop Wexflow Windows Service"; Filename: {sys}\sc.exe; Parameters: "stop Wexflow" ; IconFilename: "{app}\Wexflow.ico";
Name: "{commonprograms}\{#MyAppName}\Configuration"; Filename: "C:\Wexflow\";
Name: "{commonprograms}\{#MyAppName}\Documentation"; Filename: "{app}\Documentation";
Name: "{commonprograms}\{#MyAppName}\Logs"; Filename: "{app}\Wexflow.log";
Name: "{commonprograms}\{#MyAppName}\Install CosmosDB samples"; Filename: "{app}\Wexflow.Scripts.CosmosDB\Wexflow.Scripts.CosmosDB.exe";
Name: "{commonprograms}\{#MyAppName}\Install MongoDB samples"; Filename: "{app}\Wexflow.Scripts.MongoDB\Wexflow.Scripts.MongoDB.exe";
Name: "{commonprograms}\{#MyAppName}\Install RavenDB samples"; Filename: "{app}\Wexflow.Scripts.RavenDB\Wexflow.Scripts.RavenDB.exe";
Name: "{commonprograms}\{#MyAppName}\Install PostgreSQL samples"; Filename: "{app}\Wexflow.Scripts.PostgreSQL\Wexflow.Scripts.PostgreSQL.exe";
Name: "{commonprograms}\{#MyAppName}\Install SQL Server samples"; Filename: "{app}\Wexflow.Scripts.SQLServer\Wexflow.Scripts.SQLServer.exe";
Name: "{commonprograms}\{#MyAppName}\Install MySQL samples"; Filename: "{app}\Wexflow.Scripts.MySQL\Wexflow.Scripts.MySQL.exe";
Name: "{commonprograms}\{#MyAppName}\Install SQLite samples"; Filename: "{app}\Wexflow.Scripts.SQLite\Wexflow.Scripts.SQLite.exe";
Name: "{commonprograms}\{#MyAppName}\Uninstall"; Filename: "{uninstallexe}";

Name: "{commondesktop}\{#MyAppName}"; Filename: "{app}\Manager\{#MyAppExeName}"; Tasks: desktopicon
;Name: "{userappdata}\Microsoft\Internet Explorer\Quick Launch\{#MyAppName}"; Filename: "{app}\Manager\{#MyAppExeName}"; Tasks: quicklaunchicon

[Run]
;Filename: "{app}\Manager\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent
Filename: "{sys}\sc.exe"; Parameters: "create Wexflow start= auto binPath= ""{app}\Wexflow.Server.exe""" ; Flags: runhidden  waituntilterminated
Filename: "{sys}\sc.exe"; Parameters: "description Wexflow ""Wexflow workflow engine."""; Flags: runhidden  waituntilterminated
Filename: "{sys}\sc.exe"; Parameters: "start Wexflow"; Flags: runhidden  waituntilterminated

[UninstallRun]
Filename: "taskkill"; Parameters: "/im ""{#MyAppExeName}"" /t /f"; Flags: runhidden waituntilterminated
Filename: "{sys}\sc.exe"; Parameters: "stop Wexflow"; Flags: runhidden waituntilterminated
Filename: "taskkill"; Parameters: "/im ""Wexflow.Server.exe"" /t /f"; Flags: runhidden waituntilterminated
Filename: "taskkill"; Parameters: "/im ""chromedriver.exe"" /t /f"; Flags: runhidden waituntilterminated
Filename: "{sys}\sc.exe"; Parameters: "delete Wexflow"; Flags: runhidden waituntilterminated

[UninstallDelete]
Type: files; Name: "{app}\chromedriver.exe"

[InstallDelete]
Type: files; Name: "C:\Wexflow\Database\Wexflow.db"

[Code]
procedure InitializeWizard();
begin
  CreateDir('C:\Wexflow');
  //CreateDir('C:\Wexflow\Database');
  //CreateDir('C:\Wexflow\Workflows');
  CreateDir('C:\Wexflow\Tasks');
  CreateDir('C:\Wexflow\Temp');
  //CreateDir('C:\Wexflow\Temp\Workflows');
  CreateDir('C:\Wexflow\Approval');  
  //CreateDir('C:\Wexflow\Trash');  
  //CreateDir('C:\WexflowTesting');
end;
