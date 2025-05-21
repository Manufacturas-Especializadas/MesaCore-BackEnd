namespace MesaCore.Data
{
    public class EmailSettings
    {
        public string SmtpServer { get; set; }

        public int Port { get; set; }
        
        public string SenderEmail { get; set; }
        
        public string Password { get; set; }
        
        public bool EnableSs1 { get; set; }
        
        public string SenderName { get; set; }
    }
}
