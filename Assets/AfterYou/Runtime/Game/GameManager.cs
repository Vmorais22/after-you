using AfterYou.Core;
using UnityEngine;

namespace AfterYou.Game
{
    public enum GameState
    {
        Booting,
        Playing,
        Paused,
        Completed
    }

    public sealed class GameManager : GameServiceBehaviour
    {
        public GameState State { get; private set; } = GameState.Booting;

        public override void Initialize(ServiceRegistry services)
        {
            base.Initialize(services);
            State = GameState.Playing;
        }

        public void SetPaused(bool paused)
        {
            State = paused ? GameState.Paused : GameState.Playing;
            Time.timeScale = paused ? 0f : 1f;
        }

        public void CompleteGame()
        {
            State = GameState.Completed;
        }

        public override void Shutdown()
        {
            Time.timeScale = 1f;
        }
    }
}
