namespace XPSystem.API
{
    using CommandSystem;
    using Hints;
    using RemoteAdmin;

    /// <summary>
    /// <see cref="ReferenceHub"/> wrapper.
    /// </summary>
    public class XPPlayer
    {
        public readonly ReferenceHub Hub;

        /// <summary>
        /// Gets the player's user ID.
        /// </summary>
        public string UserId => Hub.authManager.UserId;

        /// <summary>
        /// Gets the player's nickname.
        /// </summary>
        public string Nickname => Hub.nicknameSync.Network_myNickSync;

        /// <summary>
        /// Gets whether or not the player is connected to the server.
        /// </summary>
        public bool IsConnected => Hub.gameObject != null;

        /// <summary>
        /// Gets whether or not the player has do not track enabled.
        /// </summary>
        public bool DNT => Hub.authManager.DoNotTrack;

        public void ShowHint(string message, float duration = 3f)
        {
            Hub.hints.Show(new TextHint(message, new HintParameter[]
            {
                new StringHintParameter(message)
            }, durationScalar: duration));
        }

        public void ShowBroadcast(
            string message,
            ushort duration = 5,
            Broadcast.BroadcastFlags type = Broadcast.BroadcastFlags.Normal,
            bool clearPrevious = false)
        {
            if (clearPrevious)
                Broadcast.Singleton.TargetClearElements(Hub.connectionToClient);
            Broadcast.Singleton.TargetAddElement(Hub.connectionToClient, message, duration, type);
        }

        public void SendConsoleMessage(string message, string color)
        {
            Hub.gameConsoleTransmission.SendToClient(message, color);
        }

        /// <summary>
        /// Checks if the player has a permission.
        /// </summary>
        /// <param name="permission">The permission to check for.</param>
        /// <returns>Whether or not the player has the permission.</returns>
        public bool CheckPermission(string permission)
        {
            return LoaderSpecific.CheckPermission(Hub, permission);
        }

        public XPPlayer(ReferenceHub referenceHub)
        {
            Hub = referenceHub;
        }

        public static bool TryGet(ICommandSender sender, out XPPlayer player)
        {
            if (sender is not PlayerCommandSender playerSender)
            {
                player = null;
                return false;
            }

            player = new XPPlayer(playerSender.ReferenceHub);
            return true;
        }

        /// <summary>
        /// Attempts to get a player of a <see cref="ICommandSender"/> and checks if they have a permission.
        /// </summary>
        /// <param name="sender">The sender to get the player from.</param>
        /// <param name="permission">The permission to check for.</param>
        /// <param name="player">The player if found, otherwise null.</param>
        /// <returns>Whether or not the player was found and has the permission.</returns>
        public static bool TryGetAndCheckPermission(ICommandSender sender, string permission, out XPPlayer player)
        {
            return TryGet(sender, out player) && player.CheckPermission(permission);
        }

        /// <summary>
        /// Just <see cref="LoaderSpecific.GetHub"/> with useless extra return.
        /// </summary>
        public static bool TryGet(string data, out XPPlayer player)
        {
            player = null;

            var hub = LoaderSpecific.GetHub(data);
            if (hub == null)
                return false;

            player = new XPPlayer(hub);
            return true;
        }
    }
}