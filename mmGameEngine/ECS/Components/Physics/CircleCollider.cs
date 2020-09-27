﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;
using Raylib_cs;

namespace mmGameEngine
{

    public class CircleCollider : RenderComponent
    {
		public List<Vector2> BoxPoints;
		public CircleAABB CollisionBox;
		public float Radius;
		public Vector2 Center;
		public int RadiusMultiplier;			//in case of explosions (normal is ONE)

		Rectangle boxContainer;
		Vector2 theCenter;				//origin of the texture size
		bool setScenColliders;

		/// <summary>
		/// Box to check for collisions. Box goes around the image (using scales)
		/// </summary>
		/// <param name="boxToCollide"></param>
		public CircleCollider(int _x, int _y, float _radius, int _radiusMultplier = 1)
        {
			//Rectangle _boxToCollide = new Rectangle(_x, _y, _width, _height);
			theCenter = new Vector2(_x, _y);
			Radius = _radius;
			Center = new Vector2(_x, _y);
			RadiusMultiplier = _radiusMultplier;
			//boxContainer = new Rectangle(_boxToCollide.x - theCenter.X,
			//							 _boxToCollide.y - theCenter.Y,
			//							 _boxToCollide.width,
			//							 _boxToCollide.height);


			CollisionBox = new CircleAABB();
			RenderLayer = Global.BOXCOLLIDER_LAYER;             //make sure this is drawn first
		}
        public override void Update(float deltaTime)
        {
            base.Update(deltaTime);
			//
			// Has Entity been assigned yet?
			//
			if (CompEntity == null)
				return;
			//
			// update location of box containing the collider
			//
			boxContainer.x = Transform.Position.X;
			boxContainer.y = Transform.Position.Y;
			BoxPoints = new List<Vector2>();
			Vector2 topL = new Vector2(boxContainer.x, boxContainer.y - Radius);		//north
			Vector2 topR = new Vector2(boxContainer.x - Radius,  boxContainer.y);		//west
			Vector2 botL = new Vector2(boxContainer.x + Radius, boxContainer.y);		//east
			Vector2 botR = new Vector2(boxContainer.x, boxContainer.y + Radius);		//south
			BoxPoints.Add(topL);
			BoxPoints.Add(botL);
			BoxPoints.Add(botR);
			BoxPoints.Add(topR);
			//
			// Find the min & max vectors for collision
			//
            CollisionBox.Fit(BoxPoints);				//updates the position of this collider

			if (!setScenColliders)
			{
				//
				// update the database of colliders in this scene (happens only once)
				//
				SceneColliderDatabase.SetCollider(CompEntity, CollidreShape.Circle);
				setScenColliders = true;
			}
		}
        public override void Render()
		{
            if (Global.DebugRenderEnabled)
                RenderDebug();
        }
		public void RenderDebug()
		{

			//
			// draw full rectangle
			//
			//Rectangle rt = new Rectangle(CollisionBox.min.X, CollisionBox.min.Y, boxContainer.width, boxContainer.height);
			//Raylib.DrawRectangleLines((int)rt.x, (int)rt.y,
			//					 (int)rt.width, (int)rt.height,
			//					 Color.RED);
			Raylib.DrawCircle((int)boxContainer.x, (int)boxContainer.y, Radius * RadiusMultiplier, Color.RED);
			//Raylib.DrawCircle((int)CollisionBox.min.X, (int)CollisionBox.min.X, 5, Color.GRAY);
			//Raylib.DrawCircle(Convert.ToInt32(CollisionBox.max.X), Convert.ToInt32(CollisionBox.max.Y), 5, Color.BLACK);
		}
	}
}
