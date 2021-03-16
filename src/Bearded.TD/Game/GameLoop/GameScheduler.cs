using Bearded.TD.Commands;
using Bearded.TD.Game.Commands.GameLoop;
using Bearded.TD.Game.Simulation;
using static Bearded.TD.Game.GameLoop.ChapterScheduler;
using static Bearded.TD.Utilities.DebugAssert;

namespace Bearded.TD.Game.GameLoop
{
    sealed class GameScheduler
    {
        private readonly GameState game;
        private readonly ICommandDispatcher<GameInstance> commandDispatcher;
        private readonly ChapterScheduler chapterScheduler;
        private readonly GameRequirements gameRequirements;

        private bool gameStarted;
        private int chaptersStarted;

        public GameScheduler(
            GameState game,
            ICommandDispatcher<GameInstance> commandDispatcher,
            ChapterScheduler chapterScheduler,
            GameRequirements gameRequirements)
        {
            this.game = game;
            this.commandDispatcher = commandDispatcher;
            this.chapterScheduler = chapterScheduler;
            this.gameRequirements = gameRequirements;
            chapterScheduler.ChapterEnded += onChapterEnded;
        }

        private void onChapterEnded()
        {
            if (chaptersStarted < gameRequirements.ChaptersPerGame)
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
            State.Satisfies(chaptersStarted < gameRequirements.WavesPerChapter);
            var chapterNumber = ++chaptersStarted;
            chapterScheduler.StartChapter(new ChapterRequirements(chapterNumber, gameRequirements.ChaptersPerGame));
        }

        public sealed record GameRequirements(int ChaptersPerGame, int WavesPerChapter);
    }
}
