using System.Collections.Generic;
using Bearded.TD.UI;
using Bearded.TD.UI.Components;
using Bearded.TD.Utilities;
using Bearded.TD.Utilities.Input;
using OpenTK;

namespace Bearded.TD.Game.UI.Components
{
    class ActionBar : CompositeComponent
    {
        private readonly IList<ActionBarItem.Content[]> actionPages;
        private readonly IAction unfocusAction;
        private readonly ActionBarItem[] actionBarItems;
        private int currentPage;

        public ActionBar(
            Bounds bounds, int actionBarSize, IList<ActionBarItem.Content[]> actionPages, IAction unfocusAction)
            : base(bounds)
        {
            actionPages.ForEach(page => DebugAssert.Argument.Satisfies(page.Length == actionBarSize));
            this.actionPages = actionPages;
            this.unfocusAction = unfocusAction;
            actionBarItems = new ActionBarItem[actionBarSize];
            createActionBarItems();
            updateActionBarItemContents();
        }

        private void createActionBarItems()
        {
            var size = new Vector2(Bounds.Width, Bounds.Height / actionBarItems.Length);

            for (var i = 0; i < actionBarItems.Length; i++)
            {
                actionBarItems[i] =
                        new ActionBarItem(Bounds.AnchoredBox(Bounds, 0, 0, size, i * size.Y * Vector2.UnitY));
                actionBarItems[i].Focused += onActionBarFocus;
                AddComponent(actionBarItems[i]);
            }
        }

        private void updateActionBarItemContents()
        {
            unfocusAll();
            for (var i = 0; i < actionBarItems.Length; i++)
            {
                actionBarItems[i].SetContent(actionPages[currentPage][i]);
            }
        }

        private void onActionBarFocus(IFocusable focusable)
        {
            unfocusAll(focusable);
        }

        public override void HandleInput(InputContext input)
        {
            if (unfocusAction.Hit)
                unfocusAll();
            base.HandleInput(input);
        }

        private void unfocusAll(IFocusable except = null)
        {
            foreach (var item in actionBarItems)
            {
                if (except != item)
                {
                    item.Unfocus();
                }
            }
        }
    }
}
