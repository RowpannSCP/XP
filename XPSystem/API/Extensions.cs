using System.Collections.Generic;
using Exiled.API.Features;
using XPSystem.API.Serialization;
using XPSystem.Component;
using Badge = XPSystem.API.Features.Badge;

namespace XPSystem.API
{
    public static class Extensions
    {
        public static XPComponent GetXPComponent(this Player ply)
        {
            if (ply.ReferenceHub.TryGetComponent<XPComponent>(out var comp))
                return comp;
            return ply.ReferenceHub.gameObject.AddComponent<XPComponent>();
        }

        public static void AddXP(this XPComponent comp, int amount)
        {
            comp.log.XP += amount;
        }
    }
}