﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;
using Raylib_cs;
using Entitas;

namespace mmGameEngine
{
    public delegate void ClickEventHandler(object obj);
    /*
	 * Every Renderable component inherits from this to give us the ability
	 * to ask Entitas for a collection of components that need to render that we can iterate and
	 * invoke Update() and Render() methods in our Entity class
	 */
    public class RenderComponent : Component, IRenderable 
	{
        public event ClickEventHandler Click;
        //public int Tag;

        //public bool Enabled = true;                 //is the component enabled (default true)
        //public Vector2 CompPosition;                //This is used by UI components
        //public Entity CompEntity;					//This is updated in Scene loop
        //public Transform Transform;                 //Transform component of entity
        //public bool HasBoxCollider = false;         //if this is suppose to collide with anything
        public int RenderLayer;                     //render from low to high
        /// <summary>
        /// sprite image
        /// </summary>
        public Texture2D Texture;
        /// <summary>
        /// center of the sourceRect if it had a 0,0 origin. This is basically the center in sourceRect-space.
        /// </summary>
        /// <value>The center.</value>
        public Vector2 TextureCenter;
        /// <summary>
        /// the origin that a RenderableComponent should use when using this Sprite. Defaults to the center.
        /// </summary>
        public Vector2 Origin;

        public RenderComponent()
        {
            Tag = 0;
            Enabled = true;
            //HasBoxCollider = false;
        }
        //public override void Update(float deltaTime)
        //{
        //    if (CompEntity != null)
        //        Transform = CompEntity.Get<Transform>();
        //    else
        //        Transform = new Transform();
        //}
        public virtual void Render()
        { }

        public void OnClick(object obj) { Click?.Invoke(obj); }
    }
}