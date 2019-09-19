namespace Wexflow.Core.CosmosDB
{
    public class Workflow: Core.Db.Workflow
    {
        [Newtonsoft.Json.JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        public override string GetDbId()
        {
            return Id;
        }
    }
}
