﻿using PlumeAPI.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using PlumeAPI.Entities;
using PlumeAPI.Entities.Components;

namespace PlumeAPI.World {
	public static class Camera {
		public static float X { get; set; }
		public static float Y { get; set; }
		public static float Scale { get; set; }
		public static float XGoal { get; set; }
		public static float YGoal { get; set; }
		public static float ScaleGoal { get; set; }
		public static float Speed { get; set; }

		private static float MaxSpeed = 7.5f;

		public static bool UseEasing = true;

		public static BaseEntity EntityToFollow = null;
		public static Rectangle Boundry = new Rectangle(0, 0, 99999, 99999);
		public static bool FreeCamera = false;

	public static void Initialize() {
			X = 0;
			Y = 0;
			Scale = 1.0f;
			XGoal = X;
			YGoal = Y;
			ScaleGoal = 1.0f;
			Speed = 0.1f;
		}	

		public static void SetGoal(float x, float y) {
			XGoal = x;
			YGoal = y;
		}

		public static void SetGoal(float x, float y, float scale) {
			XGoal = x;
			YGoal = y;
			ScaleGoal = scale;
		}

		public static void SetGoal(Vector2 position) {
			XGoal = position.X;
			YGoal = position.Y;
		}

		public static void ForcePosition(float x, float y) {
			X = x;
			Y = y;
			XGoal = X;
			YGoal = Y;
		}

		public static void ForcePosition(Vector2 position) {
			X = position.X;
			Y = position.Y;
			XGoal = X;
			YGoal = Y;
		}

		public static Rectangle GetBounds() {
			GraphicsDevice graphicsDevice = GameServices.GetService<GraphicsDevice>();
			return new Rectangle((int)X, (int)Y, (int)(graphicsDevice.Viewport.Width * (1 / Scale)), (int)(graphicsDevice.Viewport.Height * (1 / Scale)));
		}

		public static bool IsOnCamera(Rectangle boundingBox) {
			return GetBounds().Intersects(boundingBox);
		}

		public static Matrix GetTransformationMatrix() {
			return Matrix.CreateTranslation(-X, -Y, 0) * Matrix.CreateScale(Scale, Scale, 1.0f);
		}

		public static Vector2 ConvertViewportToCamera(Vector2 input) {
			return Vector2.Transform(input, Matrix.Invert(GetTransformationMatrix()));
		}
		public static Vector2 ConvertCameraToViewport(Vector2 input) {
			return Vector2.Transform(input, GetTransformationMatrix());
		}

		public static void AttachToEntity(BaseEntity entity) {
			EntityToFollow = entity;
		}

		public static void SetBoundry(Rectangle boundry) {
			Boundry = boundry;
		}

		public static void ToggleFreeCamera(bool toggle) {
			FreeCamera = toggle;
		}

		public static void Update() {
			Viewport viewport = GameServices.GetService<GraphicsDevice>().Viewport;

			if(!FreeCamera) {
				if(EntityToFollow != null) {
					Vector2 position = EntityToFollow.GetComponent<PositionalComponent>().Position;
					XGoal = position.X - viewport.Width / 2;
					YGoal = position.Y - viewport.Height / 2;
				}

				if(Boundry != null) {
					if(XGoal < Boundry.X) XGoal = Boundry.X;
					if(YGoal < Boundry.Y) YGoal = Boundry.Y;
					if(XGoal > Boundry.X + Boundry.Width - viewport.Width) XGoal = Boundry.X + Boundry.Width - viewport.Width;
					if(YGoal > Boundry.Y + Boundry.Height - viewport.Height) YGoal = Boundry.Y + Boundry.Height - viewport.Height;
				}
			}

			if(UseEasing) {
				//Trend towards our position
				float dX = (XGoal - X) / (1 / Speed);
				float dY = (YGoal - Y) / (1 / Speed);
				if(Math.Abs(dX) > MaxSpeed) {
					dX = dX < 0 ? -1 * MaxSpeed : MaxSpeed;
				}

				if(Math.Abs(dY) > MaxSpeed) {
					dY = dY < 0 ? -1 * MaxSpeed : MaxSpeed;
				}

				X += (int) dX;
				if(Math.Abs(dX) < 0.01) X = XGoal;

				Y += (int) dY;
				if(Math.Abs(dY) < 0.01) Y = YGoal;

				float dScale = (ScaleGoal - Scale) / (1 / Speed);
				Scale += dScale;
				if(Math.Abs(dScale) < 0.1) Scale = ScaleGoal;
			} else {
				X = XGoal;
				Y = YGoal;
				Scale = ScaleGoal;
			}
		}
	}
}
