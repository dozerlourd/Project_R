using UnityEngine;

namespace ProjectR.Player
{
    [ExecuteAlways]
    [RequireComponent(typeof(SpriteRenderer))]
    [DisallowMultipleComponent]
    public sealed class PrototypePlayerSprite : MonoBehaviour
    {
        [SerializeField] private Color bodyColor = new Color(0.18f, 0.76f, 0.92f, 1f);
        [SerializeField] private Color faceColor = new Color(0.06f, 0.12f, 0.16f, 1f);

        private const int TextureSize = 32;
        private Sprite generatedSprite;
        private Texture2D generatedTexture;
        private SpriteRenderer spriteRenderer;

#if UNITY_EDITOR
        private bool spriteRefreshQueued;
#endif

        private void OnEnable()
        {
            RequestSpriteRefresh();
        }

        private void OnValidate()
        {
            RequestSpriteRefresh();
        }

        private void OnDisable()
        {
            if (Application.isPlaying)
            {
                return;
            }

            ClearGeneratedAssets();
        }

        private void EnsureSprite()
        {
            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
            }

            if (spriteRenderer == null)
            {
                return;
            }

            ClearGeneratedAssets();

            generatedTexture = new Texture2D(TextureSize, TextureSize, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.HideAndDontSave
            };

            Color clear = new Color(0f, 0f, 0f, 0f);
            Color[] pixels = new Color[TextureSize * TextureSize];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = clear;
            }

            DrawBody(pixels);
            DrawFace(pixels);

            generatedTexture.SetPixels(pixels);
            generatedTexture.Apply();

            generatedSprite = Sprite.Create(
                generatedTexture,
                new Rect(0f, 0f, TextureSize, TextureSize),
                new Vector2(0.5f, 0.5f),
                TextureSize);

            generatedSprite.hideFlags = HideFlags.HideAndDontSave;
            spriteRenderer.sprite = generatedSprite;
        }

        private void DrawBody(Color[] pixels)
        {
            Vector2 center = new Vector2(15.5f, 14.5f);
            for (int y = 4; y < 29; y++)
            {
                for (int x = 7; x < 25; x++)
                {
                    float oval = Mathf.Pow((x - center.x) / 8.5f, 2f) + Mathf.Pow((y - center.y) / 11.5f, 2f);
                    if (oval <= 1f)
                    {
                        pixels[y * TextureSize + x] = bodyColor;
                    }
                }
            }

            for (int y = 22; y < 29; y++)
            {
                for (int x = 12; x < 20; x++)
                {
                    pixels[y * TextureSize + x] = bodyColor;
                }
            }
        }

        private void DrawFace(Color[] pixels)
        {
            SetPixel(pixels, 12, 15, faceColor);
            SetPixel(pixels, 19, 15, faceColor);

            for (int x = 13; x < 19; x++)
            {
                SetPixel(pixels, x, 10, faceColor);
            }
        }

        private static void SetPixel(Color[] pixels, int x, int y, Color color)
        {
            if (x < 0 || x >= TextureSize || y < 0 || y >= TextureSize)
            {
                return;
            }

            pixels[y * TextureSize + x] = color;
        }

        private void ClearGeneratedAssets()
        {
            if (generatedSprite != null)
            {
                DestroyGeneratedObject(generatedSprite);
                generatedSprite = null;
            }

            if (generatedTexture != null)
            {
                DestroyGeneratedObject(generatedTexture);
                generatedTexture = null;
            }
        }

        private static void DestroyGeneratedObject(Object generatedObject)
        {
            if (Application.isPlaying)
            {
                Destroy(generatedObject);
                return;
            }

            DestroyImmediate(generatedObject);
        }

        private void RequestSpriteRefresh()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                if (spriteRefreshQueued)
                {
                    return;
                }

                spriteRefreshQueued = true;
                UnityEditor.EditorApplication.delayCall += () =>
                {
                    if (this == null)
                    {
                        return;
                    }

                    spriteRefreshQueued = false;
                    EnsureSprite();
                };
                return;
            }
#endif

            EnsureSprite();
        }
    }
}
