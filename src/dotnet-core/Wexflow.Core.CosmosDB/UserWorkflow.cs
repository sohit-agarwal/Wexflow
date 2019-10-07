namespace Wexflow.Core.CosmosDB
{
    public class UserWorkflow : Core.Db.UserWorkflow
    {
        [Newtonsoft.Json.JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
    }
}
