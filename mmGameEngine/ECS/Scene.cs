﻿using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Raylib_cs;

using Entitas;

using System.Data;
using System.Dynamic;
using System.Numerics;
using System.Xml.Linq;


namespace mmGameEngine
{
    /*
	 * Main class for every game screen display.  This class :
	 *		Creates entities	(this was moved to Global static 03/01/2023)
	 *		Holds all entities
	 *			Game entities - all objects to play with
	 *			Scene entities - all objects drawn on top of all other objects (mostly UI entities)
	 *		Displays entities
	 *			sorts entities by game then render
	 *						   by scene then render
	 */
    public class Scene 
	{
		//
		// ECS used for Entities (Game & Scene entities)
		//
		//Entitas.Context EntityContext;
	    Entitas.Systems EntitySystems;
		List<Entity> GameEntities;
		List<Entity> SceneEntities;

		public ContentManager ContentManager {  get; set; }	
		public bool ForceEndScene = false;
		public GameState StateOfGame;
		//
		// 3D Camera
		//
		public Camera3D Camera3D;
		public CameraMode CameraMode;

        public bool Camera3dEnabled = false;
        //
        // 2D Camera
        //
        public Camera2D Camera;
        public bool Camera2dEnabled = false;
        public Camera2DType CameraType2D;							//free, inside a map, push bounds
		public Entity CameraEntityToFollow;							//which entity to follow

		float deltaTime = 0;
		/// <summary>
		/// Create a new Window, Reset all GameEntities
		/// </summary>
		/// <param name="winClearColor"></param>
		/// <param name="winWidth"></param>
		/// <param name="winHeight"></param>
		/// <param name="winTitle"></param>
		public Scene()
		{
            Global.DebugRenderEnabled = false;
            //
            // start ECS (Game entities & components & systems)
            //
            //EntityContext = Entitas.Contexts.Default;
            Global.EntityContext = Entitas.Contexts.Default;
            //
            // List of all entities, List of entities destroyed
            //
            GameEntities = new List<Entity>();
            Global.GameEntityToDestroy = new Dictionary<Entity, bool>();
            SceneEntities = new List<Entity>();
            Global.SceneEntityToDestroy = new Dictionary<Entity, bool>();

            //znznznznznznznznznznznznznznznznznzn
            // Camera 2D setup
            //znznznznznznznznznznznznznznznznznzn

            Camera = new Camera2D();
            Camera.Target = Global.WindowCenter;
            Camera.Offset = Global.WindowCenter;
            Camera.Rotation = 0;
            Camera.Zoom = 1.0f;
            Camera2dEnabled = false;
            CameraType2D = Camera2DType.FollowPlayer;                   //free camera no bounds
            //
			// new Content manager 06/06/2024
            //
            ContentManager = new ContentManager();
			ContentManager.BaseContnetFolder = "Assets";				//can be overridden
        }
		/// <summary>
		/// override this in Scene subclasses and do your loading here. This is called from the contructor after the scene sets itself up but
		/// before begin is ever called.
		/// </summary>
		public virtual void Initialize()
		{
			/*
			 * use this to initialiaze your particular scene
			 */
		}
		/// <summary>
		/// override this in Scene subclasses. this will be called when Core sets this scene as the active scene.
		/// </summary>
		public virtual void OnStart()
		{
			/*
			 * Each scene will set the size of the screen override in its construct.
			 * This is left as a compatibility.  You can still use it for initializations etc.
			 */
		}
		/// <summary>
		/// Add processor systems for ECS (they are run during update cycle)
		/// </summary>
		/// <param name="_system"></param>
        public void AddSystem(Entitas.ISystem _system)
		{
			EntitySystems.Add(_system);
		}
        //ZNZNZNZNZNZNZNZNZNZNZNZNZNZNZNZNZNZNZNZNZNZNZNZNZNZNZNZNZNZNZNZNZNZNZNZNZNZNZNZN
        //               Main method executing ALL logic in Scene
        //ZNZNZNZNZNZNZNZNZNZNZNZNZNZNZNZNZNZNZNZNZNZNZNZNZNZNZNZNZNZNZNZNZNZNZNZNZNZNZNZN
        /// <summary>
        /// You MUST override Play() in your Scene classes and do your loading & other logic. 
        /// This is called from mmGame after the scene construct, Begin(), then Play()
        /// </summary>
        public virtual void Play()
		{
			/*
			 * Game will override this to load assets, setup entities
			 * Components & Systems should be setup separately
			 */
		}
		public void SetWindowTitle(string title)
		{
            Raylib.SetWindowTitle(title);
        }
        //ZNZNZNZNZNZNZNZNZNZNZNZNZNZNZNZNZNZNZNZNZNZNZNZNZNZNZNZNZNZNZNZNZNZNZNZNZNZNZNZN
        //               Update components in Scene and execute systems
        //ZNZNZNZNZNZNZNZNZNZNZNZNZNZNZNZNZNZNZNZNZNZNZNZNZNZNZNZNZNZNZNZNZNZNZNZNZNZNZNZN
        public virtual void Update()
		{
			//-------------------
			// Z O O M 
			//-------------------
			if ( Camera2dEnabled)
			{
				//
				// Camera zoom control is the Mouse Wheel
				//
				Camera.Zoom += ((float)Raylib.GetMouseWheelMove() * 0.05f);

				if (Camera.Zoom > 3.0f) Camera.Zoom = 3.0f;
				else if (Camera.Zoom < 0.1f) Camera.Zoom = 0.1f;
	
				if (Raylib.IsKeyPressed(KeyboardKey.R))			// Camera reset (zoom and rotation)
				{
					Camera.Zoom = 1.0f;
					Camera.Rotation = 0.0f;
				}
			}
			//
			// Camera3D MUST be created in your scene
			//
			if (Camera3dEnabled)
				Raylib.UpdateCamera(ref Camera3D, CameraMode);

            deltaTime = Raylib.GetFrameTime();
			Global.DeltaTime = deltaTime;
			//
			// Find all Entities (Game & UI)
			//
			GameEntities = Global.EntityContext.GetEntities().ToList();

			//-----------------------------------------------------------------------
			// Update game components (holding data) attached to entities
			//-----------------------------------------------------------------------
			Entity ent;
			int gMax = GameEntities.Count;
			for (int i = 0; i < gMax;i++) 
			{
				ent = GameEntities[i];
				if (!ent.IsEnabled)
					continue;

				Entitas.IComponent[] allComp = ent.GetComponents();         //all entity components
				foreach (Entitas.IComponent comp in allComp)
				{
					{
						Component myComp = (Component)comp;
						myComp.OwnerEntity = ent;							//attach entity to component
						myComp.Update(deltaTime);
					}
				}
			}
            //-----------------------------------------------------------------------
            // Execute systems. All logic using components data
            //-----------------------------------------------------------------------
            EntitySystems.Execute();					//run IExecuteSystems
			//
			// If user wants out of this scene
			//
			if (ForceEndScene)
				Global.StateOfGame = GameState.ForcedExit;
		}
		public virtual void LateUpdate()
		{ }
		public void RemoveDeletedEntities()
        {
			//
			// Remove game entities, all children are included
			//      
			if (Global.GameEntityToDestroy.Count > 0)
			{
				foreach (KeyValuePair<Entity, bool> ent in Global.GameEntityToDestroy)
				{
					ent.Key.RemoveAllComponents();
					ent.Key.Destroy();					//release entity automatically

					//GameEntities.Remove(ent.Key);
				}
				Global.GameEntityToDestroy = new Dictionary<Entity, bool>();

			}
			//
			// Remove Scene specific entities (UI?)
			//
			if (Global.SceneEntityToDestroy.Count > 0)
			{
				foreach (KeyValuePair<Entity, bool> ent in Global.SceneEntityToDestroy)
				{
					ent.Key.RemoveAllComponents();
					ent.Key.Destroy();

					//GameEntities.Remove(ent.Key);
				}
				Global.SceneEntityToDestroy = new Dictionary<Entity, bool>();

			}
			//
			// Do ECS update/cleanup
			//
			EntitySystems.Cleanup();
		}
		/// <summary>
		/// Display all Entities that are Visible using thier RenderLayer order
		/// </summary>
		public virtual void Render()
		{
			//-------------------------------------------------------------------------------
			// Get all RenderComponent, sort them, low -> high
			//-------------------------------------------------------------------------------
			List<RenderComponent> ComponentsToRender = new List<RenderComponent>();
			//
			// Find all Entities (in case some were removed/added)
			//
			GameEntities = Global.EntityContext.GetEntities().Where(e => e.EntityType == 0).ToList();
			SceneEntities = Global.EntityContext.GetEntities().Where(e => e.EntityType == 1).ToList();
            //
            // Add all RenderComponent in a list to be displayed
			//
            int maxEnt = GameEntities.Count;
			for (int i = 0; i < maxEnt; i++)
			{
				Entity entity = GameEntities[i];
                if (!(entity.Get<TransformComponent>().Enabled && entity.Get<TransformComponent>().Visiable))
                    continue;

                //
                // Ask Entitas for all components attached to "ent" entity
                //
                Entitas.IComponent[] allComp = entity.GetComponents();       //this entity's component
                foreach (Entitas.IComponent comp in allComp)              //get the renderable ones
                {
                    if (comp is IRenderable)
                    {
                        RenderComponent myComp = (RenderComponent)comp;
                        ComponentsToRender.Add(myComp);
                    }
                }
            }

			//-------------------------------------------------------------------------------
			//   CAMERA DISPLAY  BeginMode2D
			//-------------------------------------------------------------------------------

			if (Camera2dEnabled && CameraEntityToFollow != null)
			{
				//    need to get the entity that is target of camera -> target Get<TransformComponent>().Position
				//    Any component that is in view of the camera, is drawn first

				switch(CameraType2D)
                {
					case Camera2DType.FollowPlayer:
						UpdateCameraCenter();
						break;
					case Camera2DType.FollowInsideMap:
						UpdateCameraInsideMap();
						break;
					case Camera2DType.FollowCenterSmooth:
						UpdateCameraCenterSmoothFollow();
						break;
				}
				Raylib.BeginMode2D(Camera);
			}
			//
			// Must turn on 3D camera so you can see things!
			//
			if (Camera3dEnabled)
				Raylib.BeginMode3D(Camera3D);

			//-------------------------------------------------------------------------------
			//   RENDER ORDER  sorting (low to high) then render
			//-------------------------------------------------------------------------------
			foreach (RenderComponent myComp in ComponentsToRender.OrderBy(e => e.RenderLayer))
			{
				myComp.Render();									//call draw method
			}
			//-------------------------------------------------------------------------------
			//   CAMERA DISPLAY EndMode2D
			//-------------------------------------------------------------------------------
			if (Camera2dEnabled && CameraEntityToFollow != null)
			{
				if (Global.DebugRenderEnabled)
				{ 
					//
					// display camera position
					//
					int screenHeight = Global.SceneHeight;
					int screenWidth = Global.SceneWidth;

                    int tx = (int)CameraEntityToFollow.Get<TransformComponent>().Position.X;
                    int ty = (int)CameraEntityToFollow.Get<TransformComponent>().Position.Y;

                    Raylib.DrawLine(tx, -screenHeight * 10, tx, screenHeight * 10, Color.Green);    //Verticval
					Raylib.DrawLine(-screenWidth * 10, ty, screenWidth * 10, ty, Color.Green);      //Horizontal


				}
				Raylib.EndMode2D();
			}
			//
			// Back to normal, so scene entities can be drawn on top of everything
			//
            if (Camera3dEnabled)
                Raylib.EndMode3D();
            //-------------------------------------------------------------------------------
            //   UI  ENTITIES , they are drawn on top of all other game entities
            //-------------------------------------------------------------------------------
            foreach (Entity ent in SceneEntities)
			{
				if (!ent.Get<TransformComponent>().Enabled)
					continue;

				if (!ent.Get<TransformComponent>().Visiable)
					continue;

				Entitas.IComponent[] allComp = ent.GetComponents();         //this entity's component
				foreach (Entitas.IComponent comp in allComp)
				{
					if (comp is IRenderable)
					{
						RenderComponent myComp = (RenderComponent)comp;
						myComp.Render();                                    //call draw method
					}
				}
			}

			//-----------------
			// Scene debug
			//-----------------
			if (Global.DebugRenderEnabled)
			{
				string istr = Raylib.GetMousePosition().ToString();
				Raylib.DrawText(istr, 10, 10, 20, Color.White);
				
				if (Camera2dEnabled && CameraEntityToFollow != null)
                {
					Raylib.DrawFPS(10, 30);
					Raylib.DrawText("Zoom" + Camera.Zoom.ToString(), 10, 50, 20, Color.White);
				}
				else
					Raylib.DrawFPS(10, 30);
			}


		}
		private void UpdateCameraCenterSmoothFollow()
		{
			//
			// Not a good effect
			//
			float delta = Raylib.GetFrameTime();
			int width = Global.SceneWidth;
			int height = Global.SceneHeight;
			//float minSpeed = 30;
			float minEffectLength = 10;
			//float fractionSpeed = 0.8f;
			float speed = 4.0f;

			Camera.Offset = new Vector2(width / 2, height / 2);
			Vector2 diff = Vector2.Subtract(CameraEntityToFollow.Get<TransformComponent>().Position, Camera.Target);
			float length = Vector2Ext.Length(diff);

			if (length > minEffectLength)
			{
				//float speed = fmaxf(fractionSpeed * length, minSpeed);
				Camera.Target = Vector2.Add(Camera.Target, Vector2Ext.Scale(diff, speed * delta / length));
			}
		}
		private void UpdateCameraCenter()
        {
			//
			// Keep player in center of screen, no limits on what to display
			//
			int width = Global.SceneWidth;
			int height = Global.SceneHeight;
			Camera.Target = CameraEntityToFollow.Get<TransformComponent>().Position;
			Camera.Offset = new Vector2(width / 2, height / 2);
		}
		private void UpdateCameraInsideMap()
        {
			//
			// Follow the player, display screen is limited to width & height set
			//
			int width = Global.SceneWidth;
			int height = Global.SceneHeight;

			Camera.Target = CameraEntityToFollow.Get<TransformComponent>().Position;
			Camera.Offset = new Vector2(width / 2, height/ 2);

			float minX = 0;
			float minY = 0;
			float maxX = Global.WorldWidth;
			float maxY = Global.WorldHeight;

			Vector2 max = Raylib.GetWorldToScreen2D(new Vector2(maxX, maxY), Camera);
			Vector2 min = Raylib.GetWorldToScreen2D(new Vector2(minX, minY), Camera);
			//
			// camera ends at width of world/map
			//
			if (max.X < width) Camera.Offset.X = width - (max.X - width / 2);
			if (max.X < height) Camera.Offset.X = height - (max.X - height / 2);
			if (min.X > 0) Camera.Offset.X = width / 2 - min.X;
			if (min.X > 0) Camera.Offset.X = height / 2 - min.X;
			//
			// cameran ends at height of world/map
			//
			if (max.Y < width) Camera.Offset.Y = width - (max.Y - width / 2);
			if (max.Y < height) Camera.Offset.Y = height - (max.Y - height / 2);
			if (min.Y > 0) Camera.Offset.Y = width / 2 - min.Y;
			if (min.Y > 0) Camera.Offset.Y = height / 2 - min.Y;
		}
		private void UpdateCameraPlayerBoundsPush()
		{
			Vector2 position = CameraEntityToFollow.Get<TransformComponent>().Position; 
			int width = Global.SceneWidth;
			int height = Global.SceneHeight;
			Vector2 bbox = new Vector2(0.2f, 0.2f);

			Vector2 bboxWorldMin = Raylib.GetScreenToWorld2D(new Vector2((1 - bbox.X) * 0.5f * width, (1 - bbox.X) * 0.5f * height), Camera);
			Vector2 bboxWorldMax = Raylib.GetScreenToWorld2D(new Vector2((1 + bbox.X) * 0.5f * width, (1 + bbox.X) * 0.5f * height), Camera);
			Camera.Offset = new Vector2((1 - bbox.X) * 0.5f * width, (1 - bbox.X) * 0.5f * height);

			if (position.X < bboxWorldMin.X) Camera.Target.X = position.X;
			if (position.X < bboxWorldMin.X) Camera.Target.X = position.X;
			if (position.X > bboxWorldMax.X) Camera.Target.X = bboxWorldMin.X + (position.X - bboxWorldMax.X);
			if (position.X > bboxWorldMax.X) Camera.Target.X = bboxWorldMin.X + (position.X - bboxWorldMax.X);
		}
        internal void Begin()
        {
            //
            // mmGame calls Begin() before a new scene window is created
            //
            SceneColliderManager.Initialize();
            //
            // Allow Entitas to init systems, auto collect matched systems, no manual Systems.Add(ISystem) required
            //
            //EntitySystems = new Entitas.Feature(null);               //allows all systems to be registered automatically
            EntitySystems = new Entitas.Systems();						//must add each system to the scene manually
            EntitySystems.Initialize();

            OnStart();
        }
        internal void End()
        {
            //
            // mmGame calls End() when scene changes, before new scene is created
            //
            EntitySystems.TearDown();
            EntitySystems.ClearReactiveSystems();
            Global.EntityContext.DestroyAllEntities();
        }
    }
}
