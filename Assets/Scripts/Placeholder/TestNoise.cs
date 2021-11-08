using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class TestNoise : MonoBehaviour
{
    Texture2D[] m_textures = null;


    void Start()
    {
        GenerateSprite();
    }
    
    void GenerateSprite()
    {
        int size = 400;
        m_textures = new Texture2D[6];
        for(int i = 0; i < m_textures.Length; i++)
            m_textures[i] = new Texture2D(size, size, TextureFormat.ARGB32, false);

        int frec = 4;
        float amplitude = 2;
        Worley2D[] worley = new Worley2D[] {
            new Worley2D(size, amplitude/=2, frec*=2, 1),
            new Worley2D(size, amplitude/=2, frec*=2, 2),
            new Worley2D(size, amplitude/=2, frec*=2, 3),
            new Worley2D(size, amplitude/=2, frec*=2, 4)
        };

        frec = 4;
        amplitude = 2;
        Perlin[] perlin = new Perlin[] {
            new Perlin(size, amplitude/=2, frec*=2, 1),
            new Perlin(size, amplitude/=2, frec*=2, 2),
            new Perlin(size, amplitude/=2, frec*=2, 3),
            new Perlin(size, amplitude/=2, frec*=2, 4)
        };

        frec = 4;
        amplitude = 2;
        Turbulence[] turbulence = new Turbulence[] {
            new Turbulence(size, amplitude/=2, frec*=2, 1),
            new Turbulence(size, amplitude/=2, frec*=2, 2),
            new Turbulence(size, amplitude/=2, frec*=2, 3),
            new Turbulence(size, amplitude/=2, frec*=2, 4)
        };

        for (int i = 0; i < size; i++)
            for(int j = 0; j < size; j++)
            {
                float value = 0;
                foreach(var w in worley)
                    value += w.Get(i, j, 1, Lerp.Operator.Linear);
                value *= 0.7f;
                m_textures[0].SetPixel(i, j, new Color(value, value, value));

                value = 0;
                foreach (var w in perlin)
                    value += w.Get(i, j, Lerp.Operator.Square);
                value = value / 2.5f + 0.5f;
                m_textures[1].SetPixel(i, j, new Color(value, value, value));

                value = 0;
                foreach (var w in turbulence)
                    value += w.Get(i, j, Lerp.Operator.Square);
                value *= 0.7f;
                m_textures[2].SetPixel(i, j, new Color(value, value, value));

                value = worley[0].Get(i, j, 1, Lerp.Operator.Linear);
                m_textures[3].SetPixel(i, j, new Color(value, value, value));

                value = perlin[1].Get(i, j, Lerp.Operator.Square) / 2 + 0.5f;
                m_textures[4].SetPixel(i, j, new Color(value, value, value));

                value = turbulence[1].Get(i, j, Lerp.Operator.Square);
                m_textures[5].SetPixel(i, j, new Color(value, value, value));
            }

        foreach(var t in m_textures)
            t.Apply();
    }

    private void OnGUI()
    {
        int offset = 20;
        int textureSize = m_textures[0].width;
        GUI.DrawTexture(new Rect(offset, offset, textureSize, textureSize), m_textures[0]);
        GUI.DrawTexture(new Rect(2 * offset + textureSize, offset, textureSize, textureSize), m_textures[1]);
        GUI.DrawTexture(new Rect(3 * offset + 2 * textureSize, offset, textureSize, textureSize), m_textures[2]);
        GUI.DrawTexture(new Rect(offset, 2 * offset + textureSize, textureSize, textureSize), m_textures[3]);
        GUI.DrawTexture(new Rect(2 * offset + textureSize, 2 * offset + textureSize, textureSize, textureSize), m_textures[4]);
        GUI.DrawTexture(new Rect(3 * offset + 2 * textureSize, 2 * offset + textureSize, textureSize, textureSize), m_textures[5]);
    }
}
