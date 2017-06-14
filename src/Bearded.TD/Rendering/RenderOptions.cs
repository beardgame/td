namespace Bearded.TD.Rendering
{
    struct RenderOptions
    {
        public bool RenderDeferred { get; }
        
        public RenderOptions(bool renderDeferred)
        {
            RenderDeferred = renderDeferred;
        }
        
        public static RenderOptions Default => new RenderOptions();
        public static RenderOptions Game => new RenderOptions(true);
     }
}
