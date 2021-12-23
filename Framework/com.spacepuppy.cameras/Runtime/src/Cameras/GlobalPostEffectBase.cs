﻿using UnityEngine;
using System.Collections.Generic;

namespace com.spacepuppy.Cameras
{
    public abstract class GlobalPostEffectBase : ScriptableObject, IPostProcessingEffect
    {

        #region Fields

        [SerializeField]
        private bool _enabled = true;
        protected bool supportHDRTextures = true;
        protected bool supportDX11 = false;
        protected bool isSupported = true;

        #endregion

        #region CONSTRUCTOR
        
        protected virtual void OnEnable()
        {
            isSupported = true;
        }

        #endregion

        #region Methods

        protected Material CheckShaderAndCreateMaterial(Shader s, Material m2Create)
        {
            if (!s)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD 
                Debug.Log("Missing shader in " + ToString());
#endif
                enabled = false;
                return null;
            }

            if (s.isSupported && m2Create && m2Create.shader == s)
                return m2Create;

            if (!s.isSupported)
            {
                NotSupported();
#if UNITY_EDITOR || DEVELOPMENT_BUILD 
                Debug.Log("The shader " + s.ToString() + " on effect " + ToString() + " is not supported on this platform!");
#endif
                return null;
            }
            else
            {
                m2Create = new Material(s);
                m2Create.hideFlags = HideFlags.DontSave;
                if (m2Create)
                    return m2Create;
                else return null;
            }
        }


        protected Material CreateMaterial(Shader s, Material m2Create)
        {
            if (!s)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD 
                Debug.Log("Missing shader in " + ToString());
#endif
                return null;
            }

            if (m2Create && (m2Create.shader == s) && (s.isSupported))
                return m2Create;

            if (!s.isSupported)
            {
                return null;
            }
            else
            {
                m2Create = new Material(s);
                m2Create.hideFlags = HideFlags.DontSave;
                if (m2Create)
                    return m2Create;
                else return null;
            }
        }

        protected bool CheckSupport()
        {
            return CheckSupport(false);
        }


        public virtual bool CheckResources()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD 
            Debug.LogWarning("CheckResources () for " + ToString() + " should be overwritten.");
#endif
            return isSupported;
        }


        protected bool CheckSupport(bool needDepth)
        {
            isSupported = true;
            supportHDRTextures = SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.ARGBHalf);
            supportDX11 = SystemInfo.graphicsShaderLevel >= 50 && SystemInfo.supportsComputeShaders;

            //NOTE - as of 2019.2 this is always true
            //if (!SystemInfo.supportsImageEffects)
            //{
            //    NotSupported();
            //    return false;
            //}

            if (needDepth && !SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.Depth))
            {
                NotSupported();
                return false;
            }

            //if (needDepth)
            //    GetComponent<Camera>().depthTextureMode |= DepthTextureMode.Depth;

            return true;
        }

        protected bool CheckSupport(bool needDepth, bool needHdr)
        {
            if (!CheckSupport(needDepth))
                return false;

            if (needHdr && !supportHDRTextures)
            {
                NotSupported();
                return false;
            }

            return true;
        }


        public bool Dx11Support()
        {
            return supportDX11;
        }


        protected void ReportAutoDisable()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD 
            Debug.LogWarning("The image effect " + ToString() + " has been disabled as it's not supported on the current platform.");
#endif
        }

        // deprecated but needed for old effects to survive upgrading
        bool CheckShader(Shader s)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD 
            Debug.Log("The shader " + s.ToString() + " on effect " + ToString() + " is not part of the Unity 3.2+ effects suite anymore. For best performance and quality, please ensure you are using the latest Standard Assets Image Effects (Pro only) package.");
#endif
            if (!s.isSupported)
            {
                NotSupported();
                return false;
            }
            else
            {
                return false;
            }
        }


        protected void NotSupported()
        {
            enabled = false;
            isSupported = false;
            return;
        }


        protected void DrawBorder(RenderTexture dest, Material material)
        {
            float x1;
            float x2;
            float y1;
            float y2;

            RenderTexture.active = dest;
            bool invertY = true; // source.texelSize.y < 0.0ff;
            // Set up the simple Matrix
            GL.PushMatrix();
            GL.LoadOrtho();

            for (int i = 0; i < material.passCount; i++)
            {
                material.SetPass(i);

                float y1_; float y2_;
                if (invertY)
                {
                    y1_ = 1.0f; y2_ = 0.0f;
                }
                else
                {
                    y1_ = 0.0f; y2_ = 1.0f;
                }

                // left
                x1 = 0.0f;
                x2 = 0.0f + 1.0f / (dest.width * 1.0f);
                y1 = 0.0f;
                y2 = 1.0f;
                GL.Begin(GL.QUADS);

                GL.TexCoord2(0.0f, y1_); GL.Vertex3(x1, y1, 0.1f);
                GL.TexCoord2(1.0f, y1_); GL.Vertex3(x2, y1, 0.1f);
                GL.TexCoord2(1.0f, y2_); GL.Vertex3(x2, y2, 0.1f);
                GL.TexCoord2(0.0f, y2_); GL.Vertex3(x1, y2, 0.1f);

                // right
                x1 = 1.0f - 1.0f / (dest.width * 1.0f);
                x2 = 1.0f;
                y1 = 0.0f;
                y2 = 1.0f;

                GL.TexCoord2(0.0f, y1_); GL.Vertex3(x1, y1, 0.1f);
                GL.TexCoord2(1.0f, y1_); GL.Vertex3(x2, y1, 0.1f);
                GL.TexCoord2(1.0f, y2_); GL.Vertex3(x2, y2, 0.1f);
                GL.TexCoord2(0.0f, y2_); GL.Vertex3(x1, y2, 0.1f);

                // top
                x1 = 0.0f;
                x2 = 1.0f;
                y1 = 0.0f;
                y2 = 0.0f + 1.0f / (dest.height * 1.0f);

                GL.TexCoord2(0.0f, y1_); GL.Vertex3(x1, y1, 0.1f);
                GL.TexCoord2(1.0f, y1_); GL.Vertex3(x2, y1, 0.1f);
                GL.TexCoord2(1.0f, y2_); GL.Vertex3(x2, y2, 0.1f);
                GL.TexCoord2(0.0f, y2_); GL.Vertex3(x1, y2, 0.1f);

                // bottom
                x1 = 0.0f;
                x2 = 1.0f;
                y1 = 1.0f - 1.0f / (dest.height * 1.0f);
                y2 = 1.0f;

                GL.TexCoord2(0.0f, y1_); GL.Vertex3(x1, y1, 0.1f);
                GL.TexCoord2(1.0f, y1_); GL.Vertex3(x2, y1, 0.1f);
                GL.TexCoord2(1.0f, y2_); GL.Vertex3(x2, y2, 0.1f);
                GL.TexCoord2(0.0f, y2_); GL.Vertex3(x1, y2, 0.1f);

                GL.End();
            }

            GL.PopMatrix();
        }

        #endregion

        #region IPostProcessingEffect Interface

        public bool enabled
        {
            get { return _enabled; }
            set { _enabled = value; }
        }

        public abstract void RenderImage(ICamera camera, RenderTexture source, RenderTexture destination);

        #endregion

    }
}