using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections;
using GameJam.Core;

namespace GameJam.Managers
{
    public class SceneLoader : Singleton<SceneLoader>
    {
        [SerializeField] private CanvasGroup loadingScreen;
        [SerializeField] private float minLoadTime = 0.5f;

        public event Action<float> OnLoadProgress;
        public event Action<string> OnSceneLoaded;
        public event Action OnLoadStart;

        public bool IsLoading { get; private set; }
        public string CurrentScene => SceneManager.GetActiveScene().name;

        public void LoadScene(string sceneName, bool showLoadingScreen = true)
        {
            if (IsLoading) return;
            StartCoroutine(LoadSceneAsync(sceneName, showLoadingScreen));
        }

        public void LoadScene(int sceneIndex, bool showLoadingScreen = true)
        {
            if (IsLoading) return;
            string sceneName = SceneManager.GetSceneByBuildIndex(sceneIndex).name;
            StartCoroutine(LoadSceneAsync(sceneName, showLoadingScreen));
        }

        public void ReloadCurrentScene()
        {
            LoadScene(CurrentScene);
        }

        private IEnumerator LoadSceneAsync(string sceneName, bool showLoadingScreen)
        {
            IsLoading = true;
            OnLoadStart?.Invoke();

            if (showLoadingScreen && loadingScreen != null)
            {
                yield return FadeLoadingScreen(true);
            }

            float startTime = Time.realtimeSinceStartup;

            AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
            operation.allowSceneActivation = false;

            while (operation.progress < 0.9f)
            {
                OnLoadProgress?.Invoke(operation.progress);
                yield return null;
            }

            float elapsedTime = Time.realtimeSinceStartup - startTime;
            if (elapsedTime < minLoadTime)
            {
                yield return new WaitForSecondsRealtime(minLoadTime - elapsedTime);
            }

            OnLoadProgress?.Invoke(1f);
            operation.allowSceneActivation = true;

            while (!operation.isDone)
            {
                yield return null;
            }

            if (showLoadingScreen && loadingScreen != null)
            {
                yield return FadeLoadingScreen(false);
            }

            IsLoading = false;
            OnSceneLoaded?.Invoke(sceneName);
        }

        private IEnumerator FadeLoadingScreen(bool fadeIn)
        {
            float startAlpha = fadeIn ? 0f : 1f;
            float endAlpha = fadeIn ? 1f : 0f;
            float duration = 0.3f;

            loadingScreen.gameObject.SetActive(true);
            loadingScreen.alpha = startAlpha;

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                loadingScreen.alpha = Mathf.Lerp(startAlpha, endAlpha, elapsed / duration);
                yield return null;
            }

            loadingScreen.alpha = endAlpha;

            if (!fadeIn)
            {
                loadingScreen.gameObject.SetActive(false);
            }
        }

        public void LoadAdditiveScene(string sceneName, Action onComplete = null)
        {
            StartCoroutine(LoadAdditiveAsync(sceneName, onComplete));
        }

        private IEnumerator LoadAdditiveAsync(string sceneName, Action onComplete)
        {
            AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

            while (!operation.isDone)
            {
                yield return null;
            }

            onComplete?.Invoke();
        }

        public void UnloadScene(string sceneName, Action onComplete = null)
        {
            StartCoroutine(UnloadAsync(sceneName, onComplete));
        }

        private IEnumerator UnloadAsync(string sceneName, Action onComplete)
        {
            AsyncOperation operation = SceneManager.UnloadSceneAsync(sceneName);

            while (!operation.isDone)
            {
                yield return null;
            }

            onComplete?.Invoke();
        }
    }
}
