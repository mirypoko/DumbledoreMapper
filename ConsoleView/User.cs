namespace ConsoleView
{
    public class User
    {
        private readonly string[] _roles = {"Admin", "User", "Moderator"};

        public int Id { get; set; }

        public string Name { get; set; }

        public string Email { get; set; }

        public string Role { get; set; }

        public int NullableInt { get; set; }

        public long ForIgnore { get; set; }

        public void SetData()
        {
            Id = RandomUtil.GetRandomNumber(1, 10000);
            Name = RandomUtil.GetRandomString(10);
            Email = RandomUtil.GetRandomString(10) + "@" + RandomUtil.GetRandomString(5) + ".com";
            Role = _roles[RandomUtil.GetRandomNumber(0, 2)];
        }

        public override string ToString()
        {
            return "User:\n" +
                   $"Id={Id}\n" +
                   $"Name={Name}\n" +
                   $"Email={Email}\n" +
                   $"Role={Role}\n";
        }
    }
}
