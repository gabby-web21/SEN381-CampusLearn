using System;
using System.ComponentModel.DataAnnotations;

namespace CampusLearnFrontend.Models
{
    public class User
    {
        public string Username { get; set; } = "";
        public string Email { get; set; } = "";
        public string Password { get; set; } = "";
        public string Name { get; set; } = "";
        public string Surname { get; set; } = "";
    }
}
