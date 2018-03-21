using System;
using System.Collections.Generic;
using System.Diagnostics;
using DumbledoreMapper;

namespace ConsoleView
{
    class Program
    {
        static void Main(string[] args)
        {
            TestCreateMapper();
            TestCopyMapper();
            TestCopyMapperIfNotNull();
            TestCopyMapperIfNotNullAndNotCopyNullable();

            Console.ReadKey();
        }

        private static void TestCreateMapper()
        {
            var user = new User
            {
                Id = 1,
                Email = "useremail@dfgkj.com",
                Name = "Dmitry",
                Role = "Admin",
                ForIgnore = 123
            };

            var clientFromUser = Mapper.Map<Client>(user);

            if (clientFromUser.Name == user.Name &&
                clientFromUser.Email == user.Email &&
                clientFromUser.ForIgnore == 0 &&
                clientFromUser.Id == user.Id)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Create mapper work good.");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Create mapper error.");
            }

            Console.ResetColor();
        }

        private static void TestCopyMapper()
        {
            var user = new User
            {
                Id = 1,
                Email = "useremail@dfgkj.com",
                Name = "Dmitry",
                Role = "Admin",
                ForIgnore = 123,
                NullableInt = 1234
            };
            var client = new Client()
            {
                Email = "clientemail@dsfsdg.com",
                Name = "Andrey",
                ForIgnore = 321,
                NullableInt = null
            };

            Mapper.CopyProperties(user, client, true, true);

            if (client.Name == user.Name &&
                client.Email == user.Email &&
                client.Id == user.Id &&
                client.ForIgnore == 321 &&
                client.NullableInt.Value == user.NullableInt)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Copy mapper work good.");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Copy mapper error.");
            }
        }

        private static void TestCopyMapperIfNotNull()
        {
            var user = new User
            {
                Id = 1,
                Email = "useremail@dfgkj.com",
                Name = null,
                Role = "Admin",
                ForIgnore = 123,
                NullableInt = 1234
            };
            var client = new Client()
            {
                Email = "clientemail@dsfsdg.com",
                Name = "Andrey",
                ForIgnore = 321,
                NullableInt = null
            };

            Mapper.CopyProperties(user, client, false, true);

            if (client.Name == "Andrey" &&
                client.Email == user.Email &&
                client.Id == user.Id &&
                client.ForIgnore == 321 &&
                client.NullableInt.Value == user.NullableInt)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Copy mapper (if not null) work good.");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Copy mapper (if not null) error.");
            }
        }

        private static void TestCopyMapperIfNotNullAndNotCopyNullable()
        {
            var user = new User
            {
                Id = 1,
                Email = "useremail@dfgkj.com",
                Name = null,
                Role = "Admin",
                ForIgnore = 123,
                NullableInt = 1234
            };
            var client = new Client()
            {
                Email = "clientemail@dsfsdg.com",
                Name = "Andrey",
                ForIgnore = 321,
                NullableInt = null
            };

            Mapper.CopyProperties(user, client, false, false);

            if (client.Name == "Andrey" &&
                client.Email == user.Email &&
                client.Id == user.Id &&
                client.ForIgnore == 321 &&
                client.NullableInt == null)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Copy mapper (if not null and dont copy nullable) work good.");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Copy mapper (if not null and dont copy nullable) error.");
            }
        }
    }
}
