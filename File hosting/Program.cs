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
        public enum Requests
        {
            USER_CREATE, USER_AUTHORIZE, USER_FORGOTPASS,
            FILE_CREATE, FILE_DELETE, FILE_MOVE,
            FILE_UPLOAD, FILE_DOWNLOAD
        }
        static async Task Main(string[] args)
        {
            var tcpListener = new TcpListener(IPAddress.Any, 1234);
            try
            {
                tcpListener.Start();
                string dispatchRequest = Requests.USER_CREATE.ToString();
                using TcpClient consumer = new TcpClient();
                await consumer.ConnectAsync("192.168.1.130", 1234);
                var streaming = consumer.GetStream();
                using var writer = new BinaryWriter(streaming);
                writer.Write(dispatchRequest);
                writer.Flush();
                while (true)
                {
                    using var client_ = await tcpListener.AcceptTcpClientAsync();
                    var flood = client_.GetStream();
                    using var reader = new BinaryReader(flood);
                    string receivingRequest = reader.ReadString();
                    if (receivingRequest == Requests.USER_CREATE.ToString())
                    {
                        string login = "kirillfedorov031@yandex.ru", parole = "filehosting031";
                        writer.Write(login);
                        writer.Write(parole);
                        writer.Flush();
                        Console.WriteLine("сервер запущен. ожидание подключений...");
                        string email = reader.ReadString(), password = reader.ReadString();
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
                    else if (receivingRequest == Requests.USER_AUTHORIZE.ToString())
                    {
                        string dispatchLogin = "kirillfedorov031@yandex.ru", dispatchParole = "filehosting031";
                        writer.Write(dispatchLogin);
                        writer.Write(dispatchParole);
                        writer.Flush();
                        Console.WriteLine("сервер запущен. ожидание подключений...");
                        string receivingLogin = reader.ReadString(), receivingParole = reader.ReadString();
                        User customers = new User();
                        new AuthorizationController().Authenticate(customers, receivingLogin, receivingParole);
                    }
                }
            }
            finally { tcpListener.Stop(); }
        }
    }
}