﻿using System;
using WaveEngine.Framework;
using WaveEngine.Framework.Graphics;
using WaveEngine.Mathematics;

namespace BetiJaiDemo.Behaviors
{
    public class CameraTravellingBehavior : Behavior
    {
        private const int CameraSmoothTimeSeconds = 4;

        [BindComponent]        
        public Transform3D cameraTransform;
        
        private Vector3 cameraPositionCurrentVelocity;

        private Vector3 cameraRotationCurrentVelocity;

        private Vector3 cameraTargetPosition;

        private Vector3 cameraTargetRotation;

        private bool isCameraAnimationInProgress;

        public void AnimateTo(Vector3 position, Vector3 rotation)
        {
            this.cameraTargetPosition = position;
            this.cameraTargetRotation = rotation;
            this.isCameraAnimationInProgress = true;
        }

        public void FixTo(Vector3 position, Vector3 rotation)
        {
            this.cameraTransform.Position = position;
            this.cameraTransform.Rotation = rotation;
        }

        protected override void Update(TimeSpan gameTime)
        {
            if (this.isCameraAnimationInProgress)
            {
                this.cameraTransform.Position = Vector3.SmoothDamp(
                    this.cameraTransform.Position,
                    this.cameraTargetPosition,
                    ref this.cameraPositionCurrentVelocity,
                    CameraSmoothTimeSeconds,
                    (float)gameTime.TotalSeconds);

                this.cameraTransform.Rotation = Vector3.SmoothDamp(
                    this.cameraTransform.Rotation,
                    this.cameraTargetRotation,
                    ref this.cameraRotationCurrentVelocity,
                    CameraSmoothTimeSeconds,
                    (float)gameTime.TotalSeconds);

                if ((this.cameraTransform.Position == this.cameraTargetPosition) &&
                    (this.cameraTransform.Rotation == this.cameraTargetRotation))
                {
                    this.isCameraAnimationInProgress = false;
                }
            }
        }
    }
}
