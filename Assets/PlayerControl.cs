using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerControl : MonoBehaviour
{
    public enum ClickMode { food, blob };

    [SerializeField] private GameObject foodToSpawn;
    [SerializeField] private GameObject blobToSpawn;

    private TextMeshProUGUI UIText;
    private ClickMode clickmode;

    private void Start()
    {
        UIText = GameObject.Find("MiddleText").GetComponent<TextMeshProUGUI>();
        ShowText();
        SetText("Click to spawn a new blob!");
        clickmode = ClickMode.blob;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePositionInWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            if (CurrentScene.Instance().GetCollider().ClosestPoint(mousePositionInWorld) == mousePositionInWorld)
            {
                switch (clickmode)
                {
                    case ClickMode.blob:
                        SpawnBlob(mousePositionInWorld);
                        break;
                    case ClickMode.food:
                        Instantiate(foodToSpawn, mousePositionInWorld, Quaternion.identity);
                        break;
                    default:
                        break;
                }                
            }
        }
    }

    private void SpawnBlob(Vector2 pos)
    {
        GameObject blob = Instantiate(blobToSpawn, pos, Quaternion.identity);
        CellAI blobScript = blob.GetComponent<CellAI>();
        HideText();

        if (blobScript != null)
        {
            blobScript.Initiate(2f, 10f, blob.transform.GetChild(1));
        }
        else
        { 
            Debug.Log("CellAI Script isn't attached in new spawned blob! It will not function.");
        }

        // Temporary
        ChangeClickMode(ClickMode.food);
    }

    public void ChangeClickMode(ClickMode newMode)
    {
        clickmode = newMode;
    }

    private void ShowText()
    {
        UIText.enabled = true;
    }

    private void HideText()
    {
        UIText.enabled = false;
    }

    private void SetText(string text)
    {
        UIText.SetText(text);
    }
}
