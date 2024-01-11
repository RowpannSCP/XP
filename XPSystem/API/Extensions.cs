namespace XPSystem.API
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using CommandSystem;
    using Hints;
    using MEC;
    using XPSystem.API.Serialization;

    public static class Extensions
    {
#if EXILED
        public static CoroutineHandle? HintCoroutineHandle = null;
#endif
        private static Config _cfg => Main.Instance.Config;

        public static PlayerLog GetLog(this ReferenceHub ply)
        {
            PlayerLog toInsert = null;
            if (string.IsNullOrWhiteSpace(ply.authManager.UserId))
                throw new ArgumentNullException(nameof(ply));
            if (!API.TryGetLog(ply.authManager.UserId, out var log))
            {
                toInsert = new PlayerLog()
                {
                    ID = ply.authManager.UserId,
                    LVL = 0,
                    XP = 0,
                };

                API.PlayerLogCollection.Insert(toInsert);
            }

            if (log is null)
                return toInsert;
            return log;
        }

        public static bool DeleteLog(this ReferenceHub ply) => API.PlayerLogCollection.Delete(ply.authManager.UserId);

        public static void UpdateLog(this PlayerLog log)
        {
            API.PlayerLogCollection.Update(log);
        }

        public static void AddXP(this PlayerLog log, int amount, string message = null)
        {
            if (amount == 0)
            {
                Main.DebugProgress("skipping adding 0 xp");
                return;
            }

            log.XP += amount;
            var ply = ReferenceHub.AllHubs.FirstOrDefault(x => x.authManager.UserId == log.ID);
            if (ply == null)
            {
                Main.LogWarn($"Player {log.ID} not found, skipping");
                return;
            }

            bool gainedLevel = false;
            var ordered = Main.Instance.Config.GetIncreasesOrdered();
            var increase = ordered.FirstOrDefault(x => x.Key <= log.LVL).Value;
            var xpPerLevel = Main.Instance.Config.XPPerLevel;
            var required = xpPerLevel + (increase * log.LVL);
            Main.DebugProgress($"Required: {required} ({xpPerLevel} (xpperlevel) + {increase} (increase) * {log.LVL} (lvl))");
            while (log.XP >= required)
            {
                log.XP -= required;
                log.LVL++;
                increase = ordered.FirstOrDefault(x => x.Key <= log.LVL).Value;
                required = xpPerLevel + (increase * log.LVL);
                Main.DebugProgress($"Gained lvl, new required {required} ({Main.Instance.Config.XPPerLevel} (xpperlevel) + {increase} (increase) * {log.LVL} (lvl))");
                gainedLevel = true;
            }

            if (!gainedLevel && Main.Instance.Config.ShowAddedXP && ply != null)
            {
                ply.ShowCustomHint(message == null ? $" +<color=green>{amount}</color> ({log.XP}/{required}) XP"
                    : message
                        .Replace("%amount%", amount.ToString())
                        .Replace("%required%", required.ToString())
                        .Replace("%current%", log.XP.ToString()));
            }
            log.UpdateLog();
            if (ply != null)
            {
                if (gainedLevel && Main.Instance.Config.ShowAddedLVL)
                {
                    ply.ShowCustomHint(Main.Instance.Config.AddedLVLHint
                        .Replace("%level%", log.LVL.ToString()));
                }
                if (Main.EnabledRank)
                    API.UpdateBadge(ply);
                if (Main.EnabledNick)
                    ply.nicknameSync.DisplayName = ply.nicknameSync.Network_myNickSync;
            }
        }

#if EXILED
        internal static Dictionary<ReferenceHub, List<(float, string)>> _hintQueue = new Dictionary<ReferenceHub, List<(float, string)>>();
        public static IEnumerator<float> HintCoroutine()
        {
            if (!_cfg.EnableCustomHintManager)
                yield break;
            for (;;)
            {
                for (int i = 0; i < _hintQueue.Count; i++)
                {
                    var kvp = _hintQueue.ElementAt(i);
                    List<string> hints = new List<string>();
                    for (int index = 0; index < kvp.Value.Count; index++)
                    {
                        var itemVar = kvp.Value[index];
                        hints.Add(itemVar.Item2);
                        itemVar.Item1 -= (float)_cfg.HintDelay;
                        if(itemVar.Item1 <= 0)
                            _hintQueue[kvp.Key].RemoveAt(index);
                        else
                            _hintQueue[kvp.Key][index] = itemVar;
                    }
                    if (kvp.Value.Count == 0)
                    {
                        _hintQueue.Remove(kvp.Key);
                        continue;
                    }

                    StringBuilder sb = new StringBuilder();
                    foreach (var var in hints)
                    {
                        sb.Append($"<voffset={_cfg.VOffest}em><space={_cfg.HintSpace}em><size={_cfg.HintSize}%>{var}</size></voffset>\n ");
                    }
                    var hintNew = sb.ToString();
                    try
                    {
#if EXILED
                        AdvancedHints.Extensions.ShowManagedHint(Exiled.API.Features.Player.Get(kvp.Key), hintNew, (float)(_cfg.HintDelay + _cfg.HintExtraTime), true, _cfg.HintLocation);
#else
                        kvp.Key.hints.Show(new TextHint(hintNew, new HintParameter[]
                        {
                            new StringHintParameter(hintNew)
                        }, null, (float)(_cfg.HintDelay + _cfg.HintExtraTime)));
#endif
                    }
                    catch (Exception e)
                    {
                        Main.LogError($"Error in hint coroutine: {e}");
                    }
                }
                yield return Timing.WaitForSeconds((float)_cfg.HintDelay);
            }
        }
#endif

        public static void ShowCustomHint(this ReferenceHub ply, string text)
        {
            if (_cfg.HintMode == HintMode.None)
                return;
            text = $"<voffset={_cfg.VOffest}em><space={_cfg.HintSpace}em><size={_cfg.HintSize}%>{text}</size></voffset>";
            switch (_cfg.HintMode)
            {
                case HintMode.Hint:
#if EXILED
                    if (!_cfg.EnableCustomHintManager)
                    {
#endif
                        ply.hints.Show(new TextHint(text, new HintParameter[]
                        {
                            new StringHintParameter(text)
                        }, null, _cfg.HintDuration));
                        return;
#if EXILED
                    }

                    if (_hintQueue.TryGetValue(ply, out var list))
                    {
                        list.Add((_cfg.HintDuration, text));
                        return;
                    }
                    _hintQueue.Add(ply, new List<(float, string)>()
                    {
                        (_cfg.HintDuration, text)
                    });
                    break;
#endif
                case HintMode.Broadcast:
                    Broadcast.Singleton.TargetAddElement(ply.characterClassManager.connectionToClient, text, (ushort)_cfg.HintDuration, Broadcast.BroadcastFlags.Normal);
                    break;
                case HintMode.Console:
                    ply.gameConsoleTransmission.SendToClient(text, "green");
                    break;
            }
        }

        public static bool CheckPermissionInternal(this ICommandSender ply, string perm)
        {
#if EXILED
            return Exiled.Permissions.Extensions.Permissions.CheckPermission(ply, perm);
#else
            return NWAPIPermissionSystem.PermissionHandler.CheckPermission(ply, perm);
#endif
        }
    }
}