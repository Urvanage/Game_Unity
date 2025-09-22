using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public GameObject planetPrefab;
    public Transform planetGroup;
    public List<Planet> planetPool;
    public GameObject effectPrefab;
    public Transform effectGroup;
    public List<ParticleSystem> effectPool;

    [Range(1, 30)]
    public int poolSize;
    public int poolCursor;
    public Planet lastPlanet;

    public int maxLevel;
    public int score;
    public bool isOver;

    public GameObject StartGroup;
    public GameObject EndGroup;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI maxScoreText;
    public TextMeshProUGUI subScoreText;

    public GameObject line;
    public GameObject bottom;

    void Awake(){
        Application.targetFrameRate = 60;

        planetPool = new List<Planet>();
        effectPool = new List<ParticleSystem>();

        for(int i=0; i<poolSize; i++){
            MakePlanet();
        }   

        maxScoreText.text = PlayerPrefs.GetInt("MaxScore", 0).ToString();
    }

    public void GameStart(){

        line.SetActive(true);
        bottom.SetActive(true);
        scoreText.gameObject.SetActive(true);
        maxScoreText.gameObject.SetActive(true);
        StartGroup.SetActive(false);

        Invoke("NextPlanet", 1.0f);
        //NextPlanet();
    }

    Planet MakePlanet(){
        GameObject instantEffectObj = Instantiate(effectPrefab, effectGroup);
        instantEffectObj.name = "Effect " + effectPool.Count;
        ParticleSystem instantEffect = instantEffectObj.GetComponent<ParticleSystem>();
        effectPool.Add(instantEffect);

        GameObject instantPlanetObj = Instantiate(planetPrefab, planetGroup);
        instantPlanetObj.name = "Planet " + planetPool.Count;
        Planet instantPlanet = instantPlanetObj.GetComponent<Planet>();
        instantPlanet.manager = this;
        instantPlanet.effect = instantEffect;
        planetPool.Add(instantPlanet);
    
        return instantPlanet;
    }

    Planet GetPlanet(){
        for(int i=0; i<planetPool.Count; i++){
            poolCursor = (poolCursor+1)%planetPool.Count;
            if(planetPool[poolCursor].gameObject.activeSelf == false){
                return planetPool[poolCursor];
            }
        }
        return MakePlanet();
    }

    void NextPlanet(){
        if(isOver)
            return;

        lastPlanet = GetPlanet();
        lastPlanet.level = Random.Range(0,maxLevel);
        if(lastPlanet.level > 4) lastPlanet.level = 4;
        
        lastPlanet.gameObject.SetActive(true);

        StartCoroutine(WaitNext());
    }

    IEnumerator WaitNext(){
        while(lastPlanet!=null){
            yield return null;
        }
        yield return new WaitForSeconds(1.5f);

        NextPlanet();

    }

    public void TouchDown(){
        if (lastPlanet==null)
            return;

        lastPlanet.Drag();
    }

    public void TouchUp(){
        if (lastPlanet==null)
            return;

        lastPlanet.Drop();
        lastPlanet = null;
    }

    public void GameOver(){
        if(isOver)
            return;

        isOver = true;

        StartCoroutine(GameOverRoutine());
    }

    IEnumerator GameOverRoutine(){
        Planet[] planets = GameObject.FindObjectsByType<Planet>(FindObjectsSortMode.None);

        foreach(Planet planet in planets){
            planet.rigid.simulated = false;
        }

        foreach(Planet planet in planets){
            planet.Hide(Vector3.up*100);
            yield return new WaitForSeconds(0.1f);
        }

        int maxScore = Mathf.Max(score, PlayerPrefs.GetInt("MaxScore", 0));
        PlayerPrefs.SetInt("MaxScore", maxScore);

        subScoreText.text = "Score: " + score.ToString();
        EndGroup.SetActive(true);
    }

    public void Reset(){
        StartCoroutine(ResetRoutine());
    }

    IEnumerator ResetRoutine(){
        yield return new WaitForSeconds(1.0f);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void Update(){
        if(Input.GetKeyDown(KeyCode.Space)){
            Application.Quit();
        }
    }

    void LateUpdate(){
        scoreText.text = score.ToString();
        if(score > int.Parse(maxScoreText.text)){
            maxScoreText.text = score.ToString();
        }
    }
}