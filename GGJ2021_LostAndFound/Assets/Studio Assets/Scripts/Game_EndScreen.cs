using UnityEngine;
using TMPro;

public class Game_EndScreen : MonoBehaviour
{
    public GameObject m_winMsg;
    public GameObject m_lossMsg;
    public TextMeshProUGUI m_scoreText;


    private void Start()
    {
        int score = PlayerPrefs.GetInt("FinalScore");
        bool victory = PlayerPrefs.GetInt("Victory") == 1;

        m_winMsg.SetActive(victory);
        m_lossMsg.SetActive(!victory);

        m_scoreText.text = "score: " + score.ToString();
    }
}
