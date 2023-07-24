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
            if (string.IsNullOrWhiteSpace(ply.characterClassManager.UserId))
                throw new ArgumentNullException(nameof(ply));
            if (!API.TryGetLog(ply.characterClassManager.UserId, out var log))
            {
                toInsert = new PlayerLog()
                {
                    ID = ply.characterClassManager.UserId,
                    LVL = 0,
                    XP = 0,
                };
                Main.Instance.db.GetCollection<PlayerLog>("Players").Insert(toInsert);
            }

            if (log is null)
                return toInsert;
            return log;
        }

        public static void UpdateLog(this PlayerLog log)
        {
            Main.Instance.db.GetCollection<PlayerLog>("Players").Update(log);
        }

        public static void AddXP(this PlayerLog log, int amount, string message = null)
        {
            if (amount == 0)
            {
                Main.DebugProgress("skipping adding 0 xp");
                return;
            }
            log.XP += amount;
            ReferenceHub ply = ReferenceHub.AllHubs.FirstOrDefault(x => x.characterClassManager.UserId == log.ID);
            int lvlsGained = log.XP / Main.Instance.Config.XPPerLevel;
            if (lvlsGained > 0)
            {
                log.LVL += lvlsGained;
                log.XP -= lvlsGained * Main.Instance.Config.XPPerLevel;
            }
            else if (Main.Instance.Config.ShowAddedXP && ply != null)
            {
                ply.ShowCustomHint(message == null ? $"+ <color=green>{amount}</color> XP" : message.Replace("%amount%", amount.ToString()));
            }
            log.UpdateLog();
            if (ply != null)
            {
                if (lvlsGained > 0 && Main.Instance.Config.ShowAddedLVL)
                {
                    ply.ShowCustomHint(Main.Instance.Config.AddedLVLHint
                        .Replace("%level%", log.LVL.ToString()));
                }
                API.UpdateBadge(ply);
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
#if EXILED
            if (!_cfg.EnableCustomHintManager)
            {
#endif
                text = $"<voffset={_cfg.VOffest}em><space={_cfg.HintSpace}em><size={_cfg.HintSize}%>{text}</size></voffset>";
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
#endif
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