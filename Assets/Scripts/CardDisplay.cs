using UnityEngine;
using System.Collections.Generic;

public class CardDisplay : MonoBehaviour
{
    public Renderer frontRenderer;
    public Renderer backRenderer;
    public Texture2D backTexture;
    public List<Texture2D> cardTextures = new List<Texture2D>();

  //  private bool isFaceUp = false;

    void Start()
    {
        if (backRenderer != null && backTexture != null)
        {
            backRenderer.material.mainTexture = backTexture;
        }
    }

    public void SetCard(int value, string suit)
    {
        string cardName = $"{value}_of_{suit}";

        foreach (Texture2D texture in cardTextures)
        {
            if (texture.name == cardName)
            {
                frontRenderer.material.mainTexture = texture;
                return;
            }
        }

        Debug.LogError($"Card texture not found: {cardName}");
    }

   // public void FlipCard(bool faceUp)
  //  {
   //     isFaceUp = faceUp;
  //      frontRenderer.gameObject.SetActive(faceUp);
  //      backRenderer.gameObject.SetActive(!faceUp);
  //  }
}
