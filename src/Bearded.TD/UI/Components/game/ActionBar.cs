﻿using System.Collections.Generic;
using Bearded.TD.Utilities;
using Bearded.TD.Utilities.Collections;
using Bearded.TD.Utilities.Input;
using OpenTK;
using static Bearded.TD.UI.BoundsConstants;

namespace Bearded.TD.UI.Components
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
            if (actionPages.Count > 1)
                createPageButtons();
            updateActionBarItemContents();
        }

        private void createActionBarItems()
        {
            var size = new Vector2(Bounds.Width, Bounds.Height / actionBarItems.Length);

            for (var i = 0; i < actionBarItems.Length; i++)
            {
                actionBarItems[i] =
                        new ActionBarItem(Bounds.AnchoredBox(Bounds, TopLeft, size, Offset(0, i * size.Y)));
                actionBarItems[i].Focused += onActionBarFocus;
                AddComponent(actionBarItems[i]);
            }
        }

        private void createPageButtons()
        {
            var size = new Vector2(Bounds.Width, Bounds.Height / actionBarItems.Length);

            AddComponent(new Button(
                Bounds.AnchoredBox(Bounds, TopLeft, size, OffsetFrom(TopLeft, 0, -size.Y)),
                () => updatePage(currentPage - 1), "Previous"));
            AddComponent(new Button(
                Bounds.AnchoredBox(Bounds, BottomLeft, size, OffsetFrom(BottomLeft, 0, -size.Y)),
                () => updatePage(currentPage + 1), "Next"));
        }

        private void updateActionBarItemContents()
        {
            unfocusAll();
            for (var i = 0; i < actionBarItems.Length; i++)
            {
                actionBarItems[i].SetContent(actionPages[currentPage][i]);
            }
        }

        public override void HandleInput(InputContext input)
        {
            if (unfocusAction.Hit)
                unfocusAll();
            base.HandleInput(input);
            input.CaptureMouseInBounds(Bounds);
        }

        private void onActionBarFocus(IFocusable focusable)
        {
            unfocusAll(focusable);
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

        private void updatePage(int newPageNo)
        {
            if (newPageNo == currentPage) return;

            currentPage = (newPageNo + actionPages.Count) % actionPages.Count;
            updateActionBarItemContents();
        }
    }
}
