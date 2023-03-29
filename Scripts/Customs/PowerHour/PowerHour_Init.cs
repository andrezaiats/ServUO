#region Header
//   Vorspire    _,-'/-'/  PowerHour_Init.cs
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
using System.Collections.Generic;

using Server;
using Server.Network;

using VitaNex.IO;
#endregion

namespace VitaNex.Modules.PowerHour
{
	[CoreModule("Power Hour", "1.0.0.3")]
	public static partial class PowerHour
	{
		static PowerHour()
		{
			CMOptions = new PowerHourOptions();

			States = new BinaryDataStore<Mobile, PowerHourState>(VitaNexCore.SavesDirectory + "/PowerHour", "States")
			{
				OnSerialize = SerializeStates,
				OnDeserialize = DeserializeStates,
				Async = true
			};
		}

		private static void CMConfig()
		{
			CommandUtility.Register("PowerHour", AccessLevel.Player, HandleCommand);
			CommandUtility.RegisterAlias("PowerHour", "PH");

			CommandUtility.Register("PowerHourReset", Access, HandleResetCommand);
			CommandUtility.RegisterAlias("PowerHourReset", "PHReset");

			CommandUtility.Register("PowerHourProps", Access, HandlePropsCommand);
			CommandUtility.RegisterAlias("PowerHourProps", "PHProps");
		}

		private static void CMInvoke()
		{
			EventSink.Login += OnLogin;

			_CorpseSuccessor = Mobile.CreateCorpseHandler;
			Mobile.CreateCorpseHandler = OnCreateCorpse;

			_SkillCheckTSuccessor = Mobile.SkillCheckTargetHandler;
			Mobile.SkillCheckTargetHandler = OnSkillCheckTarget;

			_SkillCheckDTSuccessor = Mobile.SkillCheckDirectTargetHandler;
			Mobile.SkillCheckDirectTargetHandler = OnSkillCheckDirectTarget;

			_SkillCheckLSuccessor = Mobile.SkillCheckLocationHandler;
			Mobile.SkillCheckLocationHandler = OnSkillCheckLocation;

			_SkillCheckDLSuccessor = Mobile.SkillCheckDirectLocationHandler;
			Mobile.SkillCheckDirectLocationHandler = OnSkillCheckDirectLocation;
		}

		private static void CMEnabled()
		{
			Timer.DelayCall(TimeSpan.FromSeconds(10), () => NetState.Instances.ForEachReverse(ns => CheckNotify(ns.Mobile)));
		}

		private static void CMSave()
		{
			States.RemoveValueRange(s => s == null || s.Owner == null || s.Owner.Deleted || !s.Owner.Player);

			States.Export();
		}

		private static void CMLoad()
		{
			States.Import();

			States.RemoveValueRange(s => s == null || s.Owner == null || s.Owner.Deleted || !s.Owner.Player);
		}

		public static bool SerializeStates(GenericWriter writer)
		{
			var version = writer.SetVersion(0);

			switch (version)
			{
				case 0:
				{
					writer.WriteBlockDictionary(
						States,
						(w, k, v) =>
						{
							w.Write(k);
							v.Serialize(w);
						});
				}
					break;
			}

			return true;
		}

		public static bool DeserializeStates(GenericReader reader)
		{
			var version = reader.GetVersion();

			switch (version)
			{
				case 0:
				{
					reader.ReadBlockDictionary(
						r =>
						{
							var k = r.ReadMobile();
							var v = new PowerHourState(r);

							return new KeyValuePair<Mobile, PowerHourState>(k, v);
						},
						States);
				}
					break;
			}

			return true;
		}
	}
}
