namespace XPSystem.API.DisplayProviders
{
    using System;
    using XPSystem.API.StorageProviders;
    using static XPAPI;

    /// <summary>
    /// A display provider that uses fake sync vars to display data.
    /// </summary>
    /// <typeparam name="TConfig">The config type.</typeparam>
    /// <typeparam name="TObject">The object type that is different for each player.</typeparam>
    public abstract class SyncVarXPDisplayProvider<TConfig, TObject> : XPDisplayProvider<TConfig> where TConfig : IXPDisplayProviderConfig, new()
    {
        /// <summary>
        /// Gets the key used to the player variables cache.
        /// </summary>
        protected abstract string VariableKey { get; }

        /// <summary>
        /// Gets the sync vars to use.
        /// </summary>
        protected abstract (Type typeName, string methodName, Func<XPPlayer, TObject, object> getFakeSyncVar, Func<XPPlayer, object> getResyncVar)[] SyncVars { get; }

        /// <summary>
        /// Creates the object for the player.
        /// </summary>
        /// <param name="player">The player to create the object for.</param>
        /// <param name="playerInfo">The player's info to create the object from.</param>
        /// <returns>The created object.</returns>
        protected abstract TObject CreateObject(XPPlayer player, PlayerInfoWrapper playerInfo);

        protected override void RefreshToEnabled(XPPlayer player)
        {
            int count = 0;

            foreach (var kvp in XPPlayer.Players)
            {
                if (player == kvp.Value)
                    continue;

                if (!ShouldEdit(kvp.Value))
                    continue;

                TObject obj = GetObject(kvp.Value);
                if (obj == null)
                    continue;

                foreach (var syncVar in SyncVars)
                {
                    player.SendFakeSyncVar(kvp.Value.NetworkIdentity, syncVar.typeName, syncVar.methodName, syncVar.getFakeSyncVar(kvp.Value, obj));
                    count++;
                }
            }

            LogDebug(GetType().Name + " sent all (" + count + ") to " + player.Nickname + " (enabled)");
        }

        protected override void RefreshToDisabled(XPPlayer player)
        {
            int count = 0;

            foreach (var kvp in XPPlayer.Players)
            {
                if (player == kvp.Value)
                    continue;

                if (!ShouldEdit(kvp.Value))
                    continue;

                foreach (var syncVar in SyncVars)
                {
                    player.SendFakeSyncVar(kvp.Value.NetworkIdentity, syncVar.typeName, syncVar.methodName, syncVar.getResyncVar(player));
                    count++;
                }
            }

            LogDebug(GetType().Name + " sent all (" + count + ") to " + player.Nickname + " (disabled)");
        }

        protected override void RefreshOfEnabled(XPPlayer player, PlayerInfoWrapper playerInfo)
        {
            if (!ShouldEdit(player))
                return;

            TObject obj = GetObject(player, playerInfo, true);
            if (obj == null)
                return;

            int count = 0;
            foreach (var syncVar in SyncVars)
                count += player.SendFakeSyncVars(x => ShouldShowTo(player, x), syncVar.typeName, syncVar.methodName, syncVar.getFakeSyncVar(player, obj));

            LogDebug(GetType().Name + " of " + player.Nickname + " sent to all (" + count + ") (enabled)");
        }

        protected override void RefreshOfDisabled(XPPlayer player)
        {
            if (!ShouldEdit(player))
                return;

            foreach (var syncVar in SyncVars)
                player.ResyncSyncVar(syncVar.typeName, syncVar.methodName);

            LogDebug(GetType().Name + " of " + player.Nickname + " sent to all (disabled)");
        }

        /// <summary>
        /// Gets the object for the player, creating it if it doesn't exist in the players variables.
        /// </summary>
        /// <param name="player">The player to get the object for.</param>
        /// <param name="playerInfo">The player's info to create the object from.</param>
        /// <param name="update">Whether or not to force creation of a new object, even if one already exists.</param>
        /// <returns>The object for the player.</returns>
        protected virtual TObject GetObject(XPPlayer player, PlayerInfoWrapper playerInfo = null, bool update = false)
        {
            if (VariableKey == null)
                return default;

            if (update || !player.Variables.TryGet(VariableKey, out TObject obj))
            {
                obj = CreateObject(player, playerInfo ?? player.GetPlayerInfo());
                player.Variables.Set(VariableKey, obj);
            }

            return obj;
        }

        /// <summary>
        /// Whether or not the player's data should be displayed.
        /// </summary>
        /// <param name="player">The player to check.</param>
        /// <returns>Whether or not the player's data should be displayed.</returns>
        protected virtual bool ShouldEdit(XPPlayer player) => true;

        /// <summary>
        /// Whether or not the player's data should be shown to the target player.
        /// </summary>
        /// <param name="player">The player to show the data of.</param>
        /// <param name="target">The target player to show the data to.</param>
        /// <returns>Whether or not the player's data should be shown to the target player.</returns>
        protected virtual bool ShouldShowTo(XPPlayer player, XPPlayer target) => true;
    }
}