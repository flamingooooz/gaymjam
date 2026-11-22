using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("Jano")]
    public float thirst = 100f;
    public float maxThirst = 100f;
    public Image thirstImage;
    public float thirstSpeed = 2f;

    [Header("Sanity")]
    public float sanity = 100f;
    public float maxSanity = 100f;
    public float minValue = 0f;
    public float sanityIncrease = 1f;       // maksimi palautumisnopeus
    public float thirstOkLimit = 60f;       // sanity palautuu
    public float lowSanityLimit = 50f;      // effektit
    public Image sanityImage;

    [Header("Sanityn laskunopeudet (jano alle rajan)")]
    public float thirstLimit2 = 40f;
    public float thirstLimit3 = 25f;
    public float drainMild = 0.2f;            // jano < 60
    public float drainMedium = 1f;          // jano < 40
    public float drainHigh = 2f;            // jano < 25
    public float drainCritical = 4f;       // jano = 0

    [Header("Ajastin")]
    public float time = 300f;
    public float displayDuration = 3f;
    public GameObject winMenu;
    public GameObject deathMenu;
    public GameObject pauseMenu;
    public GameObject dashNote;

    [Header("Sanity-efektit")]
    public AudioSource heartbeatSource;
    public VolumeProfile postProcessingVolume;
    private LensDistortion distortion;
    private ColorAdjustments hue;
    private Vignette vignette;

    [Header("Hallusinaatiot")]
    public AudioSource whisperSource;
    public AudioClip[] whisperClips;
    public GameObject[] hallucinationObjects;
    public float hallucinationMinInterval = 5f;
    public float hallucinationMaxInterval = 15f;
    public float hallucinationDuration = 1.5f;

    [Header("Varoitukset")]
    public GameObject thirstWarning60;
    public GameObject thirstWarning25;
    public GameObject lowSanityWarning;
    public GameObject lowtime;

    private bool warnedThirst60 = false;
    private bool warnedThirst25 = false;
    private bool warnedLowSanity = false;


    private float sanityDecrease;
    private bool hasWarnedLowTime = false;
    private bool hasEnded = false;
    private float hallucinationTimer;

    [Header("muu UI")]
    public GameObject drinkHint;
    public TextMeshProUGUI timer;
    public TextMeshProUGUI waterPercentage;
    public TextMeshProUGUI sanityPercentage;

    public static GameManager instance;
    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        Time.timeScale = 1;
        if (thirstWarning60 != null) thirstWarning60.SetActive(false);
        if (thirstWarning25 != null) thirstWarning25.SetActive(false);
        if (lowSanityWarning != null) lowSanityWarning.SetActive(false);
        if (drinkHint != null) drinkHint.SetActive(false);

        lowtime.SetActive(false);
        ResetHallucinationTimer();

        foreach (var obj in hallucinationObjects)
            if (obj != null) obj.SetActive(false);

        if (postProcessingVolume != null)
        {
            StartCoroutine(ResetHallucinationEffects());
        }
    }

    void Update()
    {
        if (hasEnded) return;

        sanityImage.fillAmount = sanity / maxSanity;
        thirstImage.fillAmount = thirst / maxThirst;
        thirstImage.color = Color.Lerp(Color.blue, Color.white, thirst / maxThirst);

        thirst = Mathf.Max(0f, thirst - Time.deltaTime * thirstSpeed);
        time = Mathf.Max(0f, time - Time.deltaTime);

        if (time <= 0f)
        {
            timerEnded();
            return;
        }

        if (time < 60f && !hasWarnedLowTime)
        {
            hasWarnedLowTime = true;
            timerLow();
        }

        UpdateSanity();
        UpdateSanityEffects();
        CheckWarnings();

        int minutes = Mathf.FloorToInt(time / 60f);
        int seconds = Mathf.FloorToInt(time - (minutes * 60));
        float milliseconds = (time - (seconds + (minutes * 60))) * 1000f;
        string timeStyle = string.Format("{0:0}:{1:00}:{2:000}", minutes, seconds, milliseconds);
        timer.text = "SURVIVE: " + timeStyle;

        waterPercentage.text = Mathf.RoundToInt(thirst) + "%";
        sanityPercentage.text = Mathf.RoundToInt(sanity) + "%";

        if (sanity > lowSanityLimit)
        {
            StartCoroutine(ResetHallucinationEffects());
        }
        if ((thirst <= 0f || sanity <= 0f) && Time.timeScale == 1)
        {
            death();
        }
    }

    void UpdateSanity()
    {
        if (thirst > thirstOkLimit)
        {
      
            float t = Mathf.InverseLerp(thirstOkLimit, maxThirst, thirst);
            float regen = sanityIncrease * Mathf.Lerp(0.25f, 1f, t);
            sanity = Mathf.MoveTowards(sanity, 100f, regen * Time.deltaTime);
        }
        else
        {
            sanityDecrease = GetDrainRate();
            sanity = Mathf.MoveTowards(sanity, minValue, sanityDecrease * Time.deltaTime);
        }
    }

    void CheckWarnings()
    {
        
        if (thirst < thirstOkLimit && !warnedThirst60)
        {
            warnedThirst60 = true;
            ShowWarning(thirstWarning60);
        }
        else if (thirst >= thirstOkLimit)
        {
            warnedThirst60 = false;
        }

     
        if (thirst < thirstLimit3 && !warnedThirst25)
        {
            warnedThirst25 = true;
            ShowWarning(thirstWarning25);
        }
        else if (thirst >= thirstLimit3)
        {
            warnedThirst25 = false;
        }

        if (sanity < lowSanityLimit && !warnedLowSanity)
        {
            warnedLowSanity = true;
            ShowWarning(lowSanityWarning);
        }
        else if (sanity > lowSanityLimit + 5f)
        {
            warnedLowSanity = false; 
        }
    }

    void ShowWarning(GameObject warning)
    {
        if (warning == null) return;
        StartCoroutine(ShowTemporarily(warning, displayDuration));
    }

    IEnumerator ShowTemporarily(GameObject obj, float duration)
    {
        obj.SetActive(true);
        yield return new WaitForSeconds(duration);
        obj.SetActive(false);
    }

    float GetDrainRate()
    {
        if (thirst <= 0f) return drainCritical;
        if (thirst < thirstLimit3) return drainHigh;
        if (thirst < thirstLimit2) return drainMedium;
        return drainMild; 
    }


    float GetSanityIntensity()
    {
        return Mathf.Clamp01(1f - (sanity / lowSanityLimit));
    }

    void UpdateSanityEffects()
    {
        float intensity = GetSanityIntensity();

        // Sydämenlyönnit
        if (heartbeatSource != null)
        {
            if (intensity > 0f)
            {
                if (!heartbeatSource.isPlaying) heartbeatSource.Play();
                heartbeatSource.volume = intensity;
                heartbeatSource.pitch = 1f + intensity * 0.5f;
            }
            else if (heartbeatSource.isPlaying)
            {
                heartbeatSource.Stop();
            }
        }

        // Hallusinaatiot
        if (sanity < lowSanityLimit)
            lowSanity1(intensity);
        else
            ResetHallucinationTimer();
    }



    //Hallusinaatiot
    void lowSanity1(float intensity)
    {
        hallucinationTimer -= Time.deltaTime;
        if (hallucinationTimer > 0f) return;

        TriggerHallucination();

        hallucinationTimer = Random.Range(hallucinationMinInterval, hallucinationMaxInterval)
                             * Mathf.Lerp(1f, 0.4f, intensity);
    }

    void TriggerHallucination()
    {
        if (whisperSource != null && whisperClips.Length > 0)
        {
            whisperSource.PlayOneShot(whisperClips[Random.Range(0, whisperClips.Length)]);
        }

        Audiomanager.PlayMusic(MusicType.Low_Sanity);

        StartCoroutine(HallucinationEffect());

        if (hallucinationObjects.Length > 0)
        {
            GameObject obj = hallucinationObjects[Random.Range(0, hallucinationObjects.Length)];
            if (obj != null) StartCoroutine(ShowHallucination(obj));
        }
    }
    IEnumerator HallucinationEffect()
    {
        float intensity = GetSanityIntensity();

        if (postProcessingVolume.TryGet(out distortion) && postProcessingVolume.TryGet(out hue) && postProcessingVolume.TryGet(out vignette))
        {
            for (int i = 0; i < 5; i++)
            {
                for (float f = 0f; f <= 1; f += Time.deltaTime)
                {
                    if (postProcessingVolume != null)
                    {
                        distortion.intensity.value = Mathf.Lerp(0f, -intensity, f);
                        hue.hueShift.value = Mathf.Lerp(0f, intensity * 20, f);
                        vignette.intensity.value = Mathf.Lerp(0.2f, 0.4f, f);
                        vignette.color.value = Color.Lerp(Color.black, Color.red, f);
                        yield return null;
                    }
                }
                for (float f = 0f; f <= 1; f += Time.deltaTime)
                {
                    if (postProcessingVolume != null)
                    {
                        distortion.intensity.value = Mathf.Lerp(-intensity, 0f, f);
                        hue.hueShift.value = Mathf.Lerp(intensity * 20, 0f, f);
                        vignette.intensity.value = Mathf.Lerp(0.4f, 0.2f, f);
                        vignette.color.value = Color.Lerp(Color.red, Color.black, f);                
                        yield return null;
                    }
                }   
            }
        }
    }
    IEnumerator ShowHallucination(GameObject obj)
    {
        obj.SetActive(true);
        yield return new WaitForSeconds(hallucinationDuration);
        obj.SetActive(false);
    }
    IEnumerator ResetHallucinationEffects()
    {
        for (float f = 0f; f <= 1; f += Time.deltaTime)
        {
            if (postProcessingVolume.TryGet(out distortion) && postProcessingVolume.TryGet(out hue) && postProcessingVolume.TryGet(out vignette))
            {
                distortion.intensity.value = Mathf.Lerp(distortion.intensity.value, 0f, f);  
                hue.hueShift.value = Mathf.Lerp(hue.hueShift.value, 0f, f);
                vignette.intensity.value = Mathf.Lerp(vignette.intensity.value, 0.2f, f);
                vignette.color.value = Color.Lerp(vignette.color.value, Color.black, f);         
                yield return null;
            }
        }
    }

    void ResetHallucinationTimer()
    {
        hallucinationTimer = Random.Range(hallucinationMinInterval, hallucinationMaxInterval);
    }

    // jano

    public void Drink(float amount)
    {
        thirst = Mathf.Min(maxThirst, thirst + amount);
        sanity = Mathf.Min(maxSanity, sanity + amount / 3);
    }

    // ajastin

    void timerEnded()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 0;
        hasEnded = true;
        winMenu.SetActive(true);
    }

    void timerLow()
    {
        lowtime.SetActive(true);
        Invoke(nameof(Hide), displayDuration);
    }

    void Hide()
    {
        lowtime.SetActive(false);
    }
   
    public void pause()
    {
        if (pauseMenu.activeSelf == true)
        {
            pauseMenu.SetActive(false);
        }
        else 
        {
            pauseMenu.SetActive(true);
        }
    }
    public void death()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 0;
        Audiomanager.PlaySound(SoundType.Death_Sound);
        deathMenu.SetActive(true);
        
    }
    public void Respawn()
    {
        Time.timeScale = 1;
        Audiomanager.PlaySound(SoundType.Button);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}