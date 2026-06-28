using UnityEngine;

namespace ProjectR.Items
{
    [ExecuteAlways]
    [RequireComponent(typeof(SpriteRenderer))]
    [DisallowMultipleComponent]
    public sealed class PrototypePickupItemSprite : MonoBehaviour
    {
        [SerializeField] private Color outerColor = new Color(1f, 0.9f, 0.2f, 1f);
        [SerializeField] private Color innerColor = new Color(0.2f, 0.95f, 0.55f, 1f);
        [SerializeField] private Color shineColor = Color.white;

        private const int TextureSize = 24;
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
            if (!Application.isPlaying)
            {
                ClearGeneratedAssets();
            }
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

            Color[] pixels = new Color[TextureSize * TextureSize];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = Color.clear;
            }

            DrawDiamond(pixels);
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

        private void DrawDiamond(Color[] pixels)
        {
            Vector2 center = new Vector2(11.5f, 11.5f);
            for (int y = 3; y < 21; y++)
            {
                for (int x = 3; x < 21; x++)
                {
                    float distance = Mathf.Abs(x - center.x) + Mathf.Abs(y - center.y);
                    if (distance <= 10f)
                    {
                        pixels[y * TextureSize + x] = distance > 7f ? outerColor : innerColor;
                    }
                }
            }

            SetPixel(pixels, 9, 15, shineColor);
            SetPixel(pixels, 10, 16, shineColor);
            SetPixel(pixels, 8, 16, shineColor);
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
