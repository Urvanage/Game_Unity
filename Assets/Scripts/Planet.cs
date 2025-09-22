using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class Planet : MonoBehaviour
{

    public GameManager manager;
    public ParticleSystem effect;
    public bool isDrag;
    public bool isMerge;
    public Rigidbody2D rigid;
    public int level;
    Animator anim;
    CircleCollider2D circle;
    SpriteRenderer sprite;

    float deadTime;

    void Awake(){
        rigid = GetComponent<Rigidbody2D>();
        circle = GetComponent<CircleCollider2D>();
        anim = GetComponent<Animator>();
        sprite = GetComponent<SpriteRenderer>();
    }

    void OnEnable(){
        if (anim != null)
            anim.SetInteger("Level", level);
    }

    void OnDisable(){
        level = 0;
        isDrag = false;
        isMerge = false;

        transform.localPosition = new Vector3(0, 8, 0);
        transform.localRotation = Quaternion.identity;
        transform.localScale = Vector3.zero;

        rigid.simulated = false;
        rigid.linearVelocity = Vector2.zero;
        rigid.angularVelocity = 0;

        circle.enabled = true;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start(){}

    void Update(){
        if(isDrag){
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            float leftBoarder = -3.8f + transform.localScale.x / 2f;
            float rightBoarder = 3.8f - transform.localScale.x / 2f;

            if (mousePos.x < leftBoarder)
            {
                mousePos.x = leftBoarder;
            }
            else if (mousePos.x > rightBoarder)
            {
                mousePos.x = rightBoarder;
            }

            mousePos.z = 0;
            mousePos.y = 8;
            transform.position = Vector3.Lerp(transform.position, mousePos, 0.5f); // Lerp 제거하고 직접 위치 설정
        }

        
    }

    public void Drag(){
        isDrag = true;
        rigid.simulated = false; // 드래그 중에는 물리 시뮬레이션 비활성화
    }

    public void Drop(){
        // 동적 바디로 설정
        isDrag = false;
        rigid.simulated = true;
    }

    void OnCollisionStay2D(Collision2D collision){
        if(collision.gameObject.tag == "Planet"){
            Planet other = collision.gameObject.GetComponent<Planet>();
            
            if(other != null && level == other.level && !isMerge && !other.isMerge && level<8){
                float meX = transform.position.x;
                float meY = transform.position.y;
                float otherX = other.transform.position.x;
                float otherY = other.transform.position.y;

                if(meY < otherY || (meY == otherY && meX > otherX)){
                    // 두 행성의 중간 위치 계산
                    Vector3 middlePosition = (transform.position + other.transform.position) / 2f;
                    
                    // 현재 행성을 중간 위치로 이동하고 업그레이드
                    transform.position = middlePosition;
                    other.Hide(middlePosition);
                    LevelUp();
                }
            }
        }
    }

    public void Hide(Vector3 targetPos){
        isMerge = true;
        if (rigid != null)
            rigid.simulated = false;
        if (circle != null)
            circle.enabled = false;

        if(targetPos == Vector3.up*100){
            EffectPlay();
        }

        StartCoroutine(HideRoutine(targetPos));
    }

    IEnumerator HideRoutine(Vector3 targetPos){
        int frameCount = 0;

        while(frameCount < 20){
            frameCount++;
            if(targetPos != Vector3.up*100){
                transform.position = Vector3.Lerp(transform.position, targetPos, 0.5f);
            }
            else if (targetPos == Vector3.up*100){
                transform.localScale = Vector3.Lerp(transform.localScale, Vector3.zero, 0.2f);
            }   
            yield return null;
        }

        // 게임 오버가 아닐 때만 점수 추가 (합쳐질 때만 점수 획득)
        if (manager != null && targetPos != Vector3.up*100)
            manager.score += (int) Mathf.Pow(2, level);

        isMerge = false;
        gameObject.SetActive(false);
    }

    void LevelUp(){
        isMerge = true;
        if (rigid != null) {
            rigid.linearVelocity = Vector2.zero;
            rigid.angularVelocity = 0;
        }

        StartCoroutine(LevelUpRoutine());
    }

    IEnumerator LevelUpRoutine(){
        // 약간의 대기 시간 후 레벨업 애니메이션 시작
        yield return new WaitForSeconds(0.1f);

        if (anim != null)
            anim.SetInteger("Level", level+1);
        EffectPlay();

        // 레벨업 애니메이션 시간
        yield return new WaitForSeconds(0.3f);
        level++;

        if (manager != null)
            manager.maxLevel = Mathf.Max(manager.maxLevel, level);

        isMerge = false;
    }

    void OnTriggerStay2D(Collider2D collision){
        if (collision.tag == "Finish"){
            deadTime += Time.deltaTime;

            if(deadTime > 2){
                if (sprite != null)
                    sprite.color = new Color(0.9f,0.2f,0.2f);
            }
            if(deadTime > 4){
                if (manager != null)
                    manager.GameOver();
            }
        }
    }

    void OnTriggerExit2D(Collider2D collision){
        if(collision.tag == "Finish"){
            deadTime = 0;
            if (sprite != null)
                sprite.color = Color.white;
        }
    }

    void EffectPlay(){
        if (effect != null) {
            effect.transform.position = transform.position;
            effect.transform.localScale = transform.localScale*4;
            effect.Play();
        }
    }
}
