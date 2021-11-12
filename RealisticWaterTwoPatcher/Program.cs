using System;
using System.Collections.Generic;
using System.Linq;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Skyrim;
using System.Threading.Tasks;
using Mutagen.Bethesda.Plugins;

namespace RealisticWaterTwoPatcher
{
    public class Program
    {
        public static async Task<int> Main(string[] args)
        {
            return await SynthesisPipeline.Instance
                .AddPatch<ISkyrimMod, ISkyrimModGetter>(RunPatch)
                .SetTypicalOpen(GameRelease.SkyrimSE, "RWTPatch.esp")
                .Run(args);
        }

        public static void RunPatch(IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
        {
            ISkyrimModGetter? rwtEntries = state.LoadOrder.TryGetValue(ModKey.FromFileName("RealisticWaterTwo.esp"))?.Mod ?? null;
            
            if(rwtEntries is null)
                throw new Exception("Realistic Water Two is not present in the load order, make sure you installed it correctly.");

            foreach(var rwtCellBlock in rwtEntries.Cells.Records)
            {
                foreach(var rwtSubCellBlock in rwtCellBlock.SubBlocks)
                {
                    foreach(var rwtCell in rwtSubCellBlock.Cells)
                    {
                        if (!rwtCell.AsLink().TryResolveContext<ISkyrimMod, ISkyrimModGetter, ICell, ICellGetter>(state.LinkCache, out var winningCellContext)) continue;
                        if (winningCellContext.Record.Equals(rwtCell)) continue;

                        ICell winningRecord = winningCellContext.GetOrAddAsOverride(state.PatchMod);
                        if (rwtCell.Flags.HasFlag(Cell.Flag.HasWater))
                            winningRecord.Flags |= Cell.Flag.HasWater;
                        if(!rwtCell.Water.Equals(FormLinkNullableGetter<IWaterGetter>.Null))
                            winningRecord.Water = rwtCell.Water.AsSetter().AsNullable();
                        winningRecord.WaterHeight = rwtCell.WaterHeight;
                    }
                }
            }

            foreach(var rwtWorldspace in rwtEntries.Worldspaces)
            {
                if (rwtWorldspace.AsLink().TryResolveContext<ISkyrimMod, ISkyrimModGetter, IWorldspace, IWorldspaceGetter>(state.LinkCache, out var winningWorldspaceContext))
                {
                    if (!winningWorldspaceContext.Record.Equals(rwtWorldspace))
                    {
                        IWorldspace winningRecord = winningWorldspaceContext.GetOrAddAsOverride(state.PatchMod);
                        if (!rwtWorldspace.Water.Equals(FormLinkNullableGetter<IWaterGetter>.Null))
                            winningRecord.Water = rwtWorldspace.Water.AsSetter().AsNullable();
                        if (!rwtWorldspace.Water.Equals(FormLinkNullableGetter<IWaterGetter>.Null))
                            winningRecord.LodWater = rwtWorldspace.LodWater.AsSetter().AsNullable();
                    }
                }

                foreach(var rwtCellBlock in rwtWorldspace.SubCells)
                {
                    foreach(var rwtSubCellBlock in rwtCellBlock.Items)
                    {
                        foreach(var rwtCell in rwtSubCellBlock.Items)
                        {
                            if (!rwtCell.AsLink().TryResolveContext<ISkyrimMod, ISkyrimModGetter, ICell, ICellGetter>(state.LinkCache, out var winningCellContext)) continue;
                            if (winningCellContext.Record.Equals(rwtCell)) continue;

                            ICell winningRecord = winningCellContext.GetOrAddAsOverride(state.PatchMod);
                            if (rwtCell.Flags.HasFlag(Cell.Flag.HasWater))
                                winningRecord.Flags |= Cell.Flag.HasWater;
                            if (!rwtCell.Water.Equals(FormLinkNullableGetter<IWaterGetter>.Null))
                                winningRecord.Water = rwtCell.Water.AsSetter().AsNullable();
                            winningRecord.WaterHeight = rwtCell.WaterHeight;
                        }
                    }
                }
            }
        }
    }
}
