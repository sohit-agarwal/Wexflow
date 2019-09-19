namespace Wexflow.Core.CosmosDB
{
    public class StatusCount : Core.Db.StatusCount
    {
        [Newtonsoft.Json.JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
    }
}