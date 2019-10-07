namespace Wexflow.Core.CosmosDB
{
    public class Entry : Core.Db.Entry
    {
        [Newtonsoft.Json.JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        public override string GetDbId()
        {
            return Id;
        }
    }
}
