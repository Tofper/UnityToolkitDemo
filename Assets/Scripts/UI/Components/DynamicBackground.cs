using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Scripts.UI.Components
{
    /// <summary>
    /// Manages a dynamic animated background with customizable shapes, colors, and effects.
    /// Requires a RawImage component to display the background.
    /// </summary>
    [RequireComponent(typeof(RawImage))]
    public class DynamicBackground : MonoBehaviour
    {
        /// <summary>
        /// Defines a color palette for the dynamic background.
        /// </summary>
        [System.Serializable]
        public class ColorPalette
        {
            /// <summary>The name of the color palette.</summary>
            public string paletteName = "Default";
            /// <summary>The first primary color of the gradient.</summary>
            public Color color1 = new Color(0.353f, 0.427f, 0.561f); // #5a6d8f
            /// <summary>The second primary color of the gradient.</summary>
            public Color color2 = new Color(0.137f, 0.184f, 0.243f); // #232f3e
            /// <summary>The rim light color.</summary>
            public Color rimColor = new Color(0.878f, 0.969f, 0.98f); // #e0f7fa
        }

        // Shader Property Names
        private const string SHADER_NAME = "UI/DynamicBackground";
        private const string COLOR1_PROPERTY_NAME = "_Color1";
        private const string COLOR2_PROPERTY_NAME = "_Color2";
        private const string RIM_COLOR_PROPERTY_NAME = "_RimColor";
        private const string NUM_SHAPES_PROPERTY_NAME = "_NumShapes";
        private const string OPACITY_PROPERTY_NAME = "_Opacity";
        private const string RIM_LIGHT_PROPERTY_NAME = "_RimLight";
        private const string ANIM_SPEED_PROPERTY_NAME = "_AnimSpeed";
        private const string COLOR_ANIM_SPEED_PROPERTY_NAME = "_ColorAnimSpeed";
        private const string BLUR_PROPERTY_NAME = "_Blur";
        private const string VIGNETTE_PROPERTY_NAME = "_Vignette";
        private const string CURVATURE_PROPERTY_NAME = "_Curvature";
        private const string NOISE_INTENSITY_PROPERTY_NAME = "_NoiseIntensity";
        private const string TIME_OFFSET_PROPERTY_NAME = "_TimeOffset";
        private const string SHAPE_DATA_PROPERTY_NAME = "_ShapeData";

        [Header("Shape Settings")]
        [Range(1, 10)]
        /// <summary>The number of animated shapes to display.</summary>
        [SerializeField] private int _numShapes = 5;
        [Range(0f, 1f)]
        /// <summary>The overall opacity of the shapes.</summary>
        [SerializeField] private float _opacity = 0.5f;
        [Range(0f, 1f)]
        /// <summary>The intensity of the rim lighting effect on shapes.</summary>
        [SerializeField] private float _rimLight = 0.1f;
        [Range(0f, 2f)]
        /// <summary>The curvature of the animated shapes.</summary>
        [SerializeField] private float _curvature = 0.7f;

        [Header("Animation Settings")]
        [Range(0f, 2f)]
        /// <summary>The speed of the shape movement animation.</summary>
        [SerializeField] private float _animSpeed = 0.3f;
        [Range(0f, 5f)]
        /// <summary>The speed of the color transition animation between palettes.</summary>
        [SerializeField] private float _colorAnimSpeed = 1.0f;
        [Range(0f, 60f)]
        /// <summary>The duration in seconds for one full cycle through all color palettes.</summary>
        [SerializeField] private float _paletteCycleSpeed = 12f;

        [Header("Visual Effects")]
        [Range(0f, 10f)]
        /// <summary>The intensity of the background blur.</summary>
        [SerializeField] private float _blur = 0f;
        [Range(0f, 1f)]
        /// <summary>The intensity of the vignette effect.</summary>
        [SerializeField] private float _vignette = 0.72f;
        [Range(0f, 0.1f)]
        /// <summary>The intensity of the noise overlay effect.</summary>
        [SerializeField] private float _noiseIntensity = 0.01f;

        [Header("Color Palettes")]
        /// <summary>A list of color palettes to cycle through.</summary>
        [SerializeField]
        private List<ColorPalette> _colorPalettes = new List<ColorPalette>()
        {
            new ColorPalette
            {
                paletteName = "Blue",
                color1 = new Color(0.353f, 0.427f, 0.561f), // #5a6d8f
                color2 = new Color(0.137f, 0.184f, 0.243f), // #232f3e
                rimColor = new Color(0.878f, 0.969f, 0.98f) // #e0f7fa
            },
            new ColorPalette
            {
                paletteName = "Green",
                color1 = new Color(0.478f, 0.616f, 0.482f), // #7a9d7b
                color2 = new Color(0.243f, 0.337f, 0.255f), // #3e5641
                rimColor = new Color(0.918f, 0.980f, 0.902f) // #eafbe6
            }
        };

        private RawImage _image;
        private Material _material;
        private Texture2D _shapeDataTexture;
        private float _timeOffset;
        private float[] _shapeSeeds;
        private bool _initialized = false;

        /// <summary>
        /// Initializes the background, material, and shape data.
        /// </summary>
        private void Start()
        {
            _image = GetComponent<RawImage>();

            // Create a material instance
            Shader dynamicBackgroundShader = Shader.Find(SHADER_NAME);
            if (dynamicBackgroundShader == null)
            {
                Debug.LogError($"{nameof(DynamicBackground)}: Could not find shader '{SHADER_NAME}'. Dynamic background will not be displayed.");
                enabled = false; // Disable the script if shader is not found
                return;
            }
            _material = new Material(dynamicBackgroundShader);
            _image.material = _material;

            // Initialize shape data
            InitializeShapeData();

            // Apply initial settings
            UpdateMaterialProperties();

            _initialized = true;
        }

        /// <summary>
        /// Initializes the random seed data for the shapes.
        /// </summary>
        private void InitializeShapeData()
        {
            // Generate random seeds for each shape
            _shapeSeeds = new float[10]; // Max 10 shapes
            for (int i = 0; i < _shapeSeeds.Length; i++)
            {
                _shapeSeeds[i] = Random.Range(0f, 1000f);
            }

            // Create shape data texture
            _shapeDataTexture = new Texture2D(10, 1, TextureFormat.RGBAFloat, false);
            _shapeDataTexture.filterMode = FilterMode.Point;
            UpdateShapeDataTexture();
        }

        /// <summary>
        /// Updates the shape data stored in the texture.
        /// </summary>
        private void UpdateShapeDataTexture()
        {
            Color[] pixels = new Color[10];

            for (int i = 0; i < 10; i++)
            {
                // Store shape data in texture
                // R: Shape type (0-1)
                // G: Animation speed multiplier (0-1)
                // B: Curvature multiplier (0-1)
                // A: Seed value (0-1)
                pixels[i] = new Color(
                    Random.Range(0f, 1f),
                    Random.Range(0.5f, 1.5f),
                    Random.Range(0.7f, 1.3f),
                    _shapeSeeds[i] / 1000f
                );
            }

            _shapeDataTexture.SetPixels(pixels);
            _shapeDataTexture.Apply();
        }

        /// <summary>
        /// Updates the material properties each frame, including color palette cycling.
        /// </summary>
        private void Update()
        {
            if (!_initialized || _material == null)
            {
                return;
            }

            // Update current palette based on time
            float timeVal = Time.time * _colorAnimSpeed;

            int paletteCount = _colorPalettes.Count;
            if (paletteCount >= 2)
            {
                // Calculate current and next palette indices
                float paletteIndexFloat = timeVal / _paletteCycleSpeed;
                int currentPaletteIndex = Mathf.FloorToInt(paletteIndexFloat) % paletteCount;
                int nextPaletteIndex = (currentPaletteIndex + 1) % paletteCount;

                // Calculate interpolation factor within the current cycle segment
                float segmentProgress = Mathf.Repeat(paletteIndexFloat, 1f);

                // Interpolate between palettes
                var current = _colorPalettes[currentPaletteIndex];
                var next = _colorPalettes[nextPaletteIndex];

                Color lerpedColor1 = Color.Lerp(current.color1, next.color1, segmentProgress);
                Color lerpedColor2 = Color.Lerp(current.color2, next.color2, segmentProgress);
                Color lerpedRimColor = Color.Lerp(current.rimColor, next.rimColor, segmentProgress);

                _material.SetColor(COLOR1_PROPERTY_NAME, lerpedColor1);
                _material.SetColor(COLOR2_PROPERTY_NAME, lerpedColor2);
                _material.SetColor(RIM_COLOR_PROPERTY_NAME, lerpedRimColor);
            }
            else if (paletteCount == 1 && _material != null)
            {
                // Just use the single palette
                _material.SetColor(COLOR1_PROPERTY_NAME, _colorPalettes[0].color1);
                _material.SetColor(COLOR2_PROPERTY_NAME, _colorPalettes[0].color2);
                _material.SetColor(RIM_COLOR_PROPERTY_NAME, _colorPalettes[0].rimColor);
            }
            else if (paletteCount == 0 && _material != null)
            {
                // Optionally reset colors or use defaults if no palettes are defined
                _material.SetColor(COLOR1_PROPERTY_NAME, Color.black);
                _material.SetColor(COLOR2_PROPERTY_NAME, Color.black);
                _material.SetColor(RIM_COLOR_PROPERTY_NAME, Color.black);
            }

            // Update time offset for animation (use animSpeed here)
            if (_material != null)
            {
                float currentShaderTimeOffset = _material.HasVector(TIME_OFFSET_PROPERTY_NAME) ? _material.GetVector(TIME_OFFSET_PROPERTY_NAME).x : 0f;
                _timeOffset = currentShaderTimeOffset + Time.deltaTime * _animSpeed;

                _material.SetVector(TIME_OFFSET_PROPERTY_NAME, new Vector4(_timeOffset, 0, 0, 0));
            }
        }

        /// <summary>
        /// Called in the editor when script properties are changed.
        /// Updates material properties to reflect changes immediately during play mode.
        /// </summary>
        private void OnValidate()
        {
            if (Application.isPlaying && _initialized && _material != null)
            {
                UpdateMaterialProperties();
            }
            else if (!Application.isPlaying && _material == null)
            {
                // Attempt to find shader and create material in editor if not in play mode
                Shader dynamicBackgroundShader = Shader.Find(SHADER_NAME);
                if (dynamicBackgroundShader != null)
                {
                    _material = new Material(dynamicBackgroundShader);
                }
            }
        }

        /// <summary>
        /// Applies the current serialized settings to the material properties.
        /// </summary>
        private void UpdateMaterialProperties()
        {
            if (_material == null)
            {
                return;
            }

            float currentShaderTimeOffset = _material.HasVector(TIME_OFFSET_PROPERTY_NAME) ? _material.GetVector(TIME_OFFSET_PROPERTY_NAME).x : 0f;
            _timeOffset = currentShaderTimeOffset;

            _material.SetFloat(NUM_SHAPES_PROPERTY_NAME, _numShapes);
            _material.SetFloat(OPACITY_PROPERTY_NAME, _opacity);
            _material.SetFloat(RIM_LIGHT_PROPERTY_NAME, _rimLight);
            _material.SetFloat(ANIM_SPEED_PROPERTY_NAME, _animSpeed);
            _material.SetFloat(COLOR_ANIM_SPEED_PROPERTY_NAME, _colorAnimSpeed);
            _material.SetFloat(BLUR_PROPERTY_NAME, _blur);
            _material.SetFloat(VIGNETTE_PROPERTY_NAME, _vignette);
            _material.SetFloat(CURVATURE_PROPERTY_NAME, _curvature);
            _material.SetFloat(NOISE_INTENSITY_PROPERTY_NAME, _noiseIntensity);

            if (_shapeDataTexture != null)
            {
                _material.SetTexture(SHAPE_DATA_PROPERTY_NAME, _shapeDataTexture);
            }

            if (_colorPalettes.Count > 0)
            {
                _material.SetColor(COLOR1_PROPERTY_NAME, _colorPalettes[0].color1);
                _material.SetColor(COLOR2_PROPERTY_NAME, _colorPalettes[0].color2);
                _material.SetColor(RIM_COLOR_PROPERTY_NAME, _colorPalettes[0].rimColor);
            }
            else
            {
                _material.SetColor(COLOR1_PROPERTY_NAME, Color.black);
                _material.SetColor(COLOR2_PROPERTY_NAME, Color.black);
                _material.SetColor(RIM_COLOR_PROPERTY_NAME, Color.black);
            }
        }

        /// <summary>
        /// Cleans up resources when the object is destroyed.
        /// </summary>
        private void OnDestroy()
        {
            // Destroy the created material and texture to prevent memory leaks
            if (_material != null)
            {
                Destroy(_material);
                _material = null;
            }
            if (_shapeDataTexture != null)
            {
                Destroy(_shapeDataTexture);
                _shapeDataTexture = null;
            }
        }
    }
}