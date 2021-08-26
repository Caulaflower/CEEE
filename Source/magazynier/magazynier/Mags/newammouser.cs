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
	
	public class MagazineUser : CompRangedGizmoGiver
	{
		
		public ThingWithComps BipodToAddBack;
		public bool convertedtofullauto;
		public ThingWithComps loadedmag;
		public int magazinesize = 1;
		public magazynier.MagWellDef well => Props.well;
		public CompProperties_MagazineUser Props
		{
			get
			{
				return (CompProperties_MagazineUser)this.props;
			}
		}


		public int CurMagCount
		{
			get
			{
				return this.curMagCountInt;
			}
			set
			{
				bool flag = this.curMagCountInt != value && value >= 0;
				if (flag)
				{
					this.curMagCountInt = value;
					bool flag2 = this.CompInventory != null;
					if (flag2)
					{
						this.CompInventory.UpdateInventory();
					}
				}
			}
		}


		public CompEquippable CompEquippable
		{
			get
			{
				return this.parent.GetComp<CompEquippable>();
			}
		}
		public void Kaczka()
		{
			Log.Error(BipodToAddBack?.def.defName ?? "fuck");
			if(BipodToAddBack != null)
			{
				this.parent.AllComps.Add(new BipodComp { bipodattached = BipodToAddBack });
			}
		}
		public int L = 0;

		
		public bool athing = true;
		public Thing ThingToCheckFor;
		public override void Notify_Equipped(Pawn pawn)
		{
			
			base.Notify_Equipped(pawn);
			
			bool a = Wielder.Spawned;
			CompEquippable compEquippable221 = this.CompEquippable;
			Verb primaryVerb223 = compEquippable221.PrimaryVerb;
			
		}
		public Pawn Wielder
		{
			get
			{
				bool flag;
				if (this.CompEquippable != null && this.CompEquippable.PrimaryVerb != null && this.CompEquippable.PrimaryVerb.caster != null)
				{
					CompEquippable compEquippable = this.CompEquippable;
					object obj;
					if (compEquippable == null)
					{
						obj = null;
					}
					else
					{
						ThingWithComps parent = compEquippable.parent;
						obj = ((parent != null) ? parent.ParentHolder : null);
					}
					Pawn_InventoryTracker pawn_InventoryTracker = obj as Pawn_InventoryTracker;
					Pawn pawn = (pawn_InventoryTracker != null) ? pawn_InventoryTracker.pawn : null;
					if (pawn != null)
					{
						Pawn pawn2 = pawn;
						CompEquippable compEquippable2 = this.CompEquippable;
						object obj2;
						if (compEquippable2 == null)
						{
							obj2 = null;
						}
						else
						{
							Verb primaryVerb = compEquippable2.PrimaryVerb;
							obj2 = ((primaryVerb != null) ? primaryVerb.CasterPawn : null);
						}
						flag = (pawn2 != obj2);
					}
					else
					{
						flag = false;
					}
				}
				else
				{
					flag = true;
				}
				bool flag2 = flag;
				Pawn result;
				if (flag2)
				{
					result = null;
				}
				else
				{
					result = this.CompEquippable.PrimaryVerb.CasterPawn;
				}
				return result;
				
			}
				
		}


		public bool IsEquippedGun
		{
			get
			{
				return this.Wielder != null;
			}
		}

		public Pawn Holder
		{
			get
			{
				Pawn result;
				if ((result = this.Wielder) == null)
				{
					Pawn_InventoryTracker pawn_InventoryTracker = this.CompEquippable.parent.ParentHolder as Pawn_InventoryTracker;
					result = ((pawn_InventoryTracker != null) ? pawn_InventoryTracker.pawn : null);
				}
				return result;
			}
		}

		
		public bool UseAmmo
		{
			get
			{
				return this.Props.ammoSet != null;
			}
		}

		public bool HasAndUsesAmmoOrMagazine
		{
			get
			{
				return !this.UseAmmo || this.HasAmmoOrMagazine;
			}
		}


		public bool HasAmmoOrMagazine
		{
			get
			{
				return (this.HasMagazine && this.CurMagCount > 0) || this.HasAmmo;
			}
		}


		public bool CanBeFiredNow
		{
			get
			{
				return (this.HasMagazine && this.CurMagCount > 0) || (!this.HasMagazine && (this.HasAmmo || !this.UseAmmo));
			}
		}


		public bool HasAmmo
		{
			get
			{
				return this.CompInventory != null && this.CompInventory.ammoList.Any((Thing x) => this.Props.ammoSet.ammoTypes.Any((AmmoLink a) => a.ammo == x.def));
			}
		}


		public bool HasMagazine
		{
			get
			{
				return this.magazinesize > 0;
			}
		}
		public override void PostPostMake()
		{
			
		}
		


		public AmmoDef CurrentAmmoSetting
		{
			set
			{
				CurrentAmmo = this.UseAmmo ? this.currentAmmoInt : null;
			}
		}
		public AmmoDef CurrentAmmo;

		public bool EmptyMagazine
		{
			get
			{
				return this.HasMagazine && this.CurMagCount == 0;
			}
		}


		public int MissingToFullMagazine
		{
			get
			{
				bool flag = !this.HasMagazine;
				int result;
				if (flag)
				{
					result = 0;
				}
				else
				{
					bool flag2 = this.SelectedAmmo == this.CurrentAmmo;
					if (flag2)
					{
						result = this.magazinesize - this.CurMagCount;
					}
					else
					{
						result = this.magazinesize;
					}
				}
				return result;
			}
		}
		public void CheckForThingy()
		{
			BipodComp bipodComp = this.parent.TryGetComp<BipodComp>();
			Log.Message(bipodComp.ToString());
			Log.Message(bipodComp.bipodattached.ToString());
			BipodToAddBack = bipodComp.bipodattached;
			Log.Message(BipodToAddBack.def.defName);
		}

		public bool FullMagazine
		{
			get
			{
				bool useAmmo = this.UseAmmo;
				bool result;
				if (useAmmo)
				{
					result = (this.HasMagazine && this.SelectedAmmo == this.CurrentAmmo && this.CurMagCount >= this.magazinesize);
				}
				else
				{
					result = (this.CurMagCount >= this.magazinesize);
				}
				return result;
			}
		}


		public ThingDef CurAmmoProjectile
		{
			get
			{
				//Log.Message(this.CurrentAmmo.ToString());
				List<ThingDef> list1 = DefDatabase<ThingDef>.AllDefs.Where(x => x.thingClass == MiscDefOf.Bullet_556x45mmNATO_FMJ.thingClass).ToList();
				AmmoSetDef ammoSet = this.Props.ammoSet;

				ThingDef thingDef;
				List<AmmoLink> ammoTypes = ammoSet.ammoTypes;
				//Log.Message(ammoTypes.ToString());
				AmmoLink ammoLink = ammoTypes.FirstOrDefault((AmmoLink x) => x.ammo == this.CurrentAmmo);
				thingDef = ((ammoLink != null) ? ammoLink.projectile : null);
				
				return thingDef;
			}
		}


		public CompInventory CompInventory
		{
			get
			{
				return this.Holder.TryGetComp<CompInventory>();
			}
		}


		private IntVec3 Position
		{
			get
			{
				bool isEquippedGun = this.IsEquippedGun;
				IntVec3 position;
				if (isEquippedGun)
				{
					position = this.Wielder.Position;
				}
				else
				{
					bool flag = this.turret != null;
					if (flag)
					{
						position = this.turret.Position;
					}
					else
					{
						bool flag2 = this.Holder != null;
						if (flag2)
						{
							position = this.Holder.Position;
						}
						else
						{
							position = this.parent.Position;
						}
					}
				}
				return position;
			}
		}


		private Map Map
		{
			get
			{
				bool flag = this.Holder != null;
				Map mapHeld;
				if (flag)
				{
					mapHeld = this.Holder.MapHeld;
				}
				else
				{
					bool flag2 = this.turret != null;
					if (flag2)
					{
						mapHeld = this.turret.MapHeld;
					}
					else
					{
						mapHeld = this.parent.MapHeld;
					}
				}
				return mapHeld;
			}
		}


		public bool ShouldThrowMote
		{
			get
			{
				return this.Props.throwMote && this.magazinesize > 1;
			}
		}


		public AmmoDef SelectedAmmo
		{
			get
			{
				return this.selectedAmmo;
			}
			set
			{
				this.selectedAmmo = value;
				bool flag = !this.HasMagazine && this.CurrentAmmo != value;
				if (flag)
				{
					this.currentAmmoInt = value;
				}
			}
		}


		public override void Initialize(CompProperties vprops)
		{
			base.Initialize(vprops);
			
			bool useAmmo = this.UseAmmo;
			if (useAmmo)
			{
				bool flag = this.Props.ammoSet.ammoTypes.NullOrEmpty<AmmoLink>();
				if (flag)
				{
					//Log.Error(this.parent.Label + " has no available ammo types", false);
				}
				else
				{
					bool flag2 = this.currentAmmoInt == null;
					if (flag2)
					{
						this.currentAmmoInt = this.Props.ammoSet.ammoTypes[0].ammo;
					}
					bool flag3 = this.selectedAmmo == null;
					if (flag3)
					{
						this.selectedAmmo = this.currentAmmoInt;
					}
				}
			}
		}


	

		public override void PostExposeData()
		{
		
			Scribe_Values.Look<int>(ref this.magazinesize, "MagSize");
			Scribe_Values.Look<int>(ref this.curMagCountInt, "count", 0, false);
			Scribe_Defs.Look<AmmoDef>(ref this.currentAmmoInt, "currentAmmoInt");
			Scribe_Defs.Look<AmmoDef>(ref this.CurrentAmmo, "currentAmmo");
			Scribe_Values.Look<int>(ref this.curMagCountInt, "currentMagCount");
			Scribe_Values.Look<bool>(ref this.convertedtofullauto, "convertedtofullauto");
			Scribe_References.Look<ThingWithComps>(ref this.BipodToAddBack, "BipodToAddBack");
			Scribe_Defs.Look<AmmoDef>(ref this.selectedAmmo, "selectedAmmo");
			Scribe_References.Look<ThingWithComps>(ref this.loadedmag, "loadedmag");
			
			base.PostExposeData();

		}


		private void AssignJobToWielder(Job job)
		{
			bool flag = this.Wielder.drafter != null;
			if (flag)
			{
				this.Wielder.jobs.TryTakeOrderedJob(job, JobTag.Misc);
			}
			else
			{
				ExternalPawnDrafter.TakeOrderedJob(this.Wielder, job);
			}
		}


		public bool Notify_ShotFired()
		{
			bool flag = this.ammoToBeDeleted != null;
			if (flag)
			{
				this.ammoToBeDeleted.Destroy(DestroyMode.Vanish);
				this.ammoToBeDeleted = null;
				this.CompInventory.UpdateInventory();
				bool flag2 = !this.HasAmmoOrMagazine;
				if (flag2)
				{
					return false;
				}
			}
			return true;
		}


		public bool Notify_PostShotFired()
		{
			bool flag = !this.HasAmmoOrMagazine;
			bool result;
			if (flag)
			{
				this.DoOutOfAmmoAction();
				result = false;
			}
			else
			{
				result = true;
			}
			return result;
		}



		public bool TryReduceAmmoCount(int ammoConsumedPerShot = 1)
		{
			ammoConsumedPerShot = ((ammoConsumedPerShot > 0) ? ammoConsumedPerShot : 1);
			bool flag = !this.IsEquippedGun && this.turret == null;
			if (flag)
			{
				//Log.Error(this.parent.ToString() + " tried reducing its ammo count without a wielder", false);
			}
			bool flag2 = !this.HasMagazine;
			bool result;
			if (flag2)
			{
				bool useAmmo = this.UseAmmo;
				if (useAmmo)
				{
					bool flag3 = !this.TryFindAmmoInInventory(out this.ammoToBeDeleted);
					if (flag3)
					{
						return false;
					}
					bool flag4 = this.ammoToBeDeleted.def != this.CurrentAmmo;
					if (flag4)
					{
						this.currentAmmoInt = (this.ammoToBeDeleted.def as AmmoDef);
					}
					bool flag5 = this.ammoToBeDeleted.stackCount > 1;
					if (flag5)
					{
						this.ammoToBeDeleted = this.ammoToBeDeleted.SplitOff(1);
					}
				}
				result = true;
			}
			else
			{
				bool flag6 = this.curMagCountInt <= 0;
				if (flag6)
				{
					this.CurMagCount = 0;
					result = false;
				}
				else
				{
					this.CurMagCount = ((this.curMagCountInt - ammoConsumedPerShot < 0) ? 0 : (this.curMagCountInt - ammoConsumedPerShot));
					bool flag7 = this.curMagCountInt < 0;
					if (flag7)
					{
						this.TryStartReload("2");
					}
					result = true;
				}
			}
			return result;
		}


		public void TryStartReload(String lob)
		{
				Log.Message(lob);
				bool fuckIhate = !Wielder.inventory.innerContainer.ToList().Any(IKSDE => IKSDE.TryGetComp<Gazine>() != null && IKSDE.TryGetComp<Gazine>().well == this.well);
				if (fuckIhate)
				{
					Log.Error("e");
					this.DoOutOfAmmoAction();
				}

				Job job = this.TryMakeReloadJob(Wielder.inventory.innerContainer.ToList().Find(IKSDE => IKSDE.TryGetComp<Gazine>() != null && IKSDE.TryGetComp<Gazine>().well == this.well && this.Props.ammoSet.ammoTypes.Any(J => J.ammo == IKSDE.TryGetComp<Gazine>().loadedAmmo)));
				bool flag8 = job == null;
				if (!flag8 && !fuckIhate)
				{
					Log.Message("b");
					job.playerForced = true;
					Pawn_JobTracker jobs = Wielder.jobs;
					
					Job newJob = job;
					JobCondition lastJobEndCondition = JobCondition.InterruptForced;
					ThinkNode jobGiver = null;
					Job curJob = this.Wielder.CurJob;
					jobs.StartJob(newJob, lastJobEndCondition, jobGiver, ((curJob != null) ? curJob.def : null) != job.def, true, null, null, false, false);
									
				}
				
						
		}


		public bool TryUnload(bool forceUnload = false)
		{
			Thing thing;
			return this.TryUnload(out thing, forceUnload);
		}


		public bool TryUnload(out Thing droppedAmmo, bool forceUnload = false)
		{
			droppedAmmo = null;
			bool flag = !this.HasMagazine || (this.Holder == null && this.turret == null);
			bool result;
			if (flag)
			{
				result = false;
			}
			else
			{
				bool flag2 = !this.UseAmmo || this.curMagCountInt == 0;
				if (flag2)
				{
					result = true;
				}
				else
				{
					bool flag3 = this.Props.reloadOneAtATime && !forceUnload && this.selectedAmmo == this.CurrentAmmo && this.turret == null;
					if (flag3)
					{
						result = true;
					}
					else
					{
						Thing thing = ThingMaker.MakeThing(this.currentAmmoInt, null);
						thing.stackCount = this.curMagCountInt;
						bool flag4 = this.CompInventory != null;
						bool flag5 = !flag4 || this.curMagCountInt != this.CompInventory.container.TryAdd(thing, thing.stackCount, true);
						bool flag6 = flag5;
						if (flag6)
						{
							bool flag7 = !GenThing.TryDropAndSetForbidden(thing, this.Position, this.Map, ThingPlaceMode.Near, out droppedAmmo, this.turret.Faction != Faction.OfPlayer);
							if (flag7)
							{
								//Log.Warning(base.GetType().Assembly.GetName().Name + " :: " + base.GetType().Name + " :: " + "Unable to drop " + thing.LabelCap + " on the ground, thing was destroyed.", false);
							}
						}
						this.CurMagCount = 0;
						result = true;
					}
				}
			}
			return result;
		}


		public Job TryMakeReloadJob(Thing magazinier)
		{
			return new Job(MiscDefOf.SwitchMagazine, this.parent, magazinier);
		}


		private void DoOutOfAmmoAction()
		{
			bool shouldThrowMote = this.ShouldThrowMote;
			if (shouldThrowMote)
			{
				MoteMaker.ThrowText(this.Position.ToVector3Shifted(), Find.CurrentMap, "CE_OutOfAmmo".Translate() + "!", -1f);
			}
			bool flag = this.IsEquippedGun && this.CompInventory != null && (this.Wielder.CurJob == null || this.Wielder.CurJob.def != JobDefOf.Hunt);
			if (flag)
			{
				this.CompInventory.SwitchToNextViableWeapon(true);
			}
		}


		public void LoadAmmo(Thing ammo = null)
		{
			bool flag = this.Holder == null && this.turret == null;
			if (flag)
			{
				//Log.Error(this.parent.ToString() + " tried loading ammo with no owner", false);
			}
			else
			{
				bool useAmmo = this.UseAmmo;
				int curMagCount;
				if (useAmmo)
				{
					bool flag2 = false;
					bool flag3 = ammo == null;
					Thing thing;
					if (flag3)
					{
						bool flag4 = !this.TryFindAmmoInInventory(out thing);
						if (flag4)
						{
							this.DoOutOfAmmoAction();
							return;
						}
						flag2 = true;
					}
					else
					{
						thing = ammo;
					}
					this.currentAmmoInt = (AmmoDef)thing.def;
					bool flag5 = (this.Props.reloadOneAtATime ? 1 : this.magazinesize) < thing.stackCount;
					if (flag5)
					{
						bool reloadOneAtATime = this.Props.reloadOneAtATime;
						if (reloadOneAtATime)
						{
							curMagCount = this.curMagCountInt + 1;
							thing.stackCount--;
						}
						else
						{
							curMagCount = this.magazinesize;
							thing.stackCount -= this.magazinesize;
						}
					}
					else
					{
						int num = thing.stackCount;
						bool flag6 = this.turret != null;
						if (flag6)
						{
							num += this.curMagCountInt;
						}
						curMagCount = (this.Props.reloadOneAtATime ? (this.curMagCountInt + 1) : num);
						bool flag7 = flag2;
						if (flag7)
						{
							this.CompInventory.container.Remove(thing);
						}
						else
						{
							bool flag8 = !thing.Destroyed;
							if (flag8)
							{
								thing.Destroy(DestroyMode.Vanish);
							}
						}
					}
				}
				else
				{
					curMagCount = (this.Props.reloadOneAtATime ? (this.curMagCountInt + 1) : this.magazinesize);
				}
				this.CurMagCount = curMagCount;
				bool flag9 = this.turret != null;
				if (flag9)
				{
					this.turret.SetReloading(false);
				}
				bool flag10 = this.parent.def.soundInteract != null;
				if (flag10)
				{
					this.parent.def.soundInteract.PlayOneShot(new TargetInfo(this.Position, Find.CurrentMap, false));
				}
			}
		}


		public void ResetAmmoCount(AmmoDef newAmmo = null)
		{
			bool flag = newAmmo != null;
			if (flag)
			{
				this.currentAmmoInt = newAmmo;
				this.selectedAmmo = newAmmo;
			}
			this.CurMagCount = this.magazinesize;
		}


		public bool TryFindAmmoInInventory(out Thing ammoThing)
		{
			ammoThing = null;
			bool flag = this.CompInventory == null;
			bool result;
			if (flag)
			{
				result = false;
			}
			else
			{
				ammoThing = this.CompInventory.ammoList.Find((Thing thing) => thing.def == this.selectedAmmo);
				bool flag2 = ammoThing != null;
				if (flag2)
				{
					result = true;
				}
				else
				{
					bool flag3 = this.Props.reloadOneAtATime && this.CurMagCount > 0;
					if (flag3)
					{
						result = false;
					}
					else
					{
						using (List<AmmoLink>.Enumerator enumerator = this.Props.ammoSet.ammoTypes.GetEnumerator())
						{
							while (enumerator.MoveNext())
							{
								AmmoLink link = enumerator.Current;
								ammoThing = this.CompInventory.ammoList.Find((Thing thing) => thing.def == link.ammo);
								bool flag4 = ammoThing != null;
								if (flag4)
								{
									this.selectedAmmo = link.ammo;
									return true;
								}
							}
						}
						result = false;
					}
				}
			}
			return result;
		}


		public override IEnumerable<Gizmo> CompGetGizmosExtra()
		{
			GizmoMagazineStatus ammoStatusGizmo = new GizmoMagazineStatus
			{
				compAmmo = this
			};
			yield return ammoStatusGizmo;
			Building_Turret building_Turret = this.turret;
			CompMannable mannableComp = (building_Turret != null) ? building_Turret.GetMannable() : null;
			bool flag = (this.IsEquippedGun && this.Wielder.Faction == Faction.OfPlayer) || (this.turret != null && this.turret.Faction == Faction.OfPlayer && (mannableComp != null || this.UseAmmo));
			if (flag)
			{
				Action action = null;
				bool isEquippedGun = this.IsEquippedGun;
				if (isEquippedGun)
				{
					action = delegate
					{
						this.TryStartReload("67890");
					};
				}
				else
				{
					bool flag2 = mannableComp != null;
					if (flag2)
					{
						action = new Action(this.turret.TryForceReload);
					}
				}
				bool flag3 = this.turret == null;
				string tag;
				if (flag3)
				{
					bool hasMagazine = this.HasMagazine;
					if (hasMagazine)
					{
						tag = "CE_Reload";
					}
					else
					{
						tag = "CE_ReloadNoMag";
					}
				}
				else
				{
					bool flag4 = mannableComp == null;
					if (flag4)
					{
						tag = "CE_ReloadAuto";
					}
					else
					{
						tag = "CE_ReloadManned";
					}
				}
				LessonAutoActivator.TeachOpportunity(ConceptDef.Named(tag), this.turret, OpportunityType.GoodToKnow);
				Command_Reload reloadCommandGizmo = new Command_Reload
				{
					compAmmo = this,
					action = action,
					defaultLabel = ("Switch mag"),
					defaultDesc = "Change magazine",
					icon = ContentFinder<Texture2D>.Get("UI/Buttons/Reload", true) ,
					tutorTag = tag
				};
				yield return reloadCommandGizmo;
				action = null;
				tag = null;
				reloadCommandGizmo = null;
			}
			yield break;
		}


		public override string TransformLabel(string label)
		{

			return label;
		}


		public int curMagCountInt = 0;


		public AmmoDef currentAmmoInt = null;


		private AmmoDef selectedAmmo;


		private Thing ammoToBeDeleted;


		public Building_Turret turret;


		internal static Type rgStance = null;
	}


	[StaticConstructorOnStartup]
	public class GizmoMagazineStatus : Command
	{
		private static bool initialized;
		//Link
		public MagazineUser compAmmo;
		public string prefix = "";

		private static Texture2D FullTex;
		private static Texture2D EmptyTex;
		private static new Texture2D BGTex;

		public override float GetWidth(float maxWidth)
		{
			return 120;
		}


		public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
		{
			if (!initialized)
				InitializeTextures();

			Rect overRect = new Rect(topLeft.x, topLeft.y, GetWidth(maxWidth), Height);
			Widgets.DrawBox(overRect);
			GUI.DrawTexture(overRect, BGTex);

			Rect inRect = overRect.ContractedBy(6);

			// Ammo type
			Rect textRect = inRect;
			textRect.height = overRect.height / 2;
			Text.Font = GameFont.Tiny;
			Widgets.Label(textRect, prefix + (compAmmo.CurrentAmmo == null ? compAmmo.parent.def.LabelCap : compAmmo.CurrentAmmo.ammoClass.LabelCap));

			// Bar
			if (compAmmo.HasMagazine)
			{
				Rect barRect = inRect;
				barRect.yMin = overRect.y + overRect.height / 2f;
				float ePct = (float)compAmmo.CurMagCount / compAmmo.magazinesize;
				Widgets.FillableBar(barRect, ePct);
				Text.Font = GameFont.Small;
				Text.Anchor = TextAnchor.MiddleCenter;
				Widgets.Label(barRect, compAmmo.CurMagCount + " / " + compAmmo.magazinesize);
				Text.Anchor = TextAnchor.UpperLeft;
			}

			return new GizmoResult(GizmoState.Clear);
		}

		private void InitializeTextures()
		{
			if (FullTex == null)
				FullTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.2f, 0.2f, 0.24f));
			if (EmptyTex == null)
				EmptyTex = SolidColorMaterials.NewSolidColorTexture(Color.clear);
			if (BGTex == null)
				BGTex = ContentFinder<Texture2D>.Get("UI/Widgets/DesButBG", true);
			initialized = true;
		}
	}
	public class CompProperties_MagazineUser : CompProperties
	{
		
		public CompProperties_MagazineUser()
		{
			this.compClass = typeof(MagazineUser);
		}

		
		public int magazineSize = 0;

		public magazynier.MagWellDef well;

		
		public float reloadTime = 1f;

		
		public bool reloadOneAtATime = false;

	
		public bool throwMote = true;

		
		public AmmoSetDef ammoSet = null;

		
		public float loadedAmmoBulkFactor = 0f;
	}
}
