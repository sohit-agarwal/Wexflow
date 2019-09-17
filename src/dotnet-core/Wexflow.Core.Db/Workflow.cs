using System;

namespace Wexflow.Core.Db
{
    public class Workflow
    {
        //public int Id { get; set; }
        public string Xml { get; set; }

        public virtual string GetDbId()
        {
            return "-1";
        }
    }
}
