using System;
using System.Collections.Generic;
using System.Diagnostics;
using DumbledoreMapper;

namespace ConsoleView
{
    class Program
    {
        private static int _countOfRecords;

        private static List<User> _users;

        private static readonly List<Client> _clients = new List<Client>();

        static void Main(string[] args)
        {
            Console.Write("Сount of records:");
            _countOfRecords = Convert.ToInt32(Console.ReadLine());

            _users = new List<User>();
            for (int i = 0; i < _countOfRecords; i++)
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

            double copyIfNotNullMapper = TestCopyIfNotNullMapperSpeed();
            Console.WriteLine($"TestCopyIfNotNullMapper speed::{copyIfNotNullMapper}ms");

            //Console.WriteLine();
            //Console.WriteLine("TestCopyIfNotNullMapper:");
            //TestCopyIfNotNullMapper();

            TestCopyIfNotNullMapperWithIgnore();

            Console.ReadKey();
        }

        private static double TestCopyMapper()
        {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            for (int i = 0; i < _countOfRecords; i++)
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
            for (int i = 0; i < _countOfRecords; i++)
            {
                Mapper.Map<Client>(_users[i]);
            }

            stopWatch.Stop();
            var ts = stopWatch.Elapsed;
            return ts.TotalMilliseconds;
        }

        private static void TestCopyIfNotNullMapper()
        {
            for (int i = 0; i < _countOfRecords / 2; i++)
            {
                var c = new Client();
                c.Email = "clientemail@grfdgdfg.com";
                _users[i].Email = null;
                Mapper.CopyPropertiesIfNotNull(_users[i], c);
                Console.WriteLine($"CopyIfNotNullMapper test {i} (dont copy):");
                Console.WriteLine(_users[i]);
                Console.WriteLine(c);
                Console.WriteLine();

            }
            for (int i = _countOfRecords / 2; i < _countOfRecords; i++)
            {
                var c = new Client();
                Mapper.CopyProperties(_users[i], c);
                Console.WriteLine($"CopyIfNotNullMapper test {i} (copy):");
                Console.WriteLine(_users[i]);
                Console.WriteLine(c);
                Console.WriteLine();
            }
        }


        private static double TestCopyIfNotNullMapperSpeed()
        {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            for (int i = 0; i < _countOfRecords / 2; i++)
            {
                var c = new Client();
                c.Email = "clientemail@grfdgdfg.com";
                _users[i].Email = null;
                Mapper.CopyPropertiesIfNotNull(_users[i], c);
            }

            stopWatch.Stop();
            var ts = stopWatch.Elapsed;
            return ts.TotalMilliseconds;
        }

        private static void TestCopyIfNotNullMapperWithIgnore()
        {
            var list1 = new List<Obj1>();
            var list2 = new List<Obj2>();
            for (int i = 0; i < _countOfRecords; i++)
            {
                list1.Add(new Obj1());
                list2.Add(new Obj2());
            }
            for (int i = 0; i < _countOfRecords; i++)
            {
                Mapper.CopyPropertiesIfNotNull(list1[i], list2[i], true);
                Console.WriteLine($"CopyIfNotNullMapper test {i} (WithIgnore):");
                Console.WriteLine(list1[i]);
                Console.WriteLine(list2[i]);
                Console.WriteLine();
            }
            list1 = new List<Obj1>();
            list2 = new List<Obj2>();
            for (int i = 0; i < _countOfRecords; i++)
            {
                list1.Add(new Obj1());
                list2.Add(new Obj2());
            }
            for (int i = 0; i < _countOfRecords; i++)
            {
                Mapper.CopyPropertiesIfNotNull(list2[i], list1[i], true);
                Console.WriteLine($"CopyIfNotNullMapper test {i} (WithIgnore):");
                Console.WriteLine(list2[i]);
                Console.WriteLine(list1[i]);
                Console.WriteLine();
            }
        }

        private static double TestCopyMapper()
        {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            for (int i = 0; i < _countOfRecords; i++)
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
            for (int i = 0; i < _countOfRecords; i++)
            {
                Mapper.Map<Client>(_users[i]);
            }

            stopWatch.Stop();
            var ts = stopWatch.Elapsed;
            return ts.TotalMilliseconds;
        }
    }
}
