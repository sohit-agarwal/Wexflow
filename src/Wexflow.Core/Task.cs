using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Xml.XPath;

namespace Wexflow.Core
{
    public abstract class Task
    {
        public int Id { get; private set; }
        public string Name { get; private set; }
        public string Description { get; private set; }
        public bool IsEnabled { get; private set; }
        public Workflow Workflow { get; private set; }
        public List<FileInf> Files 
        { 
            get
            {
                return this.Workflow.FilesPerTask[this.Id];
            }
        }
        public List<Entity> Entities
        {
            get
            {
                return this.Workflow.EntitiesPerTask[this.Id];
            }
        }

        private XElement _xElement;

        public Task(XElement xe, Workflow wf) 
        {
            this._xElement = xe;
            this.Id = int.Parse(xe.Attribute("id").Value);
            this.Name = xe.Attribute("name").Value;
            this.Description = xe.Attribute("description").Value;
            this.IsEnabled = bool.Parse(xe.Attribute("enabled").Value);
            this.Workflow = wf;
            this.Workflow.FilesPerTask.Add(this.Id, new List<FileInf>());
            this.Workflow.EntitiesPerTask.Add(this.Id, new List<Entity>());
        }

        public abstract TaskStatus Run();

        public string GetSetting(string name)
        {
            return this._xElement.XPathSelectElement(string.Format("wf:Setting[@name='{0}']", name), this.Workflow.XmlNamespaceManager).Attribute("value").Value;
        }

        public string GetSetting(string name, string defaultValue)
        {
            XElement xe = this._xElement.XPathSelectElement(string.Format("wf:Setting[@name='{0}']", name), this.Workflow.XmlNamespaceManager);
            if (xe == null) return defaultValue;
            return xe.Attribute("value").Value;
        }

        public string[] GetSettings(string name)
        {
            List<string> settings = new List<string>();
            foreach (XElement xe in this._xElement.XPathSelectElements(string.Format("wf:Setting[@name='{0}']", name), this.Workflow.XmlNamespaceManager))
            {
                settings.Add(xe.Attribute("value").Value);
            }
            return settings.ToArray();
        }

        public XElement[] GetXSettings(string name)
        {
            return this._xElement.XPathSelectElements(string.Format("wf:Setting[@name='{0}']", name), this.Workflow.XmlNamespaceManager).ToArray();
        }

        public FileInf[] SelectFiles() 
        {
            List<FileInf> files = new List<FileInf>();
            foreach (var xSelectFile in this.GetXSettings("selectFiles"))
            {
                XAttribute xTaskId = xSelectFile.Attribute("value");
                if (xTaskId != null)
                {
                    int taskId = int.Parse(xTaskId.Value);

                    FileInf[] qf = QueryFiles(this.Workflow.FilesPerTask[taskId], xSelectFile).ToArray();
                        
                    files.AddRange(qf);
                }
                else
                {
                    FileInf[] qf = (from lf in this.Workflow.FilesPerTask.Values
                                    from f in QueryFiles(lf, xSelectFile)
                                    select f).Distinct().ToArray();
                    
                    files.AddRange(qf);
                }
            }
            return files.ToArray();
        }

        private IEnumerable<FileInf> QueryFiles(IEnumerable<FileInf> files, XElement xSelectFile)
        {
            List<FileInf> fl = new List<FileInf>();

            if (xSelectFile.Attributes().Where(t => t.Name != "value").Count() == 1)
            {
                return files;
            }
            else
            {
                foreach (var file in files)
                {
                    // Check file tags
                    bool ok = true;
                    foreach (var xa in xSelectFile.Attributes())
                    { 
                        if(xa.Name != "name" && xa.Name != "value")
                        {
                            ok &= file.Tags.Any(tag => tag.Key == xa.Name && tag.Value == xa.Value);
                        }
                    }

                    if (ok)
                    {
                        fl.Add(file);
                    }
                }
            }

            return fl;
        }

        public Entity[] SelectEntities()
        {
            List<Entity> entities = new List<Entity>();
            foreach (string id in this.GetSettings("selectEntities"))
            {
                int taskId = int.Parse(id);
                entities.AddRange(this.Workflow.EntitiesPerTask[taskId]);
            }
            return entities.ToArray();
        }

        private string BuildLogMsg(string msg)
        {
            return string.Format("{0} [{1}] {2}", this.Workflow.LogTag, this.GetType().Name, msg);
        }

        public void Info(string msg)
        {
            Logger.Info(BuildLogMsg(msg));
        }

        public void InfoFormat(string msg, params object[] args)
        {
            Logger.InfoFormat(BuildLogMsg(msg), args);
        }

        public void Debug(string msg)
        {
            Logger.Debug(BuildLogMsg(msg));
        }

        public void DebugFormat(string msg, params object[] args)
        {
            Logger.DebugFormat(BuildLogMsg(msg), args);
        }

        public void Error(string msg)
        {
            Logger.Error(BuildLogMsg(msg));
        }

        public void ErrorFormat(string msg, params object[] args)
        {
            Logger.ErrorFormat(BuildLogMsg(msg), args);
        }

        public void Error(string msg, Exception e)
        {
            Logger.Error(BuildLogMsg(msg), e);
        }

        public void ErrorFormat(string msg, Exception e, params object[] args)
        {
            Logger.Error(string.Format(BuildLogMsg(msg), args), e);
        }
    }
}
