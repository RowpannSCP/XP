using System.Collections.Generic;
using XPSystem.API.Serialization;

namespace XPSystem.API
{
    using System.Linq;
    using CommandSystem;
    using Hints;
    using MEC;

    public static class Extensions
    {
        public static CoroutineHandle? HintCoroutineHandle = null;
        private static Config _cfg => Main.Instance.Config;
        public static PlayerLog GetLog(this ReferenceHub ply)
        {
            PlayerLog toInsert = null;
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
            log.XP += amount;
            ReferenceHub ply = ReferenceHub.AllHubs.First(x => x.characterClassManager.UserId == log.ID);
            int lvlsGained = log.XP / Main.Instance.Config.XPPerLevel;
            if (lvlsGained > 0)
            {
                log.LVL += lvlsGained;
                log.XP -= lvlsGained * Main.Instance.Config.XPPerLevel;
                if (Main.Instance.Config.ShowAddedLVL && ply != null)
                {
                    ply.ShowCustomHint(Main.Instance.Config.AddedLVLHint
                        .Replace("%level%", log.LVL.ToString()));
                }

                ply.serverRoles.SetText(string.Empty);
            }
            else if (Main.Instance.Config.ShowAddedXP && ply != null)
            {
                ply.ShowCustomHint(message == null ? $"+ <color=green>{amount}</color> XP" : message.Replace("%amount%", amount.ToString()));
            }
            log.UpdateLog();
        }

        internal static Dictionary<ReferenceHub, List<(float, string)>> _hintQueue = new Dictionary<ReferenceHub, List<(float, string)>>();
        public static IEnumerator<float> HintCoroutine()
        {
            for (;;)
            {
                for (int i = 0; i < _hintQueue.Count; i++)
                {
                    if (_hintQueue.Count == 0 || i >= _hintQueue.Count)
                    {
                        break;
                    }
                    var kvp = _hintQueue.ElementAt(i);
                    bool display = true;
                    string hint = "";
                    if (kvp.Value.Count == 0)
                    {
                        _hintQueue.Remove(kvp.Key);
                        display = false;
                    }
                    for (int index = 0; index < kvp.Value.Count; index++)
                    {
                        if (kvp.Value.Count == 0 || index >= kvp.Value.Count)
                        {
                            display = false;
                            break;
                        }
                        var itemVar = kvp.Value[index];
                        hint += itemVar.Item2;
                        hint += "\n";
                        itemVar.Item1 -= .1f;
                        if(itemVar.Item1 <= 0)
                            _hintQueue[kvp.Key].RemoveAt(index);
                        else
                            _hintQueue[kvp.Key][index] = itemVar;
                    }
                    if(!display)
                        continue;
                    string hintNew = "";
                    foreach (var var in hint.Split('\n'))
                    {
                        hintNew +=
                            $"<voffset={_cfg.VOffest}em><space={_cfg.HintSpace}em><size={_cfg.HintSize}%>{var}</size></voffset> \n ";
                    }
#if EXILED
                    AdvancedHints.Extensions.ShowManagedHint(Exiled.API.Features.Player.Get(kvp.Key), hintNew, .1f, true, _cfg.HintLocation);
#else
                    kvp.Key.hints.Show(new TextHint(hintNew, new HintParameter[]
                    {
                        new StringHintParameter(hintNew)
                    }, null, .1f));
#endif
                }
                yield return Timing.WaitForSeconds(.1f);
            }
        }

        public static void ShowCustomHint(this ReferenceHub ply, string text)
        {
            if (_hintQueue.TryGetValue(ply, out var list))
            {
                list.Add((_cfg.HintDuration, text));
                return;
            }
            _hintQueue.Add(ply, new List<(float, string)>()
            {
                (_cfg.HintDuration, text)
            });
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