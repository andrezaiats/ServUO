using System;
using Server;
using Server.Commands;
using Server.Items;
using Server.Mobiles;
using System.Collections;
using System.Collections.Generic;
using Server.Accounting;
using Server.Network;
using Server.Misc;
using Server.Multis;
using Server.Targeting;
using Server.Gumps;
using System.Net.Mail;
using System.Threading;
using System.Net;

namespace Server.Misc
{
    public class RegisterEmailClient
    {
        public static bool Enabled = true; // Is this system enabled?

        public static string ServerName = "UOshard.com"; // Your server name here.

        public static string EmailServer = "192.168.0.254"; // Your mail server here
        public static string User = ""; // Your username here
        public static string Pass = ""; // Your password here

        public static string YourAddress = "info@uoshard.com"; // Your email address here, Or Shard name
        // Server will crash on start up if the adress is incorrectly formatted.

        public static SmtpClient client;
        public static MailMessage mm;

        public static void Initialize()
        {
            if (Enabled)
            {
                client = new SmtpClient(EmailServer);
                client.Credentials = new NetworkCredential(User, Pass);

                mm = new MailMessage();
                mm.Subject = ServerName;
                mm.From = new MailAddress(YourAddress);
            }
        }

        public static void SendMail(EmailEventArgs e)
        {
            bool single = e.Single;

            if (single)
            {
                ThreadPool.QueueUserWorkItem(new WaitCallback(SendSingal), e);
            }
            else
            {
                ThreadPool.QueueUserWorkItem(new WaitCallback(SendMultiple), e);
            }

            return;
        }

        private static void SendMultiple(object e)
        {
            EmailEventArgs eea = (EmailEventArgs)e;

            List<MailAddress> emails = (List<MailAddress>)eea.Emails;
            string sub = (string)eea.Subject;
            string msg = (string)eea.Message;

            for (int i = 0; i < emails.Count; ++i)
            {
                MailAddress ma = (MailAddress)emails[i];

                mm.To.Add(ma);
            }

            mm.Subject += " - " + sub;
            mm.Body = msg;

            try
            {
                client.Send(mm);
            }
            catch { }
            mm.To.Clear();
            mm.Body = "";
            mm.Subject = ServerName;

            return;
        }

        private static void SendSingal(object e)
        {
            EmailEventArgs eea = (EmailEventArgs)e;

            string to = (string)eea.To;
            string sub = (string)eea.Subject;
            string msg = (string)eea.Message;

            mm.To.Add(to);
            mm.Subject += " " + sub;
            mm.Body = msg;

            try
            {
                client.Send(mm);
            }
            catch { }
            mm.To.Clear();
            mm.Body = "";
            mm.Subject = ServerName;

            return;
        }
    }

    public class EmailEventArgs
    {
        public bool Single;
        public List<MailAddress> Emails;
        public string To;
        public string Subject;
        public string Message;

        public EmailEventArgs(bool single, List<MailAddress> list, string to, string sub, string msg)
        {
            Single = single;
            Emails = list;
            To = to;
            Subject = sub;
            Message = msg;
        }
    }
}
