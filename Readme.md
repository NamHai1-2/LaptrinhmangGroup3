## 📂 Cấu trúc dự án (Project Structure)

```text
TCP_Chat_Solution/
│
├── .gitignore                   
├── TCP_Chat.sln                
│
├── 1_SharedLibrary/             
│   ├── Models/
│   │   ├── User.cs              
│   │   ├── MessagePacket.cs     
│   └── Utils/
│       ├── JsonParser.cs        
│       └── Constants.cs         
│
├── 2_ChatServer/                
│   ├── Data/
│   │   ├── DatabaseHelper.cs    
│   │   └── chat_database.db     
│   ├── Network/
│   │   ├── TcpServerHandler.cs  
│   │   └── ClientConnection.cs  <
│   └── UI/
│       └── ServerDashboard.cs   
│
└── 3_ChatClient/                
    ├── Network/
    │   └── TcpClientHelper.cs   
    └── UI/
        ├── LoginForm.cs         
        ├── MainChatForm.cs      
        └── CustomControls/      
