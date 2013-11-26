using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Phantom.Core
{
    public class Message
#if DEBUG
        : IDisposable
#endif
    {
        private static Stack<Message> Pool = new Stack<Message>();

        public int Type;
        public object Data;

        public object Result;

        public bool Handled { get; private set; }
        public bool Consumed { get; private set; }

        /// <summary>
        /// Use: Message.Create()
        /// </summary>
        private Message()
        {
        }

#if DEBUG
        public void Dispose()
        {
            Debug.WriteLine("Warning: An object of type Message was disposed! Please use .Recycle()!");
        }
#endif

        /// <summary>
        /// Mark this message as handled.
        /// </summary>
        public void Handle()
        {
            this.Handled = true;
        }

        /// <summary>
        /// Mark this message as consumed and handled.
        /// </summary>
        public void Consume()
        {
            this.Handled = true;
            this.Consumed = true;
        }
        
        public static bool operator ==(Message self, int type)
        {
            return self.Type == type;
        }

        public static bool operator !=(Message self, int type)
        {
            return self.Type != type;
        }

        /// <summary>
        /// A quick method to test this message type and data-type.
        /// <br/>
        /// <pre>
        ///   Vector2 pos;
        ///   if( message.Is&lt;Vector2&gt;(Messages.CameraJumpTo, out pos) ) {
        ///     Debug.WriteLine("JumpTo: " + pos);
        ///   }
        /// </pre>
        /// </summary>
        /// <typeparam name="T">The type the Data must be.</typeparam>
        /// <param name="message">The message type to check.</param>
        /// <param name="data">The out var for the data cast.</param>
        /// <returns>Wether or not the message is valid.</returns>
        public bool Is<T>(int message, ref T data)
        {
            if (this.Type == message && this.Data is T)
            {
                data = (T)this.Data;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Please use Recycle when you're done with a Message object!
        /// </summary>
        public void Recycle()
        {
            Pool.Push(this);
        }

        internal static Message Create(int type, object data, object result)
        {
            Message m;
            if (Pool.Count > 0)
                m = Pool.Pop();
            else
                m = new Message();
            m.Type = type;
            m.Data = data;
            m.Result = result;
            m.Consumed = false;
            m.Handled = false;
            return m;
        }
    }
}
