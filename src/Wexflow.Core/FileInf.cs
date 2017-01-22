using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Wexflow.Core
{
    public class FileInf
    {
        private string _path;
        private string _renameTo;

        public string Path { 
            get 
            { 
                return this._path; 
            } 
            set 
            {
                this._path = value;
                this.FileName = System.IO.Path.GetFileName(value);
            } 
        }
        public string FileName { get; private set; }
        public int TaskId { get; private set; }
        public string RenameTo {
            get
            {
                return this._renameTo;
            }
            set
            {
                this._renameTo = value;

                if (!string.IsNullOrEmpty(value))
                    this.RenameToOrName = value;
            }
        }
        public string RenameToOrName { get; private set; }
        public List<Tag> Tags { get; private set; }

        public FileInf(string path, int taskId)
        {
            this.Path = path;
            this.TaskId = taskId;
            this.RenameToOrName = this.FileName;
            this.Tags = new List<Tag>();
        }

        public override string ToString()
        {
            return this.ToXElement().ToString();
        }

        public XElement ToXElement()
        {
            return new XElement("File",
                new XAttribute("taskId", this.TaskId),
                new XAttribute("path", this.Path),
                new XAttribute("name", this.FileName),
                new XAttribute("renameTo", this.RenameTo ?? string.Empty),
                new XAttribute("renameToOrName", this.RenameToOrName),
                from tag in this.Tags
                select new XAttribute(tag.Key, tag.Value));
        }
    }
}