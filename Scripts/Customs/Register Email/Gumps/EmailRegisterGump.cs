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
using System.Text.RegularExpressions;

namespace Server.Gumps
{
	public class RegisterEmailGump : Gump
	{
		private const string _emailRegex = @"^[_a-z0-9-]+(\.[_a-z0-9-]+)*@[a-z0-9-]+(\.[a-z0-9-]+)*(\.[a-z]{2,4})$";

		public RegisterEmailGump() : base(0, 0)
		{
			Closable = true;
			Dragable = true;
			Resizable = false;

//            AddBackground(47, 372, 409, 140, 9300);
//			AddLabel(234, 376, 0, "Register Adress");
//
//            AddLabel(121, 404, 0, "Email:");
//            AddImage(172, 400, 1143);
//			AddTextEntry(180, 402, 255, 20, 0, 2, "" );
//
//            AddLabel(58, 443, 0, "Confirm Email:");
//            AddImage(172, 437, 1143);
//			AddTextEntry(180, 439, 257, 20, 0, 3, "" );
//
//            AddButton(135, 474, 4007, 4006, 1, GumpButtonType.Reply, 0);
//            AddLabel(172, 476, 0, "Submit");


            AddImage(251, 176, 1249);
            AddImage(353, 248, 1059);
            AddImage(246, 164, 50990);
            AddImage(467, 164, 50991);
            AddImage(458, 163, 50993);
            AddImage(238, 165, 50992);
            AddImage(376, 403, 5214);
            AddImage(440, 403, 5214);
            AddImage(440, 427, 5214);
            AddImage(376, 427, 5214);
            AddItem(396, 335, 3823);
            AddItem(410, 319, 3823);
            AddItem(423, 333, 3823);
            AddItem(456, 333, 3823);
            AddItem(443, 320, 3823);
            AddItem(396, 335, 3859);
            AddItem(423, 325, 3859);
            AddItem(449, 340, 3859);
            AddItem(466, 327, 3859);
            AddItem(446, 328, 3858);
            AddItem(421, 343, 3858);
            AddItem(446, 330, 3857);
            AddItem(423, 331, 3856);
            AddItem(452, 344, 3856);
            AddItem(403, 332, 3856);
            AddItem(461, 350, 3855);
            AddItem(396, 348, 3855);
            AddItem(492, 276, 2594);
            AddItem(269, 278, 2594);
            AddLabel(399, 298, 67, "Please Register!");
            AddLabel(295, 447, 1014, "Protect your Account and win Sovereigns!");
            AddLabel(387, 221, 1014, "Welcome to Forgotten Memories");
            AddLabel(295, 400, 67, "Enter Email:");
            AddTextEntry(383, 400, 139, 20, 0, 2, "");
            AddLabel(290, 425, 67, "Verify Email:");
            AddTextEntry(383, 424, 139, 20, 0, 3, "");
            AddButton(536, 411, 247, 248, 1, GumpButtonType.Reply, 0);
		}

        public bool ValidateEmail(string s, string s2)
        {
	    Match match = Regex.Match(s, _emailRegex);
            //if (!Regex.IsMatch(s, _emailRegex))
	    if (!match.Success)
            {
                return false;
            }

            if (s == "" || s2 == "")
                return false;

            if (!s.Contains("@") || !s2.Contains("@"))
                return false;

            if (s != s2)
                return false;

            return true;
        }

        public string RandomChar()
        {
            int rand = (int)Utility.Random(1, 16);

            switch (rand)
            {
                case 1:
                    {
                        return "a";
                        break;
                    }
                case 2:
                    {
                        return "G";
                        break;
                    }
                case 3:
                    {
                        return "e";
                        break;
                    }
                case 4:
                    {
                        return "4";
                        break;
                    }
                case 5:
                    {
                        return "8";
                        break;
                    }
                case 6:
                    {
                        return "k";
                        break;
                    }
                case 7:
                    {
                        return "M";
                        break;
                    }
                case 8:
                    {
                        return "6";
                        break;
                    }
                case 9:
                    {
                        return "8";
                        break;
                    }
                case 10:
                    {
                        return "v";
                        break;
                    }
                case 11:
                    {
                        return "J";
                        break;
                    }
                case 12:
                    {
                        return "f";
                        break;
                    }
                case 13:
                    {
                        return "X";
                        break;
                    }
                case 14:
                    {
                        return "2";
                        break;
                    }
                case 15:
                    {
                        return "3";
                        break;
                    }
                case 16:
                    {
                        return "1";
                        break;
                    }
            }
            return "";
        }

        public string ReturnCode()
        {
            string toreturn = "";

            for (string s = ""; s.Length < 11; s += RandomChar())
            {
                toreturn = s;
            }

            return toreturn;
        }

        public string CreateConFirmation()
        {
            if (EmailHolder.Emails == null)
                EmailHolder.Emails = new Dictionary<string, string>();
            if (EmailHolder.Confirm == null)
                EmailHolder.Confirm = new Dictionary<string, string>();
            if (EmailHolder.Codes == null)
                EmailHolder.Codes = new Dictionary<string, string>();

            string toreturn = ReturnCode();

            do
            {
                toreturn = ReturnCode();
            }
            while(EmailHolder.Codes.ContainsValue(toreturn));

            return toreturn;
        }

        public override void OnResponse(NetState sender, RelayInfo info)
		{
            if (EmailHolder.Emails == null)
                EmailHolder.Emails = new Dictionary<string, string>();
            if (EmailHolder.Confirm == null)
                EmailHolder.Confirm = new Dictionary<string, string>();
            if (EmailHolder.Codes == null)
                EmailHolder.Codes = new Dictionary<string, string>();

			Mobile from = sender.Mobile;

            switch (info.ButtonID)
            {
                case 0:
                    {
                        from.SendMessage("You will recieve this gump again next login.");
                        break;
                    }
                case 1:
                    {
                        TextRelay relay = (TextRelay)info.GetTextEntry(2);
                        string txt1 = (string)relay.Text.Trim();

                        TextRelay relay2 = (TextRelay)info.GetTextEntry(3);
                        string txt2 = (string)relay2.Text.Trim();

                        if (ValidateEmail(txt1, txt2))
                        {
                            string c = CreateConFirmation();

                            Account acct = (Account)from.Account;

                            string test = (string)acct.Username;

                            string email = txt1;

                            if (!EmailHolder.Confirm.ContainsKey(test))
                            {
                                EmailHolder.Confirm.Add(test, txt1);
                                EmailHolder.Codes.Add(test, c);
                            }
                            else
                            {
                                EmailHolder.Confirm.Remove(test);
                                EmailHolder.Codes.Remove(test);

                                EmailHolder.Confirm.Add(test, txt1);
                                EmailHolder.Codes.Add(test, c);
                            }

                            string msg = "Thank you for submiting your email, you must use the 10 character long confirmation code below to confirm you got this email \n \n" + c + "\n \nTo use this confirmation code you must type \"[auth\" in game and enter the code exactly as it appears here in the email into the text box provided. \n \n" + "Sincerely,\nUOshard.com Team"; ;

                            EmailEventArgs eea = new EmailEventArgs(true, null, email, "Email Registration", msg);

                            RegisterEmailClient.SendMail(eea);
                            from.SendMessage("You have been sent a confirmation code, it will expire when you next log out.");
                        }
                        else
                        {
                            from.SendMessage("You have not entered your email correctly, please try again.");
                            from.SendGump(new RegisterEmailGump());
                        }

                        break;
                    }

            }
		}
	}
}
