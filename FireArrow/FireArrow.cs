using System.Collections.Generic;
using System.Linq;
using MCM.Abstractions.Settings.Base.Global;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace FireArrow
{
    class FireArrow : MissionLogic
    {
        public static readonly Settings _settings = GlobalSettings<Settings>.Instance;
        private bool isEnabled = false;
        private List<WeaponClass> burnableWeapons = new List<WeaponClass>() {WeaponClass.Arrow, WeaponClass.Bolt};

        private Dictionary<Mission.Missile, MissionTimer> burningMissiles = new Dictionary<Mission.Missile, MissionTimer>();
        private List<BurningAgent> burningAgents = new List<BurningAgent>();
        class BurningAgent
        {
            public Agent agent;
            public Agent attackerAgent;
            public MissionTimer duration;
            public bool isBurning = false;
            public MissionTimer timer;
            public BurningAgent(Agent agent, Agent attackerAgent, MissionTimer duration)
            {
                this.agent = agent;
                this.attackerAgent = attackerAgent;
                this.duration = duration;
                this.timer = new MissionTimer(1);
            }
        }
        public override void OnAgentShootMissile(Agent shooterAgent, EquipmentIndex weaponIndex, Vec3 position,
            Vec3 velocity, Mat3 orientation,
            bool hasRigidBody, int forcedMissileIndex)
        {
            if (Mission.Mode != MissionMode.Battle) return;
            if (!IsAllowed())
                return;

            if (shooterAgent.Character.IsSoldier && shooterAgent.Character.Level < (int) _settings.AllowedTiers.SelectedValue) 
                return;
            if (!((_settings.AllowedUnits.SelectedValue == Settings.Unit.All)
                  || (_settings.AllowedUnits.SelectedValue == Settings.Unit.Player && shooterAgent == Agent.Main)
                  || (_settings.AllowedUnits.SelectedValue == Settings.Unit.Heroes && shooterAgent.IsHero)
                  || (_settings.AllowedUnits.SelectedValue == Settings.Unit.Companions && shooterAgent.IsHero && shooterAgent.Team.IsPlayerTeam)
                  || (_settings.AllowedUnits.SelectedValue == Settings.Unit.Allies && shooterAgent.Team.IsPlayerAlly)
                  || (_settings.AllowedUnits.SelectedValue == Settings.Unit.Enemies && !shooterAgent.Team.IsPlayerAlly)))
                return;
            if(_settings.SpecificAmmoActive && shooterAgent.IsActive() && !shooterAgent.WieldedWeapon.IsEmpty &&
               !_settings.specifiedAmmoList.Contains(shooterAgent.WieldedWeapon.GetAmmoWeaponData(false).GetItemObject()?.Name.ToString()))
                return;
            foreach (Mission.Missile missile in Mission.Current.Missiles)
            {
                
                if (missile.ShooterAgent == shooterAgent && !burningMissiles.ContainsKey(missile) &&
                    (missile.Weapon.HasAnyUsageWithWeaponClass(burnableWeapons[0]) ||
                     missile.Weapon.HasAnyUsageWithWeaponClass(burnableWeapons[1])))
                {
                    missile.Entity.AddParticleSystemComponent("psys_game_burning_agent");
                    burningMissiles.Add(missile, new MissionTimer(_settings.BurningDuration));
                    Light light = Light.CreatePointLight(_settings.LightRadius);
                    light.Intensity = _settings.LightIntensity;
                    light.LightColor = new Vec3(0.850f, 0.400f, 0f);
                    missile.Entity.AddLight(light);
                    break;
                }
            }
        }
        public override void OnMissionTick(float dt)
        {
            if (Mission.Mode != MissionMode.Battle ) return;
            if (Input.IsKeyPressed(_settings.ToggleKey.SelectedValue))
            {
                isEnabled = !isEnabled;
                InformationManager.DisplayMessage(new InformationMessage("Fire Arrow " + (isEnabled ? "Enabled" : "Disabled")));
            }

            // Check burningMissiles for if it's time to stop burning & remove all corresponding keys from burningMissiles.
            var missilesToRemove = burningMissiles.Where(pair => pair.Value.Check(false)).Select(pair => pair.Key).ToList();
            missilesToRemove.ForEach(missile =>
            {
                RemoveEffects(missile.Entity);
                burningMissiles.Remove(missile);
            });
            //missilesToRemove = attachedMissiles.Where(pair => pair.Value.Check(false)).Select(pair => pair.Key).ToList();
            //missilesToRemove.ForEach(missile => attachedMissiles.Remove(missile));
            var agentsToRemove = new List<BurningAgent>();
            foreach (var agent in burningAgents)
            {
                if (agent.duration.Check(false))
                {
                    agentsToRemove.Add(agent);
                    continue;
                }
                if(agent.timer.Check(true)&&agent.agent.IsActive())
                    BurnAndInform(agent.attackerAgent,agent.agent);
            }
            agentsToRemove.ForEach(agent =>
            {
                burningAgents.Remove(agent);
            });
        }

        public override void OnMissileCollisionReaction(Mission.MissileCollisionReaction collisionReaction, Agent attackerAgent, Agent attachedAgent,
            sbyte attachedBoneIndex)
        {
            if (Mission.Mode != MissionMode.Battle) return;
            
            List<Mission.Missile> tempList = new List<Mission.Missile>();
            // We don't know which missile collided so we need to iterate through all
            foreach (var pair in burningMissiles)
            {
                if (attackerAgent == pair.Key.ShooterAgent && Mission.Current.Missiles.Contains(pair.Key))
                {
                    //If one of these are true,entity will be unreachable after this method and a bug will occur when trying to remove it in MissionTick
                    if (collisionReaction == Mission.MissileCollisionReaction.BecomeInvisible ||
                        collisionReaction == Mission.MissileCollisionReaction.Invalid)
                    {
                    
                        RemoveEffects(pair.Key.Entity);
                        tempList.Add(pair.Key);
                    
                    }
                    else if (!isEnabled)
                    {
                        break;
                    }
                    else if (attachedAgent != null && attachedAgent.IsActive())
                    {
                        RemoveEffects(pair.Key.Entity);
                        if (_settings.BurnAgent && attachedAgent.IsHuman)
                        {
                            if(attachedBoneIndex == (int)HumanBone.Forearm1L && attachedAgent.WieldedOffhandWeapon.IsShield())
                                return;
                            burningAgents.Add(new BurningAgent(attachedAgent, attackerAgent, new MissionTimer(_settings.AgentBurningDuration)));
                            BurnAndInform(attackerAgent,attachedAgent);
                        }
                        tempList.Add(pair.Key);
                    }
                    else
                    {
                        MatrixFrame localFrame = new MatrixFrame(Mat3.Identity, new Vec3(0, 0, 0)).Elevate(0.5f);
                        ParticleSystem particle = ParticleSystem.CreateParticleSystemAttachedToEntity("psys_campfire", pair.Key.Entity, ref localFrame);
                        
                        pair.Key.Entity?.GetLight()?.Frame.Elevate(0.15f);
                        //pair.Key.Entity.AddParticleSystemComponent("psys_campfire");
                    }
                }
            }
            tempList.ForEach(missile => burningMissiles.Remove(missile));
        }

        private void BurnAndInform(Agent attacker, Agent victim)
        {
            if(!victim.IsActive()) return;

            victim.RegisterBlow(CreateBurningBlow(attacker, victim));
            if (attacker == Agent.Main && !victim.IsFriendOf(attacker))
            {
                InformationManager.DisplayMessage(new InformationMessage("Delivered "+_settings.AgentBurningDamage+" Burning damage"));
            }
            else if (attacker == Agent.Main && victim.IsFriendOf(attacker))
            {
                InformationManager.DisplayMessage(new InformationMessage("Delivered " + _settings.AgentBurningDamage + " Burning damage to ally!", Color.ConvertStringToColor("#D65252FF")));
            }
            else if (victim == Agent.Main)
            {
                InformationManager.DisplayMessage(new InformationMessage("Received " + _settings.AgentBurningDamage + " Burning damage", Color.ConvertStringToColor("#D65252FF")));
            }
        }
        private Blow CreateBurningBlow(Agent attacker, Agent victim)
        {
            Blow blow = new Blow(attacker.Index);
            blow.DamageType = DamageTypes.Blunt;
            blow.BlowFlag = BlowFlags.ShrugOff;
            blow.BlowFlag |= BlowFlags.NoSound;
            blow.BoneIndex = victim.Monster.HeadLookDirectionBoneIndex;
            blow.Position = victim.Position;
            blow.Position.z = blow.Position.z + victim.GetEyeGlobalHeight();
            blow.BaseMagnitude = 0;
            blow.WeaponRecord.FillAsMeleeBlow(null,null,-1,-1);
            blow.InflictedDamage = _settings.AgentBurningDamage;
            blow.SwingDirection = victim.LookDirection;
            blow.SwingDirection.Normalize();
            blow.Direction = blow.SwingDirection;
            blow.DamageCalculated = true;
            return blow;
        }
        public bool IsAllowed()
        {
            bool ret = isEnabled;
            ret &= !_settings.NightOnly || (Mission.Current.Scene.TimeOfDay >= 20 || Mission.Current.Scene.TimeOfDay <= 3);
            ret &= !_settings.SiegeOnly || Mission.Current.MissionTeamAIType == Mission.MissionTeamAITypeEnum.Siege;

            return ret;
        }
        public void RemoveEffects(GameEntity entity)
        {
            entity.RemoveAllParticleSystems();
            if (entity.GetLight() != null)
                entity.RemoveComponent(entity.GetLight());
        }
        
    }
}
