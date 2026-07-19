using System.Collections;
using UnityEngine;

public class BackgroundMusic : MonoBehaviour
{
    public static BackgroundMusic Instance { get; private set; }

    [Header("Настройки музыки")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioClip backgroundTrack;
    [Range(0f, 1f)][SerializeField] private float defaultVolume = 0.5f;

    private Coroutine fadeCoroutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        if (musicSource != null && backgroundTrack != null)
        {
            musicSource.clip = backgroundTrack;
            musicSource.loop = true;
            musicSource.volume = defaultVolume;
            musicSource.Play();
        }
    }

    // Внешний метод для запуска плавного изменения громкости
    public void SetVolumeSmooth(float targetVolume, float duration = 1.0f)
    {
        if (musicSource == null) return;

        // Если уже идет плавный переход, останавливаем его, чтобы начать новый
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }

        // Запускаем плавное изменение
        fadeCoroutine = StartCoroutine(FadeVolume(targetVolume, duration));
    }

    // Сопрограмма (Coroutine) для постепенного изменения громкости
    private IEnumerator FadeVolume(float targetVolume, float duration)
    {
        float startVolume = musicSource.volume;
        float time = 0;

        while (time < duration)
        {
            time += Time.deltaTime;
            // Плавно интерполируем громкость от текущей к целевой
            musicSource.volume = Mathf.Lerp(startVolume, targetVolume, time / duration);
            yield return null; // Ждем один кадр
        }

        musicSource.volume = targetVolume; // Фиксируем финальную громкость
    }

    // Вспомогательный метод, чтобы быстро узнать стандартную громкость
    public float GetDefaultVolume()
    {
        return defaultVolume;
    }
}
