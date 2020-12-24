using Bearded.TD.Commands;
using Bearded.TD.Game.Commands.GameLoop;
using static Bearded.TD.Game.GameLoop.ChapterScheduler;
using static Bearded.TD.Utilities.DebugAssert;

namespace Bearded.TD.Game.GameLoop
{
    sealed class GameScheduler
    {
        // TODO: these should be populated from mod files
        private const int chaptersPerGame = 5;
        private const int wavesPerChapter = 5;

        private readonly GameInstance game;
        private readonly ICommandDispatcher<GameInstance> commandDispatcher;
        private readonly ChapterScheduler chapterScheduler;

        private bool gameStarted;
        private int chaptersStarted;

        public GameScheduler(GameInstance game, ICommandDispatcher<GameInstance> commandDispatcher, ChapterScheduler chapterScheduler)
        {
            this.game = game;
            this.commandDispatcher = commandDispatcher;
            this.chapterScheduler = chapterScheduler;
            chapterScheduler.ChapterEnded += onChapterEnded;
        }

        private void onChapterEnded()
        {
            if (chaptersStarted < chaptersPerGame)
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
            gameStarted = true;

            chapterScheduler.OnGameStart();

            requestChapter();
        }

        private void endGame()
        {
            State.Satisfies(gameStarted);
            commandDispatcher.Dispatch(WinGame.Command(game));
        }

        private void requestChapter()
        {
            State.Satisfies(chaptersStarted < chaptersPerGame);
            var chapterNumber = ++chaptersStarted;
            chapterScheduler.StartChapter(new ChapterRequirements(chapterNumber, wavesPerChapter));
        }
    }
}
