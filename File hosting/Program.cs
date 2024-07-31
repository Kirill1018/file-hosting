using System.Net.Sockets;
using System.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore.Internal;
using System.Reflection.Metadata.Ecma335;
using Microsoft.EntityFrameworkCore.Sqlite.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Data.Sqlite;
namespace File_hosting
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var tcpListener = new TcpListener(IPAddress.Any, 1234);
            try
            {
                tcpListener.Start();
                string login = "kirillfedorov031@yandex.ru", parole = "filehosting031";
                using TcpClient client = new TcpClient();
                await client.ConnectAsync("192.168.1.130", 1234);
                var flow = client.GetStream();
                using var binaryWriter = new BinaryWriter(flow);
                binaryWriter.Write(login);
                binaryWriter.Write(parole);
                binaryWriter.Flush();
                Console.WriteLine("сервер запущен. ожидание подключений...");
                while (true)
                {
                    using var tcpClient = await tcpListener.AcceptTcpClientAsync();
                    var stream = tcpClient.GetStream();
                    using var binaryReader = new BinaryReader(stream);
                    string email = binaryReader.ReadString(), password = binaryReader.ReadString();
                    using AuthorizationController ac = new AuthorizationController(new DbContext(new DbContextOptionsBuilder().Options));
                    {
                        User user = new User { Email = email, Password = password, Permission = false };
                        ac.Users.Add(user);
                        ac.SaveChanges();
                        Console.WriteLine("объекты успешно сохранены");
                        var clients = ac.Users.ToList();
                        foreach (User customer in clients) Console.WriteLine($"{customer.Id}.{customer.Email}.{customer.Password}.{customer.Permission}");
                        new UserController().CreateUser(email, password);
                    }
                }
            }
            finally { tcpListener.Stop(); }
        }
    }
}