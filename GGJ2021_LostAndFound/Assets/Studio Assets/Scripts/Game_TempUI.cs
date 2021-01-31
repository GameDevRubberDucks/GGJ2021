using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Game_TempUI : MonoBehaviour
{
    [Header("Score")]
    public TextMeshProUGUI m_txtScore;

    [Header("Hearts")]
    public TextMeshProUGUI m_txtProgressTowardsNextHeart;
    public Image m_fillProgressTowardsNextHeart;
    public Image[] heartContainers;


    [Header("Traits")]
    public Image m_imgTargetVariation;
    public TextMeshProUGUI m_selectionTrait;
    public Image m_selectionTraitImage;
    public Sprite[] m_availableTraitImageSelections;
    public TextMeshProUGUI[] m_txtTraitProgress;
    public Image[] m_fillTraitProgress;

    [Header("End Screens")]
    public GameObject m_endScreen;
    public GameObject m_victoryMessage;
    public GameObject m_lossMessage;

    public void UpdateScoreUI(int _newScore)
    {
        m_txtScore.text = _newScore.ToString();
    }

    public void UpdateHeartCountUI(int _numHearts, int _progressTowardsNextHeart, int _peopleUntilNextHeart)
    {
        m_txtProgressTowardsNextHeart.text = _progressTowardsNextHeart.ToString() + "\n---\n" + _peopleUntilNextHeart.ToString();
        m_fillProgressTowardsNextHeart.fillAmount = (float)_progressTowardsNextHeart / (float)_peopleUntilNextHeart;

        //fill in the hearts based on the heart count
        for (int i = 0; i < heartContainers.Length; i++)
        {
            if (i < _numHearts)
                //filled color
                heartContainers[i].color = new Color(255.0f / 255.0f, 130.0f / 255.0f, 124.0f / 255.0f);
            else
                //empty color
                heartContainers[i].color = new Color(77.0f / 255.0f, 59.0f / 255.0f, 79.0f / 255.0f);


        }

    }

    public void UpdateTargetVariation(Sprite _targetVariation, Color _color)
    {
        m_imgTargetVariation.sprite = _targetVariation;
        m_imgTargetVariation.color = _color;
    }

    public void UpdateSelectableTrait(Person_Trait _selectTrait)
    {
        m_selectionTrait.text = "SELECTING: " + _selectTrait.ToString();
        m_selectionTraitImage.sprite = m_availableTraitImageSelections[(int)_selectTrait];
    }

    public void UpdateTraitProgress(int[] _traitCounters, int _numToCompleteTrait)
    {
        for (int i = 0; i < _traitCounters.Length; i++)
        {
            m_txtTraitProgress[i].text = _traitCounters[i].ToString() + " / " + _numToCompleteTrait.ToString();
            m_fillTraitProgress[i].fillAmount = (float)_traitCounters[i] / (float)_numToCompleteTrait;
        }
    }

    public void ShowEndScreen(bool _victory)
    {
        m_endScreen.SetActive(true);

        m_victoryMessage.SetActive(_victory);
        m_lossMessage.SetActive(!_victory);
    }
}
