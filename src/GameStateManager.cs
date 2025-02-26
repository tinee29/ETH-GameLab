namespace GameLab
{
    public enum GameState
    {
        None,
        Menu,
        Game,
        Pause,
        GameOver,
        Dieing,
        Controls,
        Options,
        Transitioning,

        PauseOptions,
    }

    public class GameStateManager(GameState gameState)
    {
        public GameState CurrentState = gameState;

        public void ChangeState(GameState newState)
        {
            CurrentState = newState;
        }

    }

}