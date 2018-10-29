using System;
using System.Globalization;
using System.IO;
using System.Speech.AudioFormat;
using System.Speech.Recognition;
using System.Threading;
using System.Xml.Linq;
using Wexflow.Core;

namespace Wexflow.Tasks.SpeechToText
{
    public class SpeechToText:Task
    {
        public SpeechToText(XElement xe, Workflow wf) : base(xe, wf)
        {
        }

        public override TaskStatus Run()
        {
            Info("Converting speech to text...");
            var status = Status.Success;
            var success = true;
            var atLeastOneSucceed = false;

            var files = SelectFiles();

            foreach (var file in files)
            {
                try
                {
                    using (Stream stream = new FileStream(file.Path, FileMode.Open))
                    {
                        var recognizer = new SpeechRecognitionEngine(new CultureInfo("en-US"));
                        var dictationGrammar = new DictationGrammar();
                        recognizer.LoadGrammar(dictationGrammar);
                        recognizer.SetInputToAudioStream(stream, new SpeechAudioFormatInfo(32000, AudioBitsPerSample.Sixteen, AudioChannel.Mono));
                        var result = recognizer.Recognize();
                        string text = result.Text;

                        var destFile = Path.Combine(Workflow.WorkflowTempFolder, Path.GetFileNameWithoutExtension(file.FileName) + ".txt");
                        File.WriteAllText(destFile, text);
                        Files.Add(new FileInf(destFile, Id));
                        InfoFormat("The file {0} was converted to a text file with success -> {1}", file.Path, destFile);
                    }
                }
                catch (ThreadAbortException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    ErrorFormat("An error occured while converting the file {0}", e, file.Path);
                    success = false;
                }
            }

            if (!success && atLeastOneSucceed)
            {
                status = Status.Warning;
            }
            else if (!success)
            {
                status = Status.Error;
            }

            Info("Task finished.");
            return new TaskStatus(status, false);
        }
    }
}
