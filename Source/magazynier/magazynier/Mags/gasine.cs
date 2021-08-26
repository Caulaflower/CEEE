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
using HarmonyLib;
using HarmonyMod;


namespace magazynier
{
	public class Gazine : ThingComp
	{
		public AmmoDef loadedAmmo;

		public AmmoSetDef ammosetdef => Props.ammosetdef;

		public int index;
		public bool linked;



		public int reloadTime => Props.reloadtime;

		public int MagazineSize
		{
			get
			{
				if (linked)
				{
					return idk;
				}
				else
				{
					return Props.MagazineSize;
				}
				
			}
			set
			{
				idk = MagazineSize;
			}
		}
		public int idk;

		public int Magseizuer => Props.MagazineSize;

		public int loadedAmmoAmount;
		public magazynier.MagWellDef well => Props.MagazineWell;

		public GasineProp Props => (GasineProp)this.props;
		public override void CompTick()
		{

			base.CompTick();

			Pawn pawn;
			if (this.parent.ParentHolder is Pawn_InventoryTracker)
			{
				Pawn_InventoryTracker papa = (Pawn_InventoryTracker)this.parent.ParentHolder;
				pawn = papa.pawn;
				if (this.loadedAmmoAmount != this.MagazineSize)
				{

				}

			}
		}
		public override IEnumerable<Gizmo> CompGetGizmosExtra()
		{

			yield return new Command_Action()
			{
				defaultDesc = "Load ammunition into a magazine",
				defaultLabel = "pack magazine",
				icon = ContentFinder<Texture2D>.Get("Mag/Mag1"),

				action = delegate
				{
					var pawnoptions = new List<FloatMenuOption>
					{

					};
					foreach (Pawn szteele in this.parent.Map.mapPawns.FreeColonists)
					{
						FloatMenuOption optionen = new FloatMenuOption(szteele.Name.ToString(), delegate
						{
							Pawn loader = szteele;
							List<AmmoThing> ammonmap = new List<AmmoThing>();
							foreach (AmmoThing abc in this.parent.Map.listerThings.AllThings.FindAll(ABC => ABC is AmmoThing))
							{
								ammonmap.Add(abc);


							}
							List<AmmoThing> amogus = new List<AmmoThing>();
							if (loadedAmmo == null)
							{
								amogus = ammonmap.FindAll(GHC => DefDatabase<AmmoSetDef>.AllDefs.ToList().Find(ABC => ABC == this.ammosetdef).ammoTypes.Find(BCD => BCD.ammo == GHC.def) != null);
								if(this.Props.secondaryammo != null)
								{
									amogus.AddRange(ammonmap.FindAll(gay => this.Props.secondaryammo.ammoTypes.Any(gay2 => gay2.ammo == gay.def)));
								}
								
								
							}
							else
							{
								amogus = ammonmap.FindAll(GHC => DefDatabase<AmmoSetDef>.AllDefs.ToList().Find(ABC => ABC == this.ammosetdef).ammoTypes.Find(BCD => BCD.ammo == GHC.def) != null && GHC.def == loadedAmmo);
							}
							AmmoThing amungus = new AmmoThing();
							var optionsobama = new List<FloatMenuOption>
							{

							};

							foreach (AmmoThing szteel in amogus)
							{
								FloatMenuOption optione = new FloatMenuOption(szteel.def.label, delegate
								{
									//Log.Error(szteel.def.label);
									AmmoThing ammoton = ammonmap.Find(GHC => DefDatabase<AmmoSetDef>.AllDefs.ToList().Find(ABC => ABC == this.ammosetdef).ammoTypes.Find(BCD => BCD.ammo == GHC.def) != null && GHC.IsForbidden(loader) == false);
									//Log.Error(ammoton.ToString());
									boj = new Job(MiscDefOf.PackMag, szteel, this.parent);

									ThinkNode jobGiver = null;
									Pawn_JobTracker jobs = loader.jobs;
									Job job = this.boj;
									Job newJob = job;
									JobCondition lastJobEndCondition = JobCondition.InterruptForced;
									Job curJob = loader.CurJob;
									loader.jobs.StartJob(boj, lastJobEndCondition, jobGiver, ((curJob != null) ? curJob.def : null) != job.def, true, null, null, false, false);
								});
								optionsobama.Add(optione);
								Find.WindowStack.Add(new FloatMenu(optionsobama));
							}
						});
						pawnoptions.Add(optionen);
						Find.WindowStack.Add(new FloatMenu(pawnoptions));
					}




				}
			};
			yield return new Command_Action()
			{
				defaultDesc = "unload magazine",
				defaultLabel = "unload magazine",
				icon = ContentFinder<Texture2D>.Get("Mag/Mag1"),

				action = fgh()
			};
			yield return new GizmogazineStatus()
			{
				compAmmo = this,
				prefix = "loaded with: "
			};
			yield break;

		}
		public Job boj;


		public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_Values.Look<int>(ref loadedAmmoAmount, "loadedAmmoAmount");
			Scribe_Defs.Look<AmmoDef>(ref loadedAmmo, "loadedAmmo");
			Scribe_Values.Look<int>(ref idk, "idk");
			Scribe_Values.Look<bool>(ref linked, "linked");
		}
		public Action fgh()
		{
			if(this.parent.TryGetComp<Gazine>().loadedAmmo != null)
			{
				return delegate { Thing abcdefg = ThingMaker.MakeThing(this.parent.TryGetComp<Gazine>().loadedAmmo); abcdefg.stackCount = this.parent.TryGetComp<Gazine>().loadedAmmoAmount; GenThing.TryDropAndSetForbidden(abcdefg, this.parent.Position, this.parent.Map, ThingPlaceMode.Direct, out abcdefg, true); this.parent.TryGetComp<Gazine>().loadedAmmo = null; this.parent.TryGetComp<Gazine>().loadedAmmoAmount = 0; };
			}
			else
			{
				return delegate { Log.Message("No ammodef loaded"); };
			}
			
		}
		public Action pain(Pawn dad)
		{
			Thing h = ThingMaker.MakeThing(ThingDefOf.Cloth);
			List<AmmoThing> alist = Find.CurrentMap.listerThings.AllThings.Select(F => F as AmmoThing).ToList();
			
			List<FloatMenuOption> flotmenulist = new List<FloatMenuOption>();
			foreach(AmmoThing abc in this.parent.Map.listerThings.AllThings.FindAll(ABC => ABC is AmmoThing))
			{
				
				foreach(AmmoLink amlink in this.Props.ammosetdef.ammoTypes)
				{
					
				}
					AmmoThing ammo = (AmmoThing)abc;
					if(this.Props.ammosetdef.ammoTypes.Find(G => G.ammo == ammo.def) != null)
					{
						flotmenulist.Add(new FloatMenuOption(ammo.Label, delegate { dad.jobs.StartJob(new Job { def = MiscDefOf.PackMag, targetA = ammo, targetB = this.parent }, JobCondition.InterruptForced, null); }));;
					}
				
			}
			return delegate
			{
				Log.Message("Allah thank you. Allahu Ackbar");
				if(flotmenulist.Count > 0)
				{
					Find.WindowStack.Add(new FloatMenu(flotmenulist));
				}
				else
				{
					Log.Message("keine options");
				}
				
			};
		}

		public Action idkfkfk(Pawn actor, ThingWithComps thingWithComps)
		{
			return delegate
			{
				List<Thing> things = Find.CurrentMap.listerThings.AllThings.FindAll(P => P.TryGetComp<Gazine>() != null && P.TryGetComp<Gazine>().Props.islinkable == true && P.def == thingWithComps.def);
				List<ThingWithComps> idkf = new List<ThingWithComps>();
				List<FloatMenuOption> floatMenuOptions = new List<FloatMenuOption>();
				foreach(Thing thing in things)
				{
					ThingWithComps thingWithComp = (ThingWithComps)thing;
					if(thingWithComp.TryGetComp<Gazine>().loadedAmmo == thingWithComps.TryGetComp<Gazine>().loadedAmmo)
					{
						idkf.Add(thingWithComp);
					}

				}
				if (idkf.Contains(this.parent))
				{
					idkf.Remove(this.parent);
				}
				foreach(ThingWithComps ass in idkf)
				{
					floatMenuOptions.Add(new FloatMenuOption("Link with " + ass.Label, delegate { actor.jobs.StartJob(new Job { targetA = this.parent, targetB = ass, def = DefDatabase<JobDef>.AllDefs.ToList().Find(O => O.driverClass == typeof(LinkBelts)) }); })) ;
				}
				Find.WindowStack.Add(new FloatMenu(floatMenuOptions));
			};
		}
		public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn)
		{
			yield return new FloatMenuOption("Load magazine", pain(selPawn));
			if (this.Props.islinkable)
			{
				yield return new FloatMenuOption("Link belt", idkfkfk(selPawn, this.parent));
			}
			
		}
	}

	public class GasineProp : CompProperties
	{
		public int MagazineSize;
		public magazynier.MagWellDef MagazineWell;
		public int reloadtime;
		public AmmoSetDef secondaryammo;
		public AmmoSetDef ammosetdef;
		public bool islinkable = false;

		public GasineProp()
		{
			this.compClass = typeof(Gazine);
		}

		public GasineProp(Type compClass) : base(compClass)
		{
			this.compClass = compClass;
		}
	}

	public class LinkBelts : JobDriver
    {
      

       
        private const TargetIndex belt1 = TargetIndex.A;
        private const TargetIndex belt2 = TargetIndex.B;
       
       


        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return this.pawn.Reserve(this.job.GetTarget(belt1), this.job, 1, -1, null) && this.pawn.Reserve(this.job.GetTarget(belt2), this.job, 1, -1, null);
        }


		protected override IEnumerable<Toil> MakeNewToils()
		{
			yield return Toils_Goto.GotoThing(belt2, PathEndMode.OnCell);
			yield return Toils_Haul.StartCarryThing(belt2);
			yield return Toils_General.PutCarriedThingInInventory();
			yield return Toils_Goto.GotoThing(belt1, PathEndMode.OnCell);
			yield return Toils_Haul.StartCarryThing(belt1);
			yield return Toils_General.PutCarriedThingInInventory();
			Toil idkpain = Toils_General.Wait(120);
			Gazine gaz1 = TargetB.Thing.TryGetComp<Gazine>();
			Gazine gaz2 = TargetA.Thing.TryGetComp<Gazine>();
			idkpain.AddFinishAction(delegate
			{

				gaz1.idk = gaz2.MagazineSize + gaz1.MagazineSize;
				gaz1.linked = true;
				TargetB.Thing.TryGetComp<Gazine>().loadedAmmoAmount += TargetB.Thing.TryGetComp<Gazine>().loadedAmmoAmount;
				//TargetB.Thing.set
				TargetA.Thing.Destroy();
				
				Log.Message(gaz1.MagazineSize.ToString());
			});
			yield return idkpain;

		}
    }

	
	public static class someutilagainidfk
	{
		public static List<FloatMenuOption> floatMenus(ThingWithComps thing, Pawn pawn)
		{
			Gazine c = thing.TryGetComp<Gazine>();
			if (c == null)
			{
				return null;
			}
			List<AmmoThing> ammonmap = new List<AmmoThing>();
			foreach (AmmoThing abc in Find.CurrentMap.listerThings.AllThings.FindAll(ABC => ABC is AmmoThing))
			{
				ammonmap.Add(abc);


			}
			List<AmmoThing> amogus = new List<AmmoThing>();
			if (c.loadedAmmo == null)
			{
				amogus = ammonmap.FindAll(GHC => DefDatabase<AmmoSetDef>.AllDefs.ToList().Find(ABC => ABC == c.ammosetdef).ammoTypes.Find(BCD => BCD.ammo == GHC.def) != null);
				if (c.Props.secondaryammo != null)
				{
					amogus.AddRange(ammonmap.FindAll(gay => c.Props.secondaryammo.ammoTypes.Any(gay2 => gay2.ammo == gay.def)));
				}


			}
			else
			{
				amogus = ammonmap.FindAll(GHC => DefDatabase<AmmoSetDef>.AllDefs.ToList().Find(ABC => ABC == c.ammosetdef).ammoTypes.Find(BCD => BCD.ammo == GHC.def) != null && GHC.def == c.loadedAmmo);
			}
			List<FloatMenuOption> idkfk = new List<FloatMenuOption>();
			foreach(AmmoThing ammo in amogus)
			{
				idkfk.Add(new FloatMenuOption("pack magazine", delegate {
					pawn.jobs.StartJob(new Job
					{
						def = DefDatabase<JobDef>.AllDefs.ToList().Find(G => G.defName == "PackMag"),
						targetA = ammo
					}, JobCondition.InterruptForced); })); 
			}
			return idkfk;
		}
	}

	[StaticConstructorOnStartup]
	public class GizmogazineStatus : Command
	{
		private static bool initialized;
		//Link
		public Gazine compAmmo;
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
			Widgets.Label(textRect, prefix + (compAmmo.loadedAmmo == null ? compAmmo.parent.def.LabelCap : compAmmo.loadedAmmo.ammoClass.LabelCap));

			// Bar
			
				Rect barRect = inRect;
				barRect.yMin = overRect.y + overRect.height / 2f;
				float ePct = (float)compAmmo.loadedAmmoAmount / compAmmo.MagazineSize;
				Widgets.FillableBar(barRect, ePct);
				Text.Font = GameFont.Small;
				Text.Anchor = TextAnchor.MiddleCenter;
				Widgets.Label(barRect, compAmmo.loadedAmmoAmount + " / " + compAmmo.MagazineSize);
				Text.Anchor = TextAnchor.UpperLeft;
			

			return new GizmoResult(GizmoState.Clear);
		}

		private void InitializeTextures()
		{
			if (FullTex == null)
				FullTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.24f, 0.2f, 0.2f));
			if (EmptyTex == null)
				EmptyTex = SolidColorMaterials.NewSolidColorTexture(Color.clear);
			if (BGTex == null)
				BGTex = ContentFinder<Texture2D>.Get("UI/Widgets/DesButBG", true);
			initialized = true;
		}
	}
	
}
