using System;
using System.Configuration;
using System.IO;
using System.Net.Mail;

namespace BasicBridge
{
    public class BasicLogger
    {
        private string _logDirectory;
        private string _infoLogLocation;
        private string _errorLogLocation;
        private string _errorMailRecipients;
        public BasicLogger()
        {
            try
            {
                _logDirectory = AppDomain.CurrentDomain.BaseDirectory + @"\Logs";
                _infoLogLocation = _logDirectory + @"\InfoLog.txt";
                _errorLogLocation = _logDirectory + @"\ErrorLog.txt";
                EnsureLogFilesExist();
                _errorMailRecipients = ReadConfigurationAppSetting("ErrorMailRecipients");                
            }
            catch (Exception e)
            {
                SendMail(
                    "cwelch@kasslaw.com,msowa@kasslaw.com,dcohen@kasslaw.com",
                    AppDomain.CurrentDomain.FriendlyName + " Error",
                    "Couldn't Instantiate Logging:" + Environment.NewLine + e.Message);
                Environment.Exit(0);
            }
        }

        private string ReadConfigurationAppSetting(string key)
        {
            var value = ConfigurationManager.AppSettings[key] ?? "Key Not Found!!";

            if(value == "Key Not Found!!") // email addresses need to be hardcoded here in case there's some crazy problem with app.config
            {
                SendMail(
                    "cwelch@kasslaw.com,msowa@kasslaw.com,dcohen@kasslaw.com",
                    AppDomain.CurrentDomain.FriendlyName + " Error",
                    "App.Config doesn't contain the requisite key value: " + key);
                Environment.Exit(0);
            }

            return value;
        }

        private void EnsureLogFilesExist()
        {
            if(!Directory.Exists(_logDirectory))
            {
                Directory.CreateDirectory(_logDirectory);
            }

            if (!File.Exists(_infoLogLocation))
            {
                using (var fileStream = new FileStream(_infoLogLocation, FileMode.Create))
                {
                }
            }

            if (!File.Exists(_errorLogLocation))
            {
                using (var fileStream = new FileStream(_errorLogLocation, FileMode.Create))
                {
                }
            }
        }        

        public void LogInfo(string infoToLog)
        {
            using (var streamWriter = new StreamWriter(_infoLogLocation, true))
            {
                if(!streamWriter.BaseStream.CanWrite)
                {
                    throw new InvalidOperationException("Log must not be read only!");
                }

                streamWriter.WriteLine(DateTime.Now.ToString() + ":  " + infoToLog);
            }
        }

        public void LogError(string errorToLog, bool sendMail = true)
        {
            if(sendMail == true)
                SendMail(_errorMailRecipients, AppDomain.CurrentDomain.FriendlyName + " Error", errorToLog);

            using (var streamWriter = new StreamWriter(_errorLogLocation, true))
            {
                if (!streamWriter.BaseStream.CanWrite)
                {
                    throw new InvalidOperationException("Log must not be read only!");
                }

                streamWriter.WriteLine(DateTime.Now.ToString() + ":  " + errorToLog);
            }            
        }

        public void SendMail(string toAddress, string subject, string body, Attachment attachment = null, bool IsBodyHtml = false)
        {
            using (var mail = new MailMessage("cwelch@Kasslaw.com", toAddress))
            {
                var client = new SmtpClient
                {
                    Port = 25,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    EnableSsl = false,
                    UseDefaultCredentials = false,
                    Host = "mail.kasslaw.com"
                };

                mail.IsBodyHtml = IsBodyHtml;
                mail.Subject = subject;
                mail.Body = body;
                if (attachment != null)
                    mail.Attachments.Add(attachment);
#if DEBUG
#else
                client.Send(mail);
#endif
            };
        }
    }
}
