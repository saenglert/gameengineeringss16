using System.Linq;
using Fusee.Base.Core;
using Fusee.Engine.Common;
using Fusee.Engine.Core;
using Fusee.Math.Core;
using Fusee.Serialization;
using Fusee.Xene;
using static Fusee.Engine.Core.Input;

namespace Fusee.Tutorial.Core
{

    [FuseeApplication(Name = "Tutorial Example", Description = "The official FUSEE Tutorial.")]
    public class Tutorial : RenderCanvas
    {

        private IShaderParam _albedoParam;
        private float _alpha = 0.001f;
        private float _beta;

        private SceneContainer _wuggy;
        private Renderer _renderer;
        private TransformComponent _wheelBigL;

        public static Mesh LoadMesh(string assetName)
        {
            SceneContainer sc = AssetStorage.Get<SceneContainer>(assetName);
            MeshComponent mc = sc.Children.FindComponents<MeshComponent>(c => true).First();
            return new Mesh
            {
                Vertices = mc.Vertices,
                Normals = mc.Normals,
                Triangles = mc.Triangles
            };
        }

        // Init is called on startup. 
        public override void Init()
        {
            var vertsh = AssetStorage.Get<string>("VertexShader.vert");
            var pixsh = AssetStorage.Get<string>("PixelShader.frag");

            _wuggy = AssetStorage.Get<SceneContainer>("wuggy.fus");
            _wheelBigL = _wuggy.Children.FindNodes(n => n.Name == "WheelBigL").First().GetTransform();
            _renderer = new Renderer(RC);

            // Set the clear color for the backbuffer
            RC.ClearColor = new float4(1, 1, 1, 1);
        }

        // RenderAFrame is called once a frame
        public override void RenderAFrame()
        {
            // Clear the backbuffer
            RC.Clear(ClearFlags.Color | ClearFlags.Depth);

            // Movement
            float2 speed = Mouse.Velocity + Touch.GetVelocity(TouchPoints.Touchpoint_0);
            if (Mouse.LeftButton || Touch.GetTouchActive(TouchPoints.Touchpoint_0))
            {
                _alpha -= speed.x*0.0001f;
                _beta  -= speed.y*0.0001f;
            }

            _wheelBigL.Rotation += new float3(-0.05f * Keyboard.WSAxis, 0, 0);

            float shineUpdate = Keyboard.UpDownAxis;
            changeShininess(_wuggy, shineUpdate);

            // Setup matrices
            float4x4 view = float4x4.CreateTranslation(0, 0, 5)*float4x4.CreateRotationY(_alpha)*float4x4.CreateRotationX(_beta)*float4x4.CreateTranslation(0, -0.5f, 0);

            _renderer.View = view;
            _renderer.Traverse(_wuggy.Children);


            // Swap buffers: Show the contents of the backbuffer (containing the currently rendered frame) on the front buffer.
            Present();
        }

        public void changeShininess(SceneContainer root, float value)
        {
            foreach (var child in root.Children)
            {
                if (child.GetMaterial() != null) child.GetMaterial().Specular.Shininess += value;
                if (child.Children != null) changeShininess(child, value);
            }
        }

        private void changeShininess(SceneNodeContainer root, float value)
        {
            foreach (var child in root.Children)
            {
                if (child.GetMaterial() != null) child.GetMaterial().Specular.Shininess += value;
                if (child.Children != null) changeShininess(child, value);
            }
        }


        // Is called when the window was resized
        public override void Resize()
        {
            // Set the new rendering area to the entire new windows size
            RC.Viewport(0, 0, Width, Height);

            // Create a new projection matrix generating undistorted images on the new aspect ratio.
            var aspectRatio = Width/(float) Height;

            // 0.25*PI Rad -> 45° Opening angle along the vertical direction. Horizontal opening angle is calculated based on the aspect ratio
            // Front clipping happens at 1 (Objects nearer than 1 world unit get clipped)
            // Back clipping happens at 2000 (Anything further away from the camera than 2000 world units gets clipped, polygons will be cut)
            var projection = float4x4.CreatePerspectiveFieldOfView(3.141592f * 0.25f, aspectRatio, 1, 20000);
            RC.Projection = projection;
        }

    }
}