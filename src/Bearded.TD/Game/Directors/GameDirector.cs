using Bearded.TD.Game.Events;
using Bearded.Utilities.SpaceTime;
using static Bearded.TD.Game.Directors.ChapterDirector;
using static Bearded.TD.Utilities.DebugAssert;

namespace Bearded.TD.Game.Directors
{
    sealed class GameDirector
    {
        // TODO: these should be populated from mod files
        private const int chaptersPerGame = 5;
        private const int wavesPerChapter = 5;

        private readonly GlobalGameEvents gameEvents;
        private readonly ChapterDirector chapterDirector;

        private bool gameStarted;
        private int chaptersLeftInGame;

        public GameDirector(GlobalGameEvents gameEvents, ChapterDirector chapterDirector)
        {
            this.gameEvents = gameEvents;
            this.chapterDirector = chapterDirector;
            chapterDirector.ChapterEnded += onChapterEnded;
        }

        private void onChapterEnded()
        {
            if (chaptersLeftInGame > 0)
            {
                requestChapter();
            }
            else
            {
                endGame();
            }
        }

        public void StartGame()
        {
            State.Satisfies(!gameStarted);

            chaptersLeftInGame = chaptersPerGame;
        }

        private void endGame()
        {
            State.Satisfies(gameStarted);
            gameStarted = false;
            gameEvents.Send(new GameVictoryTriggered());
        }

        private void requestChapter()
        {
            State.Satisfies(chaptersLeftInGame > 0);
            chaptersLeftInGame--;
            chapterDirector.StartChapter(new ChapterRequirements(wavesPerChapter));
        }

        public void Update(TimeSpan elapsedTime)
        {
            if (!gameStarted) return;
            chapterDirector.Update(elapsedTime);
        }
    }
}
