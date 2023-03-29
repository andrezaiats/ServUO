#region Header
//   Vorspire    _,-'/-'/  PowerHour.cs
//   .      __,-; ,'( '/
//    \.    `-.__`-._`:_,-._       _ , . ``
//     `:-._,------' ` _,`--` -: `_ , ` ,' :
//        `---..__,,--'  (C) 2020  ` -'. -'
//        #  Vita-Nex [http://core.vita-nex.com]  #
//  {o)xxx|===============-   #   -===============|xxx(o}
//        #        The MIT License (MIT)          #
#endregion

#if ServUO58
#define ServUOX
#endif

#region References
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

using Server;
using Server.Commands;
using Server.Gumps;
using Server.Items;
using Server.Misc;
using Server.Mobiles;

using VitaNex.IO;
using VitaNex.SuperGumps.UI;
using VitaNex.Targets;
using VitaNex.TimeBoosts;
#endregion

namespace VitaNex.Modules.PowerHour
{
	public static partial class PowerHour
	{
		public const AccessLevel Access = AccessLevel.Administrator;

		private static CreateCorpseHandler _CorpseSuccessor;

		private static SkillCheckTargetHandler _SkillCheckTSuccessor;
		private static SkillCheckLocationHandler _SkillCheckLSuccessor;
		private static SkillCheckDirectTargetHandler _SkillCheckDTSuccessor;
		private static SkillCheckDirectLocationHandler _SkillCheckDLSuccessor;

		public static PowerHourOptions CMOptions { get; private set; }

		public static BinaryDataStore<Mobile, PowerHourState> States { get; private set; }

		public static PowerHourState GetPowerHourState(this Mobile m, bool create = true)
		{
			if (m == null)
			{
				return null;
			}

			PowerHourState state;

			if ((!States.TryGetValue(m, out state) || state == null) && create)
			{
				if (m.Deleted || !m.Player)
				{
					state = null;
					States.Remove(m);
				}
				else
				{
					state = new PowerHourState(m);
					States[m] = state;
				}
			}
			else if (m.Deleted || !m.Player)
			{
				state = null;
				States.Remove(m);
			}

			return state;
		}

		public static bool HasPowerHour(this Mobile m)
		{
			if (!CMOptions.ModuleEnabled)
			{
				return false;
			}

			var s = GetPowerHourState(m, false);

			return s != null && !s.SessionEnded;
		}

		private static void CheckNotify(Mobile m)
		{
			if (!CMOptions.ModuleEnabled || !CMOptions.LoginNotify)
			{
				return;
			}

			if (m == null || m.Deleted || !m.Player || !m.IsOnline() || m.AccessLevel >= AccessLevel.Counselor)
			{
				return;
			}

			var state = GetPowerHourState(m);

			if (state == null || state.CheckState(m, false))
			{
				var html = new StringBuilder();

				html.AppendLine("Hey, {0}!", m.RawName);
				html.AppendLine("Your Power Hour session is ready to begin!");

				m.SendNotification(
					html.ToString(),
					false,
					1.0,
					3.0,
					Color.SkyBlue,
					ui =>
					{
						ui.AddOption(
							"Begin Session",
							b =>
							{
								ui.Close();
								Handle(m);
							},
							Color.LawnGreen);
						ui.AddOption("No Thanks", b => ui.Close(), Color.OrangeRed);
					});
			}
		}

		private static void OnLogin(LoginEventArgs e)
		{
			if (CMOptions.ModuleEnabled && CMOptions.LoginNotify && e.Mobile != null)
			{
				Timer.DelayCall(TimeSpan.FromSeconds(30), CheckNotify, e.Mobile);
			}
		}

		private static void HandleCommand(CommandEventArgs e)
		{
			Handle(e.Mobile);
		}

		public static void Handle(Mobile m)
		{
			if (m == null || m.Deleted || !CMOptions.ModuleEnabled)
			{
				return;
			}

			var state = GetPowerHourState(m);

			if (state == null)
			{
				return;
			}

			if (!state.CheckState(m, true))
			{
				if (CMOptions.UseTimeBoosts && !state.CooldownEnded && m is PlayerMobile)
				{
					var ui = new TimeBoostsUI(m, null, TimeBoosts.TimeBoosts.EnsureProfile((PlayerMobile)m))
					{
						Title = "Power Hour",
						SubTitle = "Reduce Wait?",
						SummaryText = "Next Session",
						GetTime = () => state.CooldownExpire,
						SetTime = t => state.CooldownExpire = t,
						CanApply = t => state.SessionEnded && !state.CooldownEnded
					};

					ui.BoostUsed = b =>
					{
						if (state.CooldownEnded)
						{
							ui.Close(true);
							Handle(m);
						}
					};

					ui.Send();
				}

				return;
			}

			new ConfirmDialogGump(m)
			{
				Width = 400,
				Height = 300,
				Title = "Power Hour!",
				Html =
					"Power Hour gives you special bonuses for the duration of time that it is active on your character.\n" +
					"You can start a Power Hour session now by clicking OK.\nYour session will last for " +
					state.Duration.ToSimpleString(@"!<d\d h\h m\m s\s>") + "\nWhen it ends, you can begin another session after " +
					state.Cooldown.ToSimpleString(@"!<d\d h\h m\m s\s>") + " has passed.\nClick OK to begin a new Power Hour session!",
				AcceptHandler = b => state.BeginSession(m, true)
			}.Send();
		}

		private static void HandleResetCommand(CommandEventArgs e)
		{
			GlobalReset();

			e.Mobile.SendMessage("Global Power Hour state reset completed.");
		}

		public static void GlobalReset()
		{
			States.Values.ForEach(
				s =>
				{
					s.Reset();

					CheckNotify(s.Owner);
				});
		}

		private static void HandlePropsCommand(CommandEventArgs e)
		{
			e.Mobile.SendMessage("Target a mobile to view their Power Hour state properties...");
			MobileSelectTarget<PlayerMobile>.Begin(e.Mobile, (m, t) => OpenProps(m, GetPowerHourState(m)), null);
		}

		public static void OpenProps(Mobile m, PowerHourState s)
		{
			if (m != null && s != null)
			{
				m.SendGump(new PropertiesGump(m, s));
			}
		}

		public static Container OnCreateCorpse(
			Mobile owner,
			HairInfo hair,
			FacialHairInfo facialhair,
			List<Item> initialContent,
			List<Item> equipItems)
		{
			if (_CorpseSuccessor == null)
			{
				_CorpseSuccessor = Corpse.Mobile_CreateCorpseHandler;
				Mobile.CreateCorpseHandler = OnCreateCorpse;
			}

			var c = _CorpseSuccessor(owner, hair, facialhair, initialContent, equipItems);

			if (!CMOptions.ModuleEnabled || !(owner is BaseCreature) || c == null || c.Deleted || CMOptions.GoldLootFactor == 0)
			{
				return c;
			}

			var m = (BaseCreature)owner;

			if (m.IsControlled<PlayerMobile>() || m.IsBonded)
			{
				return c;
			}

			var k = m.GetLastKiller<PlayerMobile>();

			if (k != null && !HasPowerHour(k))
			{
				return c;
			}

			Timer.DelayCall(ModifyLootGold, c);

			return c;
		}

		public static void ModifyLootGold(Container c)
		{
			if (c == null || c.Deleted || CMOptions.GoldLootFactor == 0)
			{
				return;
			}

			var gold = c.FindItemsByType<Gold>();

			foreach (var g in gold)
			{
				g.Amount = Math.Max(1, g.Amount + (int)(g.Amount * CMOptions.GoldLootFactor));
			}

#if !ServUOX
			gold.Free(true);
#endif
		}

		private static bool OnSkillCheckTarget(Mobile m, SkillName skill, object target, double min, double max)
		{
			if (_SkillCheckTSuccessor == null)
			{
				_SkillCheckTSuccessor = SkillCheck.Mobile_SkillCheckTarget;
				Mobile.SkillCheckTargetHandler = OnSkillCheckTarget;
			}

			var success = _SkillCheckTSuccessor(m, skill, target, min, max);

			if (success && CMOptions.ModuleEnabled && m.BeginAction(_SkillCheckTSuccessor))
			{
				SkillGainCheck(m, skill);

				m.EndAction(_SkillCheckTSuccessor);
			}

			return success;
		}

		private static bool OnSkillCheckDirectTarget(Mobile m, SkillName skill, object target, double chance)
		{
			if (_SkillCheckDTSuccessor == null)
			{
				_SkillCheckDTSuccessor = SkillCheck.Mobile_SkillCheckDirectTarget;
				Mobile.SkillCheckDirectTargetHandler = OnSkillCheckDirectTarget;
			}

			var success = _SkillCheckDTSuccessor(m, skill, target, chance);

			if (success && CMOptions.ModuleEnabled && m.BeginAction(_SkillCheckDTSuccessor))
			{
				SkillGainCheck(m, skill);

				m.EndAction(_SkillCheckDTSuccessor);
			}

			return success;
		}

		private static bool OnSkillCheckLocation(Mobile m, SkillName skill, double min, double max)
		{
			if (_SkillCheckLSuccessor == null)
			{
				_SkillCheckLSuccessor = SkillCheck.Mobile_SkillCheckLocation;
				Mobile.SkillCheckLocationHandler = OnSkillCheckLocation;
			}

			var success = _SkillCheckLSuccessor(m, skill, min, max);

			if (success && CMOptions.ModuleEnabled && m.BeginAction(_SkillCheckLSuccessor))
			{
				SkillGainCheck(m, skill);

				m.EndAction(_SkillCheckLSuccessor);
			}

			return success;
		}

		private static bool OnSkillCheckDirectLocation(Mobile m, SkillName skill, double chance)
		{
			if (_SkillCheckDLSuccessor == null)
			{
				_SkillCheckDLSuccessor = SkillCheck.Mobile_SkillCheckDirectLocation;
				Mobile.SkillCheckDirectLocationHandler = OnSkillCheckDirectLocation;
			}

			var success = _SkillCheckDLSuccessor(m, skill, chance);

			if (success && CMOptions.ModuleEnabled && m.BeginAction(_SkillCheckDLSuccessor))
			{
				SkillGainCheck(m, skill);

				m.EndAction(_SkillCheckDLSuccessor);
			}

			return success;
		}

		public static bool SkillGainCheck(Mobile m, SkillName skill)
		{
			if (HasPowerHour(m) && CMOptions.SkillGainChance > 0)
			{
				return m.CheckSkill(skill, CMOptions.SkillGainChance);
			}

			return false;
		}
	}
}
