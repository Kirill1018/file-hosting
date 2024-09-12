using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection.Metadata.Ecma335;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using FlexLabs.EntityFrameworkCore.Upsert;
using Microsoft.Data.Sqlite;
using Microsoft.Data.SqlClient;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using System.Runtime.CompilerServices;
namespace File_hosting
{
    internal class AuthorizationController : FileAvailability
    {
        readonly DbContext? Context;
        public DbSet<User> Users { get; set; }
        public string ConnectionString { get; } = "Data Source=DESKTOP-NEUQAJ1\\SQLEXPRESS;Trust Server Certificate=True";
        public AuthorizationController() { }
        public AuthorizationController(DbContext context)
        {
            Context = context;
            DbContextOptionsBuilder dbContextOptionsBuilder = new DbContextOptionsBuilder();
            this.OnConfiguring(dbContextOptionsBuilder);
            IServiceCollection services = new ServiceCollection();
            services.AddDbContext<AuthorizationController>();
            Users = Set<User>();
            using (var client = new AuthorizationController()) client.Database.EnsureCreated();
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var connectionStringBuilder = new SqliteConnectionStringBuilder { DataSource = this.ConnectionString };
            var linkString = connectionStringBuilder.ToString();
            var connection = new SqliteConnection(linkString);
            var sqlitePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder
                .ApplicationData), @"DESKTOP-NEUQAJ1\SQLEXPRESS");
            Directory.CreateDirectory(sqlitePath);
            optionsBuilder.UseSqlite($"DataSource={sqlitePath}\\user and file.db");
        }
        
        public bool Authorize(string receivingLogin, string receivingParole)
        {
            string query = $"select permission from [dbo].[users] where email='{receivingLogin}' and password='{receivingParole}'";
            SqlConnection connectivity = new(new AuthorizationController().ConnectionString);
            SqlCommand command = new(query, connectivity);
            connectivity.Open();
            SqlDataReader reader = command.ExecuteReader();
            reader.Read();
            bool permit = (bool)reader[0];
            connectivity.Close();
            if (permit) return permit;
            else return !permit;
        }
    }
    internal class UserController
    {
        public UserController() { }
        public void CreateUser(string email, string password)
        {
            string queryString = $"select email from [dbo].[users]", userId;
            SqlConnection connection = new(new AuthorizationController().ConnectionString);
            SqlCommand sqlCommand = new(queryString, connection);
            connection.Open();
            SqlDataReader dataReader = sqlCommand.ExecuteReader();
            dataReader.Read();
            userId = (string)dataReader[0];
            connection.Close();
            Console.WriteLine(userId);
            string query = $"insert into [dbo].[users](email, password, permission) values('{email}', '{password}', '{false}')";
            using (SqlConnection connectivity = new(new AuthorizationController().ConnectionString))
            {
                SqlCommand command = new(query, connectivity);
                try
                {
                    connectivity.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read()) Console.WriteLine("\t{0}\t{1}\t{2}", reader[0], reader[1],
                        reader[2]);
                    connectivity.Close();
                }
                catch (Exception ex) { Console.WriteLine(ex.Message); }
            }
            Directory.CreateDirectory(email);
        }
        public void UpdateUser(int id, string updated_password)
        {
            string request = $"update [dbo].[users] set password = '{updated_password}'";
            using (SqlConnection connectivity = new(new AuthorizationController().ConnectionString))
            {
                SqlCommand command = new(request, connectivity);
                try
                {
                    connectivity.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read()) Console.WriteLine("\t{0}\t{1}\t{2}", reader[0], reader[1],
                        reader[2]);
                    connectivity.Close();
                }
                catch (Exception ex) { Console.WriteLine(ex.Message); }
            }
        }
    }
    internal class FileAvailability : DbContext
    {
        public string? UserName;
        public FileAvailability() { }
        public User Authenticate(User user, string receivingLogin, string receivingParole)
        {
            string query = $"update [dbo].[users] set permission = '{new AuthorizationController().Authorize(receivingLogin, receivingParole)}' where email = '{receivingLogin}' and password = '{receivingParole}'";
            using (SqlConnection connectivity = new(new AuthorizationController().ConnectionString))
            {
                SqlCommand command = new(query, connectivity);
                try
                {
                    connectivity.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read()) Console.WriteLine("\t{0}\t{1}\t{2}", reader[0], reader[1],
                        reader[2]);
                    connectivity.Close();
                }
                catch (Exception ex) { Console.WriteLine(ex.Message); }
            }
            string queryString = $"select email from [dbo].[users] where email = '{receivingLogin}' and password = '{receivingParole}'";
            SqlConnection connection = new(new AuthorizationController().ConnectionString);
            SqlCommand sqlCommand = new(queryString, connection);
            connection.Open();
            SqlDataReader dataReader = sqlCommand.ExecuteReader();
            dataReader.Read();
            this.UserName = (string)dataReader[0];
            connection.Close();
            return user;
        }
        public void CreateFile() { Console.WriteLine(this.UserName); new FileInfo($"{this.UserName}/file.txt").Create(); }
    }
}