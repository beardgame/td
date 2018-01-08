using System;

namespace Bearded.TD.Utilities
{
    abstract class Either<TLeft, TRight>
    {
        public abstract void Match(Action<TLeft> onLeft, Action<TRight> onRight);

        public static Either<TLeft, TRight> WithLeft(TLeft left) => new Left(left);
        public static Either<TLeft, TRight> WithRight(TRight right) => new Right(right);

        public static implicit operator Either<TLeft, TRight>(TLeft left) => new Left(left);
        public static implicit operator Either<TLeft, TRight>(TRight right) => new Right(right);

        class Left : Either<TLeft, TRight>
        {
            private readonly TLeft left;
            public Left(TLeft left) => this.left = left;

            public override void Match(Action<TLeft> onLeft, Action<TRight> _) => onLeft(left);
        }

        class Right : Either<TLeft, TRight>
        {
            private readonly TRight right;
            public Right(TRight right) => this.right = right;

            public override void Match(Action<TLeft> _, Action<TRight> onRight) => onRight(right);
        }
    }
}
