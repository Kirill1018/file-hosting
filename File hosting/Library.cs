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
namespace File_hosting
{
    internal class AuthorizationController : DbContext
    {
        readonly DbContext? Context;
        public DbSet<User> Users { get; set; }
        public string ConnectionString { get; set; } = "Data Source=DESKTOP-4JEPINR;Initial Catalog=user and file;Integrated Security=True;Pooling=False;Encrypt=True;Trust Server Certificate=True";
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
            var connectionStringBuilder = new SqliteConnectionStringBuilder { DataSource = ConnectionString };
            var linkString = connectionStringBuilder.ToString();
            var connection = new SqliteConnection(linkString);
            optionsBuilder.UseSqlite(connection);
        }
        public User Authenticate(User user) { return user; }
        public bool Authorize(User user, int file_id) { return true; }
    }
    internal class UserController
    {
        public UserController() { }
        public void CreateUser(string email, string password)
        {
            string queryString = $"insert into users (email, password) values ('{email}', '{password}')";
            using (SqlConnection connectivity = new(new AuthorizationController().ConnectionString))
            {
                SqlCommand command = new(queryString, connectivity);
                try
                {
                    connectivity.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read()) Console.WriteLine("\t{0}\t{1}\t{2}", reader[0], reader[1],
                        reader[2]);
                }
                catch (Exception ex) { Console.WriteLine(ex.Message); }
            }
        }
    }
}