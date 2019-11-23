namespace Wexflow.Core.PostgreSQL
{
    public class User : Core.Db.User
    {
        public static readonly string TableStruct = "(ID SERIAL PRIMARY KEY, USERNAME CHAR(128), PASSWORD CHAR(128), USER_PROFILE INT, EMAIL CHAR(128), CREATED_ON TIMESTAMP, MODIFIED_ON TIMESTAMP)";

        public int Id { get; set; }

        public override string GetId()
        {
            return Id.ToString();
        }
    }
}
