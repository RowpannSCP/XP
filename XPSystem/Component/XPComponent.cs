using System;
using Exiled.API.Features;
using LiteDB;
using UnityEngine;
using XPSystem.API.Serialization;

namespace XPSystem.Component
{
    public class XPComponent : MonoBehaviour
    {
        private Player player;

        public PlayerLog log
        {
            get
            {
                return log;
            }
            set
            {
                log = value;
                col.Update(log);
            }
        }

        private ILiteCollection<PlayerLog> col;

        private void Start()
        {
            player = Player.Get(this.gameObject);
            col = Main.Instance.db.GetCollection<PlayerLog>("Players");
            log = col.FindById(player.UserId);
            if (log is null)
            {
                col.Insert(new PlayerLog()
                {
                    ID = player.UserId,
                    XP = 0,
                    LVL = 0,
                    Name = player.Nickname
                });
                log = col.FindById(player.UserId);
            }
        }
    }
}