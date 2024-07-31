using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace File_hosting
{
    internal class User
    {
        public int Id { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public bool Permission { get; set; }
        public User() { }
        public User(int id, string? email, string? password,
            bool permission)
        {
            Id = id;
            Email = email;
            Password = password;
            Permission = permission;
        }
    }
    internal class File
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Path { get; set; }
        public File(int id, string? name, string? path)
        {
            Id = id;
            Name = name;
            Path = path;
        }
    }
    internal class User_File
    {
        public int Id { get; set; }
        public int User_id { get; set; }
        public int File_id { get; set; }
        public User_File(int id, int user_id, int file_id)
        {
            Id = id;
            User_id = user_id;
            File_id = file_id;
        }
    }
}