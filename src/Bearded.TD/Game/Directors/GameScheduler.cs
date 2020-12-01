using Bearded.TD.Game.Events;
using static Bearded.TD.Game.Directors.ChapterScheduler;
using static Bearded.TD.Utilities.DebugAssert;

namespace Bearded.TD.Game.Directors
{
    sealed class GameScheduler
    {
        // TODO: these should be populated from mod files
        private const int chaptersPerGame = 5;
        private const int wavesPerChapter = 5;

        private readonly GlobalGameEvents gameEvents;
        private readonly ChapterScheduler chapterScheduler;

        private bool gameStarted;
        private int chaptersLeftInGame;

        public GameScheduler(GlobalGameEvents gameEvents, ChapterScheduler chapterScheduler)
        {
            this.gameEvents = gameEvents;
            this.chapterScheduler = chapterScheduler;
            chapterScheduler.ChapterEnded += onChapterEnded;
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
            chapterScheduler.StartChapter(new ChapterRequirements(wavesPerChapter));
        }
    }
}
