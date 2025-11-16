using MCM.Abstractions.Attributes.v2;
using MCM.Abstractions.Attributes;
using MCM.Abstractions.Base.Global;
using TaleWorlds.Localization;

namespace CezarDifficultyTweaks
{
    public sealed class CezarDifficultyTweaksSettings : AttributeGlobalSettings<CezarDifficultyTweaksSettings>
    {
        public override string Id => "CezarDifficultyTweaksSettings";
        public override string DisplayName => new TextObject("{=CDT_TITLE}Cezar Difficulty Tweaks").ToString();
        public override string FolderName => "CezarDifficultyTweaksSettings";
        public override string FormatType => "json";

        [SettingPropertyFloatingInteger("{=CDT_GOLD_FLAT}Daily Gold Malus (flat)", 0f, 100000000f, Order = 0, RequireRestart = false,
            HintText = "{=CDT_GOLD_FLAT_H}A flat negative amount of gold subtracted from player treasury each day. (0 means disable) ")]
        [SettingPropertyGroup("{=MCM_DAILY_GOLD_MALUS}Daily Gold Malus")]
        public float DailyGoldMalusFlat { get; set; } = 0f;

        [SettingPropertyFloatingInteger("{=CDT_GOLD_MULT}Daily Gold Malus (multiplier)", 0f, 5f, "#0.##", Order = 1, RequireRestart = false,
            HintText = "{=CDT_GOLD_MULT_H}A multiplier applied to the daily net gold change malus which is the negative amount of gold subtracted from the player's treasury each day. (0 means disable)")]
        [SettingPropertyGroup("{=MCM_DAILY_GOLD_MALUS}Daily Gold Malus")]
        public float DailyGoldMalusMultiplier { get; set; } = 0f;

        [SettingPropertyFloatingInteger("{=CDT_GOLD_NPC_FLAT}Daily Gold Bonus for NPCs (flat)", 0f, 100000000f, Order = 0, RequireRestart = false,
            HintText = "{=CDT_GOLD_NPC_FLAT_H}A flat amount of gold added to lord's treasury each day. (0 means disable)")]
        [SettingPropertyGroup("{=MCM_DAILY_GOLD_BONUS}Daily Gold Bonus")]
        public float DailyGoldBonusFlat { get; set; } = 0f;

        [SettingPropertyFloatingInteger("{=CDT_GOLD_NPC_MULT}Daily Gold Bonus for NPCs (multiplier)", 0f, 5f, "#0.##", Order = 1, RequireRestart = false,
            HintText = "{=CDT_GOLD_NPC_MULT_H}A multiplier applied to daily net gold change plus which is the amount of gold added to lord's treasury each day. (0 means disable)")]
        [SettingPropertyGroup("{=MCM_DAILY_GOLD_BONUS}Daily Gold Bonus")]
        public float DailyGoldBonusMultiplier { get; set; } = 0f;

        [SettingPropertyFloatingInteger("{=CDT_LORDS_PERC}Lords Fillstack (% of max)", 0f, 5f, "#0%", Order = 0, RequireRestart = false,
            HintText = "{=CDT_LORDS_PERC_H}When a lord party is defeated, respawn them at this % of their max size. (0 means disable)")]
        [SettingPropertyGroup("{=MCM_LORDS_FILLSTACK}Lords Fill-Stack")]
        public float LordsRespawnPercent { get; set; } = 0f;

        [SettingPropertyInteger("{=CDT_LORDS_FLAT}Lords Fillstack (flat troops)", 0, 5000, Order = 1, RequireRestart = false,
            HintText = "{=CDT_LORDS_FLAT_H}Or respawn them with this many troops (0 means disable).")]
        [SettingPropertyGroup("{=MCM_LORDS_FILLSTACK}Lords Fill-Stack")]
        public int LordsRespawnFlat { get; set; } = 0;

        [SettingPropertyInteger("{=CDT_LORDS_WAY}Lords Fillstack Troops Tiers", 1, 7, Order = 2, RequireRestart = false,
            HintText = "{=CDT_LORDS_WAY_H}What minimum tier troops they should spawn with?(if you don't use mods with 6-7 tiers, don't set this to those values).")]
        [SettingPropertyGroup("{=MCM_LORDS_FILLSTACK}Lords Fill-Stack")]
        public int LordsRespawnWay { get; set; } = 1;

        [SettingPropertyBool("{=CDT_LORDS_CULTURE}Hero Culture", Order = 3, RequireRestart = false,
            HintText = "{=CDT_LORDS_CULTURE_H}Should troops have hero's culture or closest settlement culture?")]
        [SettingPropertyGroup("{=MCM_LORDS_FILLSTACK}Lords Fill-Stack")]
        public bool LordsRespawnCult { get; set; } = false;

        [SettingPropertyFloatingInteger("{=CDT_DMG_FLAT}Player Damage Received (flat)", 0f, 500f, Order = 0, RequireRestart = false,
            HintText = "{=CDT_DMG_FLAT_H}Extra flat damage always applied to player. (0 means disable)")]
        [SettingPropertyGroup("{=MCM_PLAYER_DAMAGE_RECEIVED}Player Damage Received")]
        public float PlayerDamageFlat { get; set; } = 0f;

        [SettingPropertyFloatingInteger("{=CDT_DMG_MULT}Player Damage Received (multiplier)", 0f, 5f, "#0.##", Order = 1, RequireRestart = false,
            HintText = "{=CDT_DMG_MULT_H}Multiplier on all damage taken by player. (0 means disable)")]
        [SettingPropertyGroup("{=MCM_PLAYER_DAMAGE_RECEIVED}Player Damage Received")]
        public float PlayerDamageMultiplier { get; set; } = 0f;

        [SettingPropertyFloatingInteger("{=CDT_DMG_FLAT_MEMBERS}Player Clan Members Damage Received (flat)", 0f, 500f, Order = 2, RequireRestart = false,
            HintText = "{=CDT_DMG_FLAT_MEMBERS_H}Extra flat damage always applied to player. (0 means disable)")]
        [SettingPropertyGroup("{=MCM_PLAYER_DAMAGE_RECEIVED}Player Damage Received")]
        public float PlayerMemberDamageFlat { get; set; } = 0f;

        [SettingPropertyFloatingInteger("{=CDT_DMG_Mult_MEMBERS}Player Clan Members Damage Received (multiplier)", 0f, 5f, "#0.##", Order = 3, RequireRestart = false,
            HintText = "{=CDT_DMG_Mult_MEMBERS_H}Multiplier on all damage taken by player. (0 means disable)")]
        [SettingPropertyGroup("{=MCM_PLAYER_DAMAGE_RECEIVED}Player Damage Received")]
        public float PlayerMemberDamageMultiplier { get; set; } = 0f;

        [SettingPropertyFloatingInteger("{=CDT_DMG_FLAT_COMP}Player Companions Damage Received (flat)", 0f, 500f, Order = 4, RequireRestart = false,
            HintText = "{=CDT_DMG_FLAT_COMP_H}Extra flat damage always applied to player. (0 means disable)")]
        [SettingPropertyGroup("{=MCM_PLAYER_DAMAGE_RECEIVED}Player Damage Received")]
        public float PlayerCompanionDamageFlat { get; set; } = 0f;

        [SettingPropertyFloatingInteger("{=CDT_DMG_Mult_COMP}Player Companions Damage Received (multiplier)", 0f, 5f, "#0.##", Order = 5, RequireRestart = false,
            HintText = "{=CDT_DMG_Mult_COMP_H}Multiplier on all damage taken by player. (0 means disable)")]
        [SettingPropertyGroup("{=MCM_PLAYER_DAMAGE_RECEIVED}Player Damage Received")]
        public float PlayerCompanionDamageMultiplier { get; set; } = 0f;

        // XP Multiplier for any Hero not in the Player’s Clan
        [SettingPropertyFloatingInteger(
            "{=CDT_XP_HERO_MULT}Non-Player Hero XP Multiplier",
            0f, 5f, "#0.##", Order = 6, RequireRestart = false,
            HintText = "{=CDT_XP_HERO_MULT_H}Multiply all experience gained by any hero who is NOT the player. (0 means disable, 1 is default in game)")]
        [SettingPropertyGroup("{=MCM_XP_TWEAKS}XP Tweaks")]
        public float NonPlayerHeroXpMultiplier { get; set; } = 0f;

        [SettingPropertyBool("{=CDT_XP_HERO_PLAYER}Player Clan (XP Mult)", Order = 7, RequireRestart = false,
            HintText = "{=CDT_XP_HERO_PLAYER_H}Should it be applied to player's companions and clan members?")]
        [SettingPropertyGroup("{=MCM_XP_TWEAKS}XP Tweaks")]
        public bool NonPlayerHeroXpPlayer { get; set; } = true;

        //Skill XP Multiplier for any Hero not in the Player’s Clan
        
        [SettingPropertyFloatingInteger(
            "{=CDT_SKILL_RATE_HERO_MULT}Non-Player Hero Skill Learning Rate Multiplier",
            0f, 5f, "#0.##", Order = 0, RequireRestart = false,
            HintText = "{=CDT_SKILL_RATE_HERO_MULT_H}Add multiplied Skill Learning Rate by this value as bonus for any hero who is NOT the player. (0 means disable, 1 is default in game)")]
        [SettingPropertyGroup("{=MCM_SKILL_XP_TWEAKS}Skill XP Tweaks")]
        public float NonPlayerHeroSkillRateMultiplier { get; set; } = 0f;

        [SettingPropertyBool("{=CDT_RATE_HERO_PLAYER}Player Clan (Skill Rate)", Order = 1, RequireRestart = false,
            HintText = "{=CDT_RATE_HERO_PLAYER}Should it be applied to player's companions and clan members?")]
        [SettingPropertyGroup("{=MCM_SKILL_XP_TWEAKS}Skill XP Tweaks")]
        public bool NonPlayerHeroSkillRatePlayer { get; set; } = true;

        [SettingPropertyFloatingInteger(
            "{=CDT_SKILL_LIMIT_HERO_MULT}Non-Player Hero Skill Learning Limit Bonus",
            0f, 5f, "#0.##", Order = 2, RequireRestart = false,
            HintText = "{=CDT_SKILL_LIMIT_HERO_MULT_H}Add multiplied Skill Learning Limit by this value as bonus for any hero who is NOT the player. (0 means disable, 1 is default in game)")]
        [SettingPropertyGroup("{=MCM_SKILL_XP_TWEAKS}Skill XP Tweaks")]
        public float NonPlayerHeroSkillLimitMultiplier { get; set; } = 0f;

        [SettingPropertyBool("{=CDT_LIMIT_HERO_PLAYER}Player Clan (Skill Limit)", Order = 3, RequireRestart = false,
            HintText = "{=CDT_XP_LIMIT_PLAYER}Should it be applied to player's companions and clan members?")]
        [SettingPropertyGroup("{=MCM_SKILL_XP_TWEAKS}Skill XP Tweaks")]
        public bool NonPlayerHeroSkillLimitPlayer { get; set; } = true;

        [SettingPropertyInteger(
            "{=CDT_SKILL_MIN_HERO}Min Skill Proficiency On Comes of Age",
            0, 400, Order = 4, RequireRestart = false,
            HintText = "{=CDT_SKILL_MIN_HERO_H}Min Skill Proficiency for each skill, for any hero who comes of age and is NOT the player.(Max must be higher than Min) (0 means disable)")]
        [SettingPropertyGroup("{=MCM_SKILL_XP_TWEAKS}Skill XP Tweaks")]
        public int MinSkillHero { get; set; } = 0;

        [SettingPropertyInteger(
            "{=CDT_SKILL_MAX_HERO}Max Skill Proficiency On Comes of Age",
            0, 400, Order = 5, RequireRestart = false,
            HintText = "{=CDT_SKILL_MAX_HERO_H}Max Skill Proficiency for each skill, for any hero who comes of age and is NOT the player. (Max must be higher than Min) (0 means disable)")]
        [SettingPropertyGroup("{=MCM_SKILL_XP_TWEAKS}Skill XP Tweaks")]
        public int MaxSkillHero { get; set; } = 0;

        [SettingPropertyBool("{=CDT_SKILL_HERO_PLAYER}Player Clan (Skill Proficiency)", Order = 6, RequireRestart = false,
            HintText = "{=CDT_SKILL_HERO_PLAYER}Should it be applied to player's companions and clan members?")]
        [SettingPropertyGroup("{=MCM_SKILL_XP_TWEAKS}Skill XP Tweaks")]
        public bool SkillHeroPlayer { get; set; } = true;

        
        // ----------------------- AI Tweaks -----------------------
        // Master switch
        [SettingPropertyBool("{=CDT_AI_ENABLE}Enable AI Tweaks", Order = 0, RequireRestart = false,
            HintText = "{=CDT_AI_ENABLE_H}Turn all AI adjustments on or off.")]
        [SettingPropertyGroup("{=MCM_AI_TWEAKS}AI Tweaks")]
        public bool AiTweaksEnabled { get; set; } = false;

        // Apply to player troops?
        [SettingPropertyBool("{=CDT_AI_INCLUDE_PLAYER}Also affect player troops", Order = 1, RequireRestart = false,
            HintText = "{=CDT_AI_INCLUDE_PLAYER_H}If off, only enemy AI is tweaked.")]
        [SettingPropertyGroup("{=MCM_AI_TWEAKS}AI Tweaks")]
        public bool AffectPlayerTroops { get; set; } = false;

        // Melee bonuses
        [SettingPropertyFloatingInteger("{=CDT_AI_MELEE_LEVEL}Melee AI Level (multipler)", 0f, 5f, "#0.##", Order = 2, RequireRestart = false,
            HintText = "{=CDT_AI_MELEE_LEVEL_H}Multiplier on the melee‐related properties (parry/block/attack timing). (0 means disable)")]
        [SettingPropertyGroup("{=MCM_AI_TWEAKS}AI Tweaks/{=MCM_MELEE}Melee")]
        public float MeleeAILevelMultipler { get; set; } = 0f;

        // Ranged bonuses
        [SettingPropertyFloatingInteger("{=CDT_AI_RANGED_LEVEL}Ranged AI Level (multipler)", 0f, 5f, "#0.##", Order = 3, RequireRestart = false,
            HintText = "{=CDT_AI_RANGED_LEVEL_H}Multiplier on the ranged‐related properties (shoot‐freq, watch range). (0 means disable)")]
        [SettingPropertyGroup("{=MCM_AI_TWEAKS}AI Tweaks/{=MCM_RANGED}Ranged")]
        public float RangedAILevelMultipler { get; set; } = 0f;

        // Calc AI Attack chance
        [SettingPropertyFloatingInteger("{=CDT_AI_CHANCE_ATTACK}Decide Attack Chance", 0f, 5f, "#0.##", Order = 4, RequireRestart = false,
            HintText = "{=CDT_AI_CHANCE_ATTACK_H}AI Decide Attack Max Value. (if DifficultyModifier < 0.5f, then this value will be divided by 3, 0 means disable)")]
        [SettingPropertyGroup("{=MCM_AI_TWEAKS}AI Tweaks/{=MCM_ATTACK_CHANCE}Attack Chance")]
        public float CalculateAIAttackOnDecideMaxValue { get; set; } = 0f;

        // Use Realistic Blocking
        [SettingPropertyBool("{=CDT_AI_BLOCK}Realistic Blocking", Order = 5, RequireRestart = false,
            HintText = "{=CDT_AI_BLOCK_H}Should AI use realistic blocking? (true is default in game.)")]
        [SettingPropertyGroup("{=MCM_AI_TWEAKS}AI Tweaks/{=MCM_REALISTIC_BLOCKING}Realistic Blocking")]
        public bool RealisticBlock { get; set; } = true;

        // Agent Defensiveness
        [SettingPropertyFloatingInteger("{=CDT_AI_DEFENSIVENESS}Agent Defensiveness (multipler)", 0f, 5f, "#0.##", Order = 6, RequireRestart = false,
            HintText = "{=CDT_AI_DEFENSIVENESS_H}Multiplier on the Agent Defensiveness (0 means disable)")]
        [SettingPropertyGroup("{=MCM_AI_TWEAKS}AI Tweaks/{=MCM_AGENT_DEFENSIVENESS}Agent Defensiveness")]
        public float Defensiveness { get; set; } = 0f;

        // ----------------------- Trade Tweaks -----------------------
        
        [SettingPropertyFloatingInteger(
            "{=CDT_SELL}Sold Items Cost Multiplier",
            0f, 10f, "#0.##", Order = 0, RequireRestart = false,
            HintText = "{=CDT_SELL_H}Multiply Sold Items Cost for player. (0 means disable, 1 is default in game)")]
        [SettingPropertyGroup("{=MCM_TRADE_TWEAKS}Trade Tweaks")]
        public float SoldMult { get; set; } = 0f;

        [SettingPropertyFloatingInteger(
            "{=CDT_BUY}Bought Items Cost Multiplier",
            0f, 10f, "#0.##", Order = 1, RequireRestart = false,
            HintText = "{=CDT_BUY_H}Multiply Bought Items Cost for player. (0 means disable, 1 is default in game)")]
        [SettingPropertyGroup("{=MCM_TRADE_TWEAKS}Trade Tweaks")]
        public float BoughtMult { get; set; } = 0f;

        // ----------------------- Recruit Tweaks -----------------------

        [SettingPropertyInteger(
            "{=CDT_RECRUIT_SLOT}Substract Recruit Slots For Player",
            0, 10, Order = 0, RequireRestart = false,
            HintText = "{=CDT_RECRUIT_SLOT_H}Substract our value from Difficulty Slot. E.g. We play on very easy which use 2 as slot number with option set on 10, so it will be = 2 - 10 = -8. Easy use 1 and the rest 0. (0 means disable)")]
        [SettingPropertyGroup("{=MCM_RECRUIT_TWEAKS}Recruit Tweaks")]
        public int RecruitSlot { get; set; } = 0;

        [SettingPropertyFloatingInteger(
            "{=CDT_RECRUIT_COST}Recruit Cost Multiplier",
            0f, 100f, "#0.##", Order = 1, RequireRestart = false,
            HintText = "{=CDT_RECRUIT_COST_H}Multiply cost of recruiting for player. (0 means disable, 1 is default in game)")]
        [SettingPropertyGroup("{=MCM_RECRUIT_TWEAKS}Recruit Tweaks")]
        public float RecruitCostMult { get; set; } = 0f;

        [SettingPropertyInteger(
            "{=CDT_RECRUIT_MAX_TIER}Max Tier For Volunteers",
            0, 10, Order = 2, RequireRestart = false,
            HintText = "{=CDT_RECRUIT_MAX_TIER_H}The maximum tier for volunteers in villages and towns. Native use 4. (0 means disable)")]
        [SettingPropertyGroup("{=MCM_RECRUIT_TWEAKS}Recruit Tweaks")]
        public int RecruitTier { get; set; } = 0;

        // Bandit Tweaks
        [SettingPropertyInteger("{=CDT_BANDITS_MIN}Bandits Spawn Min Amount", 0, 5000, Order = 0, RequireRestart = false,
            HintText = "{=CDT_BANDITS_MIN_H}Minimum amount of troops for bandits to spawn with. (0 means disable).")]
        [SettingPropertyGroup("{=MCM_BANDIT_TWEAKS}Bandit Tweaks/{=MCM_BANDITS_SPAWNS}Bandits Spawns")]
        public int BanditsSpawnMin { get; set; } = 0;

        [SettingPropertyInteger("{=CDT_BANDITS_MAX}Bandits Spawn Max Amount", 0, 5000, Order = 1, RequireRestart = false,
            HintText = "{=CDT_BANDITS_MAX_H}Maximum amount of troops for bandits to spawn with. (0 means disable).")]
        [SettingPropertyGroup("{=MCM_BANDIT_TWEAKS}Bandit Tweaks/{=MCM_BANDITS_SPAWNS}Bandits Spawns")]
        public int BanditsSpawnMax { get; set; } = 0;

        [SettingPropertyInteger("{=CDT_BANDITS_MIN_TIER}Bandits Spawn Min Tier", 1, 7, Order = 2, RequireRestart = false,
            HintText = "{=CDT_BANDITS_MIN_TIER_H}Minimum tier of troops for bandits to spawn with.")]
        [SettingPropertyGroup("{=MCM_BANDIT_TWEAKS}Bandit Tweaks/{=MCM_BANDITS_SPAWNS}Bandits Spawns")]
        public int BanditsSpawnMinTier { get; set; } = 1;

        [SettingPropertyInteger("{=CDT_BANDITS_MAX_TIER}Bandits Spawn Max Tier", 1, 7, Order = 3, RequireRestart = false,
            HintText = "{=CDT_BANDITS_MAX_TIER_H}Max tier of troops for bandits to spawn with.")]
        [SettingPropertyGroup("{=MCM_BANDIT_TWEAKS}Bandit Tweaks/{=MCM_BANDITS_SPAWNS}Bandits Spawns")]
        public int BanditsSpawnMaxTier { get; set; } = 7;

        [SettingPropertyBool("{=CDT_BANDITS_MIXED}Bandits Spawn Mixed Troops", Order = 4, RequireRestart = false,
            HintText = "{=CDT_BANDITS_MIXED_H}Bandit's parties will have all kind of troops across all cultures.")]
        [SettingPropertyGroup("{=MCM_BANDIT_TWEAKS}Bandit Tweaks/{=MCM_BANDITS_SPAWNS}Bandits Spawns")]
        public bool BanditsSpawnMixed { get; set; } = false;

        [SettingPropertyInteger("{=CDT_BANDITS_MAX_PARTIES}Bandits Max Parties", 0, 1000, Order = 0, RequireRestart = false,
            HintText = "{=CDT_BANDITS_MAX_PARTIES_H}Native is 150. Also, IdealBanditPartyCount (which is parties count per bandit faction) is calculated by: MaxHideoutsAtEachBanditFaction * (MaxBanditPartiesAroundEachHideout + MaximumBanditPartiesInEachHideout) + MaximumLooterParties(our value). (0 means disable)")]
        [SettingPropertyGroup("{=MCM_BANDIT_TWEAKS}Bandit Tweaks")]
        public int BanditsMaxParties { get; set; } = 0;

        [SettingPropertyInteger("{=CDT_BANDITS_MAX_IN_HIDEOUT}Max Bandit Parties In Each Hideout", 0, 100, Order = 1, RequireRestart = false,
            HintText = "{=CDT_BANDITS_MAX_PARTIES_H}Native is 4. Also, IdealBanditPartyCount (which is parties count per bandit faction) is calculated by: MaxHideoutsAtEachBanditFaction * (MaxBanditPartiesAroundEachHideout + MaximumBanditPartiesInEachHideout(our value)) + MaximumLooterParties. (0 means disable)")]
        [SettingPropertyGroup("{=MCM_BANDIT_TWEAKS}Bandit Tweaks")]
        public int BanditsMaxPartiesInHideout { get; set; } = 0;

        [SettingPropertyInteger("{=CDT_BANDITS_MAX_AROUND_HIDEOUT}Max Bandit Parties Around Each Hideout", 0, 100, Order = 2, RequireRestart = false,
            HintText = "{=CDT_BANDITS_MAX_AROUND_HIDEOUT_H}Native is 8. Also, IdealBanditPartyCount (which is parties count per bandit faction) is calculated by: MaxHideoutsAtEachBanditFaction * (MaxBanditPartiesAroundEachHideout(our value) + MaximumBanditPartiesInEachHideout) + MaximumLooterParties. (0 means disable)")]
        [SettingPropertyGroup("{=MCM_BANDIT_TWEAKS}Bandit Tweaks")]
        public int BanditsMaxPartiesAroundHideout { get; set; } = 0;

        [SettingPropertyInteger("{=CDT_BANDITS_MAX_HIDEOUTS}Max Hideouts At Each Bandit Faction", 0, 100, Order = 3, RequireRestart = false,
            HintText = "{=CDT_BANDITS_MAX_HIDEOUTS_H}Native is 10. Also, IdealBanditPartyCount (which is parties count per bandit faction) is calculated by: MaxHideoutsAtEachBanditFaction(our value) * (MaxBanditPartiesAroundEachHideout + MaximumBanditPartiesInEachHideout) + MaximumLooterParties. (0 means disable)")]
        [SettingPropertyGroup("{=MCM_BANDIT_TWEAKS}Bandit Tweaks")]
        public int BanditsMaxHideouts { get; set; } = 0;

        [SettingPropertyInteger("{=CDT_BANDITS_INITIAL_HIDEOUTS}Initial Hideouts At Each Bandit Faction", 0, 100, Order = 4, RequireRestart = false,
            HintText = "{=CDT_BANDITS_INITIAL_HIDEOUTS_H}Native is 3. (0 means disable)")]
        [SettingPropertyGroup("{=MCM_BANDIT_TWEAKS}Bandit Tweaks")]
        public int BanditsInitialHideouts { get; set; } = 0;

        [SettingPropertyFloatingInteger(
            "{=CDT_BANDITS_SPEED}Bandits Party Speed Multiplier",
            -1f, 1f, "#0.##", Order = 5, RequireRestart = false,
            HintText = "{=CDT_BANDITS_SPEED_H}Add speed multipled by our value to bandit parties speed. [speed * our value + speed = party speed]. (0 means disable)")]
        [SettingPropertyGroup("{=MCM_BANDIT_TWEAKS}Bandit Tweaks")]
        public float BanditsSpeedMult { get; set; } = 0f;




        // ----------------------- Loot Tweaks -----------------------

        /*
        [SettingPropertyFloatingInteger(
            "{=CDT_LOOT_AMOUNT}Loot Amount",
            0f, 5f, "#0%", Order = 0, RequireRestart = false,
            HintText = "{=CDT_LOOT_AMOUNT_H}Percent of loot amount after battle for player. (0 means disable, 1 is default in game)")]
        [SettingPropertyGroup("Loot Tweaks")]
        public float LootAmount { get; set; } = 0f;

        */

    }
}
