using RimWorld;
using Verse;
using System.Collections.Generic;

namespace RimWorkaholic
{
    public class WorkTracker : GameComponent
    {
        private readonly Dictionary<WorkTags, RecordDef> workTagRecords;

        public WorkTracker(Game game)
        {
            workTagRecords = new Dictionary<WorkTags, RecordDef>();
            foreach (RecordDef recordDef in DefDatabase<RecordDef>.AllDefs)
            {
                if (recordDef.defName.StartsWith("WorkTag_"))
                {
                    WorkTags workTag = (WorkTags)System.Enum.Parse(typeof(WorkTags), recordDef.defName.Substring("WorkTag_".Length));
                    workTagRecords[workTag] = recordDef;
                }
            }
        }

        public override void GameComponentTick()
        {
            Log.Message("GameComponentTick called");
            base.GameComponentTick();

            if (Find.TickManager.TicksGame % 60 == 0) // Update every second
            {
                foreach (Pawn pawn in PawnsFinder.AllMaps_FreeColonists)
                {
                    if (pawn.records != null)
                    {
                        float totalWorkDone = 0;

                        foreach (WorkGiverDef workGiver in DefDatabase<WorkGiverDef>.AllDefs)
                        {
                            if (workGiver.workTags != WorkTags.None)
                            {
                                foreach (WorkTags workTag in workTagRecords.Keys)
                                {
                                    if ((workGiver.workTags & workTag) != WorkTags.None)
                                    {
                                        totalWorkDone += pawn.records.GetAsInt(workTagRecords[workTag]);
                                    }
                                }
                            }
                        }

                        RecordDef totalWorkDoneDef = DefDatabase<RecordDef>.GetNamed("TotalWorkDone");
                        pawn.records.AddTo(totalWorkDoneDef, totalWorkDone);
                        Log.Message($"Updated record for pawn {pawn.Name}: {pawn.records.GetValue(totalWorkDoneDef)}");
                        Log.Message($"Total work done for pawn {pawn.Name}: {totalWorkDone}");
                    }
                }
            }
        }
    }
}