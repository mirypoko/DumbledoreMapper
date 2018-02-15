using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DumbledoreMapper;

namespace ConsoleView
{
    class Program
    {
        private static int countOfRecords = 10;

        private static List<User> _users;

        private static List<Client> _clients = new List<Client>();

        static void Main(string[] args)
        {
            Console.Write("Сount of records:");
            countOfRecords = Convert.ToInt32(Console.ReadLine());

            _users = new List<User>();
            for (int i = 0; i < countOfRecords; i++)
            {
                var user = new User();
                user.SetData();
                _users.Add(user);

                Client client = new Client();
                client.SetData();
                _clients.Add(client);
            }

            double Map = TestCreateMapper();
            Console.WriteLine($"TestCreateMapper:{Map}ms");

            double сopyMap = TestCopyMapper();
            Console.WriteLine($"TestCopyMapper:{сopyMap}ms");

            Console.ReadKey();
        }

        private static double TestCopyMapper()
        {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            for (int i = 0; i < countOfRecords; i++)
            {
                var c = new Client();
                Mapper.CopyProperties(_users[i], c);
            }

            stopWatch.Stop();
            var ts = stopWatch.Elapsed;
            return ts.TotalMilliseconds;
        }

        private static double TestCreateMapper()
        {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            for (int i = 0; i < countOfRecords; i++)
            {
                Mapper.Map<Client>(_users[i]);
            }

            stopWatch.Stop();
            var ts = stopWatch.Elapsed;
            return ts.TotalMilliseconds;
        }
    }
}
