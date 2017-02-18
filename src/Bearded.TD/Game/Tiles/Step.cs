namespace Bearded.TD.Game.Tiles
{
    struct Step
    {
        public readonly sbyte X;
        public readonly sbyte Y;

        public Step(sbyte x, sbyte y)
        {
            X = x;
            Y = y;
        }

        public Step(Direction direction)
        {
            this = direction.Step();
        }
    }
}
