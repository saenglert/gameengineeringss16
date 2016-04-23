using System.Collections.Generic;
using System.Linq;
using Fusee.Base.Common;
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
        private Mesh _mesh;
        private const string _vertexShader = @"
        attribute vec3 fuVertex;
        attribute vec3 fuNormal;
        uniform mat4 FUSEE_MVP;
        uniform mat4 FUSEE_MV;
        varying vec3 modelpos;
        varying vec3 normal;
        void main()
        {
            modelpos = fuVertex;
            normal = normalize(mat3(FUSEE_MV) * fuNormal);
            gl_Position = FUSEE_MVP * vec4(fuVertex, 1.0);
        }";

        private const string _pixelShader = @"
        #ifdef GL_ES
            precision highp float;
        #endif
        varying vec3 modelpos;
        varying vec3 normal;
        uniform vec3 albedo;

        void main()
        {
            float intensity = dot(normal, vec3(0, 0, -1));
            gl_FragColor = vec4(intensity * albedo, 1);
        }";

        private float _alpha;
        private float _beta;

        private float2 arrows;
        private float2 wasd;

        private IShaderParam _albedoParam;
        private SceneOb _root;

        // Init is called on startup. 
        public override void Init()
        {
            // Initialize the shader(s)
            var shader = RC.CreateShader(_vertexShader, _pixelShader);
            RC.SetShader(shader);
            _albedoParam = RC.GetShaderParam(shader, "albedo");

            // Load some meshes
            Mesh cone = LoadMesh("Cone.fus");
            Mesh cube = LoadMesh("Cube.fus");
            Mesh cylinder = LoadMesh("Cylinder.fus");
            Mesh pyramid = LoadMesh("Pyramid.fus");
            Mesh sphere = LoadMesh("Sphere.fus");

            // Setup a list of objects
            _root = new SceneOb {name =  "root"};

            // Add Body
            _root.Children.Add(new SceneOb { Mesh = cube, Albedo = new float3(0, 0, 0), Pos = new float3(0, 2.75f, 0), ModelScale = new float3(0.5f, 1, 0.25f), name = "body" });

            // Add Legs
            _root.Children.Add(new SceneOb { Mesh = cylinder, Albedo = new float3(0, 0, 0), Pos = new float3(-0.25f, 1, 0), ModelScale = new float3(0.15f, 0.75f, 0.15f), name = "rightupperleg" });
            FindSceneOb(_root, "rightupperleg").Children.Add(new SceneOb { Mesh = sphere, Albedo = new float3(1, 0.5f, 0), Pos = new float3(0, -0.75f, 0), ModelScale = new float3(0.25f, 0.25f, 0.25f), name = "rightknee" });
            FindSceneOb(_root, "rightknee").Children.Add(new SceneOb { Mesh = cylinder, Albedo = new float3(0, 0, 0), Pos = new float3(0, -0.5f, 0), ModelScale = new float3(0.15f, 0.5f, 0.15f), name = "rightlowerleg" });
            FindSceneOb(_root, "rightlowerleg").Children.Add(new SceneOb { Mesh = sphere, Albedo = new float3(1, 0.5f, 0), Pos = new float3(0, -0.70f, 0), ModelScale = new float3(0.25f, 0.25f, 0.25f), name = "rightfoot" });

            _root.Children.Add(new SceneOb { Mesh = cylinder, Albedo = new float3(0, 0, 0), Pos = new float3(0.25f, 1, 0),Pivot = new float3(0, 0.75f, 0),ModelScale = new float3(0.15f, 0.75f, 0.15f), name = "leftupperleg" });
            FindSceneOb(_root, "leftupperleg").Children.Add(new SceneOb { Mesh = sphere, Albedo = new float3(1, 0.5f, 0), Pos = new float3(0, -0.75f, 0), ModelScale = new float3(0.25f, 0.25f, 0.25f), name = "leftknee" });
            FindSceneOb(_root, "leftknee").Children.Add(new SceneOb { Mesh = cylinder, Albedo = new float3(0, 0, 0), Pos = new float3(0, -0.5f, 0), ModelScale = new float3(0.15f, 0.5f, 0.15f), name = "leftlowerleg" });
            FindSceneOb(_root, "leftlowerleg").Children.Add(new SceneOb { Mesh = sphere, Albedo = new float3(1, 0.5f, 0), Pos = new float3(0, -0.70f, 0), ModelScale = new float3(0.25f, 0.25f, 0.25f), name = "leftfoot" });

            // Add Arms
            _root.Children.Add(new SceneOb { Mesh = sphere, Albedo = new float3(1, 0.5f, 0), Pos = new float3(-0.75f, 3.5f, 0), ModelScale = new float3(0.25f, 0.25f, 0.25f), name = "rightshoulder" });
            FindSceneOb(_root, "rightshoulder").Children.Add(new SceneOb { Mesh = cylinder, Albedo = new float3(0, 0, 0), Pos = new float3(0, -0.5f, 0), ModelScale = new float3(0.15f, 0.5f, 0.15f), name = "rightupperarm" });
            FindSceneOb(_root, "rightupperarm").Children.Add(new SceneOb { Mesh = sphere, Albedo = new float3(1, 0.5f, 0), Pos = new float3(0, -0.5f, 0), ModelScale = new float3(0.25f, 0.25f, 0.25f), name = "rightellbow" });
            FindSceneOb(_root, "rightellbow").Children.Add(new SceneOb { Mesh = cylinder, Albedo = new float3(0, 0, 0), Pos = new float3(0, -0.5f, 0), ModelScale = new float3(0.15f, 0.5f, 0.15f), name = "rightlowerarm" });
            FindSceneOb(_root, "rightlowerarm").Children.Add(new SceneOb { Mesh = sphere, Albedo = new float3(1, 0.5f, 0), Pos = new float3(0, -0.5f, 0), ModelScale = new float3(0.25f, 0.25f, 0.25f), name = "rightwrist" });

            _root.Children.Add(new SceneOb { Mesh = sphere, Albedo = new float3(1, 0.5f, 0), Pos = new float3(0.75f, 3.5f, 0), ModelScale = new float3(0.25f, 0.25f, 0.25f), name = "leftshoulder" });            
            FindSceneOb(_root, "leftshoulder").Children.Add(new SceneOb { Mesh = cylinder, Albedo = new float3(0, 0, 0), Pos = new float3(0, -0.5f, 0), ModelScale = new float3(0.15f, 0.5f, 0.15f), name = "leftupperarm" });
            FindSceneOb(_root, "leftupperarm").Children.Add(new SceneOb { Mesh = sphere, Albedo = new float3(1, 0.5f, 0), Pos = new float3(0, -0.5f, 0), ModelScale = new float3(0.25f, 0.25f, 0.25f), name = "leftellbow" });
            FindSceneOb(_root, "leftellbow").Children.Add(new SceneOb { Mesh = cylinder, Albedo = new float3(0, 0, 0), Pos = new float3(0, -0.5f, 0), ModelScale = new float3(0.15f, 0.5f, 0.15f), name = "leftlowerarm" });
            FindSceneOb(_root, "leftlowerarm").Children.Add(new SceneOb { Mesh = sphere, Albedo = new float3(1, 0.5f, 0), Pos = new float3(0, -0.5f, 0), ModelScale = new float3(0.25f, 0.25f, 0.25f), name = "leftwrist" });

            // Add Head
            _root.Children.Add(new SceneOb { Mesh = sphere, Albedo = new float3(0, 1, 0), Pos = new float3(0, 4.2f, 0), ModelScale = new float3(0.35f, 0.5f, 0.35f), name = "head" });

            // Set the clear color for the backbuffer
            RC.ClearColor = new float4(1, 1, 1, 1);
        }

        static float4x4 ModelXForm(float3 pos, float3 rot, float3 pivot)
        {
            return float4x4.CreateTranslation(pos + pivot)
                   *float4x4.CreateRotationY(rot.y)
                   *float4x4.CreateRotationX(rot.x)
                   *float4x4.CreateRotationZ(rot.z)
                   *float4x4.CreateTranslation(-pivot);
        }

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

        void RenderSceneOb(SceneOb so, float4x4 modelView)
        {
            modelView = modelView * ModelXForm(so.Pos, so.Rot, so.Pivot) * float4x4.CreateScale(so.Scale);
            if (so.Mesh != null)
            {
                RC.ModelView = modelView * float4x4.CreateScale(so.ModelScale);
                RC.SetShaderParam(_albedoParam, so.Albedo);
                RC.Render(so.Mesh);
            }

            if (so.Children != null)
            {
                foreach (var child in so.Children)
                {
                    RenderSceneOb(child, modelView);
                }
            }
        }

        public static SceneOb FindSceneOb(SceneOb so, string name)
        {
            return so.name == name ? so : so.Children.Select(child => FindSceneOb(child, name)).FirstOrDefault(result => result != null && result.name == name);
        }

        // RenderAFrame is called once a frame
        public override void RenderAFrame()
        {
            // Clear the backbuffer
            RC.Clear(ClearFlags.Color | ClearFlags.Depth);

            float2 speed = Mouse.Velocity + Touch.GetVelocity(TouchPoints.Touchpoint_0);
            if (Mouse.LeftButton || Touch.GetTouchActive(TouchPoints.Touchpoint_0))
            {
                _alpha -= speed.x * 0.0001f;
                _beta -= speed.y * 0.0001f;
            }

            arrows.x -= Keyboard.LeftRightAxis * 0.1f;
            arrows.y -= Keyboard.UpDownAxis * 0.1f;

            wasd.x -= Keyboard.ADAxis * 0.1f;
            wasd.y -= Keyboard.WSAxis * 0.1f;

            // Setup matrices
            var aspectRatio = Width / (float)Height;
            RC.Projection = float4x4.CreatePerspectiveFieldOfView(3.141592f * 0.25f, aspectRatio, 0.01f, 20);
            float4x4 view = float4x4.CreateTranslation(0, 0, 8) * float4x4.CreateRotationY(_alpha) * float4x4.CreateRotationX(_beta) * float4x4.CreateTranslation(0, -2f, 0);

            FindSceneOb(_root, "leftupperleg").Rot = new float3(arrows.x, arrows.y, 0);
            FindSceneOb(_root, "leftknee").Rot = new float3(wasd.x, wasd.y, 0);
            FindSceneOb(_root, "rightshoulder").Rot = new float3(wasd.x, wasd.y, 0);
            FindSceneOb(_root, "rightellbow").Rot = new float3(arrows.x, arrows.y, 0);

            RenderSceneOb(_root, view);

            // Swap buffers: Show the contents of the backbuffer (containing the currently rendered farame) on the front buffer.
            Present();
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