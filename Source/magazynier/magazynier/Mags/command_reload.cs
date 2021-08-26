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
	public class Command_Reload : Command_Action
	{
		
		public override bool GroupsWith(Gizmo other)
		{
			Command_Reload command_Reload = other as Command_Reload;
			return command_Reload != null;
		}

		
		public override void MergeWith(Gizmo other)
		{
			Command_Reload item = other as Command_Reload;
			bool flag = this.others == null;
			if (flag)
			{
				this.others = new List<Command_Reload>();
				this.others.Add(this);
			}
			this.others.Add(item);
		}

	
		public override void ProcessInput(Event ev)
		{
			bool flag = this.compAmmo == null;
			if (flag)
			{
				//Log.Error("Command_Reload without ammo comp", false);
			}
			else
			{
				bool flag2 = (this.compAmmo.UseAmmo && (this.compAmmo.CompInventory != null || this.compAmmo.turret != null)) || this.action == null;
				if (flag2)
				{
					Building_Turret turret = this.compAmmo.turret;
					bool? flag3;
					if (turret == null)
					{
						flag3 = null;
					}
					else
					{
						CompMannable mannable = turret.GetMannable();
						flag3 = ((mannable != null) ? new bool?(mannable.MannedNow) : null);
					}
					bool? flag4 = flag3;
					bool valueOrDefault = flag4.GetValueOrDefault();
					bool flag5 = Controller.settings.RightClickAmmoSelect && this.action != null && (this.compAmmo.turret == null || valueOrDefault);
					if (flag5)
					{
						base.ProcessInput(ev);
					}
					else
					{
						Find.WindowStack.Add(this.MakeAmmoMenu());
					}
				}
				else
				{
					bool flag6 = this.compAmmo.SelectedAmmo != this.compAmmo.CurrentAmmo || this.compAmmo.CurMagCount < this.compAmmo.Props.magazineSize;
					if (flag6)
					{
						base.ProcessInput(ev);
					}
				}
				bool flag7 = !this.tutorTag.NullOrEmpty();
				if (flag7)
				{
					PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDef.Named(this.tutorTag), KnowledgeAmount.Total);
				}
			}
		}

	
		public override IEnumerable<FloatMenuOption> RightClickFloatMenuOptions
		{
			get
			{
				bool rightClickAmmoSelect = Controller.settings.RightClickAmmoSelect;
				if (rightClickAmmoSelect)
				{
					foreach (FloatMenuOption option in this.BuildAmmoOptions())
					{
						yield return option;
						//option = null;
					}
					
					
					
					
					//List<FloatMenuOption>.Enumerator enumerator = default(List<FloatMenuOption>.Enumerator);
				}
				yield break;
				//yield break;
			}
		}
		public Thing idl;
		
		private FloatMenu MakeAmmoMenu()
		{
			magazynier.Player_teams.ControllerMGComp mGComp = this.compAmmo.Wielder.TryGetComp<magazynier.Player_teams.ControllerMGComp>();
			List<FloatMenuOption> gjhkl = this.BuildAmmoOptions();
			if(compAmmo.loadedmag != null)
			{
				gjhkl.Add(new FloatMenuOption(defaultLabel = "unload mag", action = delegate
				{
					ThingWithComps trud = compAmmo.loadedmag;
					trud.stackCount = 1;
					GenThing.TryDropAndSetForbidden(trud, compAmmo.Wielder.Position, compAmmo.Wielder.Map, ThingPlaceMode.Direct, out idl, false);
					ThingWithComps thing = (ThingWithComps)idl;
					thing.TryGetComp<Gazine>().loadedAmmo = compAmmo.CurrentAmmo;
					thing.TryGetComp<Gazine>().loadedAmmoAmount = compAmmo.CurMagCount;
					compAmmo.loadedmag = null;
					compAmmo.CurMagCount = 0;
					compAmmo.magazinesize = 1;
					
					

				}));
			}
			if (mGComp != null && mGComp.friends.Any(L => L.TryGetComp<magazynier.Player_teams.ControllerMGComp>() != null && L.TryGetComp<magazynier.Player_teams.ControllerMGComp>().AmIAmmoMan))
			{
				Pawn adude = mGComp.friends.Find(L => L.TryGetComp<magazynier.Player_teams.ControllerMGComp>().AmIAmmoMan);
				List<Thing> PawnInventory = adude.inventory.innerContainer.ToList();
				foreach(Thing mag in PawnInventory)
				{
					if (mag.TryGetComp<Gazine>() != null)
					{
						if (mag.TryGetComp<Gazine>().well == compAmmo.well && compAmmo.Props.ammoSet.ammoTypes.Any(G => G.ammo == mag.TryGetComp<Gazine>().loadedAmmo))
						{
							gjhkl.Add(new FloatMenuOption(adude.Name.ToString() + ": " + "Load " + mag.Label, delegate
							{


								ThinkNode jobGiver = null;
								Pawn_JobTracker jobs = adude.jobs;
								Job job = this.TryMakeReloadJob(mag);
								Job newJob = job;
								JobCondition lastJobEndCondition = JobCondition.InterruptForced;
								Job curJob = adude.CurJob;
								jobs.StartJob(newJob, lastJobEndCondition, jobGiver, ((curJob != null) ? curJob.def : null) != job.def, true, null, null, false, false);




							}));
						}
					}
				}
			}
		
			return new FloatMenu(gjhkl);
		}

		
		private List<FloatMenuOption> BuildAmmoOptions()
		{
			List<FloatMenuOption> list = new List<FloatMenuOption>();
			if (compAmmo.Wielder != null)
			{
				List<Thing> PawnInventory = compAmmo.Wielder.inventory.innerContainer.ToList();
				if (compAmmo.Wielder.inventory.innerContainer.ToList() != null)
				{
					//List<Thing> Magazines = PawnInventory.FindAll(AAA => AAA.TryGetComp<Gazine>().well == compAmmo.well).ToList();
					
					foreach (Thing magazine in PawnInventory)
					{
						if(magazine.TryGetComp<Gazine>() != null)
						{
							if(magazine.TryGetComp<Gazine>().well == compAmmo.well && compAmmo.Props.ammoSet.ammoTypes.Any(G => G.ammo == magazine.TryGetComp<Gazine>().loadedAmmo))
							{
								list.Add(new FloatMenuOption("Load " + magazine.Label, delegate
								{


									ThinkNode jobGiver = null;
									Pawn_JobTracker jobs = this.compAmmo.Wielder.jobs;
									Job job = this.TryMakeReloadJob(magazine);
									Job newJob = job;
									JobCondition lastJobEndCondition = JobCondition.InterruptForced;
									Job curJob = this.compAmmo.Wielder.CurJob;
									jobs.StartJob(newJob, lastJobEndCondition, jobGiver, ((curJob != null) ? curJob.def : null) != job.def, true, null, null, false, false);
									
							

								
								}));
							}
						}
						
					}
					
					return list;
					
				}

				else
				{
					Log.Error("magazines is null");
					return null;
				}
			}
			else
			{
				Log.Error("wielder is null");
				return null;
			}
			
			
			
			
			
		}

		public Job TryMakeReloadJob(Thing magazinier)
		{
			return new Job(MiscDefOf.SwitchMagazine, this.compAmmo.parent, magazinier);
		}
		private List<Command_Reload> others;

		
		public MagazineUser compAmmo;
	}
	
	public class SficzMags : JobDriver
	{
		private ThingWithComps weapon
		{
			get
			{
				return base.TargetThingA as ThingWithComps;
			}
		}
		private ThingWithComps magazine
		{
			get
			{
				return base.TargetThingB as ThingWithComps;
				
			}
		}
		
		private const TargetIndex GunToUnjam = TargetIndex.A;



		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			
			return this.pawn.Reserve(this.job.GetTarget(GunToUnjam), this.job, 1, -1, null);
		}
		public Gazine MagazineThingComp
		{
			get
			{
				return this.magazine.TryGetComp<Gazine>();
			}
		}
		public MagazineUser MagazineUserComp
		{
			get
			{
				return this.weapon.TryGetComp<MagazineUser>();
			}
		}
		protected override IEnumerable<Toil> MakeNewToils()
		{

			Toil toil = new Toil();
			if (GetActor().TryGetComp<magazynier.Player_teams.ControllerMGComp>()?.AmIAmmoMan  ?? false)
			{
				toil.AddPreInitAction(delegate
				{
					Log.Message("dfghvjkl");
				});
				
				toil = Toils_General.Wait(24 * TargetB.Thing.TryGetComp<Gazine>().reloadTime);
				
			}
			else
			{
				toil = Toils_General.Wait(60 * TargetB.Thing.TryGetComp<Gazine>().reloadTime);
			}
			
			
			
			toil.AddFinishAction(delegate
			{
				if (MagazineUserComp.loadedmag != null)
				{
					//Log.Error(MagazineUserComp.loadedmag.ToString());

					Thing nic = new Thing();
					MagazineUserComp.loadedmag.stackCount = 1;
					ThingWithComps Magazynierowanie = MagazineUserComp.loadedmag;

					GenThing.TryDropAndSetForbidden(Magazynierowanie, this.pawn.Position, this.Map, ThingPlaceMode.Direct, out nic, false);
					Magazynierowanie.TryGetComp<Gazine>().loadedAmmoAmount = MagazineUserComp.CurMagCount;
					Magazynierowanie.TryGetComp<Gazine>().loadedAmmo = MagazineUserComp.CurrentAmmo;

				}
				MagazineUserComp.magazinesize = magazine.TryGetComp<Gazine>().MagazineSize;
				MagazineUserComp.curMagCountInt = magazine.TryGetComp<Gazine>().loadedAmmoAmount;
				MagazineUserComp.currentAmmoInt = magazine.TryGetComp<Gazine>().loadedAmmo;
				MagazineUserComp.CurrentAmmo = magazine.TryGetComp<Gazine>().loadedAmmo;
				MagazineUserComp.Props.magazineSize = magazine.TryGetComp<Gazine>().MagazineSize;
				//Log.Message(MagazineUserComp.CurrentAmmo.ToString());



				//Log.Message(magazine.ToString());
				MagazineUserComp.loadedmag = magazine;
				
				//Log.Message(MagazineUserComp.loadedmag.ToString());
				if (magazine.stackCount > 1)
				{
					--magazine.stackCount;
				}
				else
				{
					magazine.Destroy();
				}
				
			});
			
			yield return toil;

			
		}
		


	}
	[DefOf]
	public static class MiscDefOf
	{
		
		static MiscDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(MiscDefOf));
		}

		public static ThingDef Bullet_556x45mmNATO_FMJ;
		public static JobDef SwitchMagazine;
		public static JobDef PackMag;
		
	}
}
