using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MCM.Abstractions.Attributes;
using MCM.Abstractions.Attributes.v2;
using MCM.Abstractions.Dropdown;
using MCM.Abstractions.FluentBuilder;
using MCM.Abstractions.Settings.Base;
using MCM.Abstractions.Settings.Base.Global;
using TaleWorlds.InputSystem;

namespace FireArrow
{
    public class Settings : AttributeGlobalSettings<Settings>

    {
        public override string FormatType => "json";
        public override string Id => "AndrogathsFireArrow";
        public override string FolderName => "AndrogathsFireArrow";

        public override string DisplayName => "Androgath's Fire Arrow 1.1.2";
        [SettingPropertyDropdown("{=FireArrow_key_setting}Toggle Key", HintText = "{=FireArrow_key_info}Set toggle key to enable/disable fire arrow during battle.", Order = 1, RequireRestart = false)]
        [SettingPropertyGroup("{=Fire_Arrow_Header}General Mod Settings")]
        public DropdownDefault<InputKey> ToggleKey { get; set; } = new DropdownDefault<InputKey>(Enum.GetValues(typeof(InputKey)).Cast<InputKey>(), 46);
        [SettingPropertyBool("{=FireArrow_enabledbydefault_setting}Fire Arrows Enabled By Default", HintText = "{=FireArrow_enabledbydefault_info}Battles will begin with fire arrows enabled.", Order = 2, RequireRestart = false)]
        [SettingPropertyGroup("{=Fire_Arrow_Header}General Mod Settings")]
        public bool EnabledByDefault { get; set; }
        [SettingPropertyBool("{=FireArrow_night_setting}Allow Fire Arrow Only At Night", HintText = "{=FireArrow_night_info}Enable fire arrows only at night.", Order = 2, RequireRestart = false)]
        [SettingPropertyGroup("{=Fire_Arrow_Header}General Mod Settings")]
        public bool NightOnly { get; set; }
        [SettingPropertyBool("{=FireArrow_siege_setting}Allow Fire Arrow Only In Siege", HintText = "{=FireArrow_siege_info}Enable fire arrows only when sieging.", Order = 3, RequireRestart = false)]
        [SettingPropertyGroup("{=Fire_Arrow_Header}General Mod Settings")]
        public bool SiegeOnly { get; set; }
        [SettingPropertyDropdown("{=FireArrow_tier_setting}Troops Allowed If Equal Or Better Than ", HintText = "{=FireArrow_tier_info}Allow troops to use fire arrows only if they are equal or above this tier.(All makes everyone use fire arrows.)", Order = 4, RequireRestart = false)]
        [SettingPropertyGroup("{=Fire_Arrow_Header}General Mod Settings")]
        public DropdownDefault<Tier> AllowedTiers { get; set; } = new DropdownDefault<Tier>(Enum.GetValues(typeof(Tier)).Cast<Tier>(), 0);

        [SettingPropertyDropdown("{=FireArrow_unit_setting}Allowed Unit Types", HintText = "{=FireArrow_unit_info}Select which units are allowed to use fire arrow.", Order = 5, RequireRestart = false)]
        [SettingPropertyGroup("{=Fire_Arrow_Header}General Mod Settings")]
        public DropdownDefault<Unit> AllowedUnits { get; set; } = new DropdownDefault<Unit>(Enum.GetValues(typeof(Unit)).Cast<Unit>(),6);

        [SettingPropertyBool("{=FireArrow_specific_ammo_setting}Allow Only Specified Ammo To Be Fiery", HintText = "{=FireArrow_specific_ammo_info}Allow only specified ammo to be fiery/fire arrow.", Order = 6, RequireRestart = false)]
        [SettingPropertyGroup("{=Fire_Arrow_Header}General Mod Settings")]
        public bool SpecificAmmoActive { get; set; }

        public List<string> specifiedAmmoList = new List<string>();
        [SettingPropertyText("{=FireArrow_specified_ammo_setting}Allowed Ammo List", HintText = "{=FireArrow_specified_ammo_info}Type ammo names seperated with comma(,).", Order = 7, RequireRestart = false)]
        [SettingPropertyGroup("{=Fire_Arrow_Header}General Mod Settings")]
        public string SpecifiedAmmo
        {
            get => String.Join(",", specifiedAmmoList);
            set => specifiedAmmoList = new List<string>(value.Split(','));
        }

        [SettingPropertyInteger("{=FireArrow_duration_setting}Burning Duration On Ground/Objects", 0,24,"0s",HintText = "{=FireArrow_duration_info}Set how long arrows will burn.", Order = 8, RequireRestart = false)]
        [SettingPropertyGroup("{=Fire_Arrow_Header}General Mod Settings")]
        public int BurningDuration { get; set; }

        [SettingPropertyBool("{=FireArrow_enable_burn_setting}Enable Dealing Burn Damage When Hit", HintText = "{=FireArrow_enable_burn_info}Enable bealing burn damage to agent when hit.", Order = 9, RequireRestart = false)]
        [SettingPropertyGroup("{=Fire_Arrow_Header}General Mod Settings")]
        public bool BurnAgent { get; set; }

        [SettingPropertyInteger("{=FireArrow_agent_duration_setting}Agent Burning Duration", 0, 24, "0s", HintText = "{=FireArrow_agent_duration_info}Set how long will agent burn after being hit.", Order = 10, RequireRestart = false)]
        [SettingPropertyGroup("{=Fire_Arrow_Header}General Mod Settings")]
        public int AgentBurningDuration { get; set; }

        [SettingPropertyInteger("{=FireArrow_agent_damage_setting}Agent Burning Damage Per Second", 0, 25, "0dps", HintText = "{=FireArrow_agent_damage_info}Set how much burning damage will be delivered per second.", Order = 11, RequireRestart = false)]
        [SettingPropertyGroup("{=Fire_Arrow_Header}General Mod Settings")]
        public int AgentBurningDamage { get; set; }
        [SettingPropertyInteger("{=FireArrow_radius_setting}Light Radius", 0, 20, HintText = "{=FireArrow_radius_info}Set light radius.(Default is 3)", Order = 12, RequireRestart = false)]
        [SettingPropertyGroup("{=Fire_Arrow_Header}General Mod Settings")]
        public int LightRadius { get; set; }
        [SettingPropertyInteger("{=FireArrow_intensity_setting}Light Intensity", 0, 200, HintText = "{=FireArrow_intensity_info}Set light intensity.(Default is 120)", Order = 13, RequireRestart = false)]
        [SettingPropertyGroup("{=Fire_Arrow_Header}General Mod Settings")]
        public int LightIntensity { get; set; }
        public enum Tier
        {
            All = 0,
            Tier1 = 6,
            Tier2 = 11,
            Tier3 = 16,
            Tier4 = 21,
            Tier5 = 26,
        }
        public enum Unit
        {
            None = 0,
            Player = 1,
            Heroes = 2,
            Companions = 3,
            Allies = 4,
            Enemies = 5,
            All = 6
        }
        public override IDictionary<string, Func<BaseSettings>> GetAvailablePresets()
        {
            IDictionary<string, Func<BaseSettings>> basePresets = new Dictionary<string, Func<BaseSettings>>();
            basePresets.Add("Default", () => new Settings()
            {
                EnabledByDefault = false,
                NightOnly = true,
                SiegeOnly = false,
                SpecificAmmoActive = false,
                specifiedAmmoList = {"Imperial Arrows","Western Arrows"},
                BurningDuration = 4,
                BurnAgent = false,
                AgentBurningDamage = 5,
                AgentBurningDuration = 4,
                LightRadius = 3,
                LightIntensity = 120
            });
            return basePresets;
        }
    }
}
