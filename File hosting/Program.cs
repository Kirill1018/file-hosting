using System.Net.Sockets;
using System.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore.Internal;
using System.Reflection.Metadata.Ecma335;
using Microsoft.EntityFrameworkCore.Sqlite.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Data.Sqlite;
using Microsoft.Identity.Client;
using Microsoft.Data.SqlClient;
using System.Net.Mail;
using System.Runtime.CompilerServices;
namespace File_hosting
{
    internal class Program
    {
        public string Login { get; set; } = "kirillfedorov031@yandex.ru";
        public string Parole { get; set; } = "Filehosting-031";
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
                await consumer.ConnectAsync("192.168.1.146", 1234);
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
                        writer.Write(new Program().Login);
                        writer.Write(new Program().Parole);
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
                        string dispatchLogin = "kirillfedorov031@yandex.ru", dispatchParole = "Filehosting-031";
                        writer.Write(dispatchLogin);
                        writer.Write(dispatchParole);
                        writer.Flush();
                        Console.WriteLine("сервер запущен. ожидание подключений...");
                        string receivingLogin = reader.ReadString(), receivingParole = reader.ReadString();
                        User customers = new User();
                        new AuthorizationController().Authenticate(customers, receivingLogin, receivingParole);
                    }
                    else if (receivingRequest == Requests.USER_FORGOTPASS.ToString())
                    {
                        string queryString = $"select Id from [dbo].[users] where email = '{ new Program().Login }'";
                        int identificator, message = new Random().Next();
                        using (SqlConnection connection = new(new AuthorizationController().ConnectionString))
                        {
                            SqlCommand command = new(queryString, connection);
                            try
                            {
                                connection.Open();
                                SqlDataReader dataReader = command.ExecuteReader();
                                while (dataReader.Read())
                                {
                                    identificator = (int)dataReader[0];
                                    Console.WriteLine(identificator);
                                    MailAddress from = new MailAddress("kirillfedorov031@gmail.com");
                                    MailAddress to = new MailAddress("kirill.fiodorov2012@list.ru");
                                    MailMessage mailMessage = new MailMessage(from, to);
                                    mailMessage.Subject = "код, который нужно ввести, чтобы обновить пароль";
                                    mailMessage.Body = message.ToString();
                                    mailMessage.IsBodyHtml = true;
                                    using (SmtpClient client = new SmtpClient("smtp.gmail.com", 587))
                                    {
                                        client.Credentials = new NetworkCredential("kirillfedorov031@gmail.com", "mdnr etmd cmbq ctcl");
                                        client.EnableSsl = true;
                                        await client.SendMailAsync(mailMessage);
                                        Console.WriteLine("письмо отправлено");
                                    }
                                    int code = message;
                                    if (code == message)
                                    {
                                        string passphrase = "Password-031";
                                        new UserController().UpdateUser(identificator, passphrase);
                                    }
                                }
                                connection.Close();
                            }
                            catch (Exception ex) { Console.WriteLine(ex.Message); }
                        }
                    }
                    else if (receivingRequest == Requests.FILE_CREATE.ToString()) new FileAvailability().CreateFile();
                }
            }
            finally { tcpListener.Stop(); }
        }
    }
}