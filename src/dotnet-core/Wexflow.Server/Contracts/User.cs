namespace Wexflow.Server.Contracts
{
    public enum UserProfile
    {
        Administrator,
        Restricted
    }

    public class User
    {
        
        public int Id { get; set; }
        
        public string Username { get; set; }
        
        public string Password { get; set; }
        
        public UserProfile UserProfile { get; set; }
        
        public string Email { get; set; }
        
        public double CreatedOn { get; set; }
        
        public double ModifiedOn { get; set; }
    }
}
