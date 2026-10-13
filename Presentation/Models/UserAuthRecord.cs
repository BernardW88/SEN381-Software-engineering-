namespace Models
{
    public sealed class UserAuthRecord
    {
        public User User { get; set; }
        public string LegacyPassword { get; set; }
        public string PasswordHash { get; set; }
        public string PasswordSalt { get; set; }
    }
}
