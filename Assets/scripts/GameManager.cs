using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public float thirst = 100f;
    public float sanity = 100f;
    public float sanityDecrease = 1f;
    public float sanityIncrease = 1f;
    public float minValue = 0f;
    public float time = 300f;
    public GameObject lowtime;
    public float displayDuration = 3f;
    public GameObject winMenu;
    public Scrollbar scrollbar;

    private bool hasWarnedLowTime = false;

    void Start()
    {
        lowtime.SetActive(false);
    }

    public void Update()
    {

        scrollbar.value = thirst / 100f;

        thirst = Mathf.Max(0f, thirst - Time.deltaTime);
        time -= Time.deltaTime;


        if (time <= 0.0f)
        {
            timerEnded();
        }

        if (time < 120f && !hasWarnedLowTime)
        {
            hasWarnedLowTime = true;
            timerLow();
        }

     
        if (thirst <= 0f)
        {
            SetSanityDrainRate(15f);
        }
        else if (thirst < 25f)
        {
            SetSanityDrainRate(6f);
        }
        else if (thirst < 40f)
        {
            SetSanityDrainRate(4f);
        }
        else if (thirst < 60f)
        {
            SetSanityDrainRate(2f);
        }
        else
        {
            SetSanityDrainRate(0f); 
        }


        if (sanityDecrease > 0f && sanity > minValue)
        {
            sanity = Mathf.MoveTowards(sanity, minValue, sanityDecrease * Time.deltaTime);
        }

        // Sanity events
        if (sanity < 50f)
        {
            lowSanity1();
        }
    }

    // Thirst actions
    void SetSanityDrainRate(float rate)
    {
        sanityDecrease = rate;
    }

    // Sanity actions
    void lowSanity1()
    {
        // Hallucinate
    }

    // Timer actions
    void timerEnded()
    {
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