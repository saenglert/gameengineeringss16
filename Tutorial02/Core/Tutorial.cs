using System;
using System.Diagnostics;
using Fusee.Base.Common;
using Fusee.Base.Core;
using Fusee.Engine.Common;
using Fusee.Engine.Core;
using Fusee.Engine.Core.GUI;
using Fusee.Math.Core;
using Fusee.Serialization;
using static Fusee.Engine.Core.Input;


namespace Fusee.Tutorial.Core
{

    [FuseeApplication(Name = "Tutorial Example", Description = "The official FUSEE Tutorial.")]
    public class Tutorial : RenderCanvas
    {
        private Mesh _mesh;

        private IShaderParam _alphaParam;
        private float _alpha;

        private IShaderParam _mouseParam;
        private float2 _mousePosition ;

        private const string VertexShader = @"
        attribute vec3 fuVertex;
        uniform float alpha;
        varying vec3 modelpos;
        varying vec3 endpos;

        void main()
        {
            modelpos = fuVertex;
            float s = sin(alpha);
            float c = cos(alpha);

            mat3 rotation = mat3(c, 0, s,
                                 0, 1, 0,
                                 -s, 0, c);

            gl_Position = vec4( rotation * modelpos, 1.0);
            endpos = gl_Position;
        }";

        private const string PixelShader = @"
            #ifdef GL_ES
                precision highp float;
            #endif
            varying vec3 modelpos;
            varying vec3 endpos;
            uniform vec2 mousepos;

            void main()
            {
                float dist = distance(mousepos, endpos.xy);
                gl_FragColor = vec4((modelpos * 0.5 + 0.5) * clamp(dist, 0, 1), 1 );
            }";


        // Init is called on startup. 
        public override void Init()
        {
            _mesh = new Mesh
            {
                Vertices = new[]
        {
            new float3(-0.5f, -0.5f, -0.5f ),
            new float3(0.5f, -0.5f, -0.5f ),
            new float3(0.5f , 0.5f , -0.5f),
            new float3(-0.5f, 0.5f, -0.5f),

            new float3(0.5f, -0.5f, 0.5f),
            new float3(-0.5f, -0.5f, 0.5f),
            new float3(-0.5f, 0.5f, 0.5f),
            new float3(0.5f, 0.5f, 0.5f),
                
            new float3(0 , 1 , 0), 
        },
                Triangles = new ushort[]
        {
            0, 1, 2, // Front 1
            0, 2, 3, // Front 2
            1, 4, 7, // Right 1
            1, 7, 2, // Right 2
            4, 5, 6, // Back 1
            4, 6, 7, // Back 2
            5, 0, 3, // Left 1
            5, 3, 6, // Left 2
            3, 2, 8, // Top Front
            2, 7, 8, // Top Right
            7, 6, 8, // Top Back
            6, 3, 8, // Top Left
        },
            };

            var shader = RC.CreateShader(VertexShader, PixelShader);
            RC.SetShader(shader);
            _mouseParam = RC.GetShaderParam(shader, "mousepos");
            _mousePosition = new float2(0,0);
            _alphaParam = RC.GetShaderParam(shader, "alpha");
            _alpha = 0;
     
            // Set the clear color for the backbuffer.
            RC.ClearColor = new float4(0.1f, 0.3f, 0.2f, 1);
        }

        // RenderAFrame is called once a frame
        public override void RenderAFrame()
        {
            // Clear the backbuffer
            RC.Clear(ClearFlags.Color | ClearFlags.Depth);

            float2 mouseSpeed = Mouse.Velocity;
            float arrowSpeed = Keyboard.LeftRightAxis;
            float adSpeed = Keyboard.ADAxis;

            if (Mouse.LeftButton)
            {
                _alpha += mouseSpeed.x * 0.0001f;
            }
            else
            {
                
                if (Keyboard.IsKeyDown(KeyCodes.Left) || Keyboard.IsKeyDown(KeyCodes.Right)) _alpha += arrowSpeed * 0.01f;
                if (Keyboard.IsKeyDown(KeyCodes.A) || Keyboard.IsKeyDown(KeyCodes.D)) _alpha += adSpeed * 0.01f;
            }
            
            _mousePosition = Mouse.Position;
            Debug.WriteLine(Mouse.Position);

            RC.SetShaderParam(_mouseParam, _mousePosition);
            RC.SetShaderParam(_alphaParam, _alpha);

            RC.Render(_mesh);

            // Swap buffers: Show the contents of the backbuffer (containing the currently rerndered farame) on the front buffer.
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
            var projection = float4x4.CreatePerspectiveFieldOfView(MathHelper.PiOver4, aspectRatio, 1, 20000);
            RC.Projection = projection;
        }

    }
}