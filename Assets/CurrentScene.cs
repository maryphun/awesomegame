using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class CurrentScene : Singleton<CurrentScene>
{
    Collider2D groundCollider;
    List<Transform> cellList;
    List<Transform> foodList;

    public void Awake()
    {
        cellList = new List<Transform>();
        foodList = new List<Transform>();
    }

    public Collider2D GetCollider()
    {
        if (groundCollider == null)
        {
            groundCollider = GameObject.Find("ground").GetComponent<Collider2D>();
        }

        return groundCollider;
    }

    public int RegisterNewFood(Transform food)
    {
        foodList.Add(food);

        return foodList.Count;
    }

    public int RegisterNewCell(Transform cell)
    {
        cellList.Add(cell);

        return cellList.Count;
    }

    public int UnregisterFood(Transform food)
    {
        foodList.Remove(food);

        return foodList.Count;
    }

    public int GetRegisteredFoodNumber()
    {
        return foodList.Count;
    }

    public Transform GetNearestFood(Vector2 position)
    {
        Transform targetFood = foodList[0];
        float compareDistance = Vector2.Distance(targetFood.transform.position, position);

        foreach (Transform food in foodList)
        {
            float newDistance = Vector2.Distance(food.transform.position, position);

            if (newDistance <= compareDistance)
            {
                compareDistance = newDistance;
                targetFood = food;
            }
        }

        return targetFood;
    }

    public Transform GetNearestPartner(Vector2 position)
    {
        Transform targetPartner = null;
        float CompareDistance = 10000f;

        foreach(Transform cell in cellList)
        {
            if(cell.gameObject.GetComponent<CellAI>().status == AI.Status.love)
            {
                float newDistance = Vector2.Distance(cell.transform.position, position);
                if (newDistance <= CompareDistance)
                {
                    targetPartner = cell;
                }
            }
        }

        return targetPartner;
    }
}
