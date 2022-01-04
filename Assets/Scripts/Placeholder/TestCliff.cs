using Noise;
using System.Collections;
using UnityEngine;

namespace Assets.Scripts.Placeholder
{
    public class TestCliff : MonoBehaviour
    {
        public int imageSize = 512;
        public int cellCount = 25;
        public int cliffLength = 10;
        public int cellAtMinHeight = 10;
        public int seed = 0;


        Texture2D m_texture;

        void Start()
        {
            m_texture = new Texture2D(imageSize, imageSize);
            Cliff cliff = new Cliff(imageSize, cellCount, cliffLength, cellAtMinHeight, seed);

            for(int i = 0; i < imageSize; i++)
            {
                for(int j = 0; j < imageSize; j++)
                {
                    float height = cliff.GetHeight(new Vector2(i, j));
                    m_texture.SetPixel(i, j, new Color(height, height, height, 1));
                }
            }

            m_texture.Apply();
        }

        private void OnGUI()
        {
            GUI.Label(new Rect(20, 20, imageSize, imageSize), m_texture);
        }
    }
}