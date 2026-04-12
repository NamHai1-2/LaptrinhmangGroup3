using System;

namespace _1_SharedLibrary.Models
{
    [Serializable]
    public class User
    {
        public int Id { get; set; }                 
        public string Username { get; set; }        
        public string Password { get; set; }         
        public bool IsOnline { get; set; }          
        public DateTime LastLogin { get; set; }     
       
        public User()
        {
        }
        
        public User(string username, string password)
        {
            Username = username;
            Password = password;
            IsOnline = false;
            LastLogin = DateTime.Now;
        }
  
        public override string ToString()
        {
            return Username + (IsOnline ? " (Online)" : " (Offline)");
        }
    }
}