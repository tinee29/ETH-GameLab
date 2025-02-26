using Penumbra;
using Microsoft.Xna.Framework;
namespace GameLab
{
    public class LightManager
    {
#pragma warning disable IDE0044 // Add readonly modifier

        private PenumbraComponent _penumbra;
#pragma warning restore IDE0044 // Add readonly modifier

        private PointLight _playerLight;

        public LightManager(PenumbraComponent penumbra)
        {
            _penumbra = penumbra;
            InitializeLights();
        }

        private void InitializeLights()
        {
            _playerLight = new PointLight
            {
                Scale = new Vector2(300), // Adjust as needed for the size of the light
                Color = Color.White,
                Intensity = 1.0f,
                Radius = 100
            };

            _penumbra.Lights.Add(_playerLight);
            _penumbra.AmbientColor = Color.Black; // Adjust for desired darkness
        }

        public void UpdateLightPosition(Vector2 position)
        {
            _playerLight.Position = position + new Vector2(Player.PLAYER_DIM / 2, Player.PLAYER_DIM / 2);
        }

        public void SetLightVisibility(bool isVisible)
        {
            _playerLight.Enabled = isVisible;
        }
    }
}
