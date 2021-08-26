using System;
using System.Collections.Generic;
using System.Linq;
using CombatExtended.Compatibility;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;
using CombatExtended;
namespace magazynier
{
	public class Verb_ShootWithMag : Verb_LaunchProjectileCE_With_ChangeAble_Projectile
	{
		
		protected MagazineUser CompMag
		{
			get
			{
				
				
				return base.EquipmentSource.TryGetComp<MagazineUser>();
			}
		}
	

		public Pawn ammoman
		{
			get
			{
				return CasterPawn.TryGetComp<magazynier.Player_teams.ControllerMGComp>().friends.Find(K => K.TryGetComp<magazynier.Player_teams.ControllerMGComp>() != null && K.TryGetComp<magazynier.Player_teams.ControllerMGComp>().AmIAmmoMan);
			}
		}
		
		protected override int ShotsPerBurst
		{
			get
			{
				bool flag = base.CompFireModes != null;
				if (flag)
				{
					bool flag2 = base.CompFireModes.CurrentFireMode == FireMode.SingleFire;
					
					if (flag2 && !CompMag.convertedtofullauto)
					{
						return 1;
					}
					if(flag2 && CompMag.convertedtofullauto)
					{
						return Rand.Range(2, 6);
					}
					bool flag3 = base.CompFireModes.CurrentFireMode == FireMode.BurstFire && base.CompFireModes.Props.aimedBurstShotCount > 0;
					if (flag3)
					{
						return base.CompFireModes.Props.aimedBurstShotCount;
					}
				}
				return base.VerbPropsCE.burstShotCount;
			}
		}

		// Token: 0x17000101 RID: 257
		// (get) Token: 0x0600043D RID: 1085 RVA: 0x000289A0 File Offset: 0x00026BA0
		private bool ShouldAim
		{
			get
			{
				
				bool flag = base.CompFireModes != null;
			
				bool result;
				if (flag)
				{
					bool flag2 = base.ShooterPawn != null;
					if (flag2)
					{
						bool flag3 = base.ShooterPawn.CurJob != null && base.ShooterPawn.CurJob.def == JobDefOf.Hunt;
						if (flag3)
						{
							return true;
						}
						bool isSuppressed = this.IsSuppressed;
						if (isSuppressed)
						{
							return false;
						}
						Pawn_PathFollower pather = base.ShooterPawn.pather;
						bool flag4 = pather != null && pather.Moving;
						if (flag4)
						{
							return false;
						}
					}
					result = (base.CompFireModes.CurrentAimMode == AimMode.AimedShot);
				}
				else
				{
					result = false;
				}
				return result;
			}
		}

		// Token: 0x17000102 RID: 258
		// (get) Token: 0x0600043E RID: 1086 RVA: 0x00028A48 File Offset: 0x00026C48
		protected override float SwayAmplitude
		{
			get
			{
				float swayAmplitude = base.SwayAmplitude;
				bool shouldAim = this.ShouldAim;
				float result;
				if (shouldAim)
				{
					result = swayAmplitude * Mathf.Max(0f, 1f - base.AimingAccuracy) / Mathf.Max(1f, base.SightsEfficiency);
				}
				else
				{
					bool isSuppressed = this.IsSuppressed;
					if (isSuppressed)
					{
						result = swayAmplitude * 1.5f;
					}
					else
					{
						result = swayAmplitude;
					}
				}
				return result;
			}
		}

		// Token: 0x17000103 RID: 259
		// (get) Token: 0x0600043F RID: 1087 RVA: 0x00028AB0 File Offset: 0x00026CB0
		private bool IsSuppressed
		{
			get
			{
				Pawn shooterPawn = base.ShooterPawn;
				bool? flag;
				if (shooterPawn == null)
				{
					flag = null;
				}
				else
				{
					CompSuppressable compSuppressable = shooterPawn.TryGetComp<CompSuppressable>();
					flag = ((compSuppressable != null) ? new bool?(compSuppressable.isSuppressed) : null);
				}
				bool? flag2 = flag;
				return flag2.GetValueOrDefault();
			}
		}

		// Token: 0x06000440 RID: 1088 RVA: 0x00028AF8 File Offset: 0x00026CF8
		public override void WarmupComplete()
		{
			float lengthHorizontal = (this.currentTarget.Cell - this.caster.Position).LengthHorizontal;
			int num = (int)Mathf.Lerp(30f, 240f, lengthHorizontal / 100f);
			bool flag = this.ShouldAim && !this._isAiming;
			if (flag)
			{
				Building_TurretGunCE building_TurretGunCE = this.caster as Building_TurretGunCE;
				bool flag2 = building_TurretGunCE != null;
				if (flag2)
				{
					building_TurretGunCE.burstWarmupTicksLeft += num;
					this._isAiming = true;
					return;
				}
				bool flag3 = base.ShooterPawn != null;
				if (flag3)
				{
					base.ShooterPawn.stances.SetStance(new Stance_Warmup(num, this.currentTarget, this));
					this._isAiming = true;
					return;
				}
			}
			base.WarmupComplete();
			this._isAiming = false;
			Pawn shooterPawn = base.ShooterPawn;
			bool flag4 = ((shooterPawn != null) ? shooterPawn.skills : null) != null && this.currentTarget.Thing is Pawn;
			if (flag4)
			{
				float num2 = this.verbProps.AdjustedFullCycleTime(this, base.ShooterPawn);
				num2 += num.TicksToSeconds();
				float num3 = this.currentTarget.Thing.HostileTo(base.ShooterPawn) ? 170f : 20f;
				num3 *= num2;
				base.ShooterPawn.skills.Learn(SkillDefOf.Shooting, num3, false);
			}
		}

		public int loadertick = 0;
		public BipodComp bipod
		{
			get
			{
				return this.EquipmentSource.TryGetComp<BipodComp>();
			}
		}
		public float idk2;
		public float stuff;
		public float stuff2;
		public bool athing = true;
		public int l = 5;
		public CompEquippable CompEquippable
		{
			get
			{
				return this.CompEquippable;
			}
		}
		public Thing ThingToCheckFor;
		public override void VerbTickCE()
		{
			bool flag2323 = this.CasterPawn.ParentHolder is Map;
			if(!flag2323)
			{
				return;
			}
			
			if(l != -1)
			{
				--l;
			}
			if(l == 0)
			{
				stuff2 = base.Recoil;
				stuff = this.VerbPropsCE.warmupTime;
				if(CompMag != null)
				{
					this.CompMag.Kaczka();
				}
				else
				{
					Log.Message(this.CasterPawn.Label);
					Log.Message("CompMag is null");
				}
				if(CasterPawn.Faction != Faction.OfPlayer)
				{
					Log.Message("test");
					CasterPawn.TryGetComp<magazynier.Mags.AddInMags>().CreateMags();
				}
				
				
				
				
				
				
				
			}
			if (this.CasterPawn.pather.Moving && bipod != null)
			{
				bipod.IsBipodSetUp = false;
			}
			if(bipod != null && bipod.IsBipodSetUp && bipod != null)
			{
				this.VerbPropsCE.warmupTime = stuff * bipod.bipodattached.GetStatValue(BipodStatDefOf.WarmupDecrease);
				this.idk2 = bipod.bipodattached.GetStatValue(BipodStatDefOf.accuracyincreasebipod);
				base.Recoil = this.VerbPropsCE.recoilAmount;
				
			}
			if(bipod != null && !bipod.IsBipodSetUp)
			{
				this.VerbPropsCE.warmupTime = stuff;
				base.Recoil = this.VerbPropsCE.recoilAmount;
			}
			if(bipod == null)
			{
				base.Recoil = this.VerbPropsCE.recoilAmount;
			}
			
			if(bipod != null)
			{
				if(this.CasterPawn.ParentHolder is Map)
				{
					if (CasterPawn.Drafted)
					{
						if (bipod.ShouldSetUpBipod)
						{
							if (!bipod.IsBipodSetUp)
							{
								if (!this.CasterPawn.pather.Moving)
								{
									CasterPawn.jobs.StartJob(new Job { def = BipodStatDefOf.Ineedtopiss, targetA = this.EquipmentSource }, JobCondition.InterruptForced);
								}
							}
						}
					}
				}
			}
			

			bool isAiming = this._isAiming;
			if (isAiming)
			{
				
				bool flag = !this.ShouldAim;
				if (flag)
				{
					this.WarmupComplete();
				}
				bool flag2;
				if (!(this.caster is Building_TurretGunCE))
				{
					Pawn shooterPawn = base.ShooterPawn;
					Type left;
					if (shooterPawn == null)
					{
						left = null;
					}
					else
					{
						Pawn_StanceTracker stances = shooterPawn.stances;
						if (stances == null)
						{
							left = null;
						}
						else
						{
							Stance curStance = stances.curStance;
							left = ((curStance != null) ? curStance.GetType() : null);
						}
					}
					flag2 = (left != typeof(Stance_Warmup));
				}
				else
				{
					flag2 = false;
				}
				bool flag3 = flag2;
				if (flag3)
				{
					this._isAiming = false;
				}
			}
		}

		// Token: 0x06000442 RID: 1090 RVA: 0x00028CF8 File Offset: 0x00026EF8
		public override void Notify_EquipmentLost()
		{
			base.Notify_EquipmentLost();
			bool flag = base.CompFireModes != null;
			if (flag)
			{
				base.CompFireModes.ResetModes();
			}
		}
		public override float CasterShootingAccuracyValue(Thing caster)
		{
			if(bipod != null && bipod.IsBipodSetUp)
			{
				return base.CasterShootingAccuracyValue(caster) * idk2;
			}
			else
			{
				return base.CasterShootingAccuracyValue(caster) * idk;
			}
			
		}

		// Token: 0x06000443 RID: 1091 RVA: 0x00028D28 File Offset: 0x00026F28
		public override bool CanHitTargetFrom(IntVec3 root, LocalTargetInfo targ)
		{
			bool flag = base.ShooterPawn != null && !base.ShooterPawn.health.capacities.CapableOf(PawnCapacityDefOf.Sight);
			return !flag && base.CanHitTargetFrom(root, targ);
		}
		public float swaychanger;
		public float idk = 1;
		// Token: 0x06000444 RID: 1092 RVA: 0x00028D74 File Offset: 0x00026F74
		public override bool TryCastShot()
		{
			
			
			
			if (CompMag.curMagCountInt <= 0 | CompMag.CurMagCount <= 0)
			{
				CompMag.TryStartReload("abecadlo");
			}
			magazynier.Mags.SupportUtil.amungus(this.CasterPawn, ref idk);
			Log.Message(CasterShootingAccuracyValue(CasterPawn).ToString());
		
			bool flag = CompMag != null;
			if (flag)
			{
				bool flag2 = !CompMag.TryReduceAmmoCount(base.VerbPropsCE.ammoConsumedPerShotCount);
				if (flag2)
				{
					return false;
				}
			}
			bool flag3 = base.TryCastShot();
			bool result;
			if (flag3)
			{
				bool flag4 = base.ShooterPawn != null;
				if (flag4)
				{
					base.ShooterPawn.records.Increment(RecordDefOf.ShotsFired);
				}
				
				bool flag6 = CompMag != null && !CompMag.HasMagazine && CompMag.UseAmmo;
				if (flag6)
				{
					bool flag7 = !CompMag.Notify_ShotFired();
					if (flag7)
					{
						bool flag8 = base.VerbPropsCE.muzzleFlashScale > 0.01f;
						if (flag8)
						{
							FleckMaker.Static(this.caster.Position, this.caster.Map, FleckDefOf.ShotFlash, base.VerbPropsCE.muzzleFlashScale);
						}
						bool flag9 = base.VerbPropsCE.soundCast != null;
						if (flag9)
						{
							base.VerbPropsCE.soundCast.PlayOneShot(new TargetInfo(this.caster.Position, this.caster.Map, false));
						}
						bool flag10 = base.VerbPropsCE.soundCastTail != null;
						if (flag10)
						{
							base.VerbPropsCE.soundCastTail.PlayOneShotOnCamera(null);
						}
						bool flag11 = base.ShooterPawn != null;
						if (flag11)
						{
							bool flag12 = base.ShooterPawn.thinker != null;
							if (flag12)
							{
								base.ShooterPawn.mindState.lastEngageTargetTick = Find.TickManager.TicksGame;
							}
						}
					}
					result = CompMag.Notify_PostShotFired();
				}
				else
				{
					result = true;
				}
			}
			else
			{
				result = false;
			}
			return result;
		}

		
		private const int AimTicksMin = 30;

		
		private const int AimTicksMax = 240;

		
		private const float PawnXp = 20f;

		
		private const float HostileXp = 170f;

		
		private const float SuppressionSwayFactor = 1.5f;

		
		private bool _isAiming;
	}
}
