namespace XPSystem.Config
{
    using Serialization;
    using YamlDotNet.Core;
    using YamlDotNet.Core.Events;
    using YamlDotNet.Serialization;
    using YamlDotNet.Serialization.ObjectGraphVisitors;

    /// <summary>
    /// Source: https://dotnetfiddle.net/8M6iIE.
    /// </summary>
    public class CommentsObjectGraphVisitor : ChainedObjectGraphVisitor
    {
        public CommentsObjectGraphVisitor(IObjectGraphVisitor<IEmitter> nextVisitor)
            : base(nextVisitor)
        {
        }

        /// <inheritdoc/>
        public override bool EnterMapping(IPropertyDescriptor key, IObjectDescriptor value, IEmitter context)
        {
            if (value is CommentsObjectDescriptor commentsDescriptor && commentsDescriptor.Comment is not null)
            {
                foreach (string subComment in commentsDescriptor.Comment.Split('\n'))
                {
                    context.Emit(new Comment(subComment, false));
                }
            }

            return base.EnterMapping(key, value, context);
        }
    }
}