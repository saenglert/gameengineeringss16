using System;
using System.Collections.Generic;
using Fusee.Base.Common;
using Fusee.Base.Core;
using Fusee.Engine.Common;
using Fusee.Engine.Core;
using Fusee.Math.Core;
using Fusee.Serialization;
using Fusee.Xene;

namespace Fusee.Tutorial.Core
{
    class Renderer : SceneVisitor
    {

        public RenderContext RC;
        public float4x4 View;

        private Dictionary<string, ITexture> textures;
        private Dictionary<string, ShaderEffect> shaderEffects;
        private Dictionary<MeshComponent, Mesh> _meshes = new Dictionary<MeshComponent, Mesh>();

        private CollapsingStateStack<float4x4> _model = new CollapsingStateStack<float4x4>();

        public Renderer(RenderContext rc, String[] textureStrings)
        {
            RC = rc;

            // Initalize textures
            textures = new Dictionary<string, ITexture>();
            this.LoadTextures(textureStrings);

            // Initialize the shader(s)
            shaderEffects = new Dictionary<string, ShaderEffect>();

        }

        private void LoadTextures(String[] names)
        {
            foreach (var name in names)
            {
                textures.Add(name, RC.CreateTexture(AssetStorage.Get<ImageData>(name)));
            }
        }

        public bool AddTexture(String name)
        {
            if (!textures.ContainsKey(name))
            {
                textures.Add(name, RC.CreateTexture(AssetStorage.Get<ImageData>(name)));
                return true;
            }

            return false;
        }

        public ITexture GetTexture(String texture)
        {
            return textures[texture];
        }

        public Dictionary<String, ITexture> GetTextures()
        {
            return textures;
        }

        public bool AddShader(String name, ShaderEffect effect)
        {
            if (!shaderEffects.ContainsKey(name))
            {
                shaderEffects.Add(name, effect);
                shaderEffects[name].AttachToContext(RC);
                return true;
            }

            return false;
        }

        private Mesh LookupMesh(MeshComponent mc)
        {
            Mesh mesh;
            if (!_meshes.TryGetValue(mc, out mesh))
            {
                mesh = new Mesh
                {
                    Vertices = mc.Vertices,
                    Normals = mc.Normals,
                    UVs = mc.UVs,
                    Triangles = mc.Triangles,
                };
                _meshes[mc] = mesh;
            }
            return mesh;
        }

        protected override void InitState()
        {
            _model.Clear();
            _model.Tos = float4x4.Identity;
        }
        protected override void PushState()
        {
            _model.Push();
        }
        protected override void PopState()
        {
            _model.Pop();
            RC.ModelView = View * _model.Tos;
        }
        [VisitMethod]
        void OnMesh(MeshComponent mesh)
        {
            shaderEffects["standard"].RenderMesh(LookupMesh(mesh));
            // RC.Render(LookupMesh(mesh));
        }
        [VisitMethod]
        void OnMaterial(MaterialComponent material)
        {
            String shaderName;

            if (shaderEffects.ContainsKey(CurrentNode.Name))
                shaderName = CurrentNode.Name;
            else shaderName = "standard";

            if (material.HasDiffuse)
            {
                shaderEffects[shaderName].SetEffectParam("albedo", material.Diffuse.Color);
                if (material.Diffuse.Texture != null)
                {
                    shaderEffects[shaderName].SetEffectParam("texture", textures[material.Diffuse.Texture]);
                    shaderEffects[shaderName].SetEffectParam("texmix", 1.0f);
                }
                else
                {
                    shaderEffects[shaderName].SetEffectParam("texmix", 0.0f);
                }
            }
            else
            {
                shaderEffects[shaderName].SetEffectParam("albedo", float3.Zero);
            }
            if (material.HasSpecular)
            {
                shaderEffects[shaderName].SetEffectParam("shininess", material.Specular.Shininess);
                shaderEffects[shaderName].SetEffectParam("specfactor", material.Specular.Intensity);
                shaderEffects[shaderName].SetEffectParam("speccolor", material.Specular.Color);
            }
            else
            {
                shaderEffects[shaderName].SetEffectParam("shininess", 0);
                shaderEffects[shaderName].SetEffectParam("specfactor", 0);
                shaderEffects[shaderName].SetEffectParam("speccolor", float3.Zero);
            }
            if (material.HasEmissive)
            {
                shaderEffects[shaderName].SetEffectParam("ambientcolor", material.Emissive.Color);
            }
            else
            {
                shaderEffects[shaderName].SetEffectParam("ambientcolor", float3.Zero);
            }
        }
        [VisitMethod]
        void OnTransform(TransformComponent xform)
        {
            _model.Tos *= xform.Matrix();
            RC.ModelView = View * _model.Tos;
        }
    }
}