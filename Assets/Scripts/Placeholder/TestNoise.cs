using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class TestNoise : MonoBehaviour
{
    Texture2D m_texture = null;

    void Start()
    {
        GenerateSprite();
    }
    
    void GenerateSprite()
    {
        int size = 512;
        m_texture = new Texture2D(size, size, TextureFormat.ARGB32, false);

        int frec = 2;
        Worley2D[] worley = new Worley2D[] {
            new Worley2D(size, 1, frec*=2, 1),
            new Worley2D(size, 0.5f, frec*=2, 2),
            new Worley2D(size, 0.25f, frec*=2, 3),
            new Worley2D(size, 0.125f, frec*=2, 4)
        };

        for(int i = 0; i < size; i++)
            for(int j = 0; j < size; j++)
            {
                float value = 0;
                foreach(var w in worley)
                    value += w.Get(i, j, 1, Lerp.Operator.Linear);
                value *= 0.7f;
                if (value > 1)
                    value = 1;

                m_texture.SetPixel(i, j, new Color(value, value, value));
            }

        m_texture.Apply();
    }

    private void OnGUI()
    {
        GUI.DrawTexture(new Rect(20, 20, m_texture.width, m_texture.height), m_texture);
    }
}
