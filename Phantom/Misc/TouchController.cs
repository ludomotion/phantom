using System;
using Phantom.Core;
using System.Diagnostics;
using Phantom.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Input.Touch;

#if WINDOWS
namespace Microsoft.Xna.Framework.Input.Touch
{
    public enum TouchLocationState
    {
        Invalid,    
        Moved,
        Pressed,
        Released,
    }
    public struct TouchLocation : IEquatable<TouchLocation>
    {
        /// <summary>
        ///Attributes 
        /// </summary>
        private int _id;
        private Vector2 _position;
        private Vector2 _previousPosition;
        private TouchLocationState _state;
        private TouchLocationState _previousState;

        // Only used in Android, for now
        private float _pressure;
        private float _previousPressure;

        // Used for gesture recognition.
        private Vector2 _velocity;
        private Vector2 _pressPosition;
        private TimeSpan _pressTimestamp;
        private TimeSpan _timestamp;

        /// <summary>
        /// Helper for assigning an invalid touch location.
        /// </summary>
        internal static readonly TouchLocation Invalid = new TouchLocation();

#region Properties

        internal Vector2 PressPosition
        {
            get { return _pressPosition; }
        }

        internal TimeSpan PressTimestamp
        {
            get { return _pressTimestamp; }
        }

        internal TimeSpan Timestamp
        {
            get { return _timestamp; }
        }

        internal Vector2 Velocity
        {
            get { return _velocity; }
        }

        public int Id 
        { 
            get
            {
                return _id;
            }
        }

        public Vector2 Position 
        { 
            get
            {
                return _position;
            }
        }
        
        public float Pressure 
        { 
            get
            {
                return _pressure;
            }
        }
                                
        public TouchLocationState State 
        { 
            get
            {
                return _state;
            } 
        }
        
        #endregion
        
#region Constructors

        public TouchLocation(int id, TouchLocationState state, Vector2 position)
            : this(id, state, position, TouchLocationState.Invalid, Vector2.Zero)
        {
        }

        public TouchLocation(   int id, TouchLocationState state, Vector2 position, 
                                TouchLocationState previousState, Vector2 previousPosition)
        {
            _id = id;
            _state = state;
            _position = position;
            _pressure = 0.0f;

            _previousState = previousState;
            _previousPosition = previousPosition;                
            _previousPressure = 0.0f;

            _timestamp = TimeSpan.FromTicks(DateTime.Now.Ticks);
            _velocity = Vector2.Zero;

            // If this is a pressed location then store the 
            // current position and timestamp as pressed.
            if (state == TouchLocationState.Pressed)
            {
                _pressPosition = _position;
                _pressTimestamp = _timestamp;
            }
            else
            {
                _pressPosition = Vector2.Zero;
                _pressTimestamp = TimeSpan.Zero;
            }
        }        
        
        #endregion

        /// <summary>
        /// Returns a copy of the touch with the state changed to moved.
        /// </summary>
        /// <returns>The new touch location.</returns>
        internal TouchLocation AsMovedState()
        {
            var touch = this;

            // Store the current state as the previous.
            touch._previousState = touch._state;
            touch._previousPosition = touch._position;
            touch._previousPressure = touch._pressure;

            // Set the new state.
            touch._state = TouchLocationState.Moved;
            
            return touch;
        }

        /// <summary>
        /// Updates the touch location using the new event.
        /// </summary>
        /// <param name="touchEvent">The next event for this touch location.</param>
        internal bool UpdateState(TouchLocation touchEvent)
        {
            Debug.Assert(Id == touchEvent.Id, "The touch event must have the same Id!");
            Debug.Assert(State != TouchLocationState.Released, "We shouldn't be changing state on a released location!");
            Debug.Assert(   touchEvent.State == TouchLocationState.Moved ||
                            touchEvent.State == TouchLocationState.Released, "The new touch event should be a move or a release!");
            Debug.Assert(touchEvent.Timestamp >= _timestamp, "The touch event is older than our timestamp!");

            // Store the current state as the previous one.
            _previousPosition = _position;
            _previousState = _state;
            _previousPressure = _pressure;

            // Set the new state.
            _position = touchEvent._position;
            _state = touchEvent._state;
            _pressure = touchEvent._pressure;

            // If time has elapsed then update the velocity.
            var delta = _position - _previousPosition;
            var elapsed = touchEvent.Timestamp - _timestamp;
            if (elapsed > TimeSpan.Zero)
            {
                // Use a simple low pass filter to accumulate velocity.
                var velocity = delta / (float)elapsed.TotalSeconds;
                _velocity += (velocity - _velocity) * 0.45f;
            }

            // Set the new timestamp.
            _timestamp = touchEvent.Timestamp;

            // Return true if the state actually changed.
            return _state != _previousState || delta.LengthSquared() > 0.001f;
        }

        public override bool Equals(object obj)
        {
            if (obj is TouchLocation)
                return Equals((TouchLocation)obj);

            return false;
        }

        public bool Equals(TouchLocation other)
        {
            return  _id.Equals(other._id) &&
                    _position.Equals(other._position) &&
                    _previousPosition.Equals(other._previousPosition);
        }

        public override int GetHashCode()
        {
            return _id;
        }

        public override string ToString()
        {
            return "Touch id:"+_id+" state:"+_state + " position:" + _position + " pressure:" + _pressure +" prevState:"+_previousState+" prevPosition:"+ _previousPosition + " previousPressure:" + _previousPressure;
        }

        public bool TryGetPreviousLocation(out TouchLocation aPreviousLocation)
        {
            if (_previousState == TouchLocationState.Invalid)
            {
                aPreviousLocation._id = -1;
                aPreviousLocation._state = TouchLocationState.Invalid;
                aPreviousLocation._position = Vector2.Zero;
                aPreviousLocation._previousState = TouchLocationState.Invalid;
                aPreviousLocation._previousPosition = Vector2.Zero; 
                aPreviousLocation._pressure = 0.0f;
                aPreviousLocation._previousPressure = 0.0f;
                aPreviousLocation._timestamp = TimeSpan.Zero;
                aPreviousLocation._pressPosition = Vector2.Zero;
                aPreviousLocation._pressTimestamp = TimeSpan.Zero;
                aPreviousLocation._velocity = Vector2.Zero;
                return false;
            }

            aPreviousLocation._id = _id;
            aPreviousLocation._state = _previousState;
            aPreviousLocation._position = _previousPosition;
            aPreviousLocation._previousState = TouchLocationState.Invalid;
            aPreviousLocation._previousPosition = Vector2.Zero;
            aPreviousLocation._pressure = _previousPressure;
            aPreviousLocation._previousPressure = 0.0f;
            aPreviousLocation._timestamp = _timestamp;
            aPreviousLocation._pressPosition = _pressPosition;
            aPreviousLocation._pressTimestamp = _pressTimestamp;
            aPreviousLocation._velocity = _velocity;
            return true;
        }

        public static bool operator !=(TouchLocation value1, TouchLocation value2)
        {
            return  value1._id != value2._id || 
                    value1._state != value2._state ||
                    value1._position != value2._position ||
                    value1._previousState != value2._previousState ||
                    value1._previousPosition != value2._previousPosition;
        }

        public static bool operator ==(TouchLocation value1, TouchLocation value2)
        {
            return  value1._id == value2._id && 
                    value1._state == value2._state &&
                    value1._position == value2._position &&
                    value1._previousState == value2._previousState &&
                    value1._previousPosition == value2._previousPosition;
        }

       
    }

    public class TouchCollection : List<TouchLocation>
    {
        public TouchCollection()
            :base()
        {
        }
        public TouchCollection(TouchLocation[] locations)
            :base(locations)
        {
        }
    }
}
#endif

namespace Phantom
{
	public class TouchController : Component
	{
		public TouchCollection CurrentTouchCollection {
			get {
				return this.touchCollection;
			}
		}
		public Renderer.ViewportPolicy ViewportPolicy 
		{
			get {
				return this.viewportPolicy;
			}
			set {
				this.viewportPolicy = value;
				Renderer r = new Renderer (0, viewportPolicy);
				this.renderinfo = r.BuildRenderInfo ();
				this.invertedWorld = Matrix.Invert (this.renderinfo.World);
			}
		}

		private TouchCollection touchCollection;
		private Renderer.ViewportPolicy viewportPolicy;
		private RenderInfo renderinfo;
		private Matrix invertedWorld;
		private MouseState previousMouse;
		private TouchLocationState mousePrevState;
		private int mouseID;

		public TouchController ( Renderer.ViewportPolicy viewportPolicy = Renderer.ViewportPolicy.None)
		{
			this.ViewportPolicy = viewportPolicy;
			previousMouse = Mouse.GetState ();
#if WINDOWS
            this.touchCollection = new TouchCollection();
#else
			this.touchCollection = TouchPanel.GetState ();
#endif
		}

		public override void Update (float elapsed)
		{
#if WINDOWS
            this.touchCollection = new TouchCollection();
#else
			this.touchCollection = TouchPanel.GetState ();
#endif
			MouseState mouse = Mouse.GetState ();
			Vector2 currentMousePosition = new Vector2 (mouse.X, mouse.Y);
			Vector2 previousMousePosition = new Vector2 (previousMouse.X, previousMouse.Y);
			TouchLocationState mouseState = TouchLocationState.Invalid;

			if (mouse.LeftButton == ButtonState.Pressed) {
				if ((currentMousePosition - previousMousePosition).LengthSquared () > 0) {
					mouseState = TouchLocationState.Moved;
				}
				if (previousMouse.LeftButton != ButtonState.Pressed) {
					mouseID--;
					mouseState = TouchLocationState.Pressed;
				}
			} else if(previousMouse.LeftButton == ButtonState.Pressed) {
				mouseState = TouchLocationState.Released;
			}

			TouchLocation[] result = new TouchLocation[this.touchCollection.Count + (mouseState!=TouchLocationState.Invalid?1:0)];

			for (int i = 0; i < this.touchCollection.Count; i++) {
				TouchLocation p, l = this.touchCollection [i];
				if (l.TryGetPreviousLocation (out p)) {
					result [i] = new TouchLocation (l.Id, l.State, Vector2.Transform (l.Position, this.invertedWorld), p.State, Vector2.Transform (p.Position, this.invertedWorld));
				} else {
					result [i] = new TouchLocation (l.Id, l.State, Vector2.Transform (l.Position, this.invertedWorld));
				}
			}

			if (mouseState != TouchLocationState.Invalid) {
				result [result.Length - 1] = new TouchLocation (mouseID, mouseState, Vector2.Transform (currentMousePosition, this.invertedWorld), mousePrevState, Vector2.Transform (previousMousePosition, this.invertedWorld));
			}

			this.touchCollection = new TouchCollection (result);

			mousePrevState = mouseState;
			previousMouse = mouse;
			base.Update (elapsed);
		}

		public Vector2 ConvertTouchToGame( Vector2 touch ) 
		{
			return Vector2.Transform (touch, this.invertedWorld);
		}
	}
}

