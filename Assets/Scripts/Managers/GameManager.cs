using UnityEngine;
using GameJam.Core;

namespace GameJam.Managers
{
    public enum GameState
    {
        MainMenu,
        Playing,
        Paused,
        GameOver,
        Victory,
        Loading
    }

    public class GameManager : Singleton<GameManager>
    {
        [SerializeField] private GameState initialState = GameState.MainMenu;

        private GameState _currentState;
        private GameState _previousState;

        public GameState CurrentState => _currentState;
        public GameState PreviousState => _previousState;
        public bool IsPaused => _currentState == GameState.Paused;
        public bool IsPlaying => _currentState == GameState.Playing;

        public int Score { get; private set; }
        public float PlayTime { get; private set; }

        protected override void Awake()
        {
            base.Awake();
            _currentState = initialState;
        }

        private void Update()
        {
            if (_currentState == GameState.Playing)
            {
                PlayTime += Time.deltaTime;
            }
        }

        public void ChangeState(GameState newState)
        {
            if (_currentState == newState) return;

            _previousState = _currentState;
            _currentState = newState;

            OnStateChanged(_previousState, newState);
        }

        private void OnStateChanged(GameState from, GameState to)
        {
            switch (to)
            {
                case GameState.MainMenu:
                    Time.timeScale = 1f;
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                    break;

                case GameState.Playing:
                    Time.timeScale = 1f;
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                    if (from == GameState.MainMenu || from == GameState.Loading)
                    {
                        EventSystem.Emit(GameEvents.GameStart);
                    }
                    else if (from == GameState.Paused)
                    {
                        EventSystem.Emit(GameEvents.GameResume);
                    }
                    break;

                case GameState.Paused:
                    Time.timeScale = 0f;
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                    EventSystem.Emit(GameEvents.GamePause);
                    break;

                case GameState.GameOver:
                    Time.timeScale = 0f;
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                    EventSystem.Emit(GameEvents.GameOver);
                    break;

                case GameState.Victory:
                    Time.timeScale = 1f;
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                    EventSystem.Emit(GameEvents.LevelComplete);
                    break;
            }
        }

        public void StartGame()
        {
            Score = 0;
            PlayTime = 0f;
            ChangeState(GameState.Playing);
        }

        public void PauseGame()
        {
            if (_currentState == GameState.Playing)
            {
                ChangeState(GameState.Paused);
            }
        }

        public void ResumeGame()
        {
            if (_currentState == GameState.Paused)
            {
                ChangeState(GameState.Playing);
            }
        }

        public void TogglePause()
        {
            if (_currentState == GameState.Playing)
            {
                PauseGame();
            }
            else if (_currentState == GameState.Paused)
            {
                ResumeGame();
            }
        }

        public void GameOver()
        {
            ChangeState(GameState.GameOver);
        }

        public void Victory()
        {
            ChangeState(GameState.Victory);
        }

        public void ReturnToMainMenu()
        {
            ChangeState(GameState.MainMenu);
            SceneLoader.Instance?.LoadScene(0);
        }

        public void AddScore(int points)
        {
            Score += points;
            EventSystem.Emit(GameEvents.ScoreChanged, Score);
        }

        public void SetScore(int score)
        {
            Score = score;
            EventSystem.Emit(GameEvents.ScoreChanged, Score);
        }

        public void RestartLevel()
        {
            Score = 0;
            PlayTime = 0f;
            SceneLoader.Instance?.ReloadCurrentScene();
            ChangeState(GameState.Playing);
        }

        public void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
