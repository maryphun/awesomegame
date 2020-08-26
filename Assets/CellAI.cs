using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Core.Easing;
using DG.Tweening.Core.Enums;
using DG.Tweening.Plugins.Core;
using DG.Tweening.Plugins.Options;

public class CellAI : MonoBehaviour
{
    [SerializeField] private float moveInterval = 2f;
    [SerializeField] private float jumpForceMin = 0.03f;
    [SerializeField] private float jumpForceMax = 0.05f;
    [SerializeField] private float gravity = 0.18f;
    [SerializeField] private float shakeMagnitude = 0.01f;
    [SerializeField] private float hungerIncreaseRate = 0.05f;
    [SerializeField, Range(0.01f, 10.0f)] private float moveSpeed = 2f;

    private float height;   // the higher the height, the smaller the shadow
    private Transform cell;
    private Transform shadow;
    private SpriteRenderer shadowRenderer;
    private SpriteRenderer cellRenderer;
    private Animator animator;
    private Vector2 originalCellScale;
    private Vector2 originalShadowScale;
    private Color originalColor;
    private Vector2 velocity;
    private bool isJump;
    private Vector3 shake;
    public AI.Status status;
    private Vector2 moveDirection;
    private Transform targetFood;
    private float bounce;
    private float bounceCnt;
    private bool bounceFlag;

    [Header("Reference")]
    public GameObject spirit;

    [Header("DebugValue")]
    [SerializeField, Range(0.0f, 1.0f)] private float hunger;

    private float searchForFoodHunger = 0.5f;

    // Start is called before the first frame update
    void Awake()
    {
        shadow = transform.GetChild(0);
        cell = transform.GetChild(1);
        originalColor = cell.GetComponent<SpriteRenderer>().color;
        cellRenderer = cell.GetComponent<SpriteRenderer>();
        originalShadowScale = shadow.localScale;
        originalCellScale = cell.localScale;
        shadowRenderer = shadow.GetComponent<SpriteRenderer>();
        animator = GetComponentInChildren<Animator>();
        isJump = false;
        bounceFlag = true;
        hunger = 0.0f;
        status = AI.Status.wander;
    }

    // Update is called once per frame
    void Update()
    {
        //jumping
        if (isJump)
        {
            cell.position = new Vector2(cell.position.x, cell.position.y + velocity.y);
            height += velocity.y;
            velocity.y = Mathf.Clamp(velocity.y - gravity * Time.deltaTime, -jumpForceMax * 1.5f, jumpForceMax);

            UpdateShadow();
            Move();

            if (cell.position.y <= transform.position.y)
            {
                cell.position = transform.position;
                height = 0.0f;
                velocity.y = 0f;
                isJump = false;
                ResetShadow();

                StartCoroutine(NextAction(moveInterval));
                StartCoroutine(JumpAfterEffect(moveInterval / 20f));
            }
        }

        // hunger
        hunger = Mathf.Clamp(hunger + (hungerIncreaseRate * Time.deltaTime), 0.0f, 2.0f);

        if (hunger >= 1.0f && status != AI.Status.dead)
        {
            cell.transform.position -= shake;
            shake.x = Random.Range(-shakeMagnitude, shakeMagnitude);
            shake.y = Random.Range(-shakeMagnitude, shakeMagnitude);
            cell.transform.position += shake;
            if (StarveToDeath())
            {
                animator.SetTrigger("Death");
                status = AI.Status.dead;
                spirit = Instantiate(spirit, transform);
                if (velocity.y > 0.0f) velocity.y = 0f;
            }
            else
            {
                Debug.Log("survived!");
            }
        }
        else if (status == AI.Status.dead)
        {
            DrawSpirit();
        }

        //Rescale
        ResizeCell();

        //animator
        animator.SetFloat("Hunger", hunger);

        //update sorting
        UpdateSorting(cellRenderer);
    }

    private IEnumerator NextAction(float waitTime)
    {
        float actualWaitTime = waitTime;
        if (hunger > searchForFoodHunger)
        {
            actualWaitTime = Mathf.Clamp(waitTime - (waitTime * hunger), 0.15f, waitTime);
        }   

        yield return new WaitForSeconds(actualWaitTime);

        Actions();
    }

    private IEnumerator JumpAfterEffect(float waitTime)
    {
        cell.transform.DOScale(originalCellScale, waitTime);
        bounceFlag = false;

        yield return new WaitForSeconds(waitTime);

        bounceCnt = 0.0f;
        bounceFlag = true;
    }

    private void Actions()
    {
        if (status == AI.Status.dead)   //it is dead already. No more action
        {
            return;
        }

        if (SearchForPath())    //return true if path search success
        {
            Jump();
        }
    }

    private void Jump()
    {
        velocity.y = Random.Range(jumpForceMin, jumpForceMax);
        isJump = true;
    }

    private void Move()
    {
        Vector2 newPos = transform.position;
        newPos += moveDirection * moveSpeed * Time.deltaTime;

        SetFlip(moveDirection.x);

        if (CurrentScene.Instance().GetCollider().ClosestPoint(newPos) == newPos)
        {
            transform.DOMove(newPos, Time.deltaTime);
        }
        else
        {
            //reverse move
            moveDirection = -moveDirection;
        }
    }

    private bool SearchForPath()
    {
        bool rtn = false;
        status = AI.Status.wander;

        if (hunger > searchForFoodHunger)
        {
            FindFood();
        }

        switch (status)
        {
            case AI.Status.eat:
                SetFlip(((targetFood.position - transform.position).normalized).x);
                animator.SetTrigger("Eat");
                break;

            case AI.Status.chase:
                moveDirection = (targetFood.position - transform.position).normalized;
                rtn = true;
                break;

            case AI.Status.wander:
                moveDirection = Random.insideUnitCircle;
                rtn = true;
                break;

            default:
                break;
        }
        
        return rtn;
    }

    private void FindFood()
    {
        if (CurrentScene.Instance().GetRegisteredFoodNumber() > 0)
        {
            targetFood = CurrentScene.Instance().GetNearestFood(transform.position);
            Vector2 targetPos = targetFood.position;
            if (cell.GetComponent<Collider2D>().ClosestPoint(targetPos) == targetPos)
            {
                status = AI.Status.eat;
            }
            else
            {
                status = AI.Status.chase;
            }
        }
        else
        {
            status = AI.Status.wander;
        }
    }

    private void UpdateShadow()
    {
        // update scale
        shadow.localScale = new Vector3(Mathf.Clamp(originalShadowScale.x - height / 2.5f, 0.0f, originalShadowScale.x), originalShadowScale.y, shadow.localScale.z);

        // update shadow transparent
        Color tmp = shadowRenderer.color;
        tmp.a = 1f - height / 2f;
        shadowRenderer.color = tmp;
    }

    private void ResetShadow()
    {
        //reset scale
        shadow.localScale = originalShadowScale;

        //reset shadow transparent
        Color tmp = shadowRenderer.color;
        tmp.a = 1f;
        shadowRenderer.color = tmp;
    }

    private bool StarveToDeath()
    {
        return (Random.Range(1.0f, 2.0f) >= hunger);
    }

    public void Eat()
    {
        if (targetFood != null)
        {
            targetFood.GetComponent<Food>().Consume();
            hunger = 0.0f;
        }

        Actions();
    }

    private void SetFlip(float x)
    {
        if (x > 0 && cellRenderer.flipX == false)
        {
            cellRenderer.flipX = true;
            return;
        }
        
        if (x < 0 && cellRenderer.flipX == true)
        {
            cellRenderer.flipX = false;
            return;
        }
    }

    private void ResizeCell()
    {
        if (status == AI.Status.dead) return;

        if (isJump)
        {
            Vector2 newScale = new Vector2(
                /*x*/ Mathf.Lerp(originalCellScale.x, originalCellScale.x / 2f, Mathf.Abs(velocity.y) * 15f),
                /*y*/ Mathf.Lerp(originalCellScale.y, originalCellScale.y * 2f, Mathf.Abs(velocity.y) * 10f));
            cell.transform.localScale = newScale;
        }
        else if (bounceFlag)
        {
            bounceCnt += 0.005f;
            bounce = Mathf.Sin(bounceCnt);
            Vector2 newScale = new Vector2(originalCellScale.x, originalCellScale.y + bounce / 30);
            cell.transform.DOScale(newScale, Time.deltaTime);
        }
    }

    private void DrawSpirit()
    {
        spirit.transform.position = new Vector2(spirit.transform.position.x, spirit.transform.position.y + 0.01f);
        var tmp = spirit.GetComponent<SpriteRenderer>().color;

        tmp.a = tmp.a - 0.001f;
        spirit.GetComponent<SpriteRenderer>().color = tmp;

        if (tmp.a > 0.0f)
        {
            tmp = shadowRenderer.color;
            tmp.a = tmp.a - 0.001f;
            shadowRenderer.color = tmp;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void UpdateSorting(SpriteRenderer targetrenderer)
    {
        targetrenderer.sortingOrder = -(int)(transform.position.y * 100);
    }

    public void Initiate(float h, float distanceY, Transform cellTrans)
    {
        height = h;
        velocity.y = -0.5f;
        isJump = true;
        UpdateShadow();

        if (cellTrans != null)
        {
            cellTrans.position = new Vector2(cellTrans.position.x, cellTrans.position.y + distanceY);
        }
        else
        {
            Debug.Log("Can't find cell");
        }
    }
}
