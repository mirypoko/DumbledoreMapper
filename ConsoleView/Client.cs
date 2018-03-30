using System.Text;

namespace ConsoleView
{
    public class Client
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Email { get; set; }

        public string CardNumber { get; set; }

        public string ErrorType { get; set; }

        public int? NullableInt { get; set; }

        public int ForIgnore { get; set; }

        public void SetData()
        {
            Id = RandomUtil.GetRandomNumber(1, 10000);
            Name = RandomUtil.GetRandomString(10);
            Email = RandomUtil.GetRandomString(10) + "@" + RandomUtil.GetRandomString(5) + ".com";
            StringBuilder stringBuilder = new StringBuilder();
            for (int i = 16; i > 0; i--)
            {
                stringBuilder.Append(RandomUtil.GetRandomNumber(1, 9));
            }

            CardNumber = stringBuilder.ToString();
        }

        public override string ToString()
        {
            return "Client:\n" +
                   $"Id={Id}\n" +
                   $"Name={Name}\n" +
                   $"Email={Email}\n" +
                   $"CardNumber={CardNumber}\n";
        }
    }
}
