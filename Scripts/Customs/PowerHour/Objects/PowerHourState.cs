#region Header
//   Vorspire    _,-'/-'/  PowerHourState.cs
//   .      __,-; ,'( '/
//    \.    `-.__`-._`:_,-._       _ , . ``
//     `:-._,------' ` _,`--` -: `_ , ` ,' :
//        `---..__,,--'  (C) 2020  ` -'. -'
//        #  Vita-Nex [http://core.vita-nex.com]  #
//  {o)xxx|===============-   #   -===============|xxx(o}
//        #        The MIT License (MIT)          #
#endregion

#region References
using System;
using System.Linq;

using Server;
using Server.Accounting;
#endregion

namespace VitaNex.Modules.PowerHour
{
	public sealed class PowerHourState : PropertyObject
	{
		[CommandProperty(PowerHour.Access, true)]
		public Mobile Owner { get; private set; }

		[CommandProperty(PowerHour.Access)]
		public TimeSpan Duration { get; set; }

		[CommandProperty(PowerHour.Access)]
		public TimeSpan Cooldown { get; set; }

		[CommandProperty(PowerHour.Access)]
		public DateTime SessionStart { get; set; }

		[CommandProperty(PowerHour.Access)]
		public DateTime SessionEnd { get { return SessionStart.Add(Duration); } }

		[CommandProperty(PowerHour.Access)]
		public TimeSpan SessionExpire
		{
			get { return TimeSpan.FromTicks(Math.Max(0, unchecked(SessionEnd.Ticks - DateTime.UtcNow.Ticks))); }
			set { SessionStart += value - SessionExpire; }
		}

		[CommandProperty(PowerHour.Access)]
		public bool SessionEnded { get { return SessionExpire <= TimeSpan.Zero; } }

		[CommandProperty(PowerHour.Access)]
		public DateTime CooldownEnd { get { return SessionEnd.Add(Cooldown); } }

		[CommandProperty(PowerHour.Access)]
		public TimeSpan CooldownExpire
		{
			get { return TimeSpan.FromTicks(Math.Max(0, unchecked(CooldownEnd.Ticks - DateTime.UtcNow.Ticks))); }
			set { SessionStart += value - CooldownExpire; }
		}

		[CommandProperty(PowerHour.Access)]
		public bool CooldownEnded { get { return CooldownExpire <= TimeSpan.Zero; } }

		public PowerHourState(Mobile owner)
		{
			Owner = owner;

			EnsureDefaults();
		}

		public PowerHourState(GenericReader reader)
			: base(reader)
		{ }

		public void EnsureDefaults()
		{
			ResolveTimes();

			SessionStart = DateTime.MinValue;
		}

		public override void Clear()
		{
			EnsureDefaults();
		}

		public override void Reset()
		{
			EnsureDefaults();
		}

		public void ResolveTimes()
		{
			ResolveTimes(Owner);
		}

		public void ResolveTimes(Mobile m)
		{
			var duHours = PowerHour.CMOptions.Duration.TotalHours;
			var cdHours = PowerHour.CMOptions.Cooldown.TotalHours;

			Duration = TimeSpan.FromHours(duHours);
			Cooldown = TimeSpan.FromHours(cdHours);
		}

		public bool CheckState(Mobile m, bool message)
		{
			if (m == null || m.Deleted || m != Owner)
			{
				return false;
			}

			if (!PowerHour.CMOptions.ModuleEnabled)
			{
				if (message)
				{
					m.SendMessage(34, "[Power Hour]: Currently Unavailable.");
				}

				return false;
			}

			if (!SessionEnded)
			{
				if (message)
				{
					m.SendMessage(85, "[Power Hour]: Session Time Remaining: {0}", SessionExpire.ToSimpleString("h:m:s"));
				}

				return false;
			}

			if (!CooldownEnded)
			{
				if (message)
				{
					m.SendMessage(85, "[Power Hour]: Cooldown Time Remaining: {0}", CooldownExpire.ToSimpleString("h:m:s"));
				}

				return false;
			}

			ResolveTimes(m);

			return true;
		}

		public bool BeginSession(Mobile m, bool message)
		{
			if (!CheckState(m, message))
			{
				return false;
			}

			SessionStart = DateTime.UtcNow;

			if (message)
			{
				m.SendMessage(85, "[Power Hour]: Session Started!");
				m.SendMessage(85, "[Power Hour]: Session Time Remaining: {0}", SessionExpire.ToSimpleString("h:m:s"));
			}

			return true;
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			var version = writer.SetVersion(2);

			switch (version)
			{
				case 2:
					writer.Write(Owner);
					goto case 1;
				case 1:
				{
					writer.Write(Duration);
					writer.Write(Cooldown);
				}
					goto case 0;
				case 0:
				{
					if (version < 2)
					{
						writer.Write(Owner != null ? Owner.Account : default(Account));
					}

					writer.WriteDeltaTime(SessionStart);
				}
					break;
			}
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			var version = reader.GetVersion();

			switch (version)
			{
				case 2:
					Owner = reader.ReadMobile();
					goto case 1;
				case 1:
				{
					Duration = reader.ReadTimeSpan();
					Cooldown = reader.ReadTimeSpan();
				}
					goto case 0;
				case 0:
				{
					if (version < 1)
					{
						Duration = PowerHour.CMOptions.Duration;
						Cooldown = PowerHour.CMOptions.Cooldown;
					}

					if (version < 2)
					{
						var a = reader.ReadAccount<Account>();

						if (a != null)
						{
							Owner = a.FindMobiles().FirstOrDefault();
						}
					}

					SessionStart = reader.ReadDeltaTime();
				}
					break;
			}
		}
	}
}
