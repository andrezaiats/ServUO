#region Header
//   Vorspire    _,-'/-'/  PowerHourResetToken.cs
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
using System.Drawing;

using Server;
#endregion

namespace VitaNex.Modules.PowerHour
{
	public class PowerHourResetToken : Item
	{
		public override bool DisplayLootType { get { return false; } }

		[Constructable]
		public PowerHourResetToken()
			: base(0x2AAA)
		{
			Name = "Power Hour Reset Token";
			Weight = 1.0;
			Hue = 150;
			Stackable = true;
			LootType = LootType.Blessed;
		}

		public PowerHourResetToken(Serial serial)
			: base(serial)
		{ }

		public override void OnDoubleClick(Mobile m)
		{
			if (!this.CheckDoubleClick(m, true, false, 2, true, false, false))
			{
				return;
			}

			var state = m.GetPowerHourState(false);

			if (state == null)
			{
				m.SendMessage("You don't have a previous Power Hour session to reset.");
				return;
			}

			if (!state.SessionEnded)
			{
				m.SendMessage("Your Power Hour session is currently active.");
				return;
			}

			if (state.CooldownEnded)
			{
				m.SendMessage("You can already start a new Power Hour session.");
				return;
			}

			state.Reset();
			Consume();
			m.SendMessage("Power Hour session cooldown reset. You can now start a new session!");
		}

		public override void GetProperties(ObjectPropertyList list)
		{
			base.GetProperties(list);

			list.Add("Use: Resets Power Hour, allowing you to start a new session.".WrapUOHtmlColor(Color.LawnGreen));
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			reader.ReadInt();
		}
	}
}
