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
            //TestCreateMapperWithIgnore();
            TestCopyMapper();
            //TestCopyMapperWithIgnore();
            TestCopyMapperIfNotNull();
            //TestCopyMapperIfNotNullWithIgnore();

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
                clientFromUser.ForIgnore == 0)
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

        private static void TestCreateMapperWithIgnore()
        {
            var user = new User
            {
                Id = 1,
                Email = "useremail@dfgkj.com",
                Name = "Dmitry",
                Role = "Admin",
                ForIgnore = 123
            };
            var clientFromUser = Mapper.Map<Client>(user, true);
            if (clientFromUser.Name == user.Name &&
                clientFromUser.Email == user.Email &&
                clientFromUser.ForIgnore == user.ForIgnore)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Create mapper with ignore work good.");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Create mapper with ignore error.");
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
                ForIgnore = 123
            };
            var client = new Client()
            {
                Email = "clientemail@dsfsdg.com",
                Name = "Andrey",
                ForIgnore = 321
            };

            Mapper.CopyProperties(user, client);

            if (client.Name == user.Name &&
                client.Email == user.Email &&
                client.Id == user.Id &&
                client.ForIgnore == 321)
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

        private static void TestCopyMapperWithIgnore()
        {
            var user = new User
            {
                Id = 1,
                Email = "useremail@dfgkj.com",
                Name = "Dmitry",
                Role = "Admin",
                ForIgnore = 123
            };
            var client = new Client()
            {
                Email = "clientemail@dsfsdg.com",
                Name = "Andrey",
                ForIgnore = 321
            };

            Mapper.CopyProperties(user, client, true);

            if (client.Name == user.Name &&
                client.Email == user.Email &&
                client.Id == user.Id &&
                client.ForIgnore == 321)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Copy mapper with ignore work good.");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Copy mapper with ignore error.");
            }
        }

        private static void TestCopyMapperIfNotNull()
        {
            var user = new User
            {
                Id = 1,
                Email = "useremail@dfgkj.com",
                Name = "Dmitry",
                Role = "Admin",
                ForIgnore = 123
            };
            var client = new Client()
            {
                Email = "clientemail@dsfsdg.com",
                Name = "Andrey",
                ForIgnore = 321
            };

            Mapper.CopyProperties(user, client, true);

            if (client.Name == user.Name &&
                client.Email == user.Email &&
                client.Id == user.Id &&
                client.ForIgnore == user.ForIgnore)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Copy mapper (if not null) with ignore work good.");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Copy mapper (if not null) with ignore error.");
            }
        }

        private static void TestCopyMapperIfNotNullWithIgnore()
        {
            var user = new User
            {
                Id = 1,
                Email = "useremail@dfgkj.com",
                Name = null,
                Role = "Admin",
                ForIgnore = 123
            };
            var client = new Client()
            {
                Email = "clientemail@dsfsdg.com",
                Name = "Andrey",
                ForIgnore = 321
            };

            Mapper.CopyProperties(user, client, true);

            if (client.Name == "Andrey" &&
                client.Email == user.Email &&
                client.Id == user.Id &&
                client.ForIgnore == 321)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Copy mapper (if not null) with ignore work good.");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Copy mapper (if not null) with ignore error.");
            }
        }
    }
}
