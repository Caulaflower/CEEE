using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using Verse.Sound;
using UnityEngine;
using CombatExtended.CombatExtended.LoggerUtils;
using CombatExtended.CombatExtended.Jobs.Utils;
using CombatExtended;
using Verse.Noise;
using CombatExtended.Compatibility;

namespace magazynier.Grenade_shit
{
    [StaticConstructorOnStartup]
    public class Building_TurretGunCEEE : Building_Turret
    {
        private const int minTicksBeforeAutoReload = 1800;              // This much time must pass before haulers will try to automatically reload an auto-turret
        private const int ticksBetweenAmmoChecks = 300;                 // Test nearby ammo every 5 seconds if there's many ammo changes
        private const int ticksBetweenSlowAmmoChecks = 3600;               // Test nearby ammo every minute if there's no ammo changes
        public bool isSlow = false;

        private int TicksBetweenAmmoChecks => isSlow ? ticksBetweenSlowAmmoChecks : ticksBetweenAmmoChecks;

        #region Fields

        public int burstCooldownTicksLeft;
        public int burstWarmupTicksLeft;                                // Need this public so aim mode can modify it
        public LocalTargetInfo currentTargetInt = LocalTargetInfo.Invalid;
        private bool holdFire;
        private Thing gunInt;                                           // Better to be private, because Gun is used for access, instead
        public TurretTop top;
        public CompPowerTrader powerComp;
        public CompCanBeDormant dormantComp;
        public CompInitiatable initiatableComp;
        public CompMannable mannableComp;

        public static Material ForcedTargetLineMat = MaterialPool.MatFrom(GenDraw.LineTexPath, ShaderDatabase.Transparent, new Color(1f, 0.5f, 0.5f));

        // New fields
        private MagazineUser compAmmo = null;
        private CompFireModes compFireModes = null;
        private CompChangeableProjectile compChangeable = null;
        public bool isReloading = false;
        private int ticksUntilAutoReload = 0;
        private bool everSpawned = false;

        #endregion

        #region Properties
        // Core properties
        public bool Active => (powerComp == null || powerComp.PowerOn) && (dormantComp == null || dormantComp.Awake) && (initiatableComp == null || initiatableComp.Initiated);
        public CompEquippable GunCompEq => Gun.TryGetComp<CompEquippable>();
        public override LocalTargetInfo CurrentTarget => currentTargetInt;
        private bool WarmingUp => burstWarmupTicksLeft > 0;
        public override Verb AttackVerb => Gun == null ? null : GunCompEq.verbTracker.PrimaryVerb;
        public bool IsMannable => mannableComp != null;
        public bool PlayerControlled => (Faction == Faction.OfPlayer || MannedByColonist) && !MannedByNonColonist;
        private bool CanSetForcedTarget => mannableComp != null && PlayerControlled;
        private bool CanToggleHoldFire => PlayerControlled;
        private bool IsMortar => def.building.IsMortar;
        private bool IsMortarOrProjectileFliesOverhead => Projectile.projectile.flyOverhead || IsMortar;
        //Not included: CanExtractShell
        private bool MannedByColonist => mannableComp != null && mannableComp.ManningPawn != null
            && mannableComp.ManningPawn.Faction == Faction.OfPlayer;
        private bool MannedByNonColonist => mannableComp != null && mannableComp.ManningPawn != null
            && mannableComp.ManningPawn.Faction != Faction.OfPlayer;

        // New properties
        public Thing Gun
        {
            get
            {
                if (this.gunInt == null && Map != null)
                {
                    // I am leaving this here because god knows what uses it before postmake gets called.
                    //CELogger.Warn($"Gun {this.ToString()} was referenced before PostMake. If you're seeing this, please report this to the Combat Extended team!", showOutOfDebugMode: true);
                    MakeGun();

                    if (!everSpawned && (!Map.IsPlayerHome || Faction != Faction.OfPlayer))
                    {
                        compAmmo?.ResetAmmoCount();
                        everSpawned = true;
                    }
                }
                return this.gunInt;
            }
        }

        public ThingDef Projectile
        {
            get
            {
                if (CompAmmo != null && CompAmmo.CurrentAmmo != null)
                {
                    return CompAmmo.CurAmmoProjectile;
                }
                if (CompChangeable != null && CompChangeable.Loaded)
                {
                    return CompChangeable.Projectile;
                }
                return this.GunCompEq.PrimaryVerb.verbProps.defaultProjectile;
            }
        }

        public CompChangeableProjectile CompChangeable
        {
            get
            {
                if (compChangeable == null && Gun != null) compChangeable = Gun.TryGetComp<CompChangeableProjectile>();
                return compChangeable;
            }
        }

        public MagazineUser CompAmmo
        {
            get
            {
                if (compAmmo == null && Gun != null) compAmmo = Gun.TryGetComp<MagazineUser>();
                return compAmmo;
            }
        }

        public CompFireModes CompFireModes
        {
            get
            {
                if (compFireModes == null && Gun != null) compFireModes = Gun.TryGetComp<CompFireModes>();
                return compFireModes;
            }
        }

        public bool EmptyMagazine => CompAmmo?.EmptyMagazine ?? false;
        public bool FullMagazine => CompAmmo?.FullMagazine ?? false;
        public bool AutoReloadableMagazine => AutoReloadableNow && CompAmmo.CurMagCount <= Mathf.CeilToInt(CompAmmo.Props.magazineSize / 6);
        public bool AutoReloadableNow => (mannableComp == null || (!mannableComp.MannedNow && ticksUntilAutoReload == 0)) && Reloadable;    //suppress manned turret auto-reload for a short time after spawning
        public bool Reloadable => CompAmmo?.HasMagazine ?? false;
        public CompMannable MannableComp => mannableComp;
        #endregion

        public Building_TurretGunCEEE()
        {
            top = new TurretTop(this);
        }

        #region Methods

        public override void SpawnSetup(Map map, bool respawningAfterLoad)      //Add mannableComp, ticksUntilAutoReload, register to GenClosestAmmo
        {
            base.SpawnSetup(map, respawningAfterLoad);
            Map.GetComponent<TurretTracker>().Register(this);

            dormantComp = GetComp<CompCanBeDormant>();
            initiatableComp = GetComp<CompInitiatable>();
            powerComp = GetComp<CompPowerTrader>();
            mannableComp = GetComp<CompMannable>();

            if (!everSpawned && (!Map.IsPlayerHome || Faction != Faction.OfPlayer))
            {
                compAmmo?.ResetAmmoCount();
                everSpawned = true;
            }

            if (!respawningAfterLoad)
            {
                //CELogger.Message($"top is {top?.ToString() ?? "null"}");
                top.SetRotationFromOrientation();
                burstCooldownTicksLeft = def.building.turretInitialCooldownTime.SecondsToTicks();

                //Delay auto-reload for a few seconds after spawn, so player can operate the turret right after placing it, before other colonists start reserving it for reload jobs
                if (mannableComp != null)
                    ticksUntilAutoReload = minTicksBeforeAutoReload;
            }

            // if (CompAmmo == null || CompAmmo.Props == null || CompAmmo.Props.ammoSet == null || CompAmmo.Props.ammoSet.ammoTypes.NullOrEmpty())
            //     return;

            // //"Subscribe" turret to GenClosestAmmo
            // foreach (var ammo in CompAmmo.Props.ammoSet.ammoTypes.Select(x => x.ammo))
            // {
            //     if (!GenClosestAmmo.listeners.ContainsKey(ammo))
            //         GenClosestAmmo.listeners.Add(ammo, new List<Building_TurretGunCE>() { this });
            //     else
            //         GenClosestAmmo.listeners[ammo].Add(this);

            //     if (!GenClosestAmmo.latestAmmoUpdate.ContainsKey(ammo))
            //         GenClosestAmmo.latestAmmoUpdate.Add(ammo, 0);
            // }
        }

        //PostMake not added -- MakeGun-like code is run whenever Gun is called
        //No. Fuck you. ^

        public override void PostMake()
        {
            base.PostMake();
            MakeGun();
        }

        private void MakeGun()
        {
            this.gunInt = ThingMaker.MakeThing(this.def.building.turretGunDef, null);
            this.compAmmo = gunInt.TryGetComp<MagazineUser>();

            InitGun();
        }

        public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)    // Added GenClosestAmmo unsubscription
        {
            Map.GetComponent<TurretTracker>().Unregister(this);
            base.DeSpawn(mode);
            ResetCurrentTarget();
        }


        public override void ExposeData()           // Added new variables, removed bool loaded (not used in CE)
        {
            base.ExposeData();

            // New variables
            Scribe_Deep.Look(ref gunInt, "gunInt");
            InitGun();
            Scribe_Values.Look(ref isReloading, "isReloading", false);
            Scribe_Values.Look(ref ticksUntilAutoReload, "ticksUntilAutoReload", 0);
            //lastSurroundingAmmoCheck should never be saved

            Scribe_Values.Look<int>(ref this.burstCooldownTicksLeft, "burstCooldownTicksLeft", 0, false);
            Scribe_Values.Look<int>(ref this.burstWarmupTicksLeft, "burstWarmupTicksLeft", 0, false);
            Scribe_TargetInfo.Look(ref this.currentTargetInt, "currentTarget");
            Scribe_Values.Look<bool>(ref this.holdFire, "holdFire", false, false);
            Scribe_Values.Look<bool>(ref this.everSpawned, "everSpawned", false, false);
            BackCompatibility.PostExposeData(this);
        }

        public override bool ClaimableBy(Faction by)        // Core method
        {
            return base.ClaimableBy(by) && (this.mannableComp == null || this.mannableComp.ManningPawn == null) && (!this.Active || this.mannableComp != null) && (((this.dormantComp == null || this.dormantComp.Awake) && (this.initiatableComp == null || this.initiatableComp.Initiated)) || (this.powerComp != null && !this.powerComp.PowerOn));
        }

        public override void OrderAttack(LocalTargetInfo targ)      // Core method
        {
            if (!targ.IsValid)
            {
                if (this.forcedTarget.IsValid)
                {
                    this.ResetForcedTarget();
                }
                return;
            }
            if ((targ.Cell - base.Position).LengthHorizontal < this.GunCompEq.PrimaryVerb.verbProps.minRange)
            {
                Messages.Message("MessageTargetBelowMinimumRange".Translate(), this, MessageTypeDefOf.RejectInput);
                return;
            }
            if ((targ.Cell - base.Position).LengthHorizontal > this.GunCompEq.PrimaryVerb.verbProps.range)
            {
                Messages.Message("MessageTargetBeyondMaximumRange".Translate(), this, MessageTypeDefOf.RejectInput);
                return;
            }
            if (this.forcedTarget != targ)
            {
                this.forcedTarget = targ;
                if (this.burstCooldownTicksLeft <= 0)
                {
                    this.TryStartShootSomething(false);
                }
            }
            if (this.holdFire)
            {
                Messages.Message("MessageTurretWontFireBecauseHoldFire".Translate(this.def.label), this, MessageTypeDefOf.RejectInput, false);
            }
        }

        public override void Tick()     //Autoreload code and IsReloading check
        {
            //Log.Message("fgvhbjkl;");
            base.Tick();
            if (ticksUntilAutoReload > 0) ticksUntilAutoReload--;   // Reduce time until we can auto-reload

            if (!isReloading && this.IsHashIntervalTick(TicksBetweenAmmoChecks) && (MannableComp?.MannedNow ?? false))
            {
                TryOrderReload();
            }

            //This code runs TryOrderReload for manning pawns or for non-humanlike intelligence such as mechs
            /*if (this.IsHashIntervalTick(TicksBetweenAmmoChecks) && !isReloading && (MannableComp?.MannedNow ?? false))
                  TryOrderReload(CompAmmo?.CurMagCount == 0);*/
            if (!CanSetForcedTarget && !isReloading && forcedTarget.IsValid && burstCooldownTicksLeft <= 0)
            {
                ResetForcedTarget();
            }
            if (!CanToggleHoldFire)
            {
                holdFire = false;
            }
            if (forcedTarget.ThingDestroyed)
            {
                ResetForcedTarget();
            }
            if (Active && (this.mannableComp == null || this.mannableComp.MannedNow) && base.Spawned && !(isReloading && WarmingUp))
            {
                this.GunCompEq.verbTracker.VerbsTick();
                if (!this.stunner.Stunned && this.GunCompEq.PrimaryVerb.state != VerbState.Bursting)
                {
                    if (this.WarmingUp)
                    {
                        this.burstWarmupTicksLeft--;
                        if (this.burstWarmupTicksLeft == 0)
                        {
                            this.BeginBurst();
                        }
                    }
                    else
                    {
                        if (this.burstCooldownTicksLeft > 0)
                        {
                            this.burstCooldownTicksLeft--;
                        }
                        if (this.burstCooldownTicksLeft <= 0)
                        {
                            this.TryStartShootSomething(true);
                        }
                    }
                    this.top.TurretTopTick();
                    return;
                }
            }
            else
            {
                this.ResetCurrentTarget();
            }
        }

        public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn selPawn)
        {
            yield return new FloatMenuOption("Load up with smth idk it's 10 pm", delegate { Find.WindowStack.Add(new FloatMenu(whatamIdoing(selPawn))); });
        }

        public List<Thing> somethingidk()
        {
            return Find.CurrentMap.listerThings.AllThings.FindAll(L => L.TryGetComp<Gazine>() != null && L.TryGetComp<Gazine>().well == CompAmmo.well);
        }

        public List<FloatMenuOption> whatamIdoing(Pawn pawn)
        {
            List<FloatMenuOption> idkidkidk = new List<FloatMenuOption>();
            foreach(Thing ting in somethingidk())
            {
                idkidkidk.Add(new FloatMenuOption(ting.Label,
                    delegate
                    {
                        JobDef hobdef = DefDatabase<JobDef>.AllDefs.ToList().Find(fee => fee.defName == "SwitchBeltOnTurret");
                       
                        if(hobdef != null)
                        {
                            Log.Message("hobdef isn't null");
                            Job boj = JobMaker.MakeJob(hobdef);
                            boj.targetA = this.Gun;
                            boj.targetB = ting;
                            pawn.jobs.StartJob(new Job { def = hobdef, targetA = this.gunInt, targetB = ting, targetC = this }, JobCondition.Succeeded);
                           
                            Log.Message(pawn.CurJob.def.defName + " " + pawn.Name.ToString() + " " + pawn.jobs.startingNewJob.ToString());
                            Log.Message(boj.targetA.ToString());
                            //pawn.jobs.StartJob(new Job(def: JobDefOf.Goto, targetA: this.Position), JobCondition.InterruptForced);
                            //pawn.jobs.StartJob(new Job {def = BipodStatDefOf.amogus, targetA = this.gunInt, targetB = ting}, JobCondition.InterruptForced);
                        }
                        else
                        {
                            Log.Error("fucffk");
                            
                        }
                     
                    }));
            }
            return idkidkidk;
        }

        public void TryStartShootSomething(bool canBeginBurstImmediately)    // Added ammo check and use verb warmup time instead of turret's
        {
            // Check for ammo first
            if (!Spawned
                || (holdFire && CanToggleHoldFire)
                || (Projectile.projectile.flyOverhead && Map.roofGrid.Roofed(Position))
                //|| !AttackVerb.Available()  -- Check replaced by the following:
                || (CompAmmo != null && (isReloading || (mannableComp == null && CompAmmo.CurMagCount <= 0))))
            {
                ResetCurrentTarget();
                return;
            }
            //Copied and modified from Verb_LaunchProjectileCE.Available
            if (!isReloading && (Projectile == null || (CompAmmo != null && !CompAmmo.CanBeFiredNow)))
            {
                ResetCurrentTarget();
                TryOrderReload();
                return;
            }
            bool isValid = currentTargetInt.IsValid;
            currentTargetInt = forcedTarget.IsValid ? forcedTarget : TryFindNewTarget();
            if (!isValid && currentTargetInt.IsValid)
            {
                SoundDefOf.TurretAcquireTarget.PlayOneShot(new TargetInfo(Position, Map, false));
            }
            if (!currentTargetInt.IsValid)
            {
                ResetCurrentTarget();
                return;
            }
            // Use verb warmup time instead of turret's
            if (AttackVerb.verbProps.warmupTime > 0f)
            {
                burstWarmupTicksLeft = AttackVerb.verbProps.warmupTime.SecondsToTicks();
                return;
            }
            if (canBeginBurstImmediately)
            {
                BeginBurst();
                return;
            }
            burstWarmupTicksLeft = 1;
        }

        public LocalTargetInfo TryFindNewTarget()    // Core method
        {
            IAttackTargetSearcher attackTargetSearcher = this.TargSearcher();
            Faction faction = attackTargetSearcher.Thing.Faction;
            float range = this.AttackVerb.verbProps.range;
            Building t;
            if (Rand.Value < 0.5f && this.AttackVerb.ProjectileFliesOverhead() && faction.HostileTo(Faction.OfPlayer) && base.Map.listerBuildings.allBuildingsColonist.Where(delegate (Building x)
            {
                float num = this.AttackVerb.verbProps.EffectiveMinRange(x, this);
                float num2 = (float)x.Position.DistanceToSquared(this.Position);
                return num2 > num * num && num2 < range * range;
            }).TryRandomElement(out t))
            {
                return t;
            }
            TargetScanFlags targetScanFlags = TargetScanFlags.NeedThreat;
            if (!this.AttackVerb.ProjectileFliesOverhead())
            {
                targetScanFlags |= TargetScanFlags.NeedLOSToAll;
                targetScanFlags |= TargetScanFlags.LOSBlockableByGas;
            }
            if (this.AttackVerb.IsIncendiary())
            {
                targetScanFlags |= TargetScanFlags.NeedNonBurning;
            }
            return (Thing)AttackTargetFinder.BestShootTargetFromCurrentPosition(attackTargetSearcher, targetScanFlags, new Predicate<Thing>(this.IsValidTarget), 0f, 9999f);
        }

        private IAttackTargetSearcher TargSearcher()    // Core method
        {
            if (mannableComp != null && mannableComp.MannedNow)
            {
                return mannableComp.ManningPawn;
            }
            return this;
        }

        private bool IsValidTarget(Thing t)             // Projectile flyoverhead check instead of verb
        {
            Pawn pawn = t as Pawn;
            if (pawn != null)
            {
                //if (this.GunCompEq.PrimaryVerb.verbProps.projectileDef.projectile.flyOverhead)
                if (Projectile.projectile.flyOverhead)
                {
                    RoofDef roofDef = base.Map.roofGrid.RoofAt(t.Position);
                    if (roofDef != null && roofDef.isThickRoof)
                    {
                        return false;
                    }
                }
                if (this.mannableComp == null)
                {
                    return !GenAI.MachinesLike(base.Faction, pawn);
                }
                if (pawn.RaceProps.Animal && pawn.Faction == Faction.OfPlayer)
                {
                    return false;
                }
            }
            return true;
        }

        public void BeginBurst()                     // Added handling for ticksUntilAutoReload
        {
            ticksUntilAutoReload = minTicksBeforeAutoReload;
            AttackVerb.TryStartCastOn(CurrentTarget, false, true);
            OnAttackedTarget(CurrentTarget);
        }

        public void BurstComplete()                  // Added CompAmmo reload check
        {
            burstCooldownTicksLeft = BurstCooldownTime().SecondsToTicks();
            if (CompAmmo != null && CompAmmo.CurMagCount <= 0)
            {
                TryForceReload();
            }
        }

        public float BurstCooldownTime()             // Core method
        {
            if (def.building.turretBurstCooldownTime >= 0f)
            {
                return def.building.turretBurstCooldownTime;
            }
            return AttackVerb.verbProps.defaultCooldownTime;
        }

        public override string GetInspectString()       // Replaced vanilla loaded text with CE reloading
        {
            StringBuilder stringBuilder = new StringBuilder();
            string inspectString = base.GetInspectString();
            if (!inspectString.NullOrEmpty())
            {
                stringBuilder.AppendLine(inspectString);
            }

            stringBuilder.AppendLine("GunInstalled".Translate() + ": " + this.Gun.LabelCap);    // New code

            if (this.GunCompEq.PrimaryVerb.verbProps.minRange > 0f)
            {
                stringBuilder.AppendLine("MinimumRange".Translate() + ": " + this.GunCompEq.PrimaryVerb.verbProps.minRange.ToString("F0"));
            }

            if (isReloading)        // New code
            {
                stringBuilder.AppendLine("CE_TurretReloading".Translate());
            }

            else if (Spawned && IsMortarOrProjectileFliesOverhead && Position.Roofed(Map))
            {
                stringBuilder.AppendLine("CannotFire".Translate() + ": " + "Roofed".Translate().CapitalizeFirst());
            }
            else if (Spawned && burstCooldownTicksLeft > 0)
            {
                stringBuilder.AppendLine("CanFireIn".Translate() + ": " + this.burstCooldownTicksLeft.ToStringSecondsFromTicks());
            }
            /*
            if (this.def.building.turretShellDef != null)
            {
                if (this.loaded)
                {
                    stringBuilder.AppendLine("ShellLoaded".Translate());
                }
                else
                {
                    stringBuilder.AppendLine("ShellNotLoaded".Translate());
                }
            }
            */
            return stringBuilder.ToString().TrimEndNewlines();
        }

        public override void Draw()
        {
            top.DrawTurret(Vector3.zero, 0f);
            base.Draw();
        }

        public override void DrawExtraSelectionOverlays()           // Draw at range less than 1.42 tiles
        {
            float range = this.GunCompEq.PrimaryVerb.verbProps.range;
            if (range < 90f)
            {
                GenDraw.DrawRadiusRing(base.Position, range);
            }
            float minRange = AttackVerb.verbProps.minRange;     // Changed to minRange instead of EffectiveMinRange
            if (minRange < 90f && minRange > 0.1f)
            {
                GenDraw.DrawRadiusRing(base.Position, minRange);
            }
            if (this.WarmingUp)
            {
                int degreesWide = (int)((float)this.burstWarmupTicksLeft * 0.5f);
                GenDraw.DrawAimPie(this, this.CurrentTarget, degreesWide, def.size.x * 0.5f);
            }
            if (this.forcedTarget.IsValid && (!this.forcedTarget.HasThing || this.forcedTarget.Thing.Spawned))
            {
                Vector3 b;
                if (this.forcedTarget.HasThing)
                {
                    b = this.forcedTarget.Thing.TrueCenter();
                }
                else
                {
                    b = this.forcedTarget.Cell.ToVector3Shifted();
                }
                Vector3 a = this.TrueCenter();
                b.y = Altitudes.AltitudeFor(AltitudeLayer.MetaOverlays);
                a.y = b.y;
                GenDraw.DrawLineBetween(a, b, Building_TurretGun.ForcedTargetLineMat);
            }
        }

        public override IEnumerable<Gizmo> GetGizmos()              // Modified
        {
            foreach (Gizmo gizmo in base.GetGizmos())
            {
                yield return gizmo;
            }
            // Ammo gizmos
            if (CompAmmo != null && (PlayerControlled || Prefs.DevMode))
            {
                foreach (Command com in CompAmmo.CompGetGizmosExtra())
                {
                    if (!PlayerControlled && Prefs.DevMode && com is GizmoAmmoStatus)
                        (com as GizmoAmmoStatus).prefix = "DEV: ";

                    yield return com;
                }
            }
            // Don't show CONTROL gizmos on enemy turrets (even with dev mode enabled)
            if (PlayerControlled)
            {
                // Fire mode gizmos
                if (CompFireModes != null)
                {
                    foreach (Command com in CompFireModes.GenerateGizmos())
                    {
                        yield return com;
                    }
                }
                // Set forced target gizmo
                if (CanSetForcedTarget)
                {
                    var vt = new Command_VerbTarget
                    {
                        defaultLabel = "CommandSetForceAttackTarget".Translate(),
                        defaultDesc = "CommandSetForceAttackTargetDesc".Translate(),
                        icon = ContentFinder<Texture2D>.Get("UI/Commands/Attack", true),
                        verb = GunCompEq.PrimaryVerb,
                        hotKey = KeyBindingDefOf.Misc4
                    };
                    if (Spawned && IsMortarOrProjectileFliesOverhead && Position.Roofed(Map))
                    {
                        vt.Disable("CannotFire".Translate() + ": " + "Roofed".Translate().CapitalizeFirst());
                    }
                    yield return vt;
                }
                // Stop forced attack gizmo
                if (forcedTarget.IsValid)
                {
                    Command_Action stop = new Command_Action();
                    stop.defaultLabel = "CommandStopForceAttack".Translate();
                    stop.defaultDesc = "CommandStopForceAttackDesc".Translate();
                    stop.icon = ContentFinder<Texture2D>.Get("UI/Commands/Halt", true);
                    stop.action = delegate
                    {
                        ResetForcedTarget();
                        SoundDefOf.Tick_Low.PlayOneShotOnCamera(null);
                    };
                    if (!this.forcedTarget.IsValid)
                    {
                        stop.Disable("CommandStopAttackFailNotForceAttacking".Translate());
                    }
                    stop.hotKey = KeyBindingDefOf.Misc5;
                    yield return stop;
                }
                // Toggle fire gizmo
                if (CanToggleHoldFire)
                {
                    yield return new Command_Toggle
                    {
                        defaultLabel = "CommandHoldFire".Translate(),
                        defaultDesc = "CommandHoldFireDesc".Translate(),
                        icon = ContentFinder<Texture2D>.Get("UI/Commands/HoldFire", true),
                        hotKey = KeyBindingDefOf.Misc6,
                        toggleAction = delegate
                        {
                            holdFire = !holdFire;
                            if (holdFire)
                            {
                                ResetForcedTarget();
                            }
                        },
                        isActive = (() => holdFire)
                    };
                }
            }
        }

        // ExtractShell not added

        private void ResetForcedTarget()                // Core method
        {
            this.forcedTarget = LocalTargetInfo.Invalid;
            this.burstWarmupTicksLeft = 0;
            if (this.burstCooldownTicksLeft <= 0)
            {
                this.TryStartShootSomething(false);
            }
        }

        private void ResetCurrentTarget()               // Core method
        {
            this.currentTargetInt = LocalTargetInfo.Invalid;
            this.burstWarmupTicksLeft = 0;
        }

        //MakeGun not added -- MakeGun-like code is run whenever Gun is called
        //UpdateGunVerbs not added

        // New methods
        private void InitGun()
        {
            // Callback for ammo comp
            if (CompAmmo != null)
            {
                CompAmmo.turret = this;
                //if (def.building.turretShellDef != null && def.building.turretShellDef is AmmoDef) CompAmmo.selectedAmmo = (AmmoDef)def.building.turretShellDef;
            }
            List<Verb> allVerbs = this.gunInt.TryGetComp<CompEquippable>().AllVerbs;
            for (int i = 0; i < allVerbs.Count; i++)
            {
                Verb verb = allVerbs[i];
                verb.caster = this;
                verb.castCompleteCallback = new Action(this.BurstComplete);
            }
        }

        public void TryForceReload()
        {
            TryOrderReload(true);
        }

        public Thing InventoryAmmo(CompInventory inventory)
        {
            if (inventory == null)
                return null;

            Thing ammo = inventory.container.FirstOrDefault(x => x.def == CompAmmo.SelectedAmmo);

            // NPC's switch ammo types
            if (ammo == null)
            {
                ammo = inventory.container.FirstOrDefault(x => CompAmmo.Props.ammoSet.ammoTypes.Any(a => a.ammo == x.def));
            }

            return ammo;
        }

        public void TryOrderReload(bool forced = false)
        {
            //No reload necessary at all --
            if ((CompAmmo.CurrentAmmo == CompAmmo.SelectedAmmo && (!CompAmmo.HasMagazine || CompAmmo.CurMagCount == CompAmmo.Props.magazineSize)))
                return;

            //Non-mannableComp interaction
            if (!mannableComp?.MannedNow ?? true)
            {
                return;
            }

            //Only have manningPawn reload after a long time of no firing
            if (!forced && Reloadable && (compAmmo.CurMagCount != 0 || ticksUntilAutoReload > 0))
                return;

            //Already reserved for manning
            Pawn manningPawn = mannableComp.ManningPawn;
            if (manningPawn != null)
            {
                if (!JobGiverUtils_Reload2.CanReload(manningPawn, this))
                {
                    return;
                }
                var jobOnThing = JobGiverUtils_Reload2.MakeReloadJob(manningPawn, this);

                if (jobOnThing != null)
                {
                    manningPawn.jobs.StartJob(jobOnThing, JobCondition.Ongoing, null, manningPawn.CurJob?.def != CE_JobDefOf.ReloadTurret);
                }
            }

        }
        #endregion
    }
    class JobGiverUtils_Reload2
    {
        /// <summary>
        /// The maximum allowed pathing cost to reach potential ammo. 2 ingame hours.
        /// This is arbitrarily set. If you think this is too high or too low, feel free to change.
        /// </summary>
        private const float MaxPathCost = 2f * 60f * GenDate.TicksPerHour;
        /// <summary>
        /// Magic number. I took it from the now deprecated GenClosestAmmo class. Not sure why we would want 10 reservations, but there it is.
        /// </summary>
        private const int MagicMaxPawns = 10;

        public static Job MakeReloadJob(Pawn pawn, Building_Turret turret)
        {
            var compAmmo = turret.GetAmmo();
            if (compAmmo == null)
            {
                //CELogger.Error($"{pawn} tried to create a reload job on a thing ({turret}) that's not reloadable.");
                return null;
            }

            if (!compAmmo.UseAmmo)
            {
                return MakeReloadJobNoAmmo(turret);
            }

            var ammo = FindBestAmmo(pawn, turret);
            if (ammo == null)
            {
                //CELogger.Error($"{pawn} tried to create a reload job without ammo. This should have been checked earlier.");
                return null;
            }
            //CELogger.Message($"Making a reload job for {pawn}, {turret} and {ammo}");

            Job job = JobMaker.MakeJob(CE_JobDefOf.ReloadTurret, turret, ammo);
            job.count = Mathf.Min(ammo.stackCount, compAmmo.MissingToFullMagazine);
            return job;
        }

        private static Job MakeReloadJobNoAmmo(Building_Turret turret)
        {
            var compAmmo = turret.GetAmmo();
            if (compAmmo == null)
            {
                //CELogger.Error("Tried to create a reload job on a thing that's not reloadable.");
                return null;
            }

            return JobMaker.MakeJob(CE_JobDefOf.ReloadTurret, turret, null);
        }

        public static bool CanReload(Pawn pawn, Thing thing, bool forced = false, bool emergency = false)
        {
            if (pawn == null || thing == null)
            {
                //CELogger.Warn($"{pawn?.ToString() ?? "null pawn"} could not reload {thing?.ToString() ?? "null thing"} one of the two was null.");
                return false;
            }

            if (!(thing is Building_Turret turret))
            {
               // CELogger.Warn($"{pawn} could not reload {thing} because {thing} is not a Turret. If you are a modder, make sure to use {nameof(CombatExtended)}.{nameof(Building_TurretGunCE)} for your turret's compClass.");
                return false;
            }
            var compAmmo = turret.GetAmmo();

            if (compAmmo == null)
            {
               // CELogger.Warn($"{pawn} could not reload {turret} because turret has no {nameof(CompAmmoUser)}.");
                return false;
            }
            if (turret.GetReloading())
            {
               // CELogger.Message($"{pawn} could not reload {turret} because turret is already reloading.");
                JobFailReason.Is("CE_TurretAlreadyReloading".Translate());
                return false;
            }
            if (turret.IsBurning() && !emergency)
            {
               // CELogger.Message($"{pawn} could not reload {turret} because turret is on fire.");
                JobFailReason.Is("CE_TurretIsBurning".Translate());
                return false;
            }
            if (compAmmo.FullMagazine)
            {
                //CELogger.Message($"{pawn} could not reload {turret} because it is full of ammo.");
                JobFailReason.Is("CE_TurretFull".Translate());
                return false;
            }
            if (turret.IsForbidden(pawn) || !pawn.CanReserve(turret, 1, -1, null, forced))
            {
               // CELogger.Message($"{pawn} could not reload {turret} because it is forbidden or otherwise busy.");
                return false;
            }
            if (turret.Faction != pawn.Faction && (turret.Faction != null && pawn.Faction != null && turret.Faction.RelationKindWith(pawn.Faction) != FactionRelationKind.Ally))
            {
               // CELogger.Message($"{pawn} could not reload {turret} because the turret is hostile to them.");
                JobFailReason.Is("CE_TurretNonAllied".Translate());
                return false;
            }
            if ((turret.GetMannable()?.ManningPawn != pawn) && !pawn.CanReserveAndReach(turret, PathEndMode.ClosestTouch, forced ? Danger.Deadly : pawn.NormalMaxDanger(), MagicMaxPawns))
            {
               // CELogger.Message($"{pawn} could not reload {turret} because turret is manned (or was recently manned) by someone else.");
                return false;
            }
            if (compAmmo.UseAmmo && FindBestAmmo(pawn, turret) == null)
            {
                JobFailReason.Is("CE_NoAmmoAvailable".Translate());
                return false;
            }
            return true;
        }

        private static Thing FindBestAmmo(Pawn pawn, Building_Turret turret)
        {
            var ammoComp = turret.GetAmmo();
            AmmoDef requestedAmmo = ammoComp.SelectedAmmo;
            var bestAmmo = FindBestAmmo(pawn, requestedAmmo);   // try to find currently selected ammo first
            if (bestAmmo == null && ammoComp.EmptyMagazine && requestedAmmo.AmmoSetDefs != null && turret.Faction != Faction.OfPlayer)
            {
                //Turret's selected ammo not available, and magazine is empty. Pick a new ammo from the set to load.
                foreach (AmmoSetDef set in requestedAmmo.AmmoSetDefs)
                {
                    foreach (AmmoLink link in set.ammoTypes)
                    {
                        bestAmmo = FindBestAmmo(pawn, link.ammo);
                        if (bestAmmo != null) return bestAmmo;
                    }
                }
            }
            return bestAmmo;
        }

        private static Thing FindBestAmmo(Pawn pawn, AmmoDef requestedAmmo)
        {
            Predicate<Thing> validator = (Thing potentialAmmo) =>
            {
                if (potentialAmmo.IsForbidden(pawn) || !pawn.CanReserve(potentialAmmo))
                {
                    return false;
                }
                return GetPathCost(pawn, potentialAmmo) <= MaxPathCost;
            };

            return GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForDef(requestedAmmo), PathEndMode.ClosestTouch, TraverseParms.For(pawn), 9999f, validator);
        }

        /// <summary>
        /// This method is a direct copy/paste of the <see cref="RefuelWorkGiverUtility"/> private FindAllFuel method.
        /// 
        /// Finds all relevant ammo in order of distance.
        /// </summary>
        /// <param name="pawn"></param>
        /// <param name="reloadable"></param>
        /// <returns></returns>
        private static List<Thing> FindAllAmmo(Pawn pawn, Building_Turret reloadable)
        {
            var compAmmo = reloadable.GetAmmo();
            int quantity = compAmmo.MissingToFullMagazine;
            var ammoKind = compAmmo.SelectedAmmo;
            Predicate<Thing> validator = (Thing potentialAmmo) =>
            {
                if (potentialAmmo.IsForbidden(pawn) || !pawn.CanReserve(potentialAmmo))
                {
                    return false;
                }
                return GetPathCost(pawn, potentialAmmo) <= MaxPathCost;
            };
            Region region = reloadable.Position.GetRegion(pawn.Map);
            TraverseParms traverseParams = TraverseParms.For(pawn);
            Verse.RegionEntryPredicate entryCondition = (Region from, Region r) => r.Allows(traverseParams, isDestination: false);
            var chosenThings = new List<Thing>();
            int accumulatedQuantity = 0;
            Verse.RegionProcessor regionProcessor = (Region r) =>
            {
                List<Thing> list = r.ListerThings.ThingsMatching(ThingRequest.ForDef(ammoKind));
                foreach (var thing in list)
                {
                    if (validator(thing) && !chosenThings.Contains(thing) && ReachabilityWithinRegion.ThingFromRegionListerReachable(thing, r, PathEndMode.ClosestTouch, pawn))
                    {
                        chosenThings.Add(thing);
                        accumulatedQuantity += thing.stackCount;
                        if (accumulatedQuantity >= quantity)
                        {
                            return true;
                        }
                    }
                }
                return false;
            };
            RegionTraverser.BreadthFirstTraverse(region, entryCondition, regionProcessor, 99999);
            if (accumulatedQuantity >= quantity)
            {
                return chosenThings;
            }
            return null;
        }

        private static float GetPathCost(Pawn pawn, Thing thing)
        {
            var cell = thing.Position;
            var pos = pawn.Position;
            var traverseParams = TraverseParms.For(pawn, Danger.Deadly, TraverseMode.PassDoors, false);

            using (PawnPath path = pawn.Map.pathFinder.FindPath(pos, cell, traverseParams, PathEndMode.Touch))
            {
                return path.TotalCost;
            }
        }
    }
    [DefOf]
    public static class letmefuckingsleep
    {
        // Token: 0x06007FE6 RID: 32742 RVA: 0x002D904F File Offset: 0x002D724F
        static letmefuckingsleep()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(letmefuckingsleep));
        }

        // Token: 0x04004F86 RID: 20358
       
        public static JobDef SwitchBeltOnTurret;
    }
}
