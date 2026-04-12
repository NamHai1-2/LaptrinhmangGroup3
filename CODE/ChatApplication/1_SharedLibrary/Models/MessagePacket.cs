using System;

namespace _1_SharedLibrary.Models
{
    public enum CommandType
    {
        Login,              
        LoginSuccess,       
        LoginFail,          
        Register,           
        BroadcastMessage,   
        PrivateMessage,     
        UserListUpdate,     
        Disconnect,          
        RegisterSuccess,    
        RegisterFail     
    }

    [Serializable]
    public class MessagePacket
    {
        public CommandType Command { get; set; } 
        public string Sender { get; set; }       
        public string Receiver { get; set; }      
        public string Content { get; set; }      
        public DateTime Timestamp { get; set; }  

        public MessagePacket()
        {
            Timestamp = DateTime.Now;
        }
    }
}