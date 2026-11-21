using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [Header("Jano")]
    public float thirst = 100f;
    public float maxThirst = 100f;
    public Image thirstImage;

    [Header("Sanity")]
    public float sanity = 100f;
    public float minValue = 0f;
    public float sanityIncrease = 1f;       // maksimi palautumisnopeus
    public float thirstOkLimit = 60f;       // sanity palautuu
    public float lowSanityLimit = 50f;      // effektit

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

    [Header("Sanity-efektit")]
    public AudioSource heartbeatSource;    
    public Transform cameraTransform;
    public float maxShake = 0.03f;

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
    private Vector3 camStartLocalPos;

    void Start()
    {
        if (thirstWarning60 != null) thirstWarning60.SetActive(false);
        if (thirstWarning25 != null) thirstWarning25.SetActive(false);
        if (lowSanityWarning != null) lowSanityWarning.SetActive(false);

        lowtime.SetActive(false);
        ResetHallucinationTimer();

        if (cameraTransform != null)
            camStartLocalPos = cameraTransform.localPosition;

        foreach (var obj in hallucinationObjects)
            if (obj != null) obj.SetActive(false);
    }

    void Update()
    {
        if (hasEnded) return;

        thirstImage.fillAmount = thirst / maxThirst;
        thirstImage.color = Color.Lerp(Color.blue, Color.white, thirst / maxThirst);

        thirst = Mathf.Max(0f, thirst - Time.deltaTime);
        time = Mathf.Max(0f, time - Time.deltaTime);

        if (time <= 0f)
        {
            timerEnded();
            return;
        }

        if (time < 120f && !hasWarnedLowTime)
        {
            hasWarnedLowTime = true;
            timerLow();
        }

        UpdateSanity();
        UpdateSanityEffects();
        CheckWarnings();

    }

    void LateUpdate()
    {
   
        if (cameraTransform == null || hasEnded) return;

        float intensity = GetSanityIntensity();
        if (intensity > 0f)
            cameraTransform.localPosition = camStartLocalPos + Random.insideUnitSphere * maxShake * intensity;
        else
            cameraTransform.localPosition = camStartLocalPos;
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
        return Mathf.Clamp01(1f - sanity / lowSanityLimit);
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

        if (hallucinationObjects.Length > 0)
        {
            GameObject obj = hallucinationObjects[Random.Range(0, hallucinationObjects.Length)];
            if (obj != null) StartCoroutine(ShowHallucination(obj));
        }
    }

    IEnumerator ShowHallucination(GameObject obj)
    {
        obj.SetActive(true);
        yield return new WaitForSeconds(hallucinationDuration);
        obj.SetActive(false);
    }

    void ResetHallucinationTimer()
    {
        hallucinationTimer = Random.Range(hallucinationMinInterval, hallucinationMaxInterval);
    }

    // jano

    public void Drink(float amount)
    {
        thirst = Mathf.Min(maxThirst, thirst + amount);
    }

    // ajastin

    void timerEnded()
    {
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
}