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
        private float _alpha = 0.001f;
        private float _beta;

        private SceneContainer _wuggy;
        private Renderer _renderer;
        private TransformComponent _wheelBigL;
        private TransformComponent _wheelBigR;
        private TransformComponent _wheelSmallL;
        private TransformComponent _wheelSmallR;
        private TransformComponent _neckLo;
        private TransformComponent _neckHi;
        private TransformComponent _eyesPitch;
        private float3 _lightPosition;

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
            _wuggy = AssetStorage.Get<SceneContainer>("wuggy.fus");
            _wheelBigL = _wuggy.Children.FindNodes(n => n.Name == "WheelBigL").First().GetTransform();
            _wheelBigR = _wuggy.Children.FindNodes(n => n.Name == "WheelBigR").First().GetTransform();
            _wheelSmallR = _wuggy.Children.FindNodes(n => n.Name == "WheelSmallR").First().GetTransform();
            _wheelSmallL = _wuggy.Children.FindNodes(n => n.Name == "WheelSmallL").First().GetTransform();
            _neckLo = _wuggy.Children.FindNodes(n => n.Name == "NeckLo").First().GetTransform();
            _neckHi = _wuggy.Children.FindNodes(n => n.Name == "NeckHi").First().GetTransform();
            _eyesPitch = _wuggy.Children.FindNodes(n => n.Name == "Eyes_Pitch").First().GetTransform();
            
            _lightPosition = new float3(0, 0, -1);

            _renderer = new Renderer(RC);
            _renderer.setLightPosition(_lightPosition);

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

            ControlEyes();
            ControlWeels();
            ControlNeck();
            ControlLight();
            ControlShininess(_wuggy, 0.05f * Mouse.WheelVel);
            
            // Setup matrices
            float4x4 view = float4x4.CreateTranslation(0, 0, 5)*float4x4.CreateRotationY(_alpha)*float4x4.CreateRotationX(_beta)*float4x4.CreateTranslation(0, -0.5f, 0);

            _renderer.View = view;
            _renderer.Traverse(_wuggy.Children);


            // Swap buffers: Show the contents of the backbuffer (containing the currently rendered frame) on the front buffer.
            Present();
        }

        public void ControlLight()
        {
            _lightPosition.x += 0.5f * Keyboard.LeftRightAxis;
            _renderer.setLightPosition(_lightPosition);
        }

        public void ControlEyes()
        {
            _neckHi.Rotation.y = -_alpha;
            _eyesPitch.Rotation.x = -_beta;
        }

        public void ControlWeels()
        {
            float wsInput = Keyboard.WSAxis;
            _wheelBigL.Rotation += new float3(-0.05f * wsInput, 0, 0);
            _wheelBigR.Rotation += new float3(-0.05f * wsInput, 0, 0);
            _wheelSmallL.Rotation += new float3(-0.05f * 2 * wsInput, 0, 0);
            _wheelSmallR.Rotation += new float3(-0.05f * 2 * wsInput, 0, 0);
            ControlSteering();
        }

        public void ControlSteering()
        {
            float adInput = -Keyboard.ADAxis;
            float maxAngle = 0.3f;
            System.Diagnostics.Debug.WriteLine("Input: " + adInput + " Rotation: " + _wheelSmallL.Rotation.y);
            if (adInput > 0 && _wheelSmallL.Rotation.y < maxAngle)
                updateSteering(adInput);
            
            if (adInput < 0 && _wheelSmallL.Rotation.y > -maxAngle)
                updateSteering(adInput);             
        }

        public void updateSteering(float adInput)
        {
            _wheelSmallL.Rotation += new float3(0, 0.05f * adInput, 0);
            _wheelSmallR.Rotation += new float3(0, 0.05f * adInput, 0);
        }

        public void ControlNeck()
        {
            float input = Keyboard.UpDownAxis;

            if (input > 0 && _neckLo.Translation.y < 210)
               updateNeck();
            

            if (input < 0 && _neckLo.Translation.y > 160)
                updateNeck();
        }

        public void updateNeck()
        {
            _neckLo.Translation.y += Keyboard.UpDownAxis;
            _neckHi.Translation.y += Keyboard.UpDownAxis;
        }

        public void ControlShininess(SceneContainer root, float value)
        {
            foreach (var child in root.Children)
            {
                if (child.GetMaterial() != null) child.GetMaterial().Specular.Shininess += value;
                if (child.Children != null) ControlShininess(child, value);
            }
        }

        private void ControlShininess(SceneNodeContainer root, float value)
        {
            foreach (var child in root.Children)
            {
                if (child.GetMaterial() != null) child.GetMaterial().Specular.Shininess += value;
                if (child.Children != null) ControlShininess(child, value);
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