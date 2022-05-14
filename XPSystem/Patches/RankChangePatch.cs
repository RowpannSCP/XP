using Exiled.API.Features;
using HarmonyLib;
using NorthwoodLib.Pools;
using System.Collections.Generic;
using System.Reflection.Emit;
using static HarmonyLib.AccessTools;

namespace XPSystem.Patches
{
    [HarmonyPatch(typeof(ServerRoles), nameof(ServerRoles.SetText))]
    public class RankChangePatch
    {

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            List<CodeInstruction> newInstructions = ListPool<CodeInstruction>.Shared.Rent(instructions);
            int index = 0;
            Label DNTLabel = generator.DefineLabel();
            LocalBuilder player = generator.DeclareLocal(typeof(Player));
            var inserted = new[]
            {
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldfld, Field(typeof(ServerRoles), nameof(ServerRoles._hub))),
                new CodeInstruction(OpCodes.Call, Method(typeof(Player), nameof(Player.Get), new[] { typeof(ReferenceHub) })),
                new CodeInstruction(OpCodes.Dup),
                new CodeInstruction(OpCodes.Stloc, player.LocalIndex),
                new CodeInstruction(OpCodes.Callvirt, PropertyGetter(typeof(Player), nameof(Player.DoNotTrack))),
                new CodeInstruction(OpCodes.Brtrue_S, DNTLabel),
                new CodeInstruction(OpCodes.Call, PropertyGetter(typeof(Main), nameof(Main.Players))),
                new CodeInstruction(OpCodes.Ldloc, player.LocalIndex),
                new CodeInstruction(OpCodes.Callvirt, PropertyGetter(typeof(Player), nameof(Player.UserId))),
                new CodeInstruction(OpCodes.Callvirt, Method(typeof(Dictionary<string, PlayerLog>), "get_Item")), // cs0571 moment
                new CodeInstruction(OpCodes.Ldarg_1),
                new CodeInstruction(OpCodes.Callvirt, Method(typeof(PlayerLog), nameof(PlayerLog.EvaluateRank))),
                new CodeInstruction(OpCodes.Starg_S, 1),
                new CodeInstruction(OpCodes.Nop).WithLabels(DNTLabel)
            };

            newInstructions.InsertRange(index, inserted);

            for (int z = 0; z < newInstructions.Count; z++)
                yield return newInstructions[z];

            ListPool<CodeInstruction>.Shared.Return(newInstructions);
        }
    }
}
