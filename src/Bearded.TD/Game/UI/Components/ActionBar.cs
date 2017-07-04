using Bearded.TD.UI;
using Bearded.TD.UI.Components;
using OpenTK;

namespace Bearded.TD.Game.UI.Components
{
    class ActionBar : CompositeComponent
    {
        private readonly ActionBarItem[] actionBarItems;

        public ActionBar(Bounds bounds) : base(bounds)
        {
            actionBarItems = new ActionBarItem[10];
            createActionBarItems();
        }

        private void createActionBarItems()
        {
            var size = new Vector2(Bounds.Width, Bounds.Height / actionBarItems.Length);

            for (var i = 0; i < actionBarItems.Length; i++)
            {
                actionBarItems[i] =
                        new ActionBarItem(Bounds.AnchoredBox(Bounds, 0, 0, size, i * size.Y * Vector2.UnitY));
                var i1 = i;
                actionBarItems[i].SetContent(new ActionBarItem.Content(() => System.Console.WriteLine(i1), $"item {i}"));
                actionBarItems[i].Focused += onActionBarFocus;
                AddComponent(actionBarItems[i]);
            }
        }

        private void onActionBarFocus(IFocusable focusable)
        {
            foreach (var item in actionBarItems)
            {
                if (focusable != item)
                {
                    item.Unfocus();
                }
            }
        }
    }
}
