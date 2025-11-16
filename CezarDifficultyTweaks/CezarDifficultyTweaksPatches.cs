using HarmonyLib;
using Helpers;
using SandBox.GameComponents;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Reflection;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.LogEntries;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Party.PartyComponents;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;

namespace CezarDifficultyTweaks
{
    // 1) DAILY GOLD MALUS — patch the clan finance model
    [HarmonyPatch(typeof(DefaultClanFinanceModel), nameof(DefaultClanFinanceModel.CalculateClanExpensesInternal))]
    static class DailyGoldMalusPatch
    {
        static void Postfix(Clan clan, ref ExplainedNumber goldChange, bool applyWithdrawals, bool includeDetails)
        {
            var s = CezarDifficultyTweaksSettings.Instance;
            if (s == null)
                return;
            if(clan == Clan.PlayerClan)
            {

                if (s.DailyGoldMalusFlat == 0 && s.DailyGoldMalusMultiplier == 0) return;

                // apply flat malus
                goldChange.Add(-s.DailyGoldMalusFlat, new TextObject("{=CDT_GOLD_FLAT_DESC}Cezar Daily Difficulty Malus"));
                // apply multiplier to net-change so far
                var current = goldChange.ResultNumber;
                var extra = current * s.DailyGoldMalusMultiplier;

                if (extra < 0f)
                {
                    goldChange.Add(extra, new TextObject("{=CDT_GOLD_MULT_DESC}Cezar Daily Difficulty Multiplier Malus"));
                }
                return;
            } 
            if (clan != null)
            {
                if (s.DailyGoldBonusFlat == 0 && s.DailyGoldBonusMultiplier == 0) return;

                goldChange.Add(s.DailyGoldBonusFlat, null, null);

                var current = goldChange.ResultNumber;
                var extra = current * s.DailyGoldBonusMultiplier;

                if (extra > 0f)
                {
                    goldChange.Add(extra, null, null);
                }
            }
        }
    }

    // 2) LORDS FILL-STACK
    [HarmonyPatch(typeof(HeroSpawnCampaignBehavior), "SpawnLordParty")]
    static class SpawnLordPartyFillStackPatch
    {
        // Postfix to catch the newly created party
        static void Postfix(Hero hero, ref MobileParty __result)
        {
            var s = CezarDifficultyTweaksSettings.Instance;
            if (s == null) return;
            if (s.LordsRespawnFlat == 0 && s.LordsRespawnPercent == 0) return;
            if (hero == Hero.MainHero || hero?.Clan == Clan.PlayerClan || !hero.IsLord) return;        // skip player or player clan

            var party = __result;
            // compute how many troops we want
            var heroes = party.MemberRoster.GetTroopRoster().Where(h => h.Character.IsHero).Select(h => h.Character).ToList();
            int limit = party.LimitedPartySize - heroes.Count();
            int desired = 0;
            if (s.LordsRespawnPercent > 0) desired += (int)(limit * s.LordsRespawnPercent);
            if (s.LordsRespawnFlat > 0) desired += s.LordsRespawnFlat;
            if (desired <= 0) return;
            
            // pull out only the non-hero stacks
            var troopStacks = party.MemberRoster.GetTroopRoster()
                                  .Where(r => !r.Character.IsHero)
                                  .ToList();

            // delete all troops
            while (troopStacks.Count > 0)
            {
                var stack = troopStacks[0];

                // remove exactly one of that troop type
                party.MemberRoster.AddToCounts(stack.Character, -stack.Number);

                // update that stack’s count in our list
                    troopStacks.RemoveAt(0);
            }
            // Too few? randomly add troops until we're at desired
            if (desired > 0)
            {
                Settlement settlement = HeroHelper.GetClosestSettlement(hero) ?? SettlementHelper.GetBestSettlementToSpawnAround(hero);
                var cult = hero.Culture;
                if(!s.LordsRespawnCult && settlement != null)
                {
                    cult = settlement.Culture;
                }
                var troops = CharacterObject.All.Where(c => c.IsSoldier && c.Culture == cult && c.Tier >= s.LordsRespawnWay).ToList();
                if(troops == null && troops.Count == 0)
                {
                    troops = CharacterObject.All.Where(c => c.IsSoldier && c.Culture == cult).ToList();
                }
                int toAdd = desired;
                while (toAdd > 0)
                {
                    int idx = MBRandom.RandomInt(troops.Count);
                    var stack = troops[idx];
                    
                    party.MemberRoster.AddToCounts(stack, 1);
                    toAdd--;
                }
            }
        }
    }

    // 3) PLAYER INCREASED DAMAGE RECEIVED — like your RaceTweaks absorption example
    [HarmonyPatch(typeof(SandboxAgentApplyDamageModel), nameof(SandboxAgentApplyDamageModel.CalculateDamage))]
    static class PlayerDamageReceivedPatch
    {
        static void Postfix(ref float __result, in AttackInformation attackInformation)
        {
            var s = CezarDifficultyTweaksSettings.Instance;
            if (s == null) return;
            if (s.PlayerDamageFlat == 0 && s.PlayerDamageMultiplier == 0 && s.PlayerCompanionDamageFlat == 0 && s.PlayerCompanionDamageMultiplier == 0 && s.PlayerMemberDamageFlat == 0 && s.PlayerMemberDamageMultiplier == 0) return;

            var victimAgent = attackInformation.VictimAgent;
            // only care about heroes
            if (victimAgent == null || victimAgent.Character == null || !victimAgent.Character.IsHero) return;
            // find the Hero instance
            var victimHero = Campaign.Current?.AliveHeroes
                .FirstOrDefault(h => h.CharacterObject == victimAgent.Character);
            if (victimHero is null) return;

            // MAIN HERO
            if (victimHero == Hero.MainHero)
            {
                // apply multipler then flat
                if (s.PlayerDamageMultiplier != 0) __result *= s.PlayerDamageMultiplier;
                if (s.PlayerDamageFlat != 0) __result += s.PlayerDamageFlat;
                return;
            }

            // COMPANIONS
            if (victimHero.Occupation == Occupation.Wanderer
             && victimHero.Clan == Clan.PlayerClan)
            {
                // apply multipler then flat
                if (s.PlayerCompanionDamageMultiplier != 0) __result *= s.PlayerCompanionDamageMultiplier;
                if (s.PlayerCompanionDamageFlat != 0) __result += s.PlayerCompanionDamageFlat;
                return;
            }

            // OTHER PLAYER‐CLAN MEMBERS
            if (victimHero.Clan == Clan.PlayerClan)
            {
                // apply multipler then flat
                if (s.PlayerMemberDamageMultiplier != 0) __result *= s.PlayerMemberDamageMultiplier;
                if (s.PlayerMemberDamageFlat != 0) __result += s.PlayerMemberDamageFlat;
            }
        }
    }

    // --- 1) XP MULTIPLIER for NON-PLAYER-CLAN HEROES ---
    [HarmonyPatch(typeof(DefaultGenericXpModel), nameof(DefaultGenericXpModel.GetXpMultiplier))]
    static class GenericXpMultiplierPatch
    {
        static void Postfix(Hero hero, ref float __result)
        {
            var s = CezarDifficultyTweaksSettings.Instance;
            if (s == null) return;
            if (s.NonPlayerHeroXpMultiplier == 0) return;
            // only adjust for heroes outside the player's clan:
            if (hero.Clan == null) return;
            if (!s.NonPlayerHeroXpPlayer && hero.Clan == Clan.PlayerClan) return;
            if (!hero.IsHumanPlayerCharacter)
            {
                __result = s.NonPlayerHeroXpMultiplier;
            }
        }
    }

    [HarmonyPatch(typeof(Agent), nameof(Agent.Defensiveness), MethodType.Getter)]
    static class DefensivenessPatch
    {
        static void Postfix(Agent __instance, ref float __result)
        {
            var s = CezarDifficultyTweaksSettings.Instance;
            if (s == null || s.Defensiveness <= 0 || !s.AiTweaksEnabled) return;

            // apply your multiplier (0 disables, 1 leaves as-is, etc)
            __result *= s.Defensiveness;
        }
    }


    [HarmonyPatch(typeof(AgentStatCalculateModel), "SetAiRelatedProperties")]
    static class AiTweaksPostfix
    {
        static void Postfix(
            Agent agent,
            AgentDrivenProperties agentDrivenProperties,
            WeaponComponentData equippedItem,
            WeaponComponentData secondaryItem,
            AgentStatCalculateModel __instance)
        {
            var s = CezarDifficultyTweaksSettings.Instance;
            if (__instance == null || (!agent.Mission.IsFieldBattle && !agent.Mission.IsSiegeBattle && !agent.Mission.IsSallyOutBattle) || s == null || !s.AiTweaksEnabled) return;

            bool isPlayerTroop = agent.Team.IsPlayerTeam;
            if (isPlayerTroop && !s.AffectPlayerTroops) return;
            //AI level mult
            var aiLevelMultField = AccessTools.Field(typeof(AgentStatCalculateModel), "_AILevelMultiplier");
            var AILevelMult = (float)aiLevelMultField.GetValue(__instance);
            
            // Diff modifier
            var diffmod = __instance.GetDifficultyModifier();

            if (!s.RealisticBlock)
            {
                agentDrivenProperties.SetStat(DrivenProperty.UseRealisticBlocking, (agent.Controller != Agent.ControllerType.Player) ? 0f : 1f);
            }

            if(s.CalculateAIAttackOnDecideMaxValue != 0)
            {
                var calc = s.CalculateAIAttackOnDecideMaxValue;
                if (diffmod < 0.5f) calc /= 3;
                float dDecideOn = agentDrivenProperties.AIAttackOnDecideChance - MathF.Clamp(0.1f * calc * (3f - agent.Defensiveness), 0.05f, 1f);
                agentDrivenProperties.AIAttackOnDecideChance = MathF.Clamp(0.1f * calc * (3f - agent.Defensiveness), 0.05f, 1f) + dDecideOn;

            }
            
            // === REAPPLY + OUR BONUS + COMPUTE OUR DELTAS ===
            if (s.RangedAILevelMultipler != 0)
            {
                SkillObject skill = (equippedItem == null) ? DefaultSkills.Athletics : equippedItem.RelevantSkill;
                int effectiveSkill = __instance.GetEffectiveSkill(agent, skill);
                float num2 = MBMath.ClampFloat(CalculateAILevel(effectiveSkill, diffmod) * AILevelMult, 0f, 1f);
                float num4 = 1f - num2;

                // deltas
                float dHorse = agentDrivenProperties.AiRangedHorsebackMissileRange - (0.3f + 0.4f * num2);
                float dShoot = agentDrivenProperties.AiShootFreq - (0.3f + 0.7f * num2);
                float dWait = agentDrivenProperties.AiWaitBeforeShootFactor - (agent.PropertyModifiers.resetAiWaitBeforeShootFactor
                                     ? 0f : (1f - 0.5f * num2));
                float dErrorMin = agentDrivenProperties.AiRangerLeadErrorMin - (-num4 * 0.35f);
                float dErrorMax = agentDrivenProperties.AiRangerLeadErrorMax - (num4 * 0.2f);
                float dVerticalErrorMultiplier = agentDrivenProperties.AiRangerVerticalErrorMultiplier - (num4 * 0.1f);
                float dHorizontalErrorMultiplier = agentDrivenProperties.AiRangerHorizontalErrorMultiplier - (num4 * 0.0349065848f);
                float dMoveDelay = agentDrivenProperties.AiMovementDelayFactor - (4f / (3f + num2));

                // new level
                num2 = MBMath.ClampFloat(CalculateAILevel(effectiveSkill, diffmod) * s.RangedAILevelMultipler, 0f, 1f);
                num4 = 1f - num2;

                // reapply
                agentDrivenProperties.AiMovementDelayFactor = (4f / (3f + num2)) + dMoveDelay;
                agentDrivenProperties.AiRangedHorsebackMissileRange = (0.3f + 0.4f * num2) + dHorse;
                agentDrivenProperties.AiShootFreq = (0.3f + 0.7f * num2) + dShoot;
                agentDrivenProperties.AiWaitBeforeShootFactor = (agent.PropertyModifiers.resetAiWaitBeforeShootFactor
                                                     ? 0f : (1f - 0.5f * num2)) + dWait;
                agentDrivenProperties.AiRangerLeadErrorMin = (-num4 * 0.35f) + dErrorMin;
                agentDrivenProperties.AiRangerLeadErrorMax = (num4 * 0.2f) + dErrorMax;
                agentDrivenProperties.AiRangerVerticalErrorMultiplier = (num4 * 0.1f) + dVerticalErrorMultiplier;
                agentDrivenProperties.AiRangerHorizontalErrorMultiplier = (num4 * 0.0349065848f) + dHorizontalErrorMultiplier;
            }
            if (s.MeleeAILevelMultipler != 0f)
            {
                int meleeSkill = __instance.GetEffectiveSkill(agent, GetMeleeSkill(agent, equippedItem, secondaryItem));
                float num = MBMath.ClampFloat(CalculateAILevel(meleeSkill, diffmod) * AILevelMult, 0f, 1f);
                float num3 = num + agent.Defensiveness;

                // deltas
                float dBlock = agentDrivenProperties.AIBlockOnDecideAbility - (MBMath.Lerp(0.5f, 0.99f, MBMath.ClampFloat(MathF.Pow(num, 0.5f), 0f, 1f), 1E-05f));
                float dParry = agentDrivenProperties.AIParryOnDecideAbility - (MBMath.Lerp(0.5f, 0.95f, MBMath.ClampFloat(num, 0f, 1f), 1E-05f));
                float dTryCham = agentDrivenProperties.AiTryChamberAttackOnDecide - ((num - 0.15f) * 0.1f);
                float dAtkParry = agentDrivenProperties.AiAttackOnParryTiming - (-0.2f + 0.3f * num);
                float dAtkOnParry = agentDrivenProperties.AIAttackOnParryChance - (0.08f - 0.02f * agent.Defensiveness);
                float dDecAtk = agentDrivenProperties.AIDecideOnAttackChance - (0.5f * agent.Defensiveness);
                float dParryAtk = agentDrivenProperties.AIParryOnAttackAbility - (MBMath.ClampFloat(num, 0f, 1f));
                float dKick = agentDrivenProperties.AiKick - (-0.1f + ((num > 0.4f) ? 0.4f : num));
                float dCalcTime = agentDrivenProperties.AiAttackCalculationMaxTimeFactor - (num);
                float dWhenHit = agentDrivenProperties.AiDecideOnAttackWhenReceiveHitTiming - (-0.25f * (1f - num));
                float dContAct = agentDrivenProperties.AiDecideOnAttackContinueAction - (-0.5f * (1f - num));
                float dContinue = agentDrivenProperties.AiDecideOnAttackingContinue - (0.1f * num);
                float dParryCnt = agentDrivenProperties.AIParryOnAttackingContinueAbility - (MBMath.Lerp(0.5f, 0.95f, MBMath.ClampFloat(num, 0f, 1f), 1E-05f));
                float dRealize = agentDrivenProperties.AIDecideOnRealizeEnemyBlockingAttackAbility - (MBMath.ClampFloat(MathF.Pow(num, 2.5f) - 0.1f, 0f, 1f));
                float dRealize2 = agentDrivenProperties.AIRealizeBlockingFromIncorrectSideAbility - (MBMath.ClampFloat(MathF.Pow(num, 2.5f) - 0.01f, 0f, 1f));
                float dShieldCh = agentDrivenProperties.AiAttackingShieldDefenseChance - (0.2f + 0.3f * num);
                float dShieldTm = agentDrivenProperties.AiAttackingShieldDefenseTimer - (-0.3f + 0.3f * num);
                float dRandDir = agentDrivenProperties.AiRandomizedDefendDirectionChance - (1f - MathF.Pow(num, 3f));
                float dFacing = agentDrivenProperties.AiFacingMissileWatch - (-0.96f + 0.06f * num);
                float dFly = agentDrivenProperties.AiFlyingMissileCheckRadius - (8f - 6f * num);
                float dHit = agentDrivenProperties.AISetNoAttackTimerAfterBeingHitAbility - MBMath.Lerp(0.33f, 1f, num, 1E-05f);
                float dPar = agentDrivenProperties.AISetNoAttackTimerAfterBeingParriedAbility - MBMath.Lerp(0.2f, 1f, num * num, 1E-05f);
                float dDefH = agentDrivenProperties.AISetNoDefendTimerAfterHittingAbility - MBMath.Lerp(0.1f, 0.99f, num * num, 1E-05f);
                float dDefP = agentDrivenProperties.AISetNoDefendTimerAfterParryingAbility - MBMath.Lerp(0.15f, 1f, num * num, 1E-05f);
                float dStun = agentDrivenProperties.AIEstimateStunDurationPrecision - (1f - MBMath.Lerp(0.2f, 1f, num, 1E-05f));
                float dHold = agentDrivenProperties.AIHoldingReadyMaxDuration - MBMath.Lerp(0.25f, 0f, MathF.Min(1f, num * 2f), 1E-05f);
                float dHoldV = agentDrivenProperties.AIHoldingReadyVariationPercentage - num;
                float dShieldDelay = agentDrivenProperties.AiRaiseShieldDelayTimeBase - (-0.75f + 0.5f * num);
                float dUseMissile = agentDrivenProperties.AiUseShieldAgainstEnemyMissileProbability - (0.1f + num * 0.6f + num3 * 0.2f);
                float dCheckMove = agentDrivenProperties.AiCheckMovementIntervalFactor - (0.005f * (1.1f - num));
                float dParryDec = agentDrivenProperties.AiParryDecisionChangeValue - (0.05f + 0.7f * num);
                float dDefCh = agentDrivenProperties.AiDefendWithShieldDecisionChanceValue - MathF.Min(2f, 0.5f + num + 0.6f * num3);
                float dSide = agentDrivenProperties.AiMoveEnemySideTimeValue - (-2.5f + 0.5f * num);
                float dMin = agentDrivenProperties.AiMinimumDistanceToContinueFactor - (2f + 0.3f * (3f - num));
                float dHear = agentDrivenProperties.AiHearingDistanceFactor - (1f + num);
                float dCharge = agentDrivenProperties.AiChargeHorsebackTargetDistFactor - (1.5f * (3f - num));



                // recalc with bonus
                num = MBMath.ClampFloat(CalculateAILevel(meleeSkill, diffmod) * s.MeleeAILevelMultipler, 0f, 1f);
                num3 = num + agent.Defensiveness;

                // reapply
                agentDrivenProperties.AiMoveEnemySideTimeValue = (-2.5f + 0.5f * num) + dSide;
                agentDrivenProperties.AiMinimumDistanceToContinueFactor = (2f + 0.3f * (3f - num)) + dMin;
                agentDrivenProperties.AiHearingDistanceFactor = (1f + num) + dHear;
                agentDrivenProperties.AiChargeHorsebackTargetDistFactor = (1.5f * (3f - num)) + dCharge;
                agentDrivenProperties.AiParryDecisionChangeValue = (0.05f + 0.7f * num) + dParryDec;
                agentDrivenProperties.AiDefendWithShieldDecisionChanceValue = MathF.Min(2f, 0.5f + num + 0.6f * num3) + dDefCh;
                agentDrivenProperties.AiCheckMovementIntervalFactor = (0.005f * (1.1f - num)) + dCheckMove;
                agentDrivenProperties.AiRaiseShieldDelayTimeBase = (-0.75f + 0.5f * num) + dShieldDelay;
                agentDrivenProperties.AiUseShieldAgainstEnemyMissileProbability = (0.1f + num * 0.6f + num3 * 0.2f) + dUseMissile;
                agentDrivenProperties.AIEstimateStunDurationPrecision = (1f - MBMath.Lerp(0.2f, 1f, num, 1E-05f)) + dStun;
                agentDrivenProperties.AIHoldingReadyMaxDuration = MBMath.Lerp(0.25f, 0f, MathF.Min(1f, num * 2f), 1E-05f) + dHold;
                agentDrivenProperties.AIHoldingReadyVariationPercentage = num + dHoldV;
                agentDrivenProperties.AISetNoAttackTimerAfterBeingHitAbility = MBMath.Lerp(0.33f, 1f, num, 1E-05f) + dHit;
                agentDrivenProperties.AISetNoAttackTimerAfterBeingParriedAbility = MBMath.Lerp(0.2f, 1f, num * num, 1E-05f) + dPar;
                agentDrivenProperties.AISetNoDefendTimerAfterHittingAbility = MBMath.Lerp(0.1f, 0.99f, num * num, 1E-05f) + dDefH;
                agentDrivenProperties.AISetNoDefendTimerAfterParryingAbility = MBMath.Lerp(0.15f, 1f, num * num, 1E-05f) + dDefP;
                agentDrivenProperties.AiFacingMissileWatch = (-0.96f + 0.06f * num) + dFacing;
                agentDrivenProperties.AiFlyingMissileCheckRadius = (8f - 6f * num) + dFly;
                agentDrivenProperties.AIBlockOnDecideAbility = MBMath.Lerp(0.5f, 0.99f, MBMath.ClampFloat(MathF.Pow(num, 0.5f), 0f, 1f), 1E-05f) + dBlock;
                agentDrivenProperties.AIParryOnDecideAbility = MBMath.Lerp(0.5f, 0.95f, MBMath.ClampFloat(num, 0f, 1f), 1E-05f) + dParry;
                agentDrivenProperties.AiTryChamberAttackOnDecide = (num - 0.15f) * 0.1f + dTryCham;
                agentDrivenProperties.AiAttackOnParryTiming = -0.2f + 0.3f * num + dAtkParry;
                agentDrivenProperties.AIAttackOnParryChance = 0.08f - 0.02f * agent.Defensiveness + dAtkOnParry;
                agentDrivenProperties.AIDecideOnAttackChance = 0.5f * agent.Defensiveness + dDecAtk;
                agentDrivenProperties.AIParryOnAttackAbility = MBMath.ClampFloat(num, 0f, 1f) + dParryAtk;
                agentDrivenProperties.AiKick = -0.1f + ((num > 0.4f) ? 0.4f : num) + dKick;
                agentDrivenProperties.AiAttackCalculationMaxTimeFactor = num + dCalcTime;
                agentDrivenProperties.AiDecideOnAttackWhenReceiveHitTiming = -0.25f * (1f - num) + dWhenHit;
                agentDrivenProperties.AiDecideOnAttackContinueAction = -0.5f * (1f - num) + dContAct;
                agentDrivenProperties.AiDecideOnAttackingContinue = 0.1f * num + dContinue;
                agentDrivenProperties.AIParryOnAttackingContinueAbility = MBMath.Lerp(0.5f, 0.95f, MBMath.ClampFloat(num, 0f, 1f), 1E-05f) + dParryCnt;
                agentDrivenProperties.AIDecideOnRealizeEnemyBlockingAttackAbility = MBMath.ClampFloat(MathF.Pow(num, 2.5f) - 0.1f, 0f, 1f) + dRealize;
                agentDrivenProperties.AIRealizeBlockingFromIncorrectSideAbility = MBMath.ClampFloat(MathF.Pow(num, 2.5f) - 0.01f, 0f, 1f) + dRealize2;
                agentDrivenProperties.AiAttackingShieldDefenseChance = 0.2f + 0.3f * num + dShieldCh;
                agentDrivenProperties.AiAttackingShieldDefenseTimer = -0.3f + 0.3f * num + dShieldTm;
                agentDrivenProperties.AiRandomizedDefendDirectionChance = 1f - MathF.Pow(num, 3f) + dRandDir;
            }
        }



        private static SkillObject GetMeleeSkill(Agent agent, WeaponComponentData equippedItem, WeaponComponentData secondaryItem)
        {
            SkillObject skill = DefaultSkills.Athletics;
            if (equippedItem != null)
            {
                SkillObject relevantSkill = equippedItem.RelevantSkill;
                if (relevantSkill == DefaultSkills.OneHanded || relevantSkill == DefaultSkills.Polearm)
                {
                    skill = relevantSkill;
                }
                else if (relevantSkill == DefaultSkills.TwoHanded)
                {
                    skill = ((secondaryItem == null) ? DefaultSkills.TwoHanded : DefaultSkills.OneHanded);
                }
                else
                {
                    skill = DefaultSkills.OneHanded;
                }
            }
            return skill;
        }

        private static float CalculateAILevel(int relevantSkillLevel, float difficultyModifier)
        {
            return MBMath.ClampFloat((float)relevantSkillLevel / 300f * difficultyModifier, 0f, 1f);
        }

    }

    // Skills
    //Hero for skills
    public static class SkillRateContext
    {
        public static Hero CurrentHero;
    }

    [HarmonyPatch(
        typeof(DefaultCharacterDevelopmentModel),
        nameof(DefaultCharacterDevelopmentModel.CalculateLearningRate),
        new Type[]{
            typeof(Hero),
            typeof(SkillObject)
        }
    )]
    static class SkillRateHero
    {
        [HarmonyPrefix]
        public static void Prefix_SetCurrentHero(Hero hero, SkillObject skill)
        {
            SkillRateContext.CurrentHero = hero;
        }

        [HarmonyPostfix]
        public static void Postfix_ClearCurrentHero(Hero hero, SkillObject skill)
        {
            // Clear unconditionally (even if hero is null or missing in JSON)
            SkillRateContext.CurrentHero = null;
        }
    }
    
    // 1) XP–Gain Rate (Hero + Skill) override
    [HarmonyPatch(
        typeof(DefaultCharacterDevelopmentModel),
        nameof(DefaultCharacterDevelopmentModel.CalculateLearningRate),
        new Type[]{
            typeof(int),      // attributeValue
            typeof(int),      // focusValue
            typeof(int),      // skillValue
            typeof(int),      // characterLevel
            typeof(TextObject),
            typeof(bool)      // includeDescriptions
        }
    )]
    static class DetailedLearningRateFlatPatch
    {
        [HarmonyPostfix]
        public static void Postfix(
            int attributeValue,
            int focusValue,
            int skillValue,
            int characterLevel,
            TextObject attributeName,
            bool includeDescriptions,
            ref ExplainedNumber __result)
        {
            try
            {

                var hero = SkillRateContext.CurrentHero;

                if (hero == null || hero == Hero.MainHero) return;

                var s = CezarDifficultyTweaksSettings.Instance;

                if (s == null)
                    return;
                if (!s.NonPlayerHeroSkillRatePlayer && hero.Clan == Clan.PlayerClan) return;

                // never go negative:
                var ratio = MathF.Max(-1f, s.NonPlayerHeroSkillRateMultiplier);
                // add percent
                var factor = __result.ResultNumber * ratio;

                // Add a flat bonus to the explained number:
                __result.Add(
                    (float)factor,
                    null);

                // Optional: clamp so rate never goes below zero
                __result.LimitMin(0f);


            }
            catch (Exception ex)
            {
                InformationManager.DisplayMessage(
                    new InformationMessage(
                        $"CezarDifficulty LearnRate patch error: {ex.Message}"));
            }
        }
    }

    [HarmonyPatch(
        typeof(DefaultCharacterDevelopmentModel),
        nameof(DefaultCharacterDevelopmentModel.CalculateLearningLimit),
        new Type[] { typeof(int), typeof(int), typeof(TextObject), typeof(bool) }
    )]
    static class LearningLimitPatch
    {
        public static void Postfix(
            int attributeValue,
            int focusValue,
            TextObject attributeName,
            bool includeDescriptions,
            ref ExplainedNumber __result)
        {
            try
            {

                var hero = SkillRateContext.CurrentHero;

                if (hero == null || hero == Hero.MainHero) return;

                var s = CezarDifficultyTweaksSettings.Instance;

                if (s == null)
                    return;
                if (!s.NonPlayerHeroSkillLimitPlayer && hero.Clan == Clan.PlayerClan) return;


                // never go negative:
                var ratio = MathF.Max(-1f, s.NonPlayerHeroSkillLimitMultiplier);
                // add percent
                var factor = __result.ResultNumber * ratio;

                __result.Add(
                    (float)factor,
                    null);

            }
            catch (Exception ex)
            {
                InformationManager.DisplayMessage(
                    new InformationMessage(
                        $"CezarDifficulty LearningLimit patch error: {ex.Message}"));
            }
        }
    }

    [HarmonyPatch(typeof(CampaignEvents), nameof(CampaignEvents.OnHeroComesOfAge))]
    static class OnHeroComesOfAge_Postfix
    {
        static List<SkillObject> skills = new List<SkillObject>();

        static void Postfix(Hero hero)
        {
            var s = CezarDifficultyTweaksSettings.Instance;
            if (s == null) return;
            if (hero == null || hero == Hero.MainHero) return;
            if(!s.SkillHeroPlayer && hero.Clan == Clan.PlayerClan) return;
            if ((s.MinSkillHero <= 0 && s.MaxSkillHero <= 0) || (s.MinSkillHero > s.MaxSkillHero)) return;
            if(skills == null || skills.Count == 0)
            {
                skills = MBObjectManager.Instance.GetObjectTypeList<SkillObject>();
            }
            if (!skills.Any()) return;

            foreach(var skill in skills)
            {
                var heroSkill = hero.GetSkillValue(skill);
                if(heroSkill < s.MinSkillHero || heroSkill > s.MaxSkillHero)
                {
                    var newSkill = MBRandom.RandomInt(s.MinSkillHero, s.MaxSkillHero);
                    hero.SetSkillValue(skill, newSkill);
                }
            }
            
        }
    }

    
    //Trade
    [HarmonyPatch(typeof(DefaultTradeItemPriceFactorModel), nameof(DefaultTradeItemPriceFactorModel.GetPrice))]
    static class DefaultTradeItemPriceFactorModel_GetPrice_Postfix
    {
        static void Postfix(
            EquipmentElement itemRosterElement,
            MobileParty clientParty,
            PartyBase merchant,
            bool isSelling,
            float inStoreValue,
            float supply,
            float demand,
            ref int __result)
        {
            var s = CezarDifficultyTweaksSettings.Instance;
            if (s == null) return;

            // only affect player party (change condition if you want other parties affected)
            bool isPlayer = clientParty != null && clientParty.IsMainParty;
            if (!isPlayer) return;

            // sold = player sells to merchant (player receives money) -> use SoldMult
            if (isSelling && s.SoldMult > 0f)
            {
                // Multiply finalized price and keep >=1
                float modified = __result * s.SoldMult;
                __result = Math.Max(1, (int)MathF.Floor(modified));
            }
            // buy = player buys from merchant -> use BoughtMult
            else if (!isSelling && s.BoughtMult > 0f)
            {
                float modified = __result * s.BoughtMult;
                __result = Math.Max(1, (int)MathF.Ceiling(modified));
            }
        }
    }

    //Recruit
    [HarmonyPatch(typeof(DefaultDifficultyModel), nameof(DefaultDifficultyModel.GetPlayerRecruitSlotBonus))]
    static class DefaultDifficultyModel_GetPlayerRecruitSlotBonus_Postfix
    {
        static void Postfix(
            ref int __result)
        {
            var s = CezarDifficultyTweaksSettings.Instance;
            if (s == null || s.RecruitSlot == 0) return;

            __result -= s.RecruitSlot;
        }
    }

    [HarmonyPatch(typeof(DefaultPartyWageModel), nameof(DefaultPartyWageModel.GetTroopRecruitmentCost))]
    static class DefaultPartyWageModel_GetTroopRecruitmentCost_Postfix
    {
        static void Postfix(
            CharacterObject troop,
            Hero buyerHero,
            ref int __result)
        {
            var s = CezarDifficultyTweaksSettings.Instance;
            if (buyerHero == null || !buyerHero.IsHumanPlayerCharacter || s == null || s.RecruitCostMult == 0) return;

            __result = (int)(__result * s.RecruitCostMult);
        }
    }

    [HarmonyPatch(typeof(DefaultVolunteerModel), nameof(DefaultVolunteerModel.MaxVolunteerTier), MethodType.Getter)]
    static class DefaultVolunteerModel_MaxVolunteerTier_Postfix
    {
        static void Postfix(
            ref int __result)
        {
            var s = CezarDifficultyTweaksSettings.Instance;
            if (s == null || s.RecruitTier == 0) return;

            __result = s.RecruitTier;
        }
    }

    //Bandits
    [HarmonyPatch(typeof(DefaultBanditDensityModel), nameof(DefaultBanditDensityModel.NumberOfMaximumLooterParties), MethodType.Getter)]
    static class DefaultBanditDensityModel_NumberOfMaximumLooterParties_Postfix
    {
        static void Postfix(
            ref int __result)
        {
            var s = CezarDifficultyTweaksSettings.Instance;
            if (s == null || s.BanditsMaxParties == 0) return;

            __result = s.BanditsMaxParties;
        }
    }

    [HarmonyPatch(typeof(DefaultBanditDensityModel), nameof(DefaultBanditDensityModel.NumberOfMaximumBanditPartiesInEachHideout), MethodType.Getter)]
    static class DefaultBanditDensityModel_NumberOfMaximumBanditPartiesInEachHideout_Postfix
    {
        static void Postfix(
            ref int __result)
        {
            var s = CezarDifficultyTweaksSettings.Instance;
            if (s == null || s.BanditsMaxPartiesInHideout == 0) return;

            __result = s.BanditsMaxPartiesInHideout;
        }
    }

    [HarmonyPatch(typeof(DefaultBanditDensityModel), nameof(DefaultBanditDensityModel.NumberOfMaximumBanditPartiesAroundEachHideout), MethodType.Getter)]
    static class DefaultBanditDensityModel_NumberOfMaximumBanditPartiesAroundEachHideout_Postfix
    {
        static void Postfix(
            ref int __result)
        {
            var s = CezarDifficultyTweaksSettings.Instance;
            if (s == null || s.BanditsMaxPartiesAroundHideout == 0) return;

            __result = s.BanditsMaxPartiesAroundHideout;
        }
    }

    [HarmonyPatch(typeof(DefaultBanditDensityModel), nameof(DefaultBanditDensityModel.NumberOfMaximumHideoutsAtEachBanditFaction), MethodType.Getter)]
    static class DefaultBanditDensityModel_NumberOfMaximumHideoutsAtEachBanditFaction_Postfix
    {
        static void Postfix(
            ref int __result)
        {
            var s = CezarDifficultyTweaksSettings.Instance;
            if (s == null || s.BanditsMaxHideouts == 0) return;

            __result = s.BanditsMaxHideouts;
        }
    }

    [HarmonyPatch(typeof(DefaultBanditDensityModel), nameof(DefaultBanditDensityModel.NumberOfInitialHideoutsAtEachBanditFaction), MethodType.Getter)]
    static class DefaultBanditDensityModel_NumberOfInitialHideoutsAtEachBanditFaction_Postfix
    {
        static void Postfix(
            ref int __result)
        {
            var s = CezarDifficultyTweaksSettings.Instance;
            if (s == null || s.BanditsInitialHideouts == 0) return;

            __result = s.BanditsInitialHideouts;
        }
    }

    //Bandit Speed
    [HarmonyPatch(typeof(MobileParty), nameof(MobileParty.Speed), MethodType.Getter)]
    static class MobileParty_SpeedExplained_Postfix
    {

        static void Postfix(
            MobileParty __instance,
            ref float __result)
        {
            var s = CezarDifficultyTweaksSettings.Instance;
            if (s == null || s.BanditsSpeedMult == 0 || __result == 0 || !__instance.IsBandit) return;
            var minSpeed = Campaign.Current?.Models?.PartySpeedCalculatingModel?.MinimumSpeed ?? 1f;
            __result += __result * s.BanditsSpeedMult;
            __result = MathF.Max(minSpeed, __result);
        }
    }

    //Bandits Spawns
    [HarmonyPatch(typeof(BanditPartyComponent), nameof(BanditPartyComponent.CreateBanditParty))]
    static class CreateBanditPartyPatch
    {

        // Postfix to catch the newly created party
        static void Postfix(string stringId, Clan clan, Hideout hideout, bool isBossParty, ref MobileParty __result)
        {
            var s = CezarDifficultyTweaksSettings.Instance;
            if (s == null || clan == null) return;
            if (s.BanditsSpawnMax == 0 || s.BanditsSpawnMin == 0 || s.BanditsSpawnMin > s.BanditsSpawnMax) return;

            var party = __result;
            // compute how many troops we want
            int desired = MBRandom.RandomInt(s.BanditsSpawnMin, s.BanditsSpawnMax);
            if (desired <= 0) return;

            // pull out only the non-hero stacks
            var troopStacks = party.MemberRoster.GetTroopRoster()
                                  .Where(r => !r.Character.IsHero)
                                  .ToList();

            // delete all troops
            while (troopStacks.Count > 0)
            {
                var stack = troopStacks[0];

                // remove all
                party.MemberRoster.AddToCounts(stack.Character, -stack.Number);

                // update that stack’s count in our list
                troopStacks.RemoveAt(0);
            }
            // Too few? randomly add troops until we're at desired
            if (desired > 0)
            {
                var cult = clan.Culture;
                var troops = !s.BanditsSpawnMixed ? CharacterObject.All.Where(c => !c.IsHero && !c.IsTemplate && (c.IsSoldier || c.Occupation == Occupation.Bandit) && c.Culture == cult && c.Tier >= s.BanditsSpawnMinTier && c.Tier <= s.BanditsSpawnMaxTier).ToList() : CharacterObject.All.Where(c => !c.IsHero && !c.IsTemplate && (c.IsSoldier || c.Occupation == Occupation.Bandit) && c.Tier >= s.BanditsSpawnMinTier && c.Tier <= s.BanditsSpawnMaxTier).ToList();
                if (troops == null && troops.Count == 0)
                {
                    troops = !s.BanditsSpawnMixed ? CharacterObject.All.Where(c => !c.IsHero && !c.IsTemplate && (c.IsSoldier || c.Occupation == Occupation.Bandit) && c.Culture == cult).ToList() : CharacterObject.All.Where(c => !c.IsHero && !c.IsTemplate && (c.IsSoldier || c.Occupation == Occupation.Bandit)).ToList();
                }
                int toAdd = desired;
                while (toAdd > 0)
                {
                    int idx = MBRandom.RandomInt(troops.Count);
                    var stack = troops[idx];

                    party.MemberRoster.AddToCounts(stack, 1);
                    toAdd--;
                }
            }
        }
    }
    [HarmonyPatch(typeof(BanditPartyComponent), nameof(BanditPartyComponent.CreateLooterParty))]
    static class CreateLooterPartyPatch
    {

        // Postfix to catch the newly created party
        static void Postfix(string stringId, Clan clan, Settlement relatedSettlement, bool isBossParty, ref MobileParty __result)
        {
            var s = CezarDifficultyTweaksSettings.Instance;
            if (s == null || clan == null) return;
            if (s.BanditsSpawnMax == 0 || s.BanditsSpawnMin == 0 || s.BanditsSpawnMin > s.BanditsSpawnMax) return;

            var party = __result;
            // compute how many troops we want
            int desired = MBRandom.RandomInt(s.BanditsSpawnMin, s.BanditsSpawnMax);
            if (desired <= 0) return;

            // pull out only the non-hero stacks
            var troopStacks = party.MemberRoster.GetTroopRoster()
                                  .Where(r => !r.Character.IsHero)
                                  .ToList();

            // delete all troops
            while (troopStacks.Count > 0)
            {
                var stack = troopStacks[0];

                // remove all
                party.MemberRoster.AddToCounts(stack.Character, -stack.Number);

                // update that stack’s count in our list
                troopStacks.RemoveAt(0);
            }
            // Too few? randomly add troops until we're at desired
            if (desired > 0)
            {
                var cult = clan.Culture;
                var troops = !s.BanditsSpawnMixed ? CharacterObject.All.Where(c => !c.IsHero && !c.IsTemplate && (c.IsSoldier || c.Occupation == Occupation.Bandit) && c.Culture == cult && c.Tier >= s.BanditsSpawnMinTier && c.Tier <= s.BanditsSpawnMaxTier).ToList() : CharacterObject.All.Where(c => !c.IsHero && !c.IsTemplate && (c.IsSoldier || c.Occupation == Occupation.Bandit) && c.Tier >= s.BanditsSpawnMinTier && c.Tier <= s.BanditsSpawnMaxTier).ToList();
                if (troops == null && troops.Count == 0)
                {
                    troops = !s.BanditsSpawnMixed ? CharacterObject.All.Where(c => !c.IsHero && !c.IsTemplate && (c.IsSoldier || c.Occupation == Occupation.Bandit) && c.Culture == cult).ToList() : CharacterObject.All.Where(c => !c.IsHero && !c.IsTemplate && (c.IsSoldier || c.Occupation == Occupation.Bandit)).ToList();
                }
                int toAdd = desired;
                while (toAdd > 0)
                {
                    int idx = MBRandom.RandomInt(troops.Count);
                    var stack = troops[idx];

                    party.MemberRoster.AddToCounts(stack, 1);
                    toAdd--;
                }
            }
        }
    }




}
