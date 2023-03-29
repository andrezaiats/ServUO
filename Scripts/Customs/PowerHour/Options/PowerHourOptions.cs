#region Header
//   Vorspire    _,-'/-'/  PowerHourOptions.cs
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

using Server;
#endregion

namespace VitaNex.Modules.PowerHour
{
	public sealed class PowerHourOptions : CoreModuleOptions
	{
		[CommandProperty(PowerHour.Access)]
		public TimeSpan Duration { get; set; }

		[CommandProperty(PowerHour.Access)]
		public TimeSpan Cooldown { get; set; }

		[CommandProperty(PowerHour.Access)]
		public bool LoginNotify { get; set; }

		[CommandProperty(PowerHour.Access)]
		public bool UseTimeBoosts { get; set; }

		[CommandProperty(PowerHour.Access)]
		public double GoldLootFactor { get; set; }

		[CommandProperty(PowerHour.Access)]
		public double SkillGainChance { get; set; }

		public PowerHourOptions()
			: base(typeof(PowerHour))
		{
			SetDefaults();
		}

		public PowerHourOptions(GenericReader reader)
			: base(reader)
		{ }

		public void SetDefaults()
		{
			Duration = TimeSpan.FromHours(1.0);
			Cooldown = TimeSpan.FromHours(23.0);

			LoginNotify = true;
			UseTimeBoosts = false;

			GoldLootFactor = 0;
			SkillGainChance = 0;
		}

		public override void Clear()
		{
			base.Clear();

			SetDefaults();
		}

		public override void Reset()
		{
			base.Reset();

			SetDefaults();
		}

		public override string ToString()
		{
			return "Power Hour Options";
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			var version = writer.SetVersion(1);

			switch (version)
			{
				case 1:
				{
					writer.Write(GoldLootFactor);
					writer.Write(SkillGainChance);
				}
					goto case 0;
				case 0:
				{
					writer.Write(Duration);
					writer.Write(Cooldown);

					writer.Write(LoginNotify);
					writer.Write(UseTimeBoosts);
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
				case 1:
				{
					GoldLootFactor = reader.ReadDouble();
					SkillGainChance = reader.ReadDouble();
				}
					goto case 0;
				case 0:
				{
					Duration = reader.ReadTimeSpan();
					Cooldown = reader.ReadTimeSpan();

					LoginNotify = reader.ReadBool();
					UseTimeBoosts = reader.ReadBool();
				}
					break;
			}
		}
	}
}
