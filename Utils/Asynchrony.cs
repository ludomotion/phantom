using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Phantom.Core;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Threading;

namespace Phantom.Utils
{
    /// <summary>
    /// Add-in for PhantomGame for easy asynchronous tasks. 
    /// 
    /// This Component has a few Create methods that allows you 
    /// to create tasks that have a callback Action that is executed
    /// in the GameLoop (main thread) whenever the Task is completed.
    /// </summary>
    /// <exmaple>
    /// Just add an instance of this class to your PhantomGame:
    /// <code>
	/// PhantomGame.Game.AddComponent(Asynchrony.Instance);
    /// </code>
    /// Then you can start creating tasks:
    /// <code>
    /// void Work() {
    ///   Thread.Sleep(1000);
    /// }
    /// void Callback() {
    ///   Debug.Assert(Thread.CurrentThread.ManagedThreadId == 1);
    ///   Debug.WriteLine("Work finished");
    /// }
    /// Task t = Asynchrony.Instance.Create(Work, Callback);
    /// t.Start();
    /// </code>
    /// In this exmaple both Work and Callback are methods without arguments 
    /// or a return type.
    /// 
    /// If you want the task to result in data use the generic version of the
    /// Create method:
    /// <code>
    /// int Work() {
    ///   Thread.Sleep(1000);
    ///   return 42;
    /// }
    /// void Callback( int result ) {
    ///   Debug.WriteLine("Work finished: " + result);
    /// }
    /// Asynchrony.Instance.Create&lt;int&gt;(Work, Callback).Start();
    /// </code>
    /// 
    /// There is also a version of Create that allow for parameters to be
    /// supplied to the task. For example:
    /// <code>
    /// int Work( short time ) {
    ///   Thread.Sleep(time);
    ///   return 4200000 + time;
    /// }
    /// void Callback( int result ) {
    ///   Debug.WriteLine("Work finished: " + result);
    /// }
    /// Asynchrony.Instance.Create&lt;int, short&gt;(Work, (short)1000, Callback).Start();
    /// Asynchrony.Instance.Create&lt;int, short&gt;(Work, (short)2000, Callback).Start();
    /// </code>
    /// 
    /// Obviously you can use any Create method using inline delegates or lambda functions:
    /// <code>
    /// Asynchrony.Instance.Create(() => Thread.Sleep(1000), () => Debug.WriteLine("The Cake is a lie!")).Start();
    /// Asynchrony.Instance.Create<Component>(() => {
    ///     Thread.Sleep(1000); // Creating an component, or a level...
    ///     return new Entity(Vector2.Zero);
    /// }, (c) => this.AddComponent(c) ).Start();
    /// </code>
    /// </exmaple>
    public class Asynchrony : Component
    {
        /// <summary>Quick access to the Asynchrony instance.</summary>
        public static Asynchrony Instance { get; private set; }
		static Asynchrony() {
			Asynchrony.Instance = new Asynchrony();
		}

        /// <summary>
        /// This List contains all tasks as ITaskTester. This contruction
        /// using the interface was required to handle different types of
        /// generics.
        /// </summary>
        private List<ITaskTester> tasks;

        /// <summary>
        /// Nothing to configure, just add an instance of this class to 
        /// the PhantomGame.
        /// </summary>
		private Asynchrony()
        {
            this.tasks = new List<ITaskTester>();
        }

        /// <summary>
        /// Create a task with an attached ready Action (a callback if you will).
        /// </summary>
        /// <param name="task">The action to preform asynchronous.</param>
        /// <param name="ready">The action to call when the task is completed (this will be called in the gameloop).</param>
        /// <returns>The resulting task, this still need to be Start()ed.</returns>
        public Task Create(Action task, Action ready)
        {
            Task t = new Task(task);
            lock (this.tasks)
            {
                this.tasks.Add(new TaskTester(t, ready));
            }
            return t;
        }

        /// <summary>
        /// Create a task with an attached ready Action (a callback if you will).
        /// </summary>
        /// <param name="task">The action to preform asynchronous.</param>
        /// <param name="ready">The action to call when the task is completed (this will be called in the gameloop).</param>
        /// <returns>The resulting task, this still need to be Start()ed.</returns>
        public Task<T> Create<T>(Func<T> task, Action<T> ready)
        {
            Task<T> t = new Task<T>(task);
            lock (this.tasks)
            {
                this.tasks.Add(new TaskTester<T>(t, ready));
            }
            return t;
        }

        /// <summary>
        /// Create a task with an attached ready Action (a callback if you will).
        /// </summary>
        /// <param name="task">The action to preform asynchronous.</param>
        /// <param name="data">The paramater data to pass along to the task.</param>
        /// <param name="ready">The action to call when the task is completed (this will be called in the gameloop).</param>
        /// <returns>The resulting task, this still need to be Start()ed.</returns>
        public Task<T> Create<T, I>(Func<I, T> task, I data, Action<T> ready)
        {
            Task<T> t = new Task<T>(()=>task(data));
            lock (this.tasks)
            {
                this.tasks.Add(new TaskTester<T>(t, ready));
            }
            return t;
        }

        /// <summary>
        /// Simply execute an action in the first game update.
        /// </summary>
        /// <param name="action">The action to invoke.</param>
        public void Dispatch(Action action)
        {
            lock (this.tasks)
            {
                this.tasks.Add(new DispatchTester(action));
            }
        }

        /// <summary>
        /// Within the Update method of this component every
        /// created task is checked if it's ready or not.
        /// If it was ready the callback/action is invoked.
        /// </summary>
        /// <param name="elapsed"></param>
        public override void Update(float elapsed)
        {
            lock (this.tasks)
            {
                for (int i = this.tasks.Count - 1; i >= 0; i--)
                {
                    if (i >= 0 && i < tasks.Count)
                    {
                        ITaskTester tester = this.tasks[i];
                        if (tester.PerformTest())
                        {
                            this.tasks.RemoveAt(i);
                            tester.PerformInvoke();
                        }
                    }
                }
            }
            base.Update(elapsed);
        }

        public override void OnAdd(Component parent)
        {
            base.OnAdd(parent);
            Debug.Assert(parent is PhantomGame, "Asynchrony must be added directly to the PhantomGame");
        }

        /// <summary>
        /// Thanks Nils, for this idea.
        /// </summary>
        private interface ITaskTester
        {
            bool PerformTest();
            void PerformInvoke();
        }
        private class TaskTester<T> : ITaskTester
        {
            public Task<T> Task;
            public Action<T> Action;
            public TaskTester(Task<T> task, Action<T> action)
            {
                this.Task = task;
                this.Action = action;
            }

            public bool PerformTest()
            {
                if (Task.IsCompleted)
                {
                    return true;
                }
                return false;
            }

            public void PerformInvoke()
            {
                Action.Invoke(Task.Result);
            }
        }
        private class TaskTester : ITaskTester
        {
            public Task Task;
            public Action Action;
            public TaskTester(Task task, Action action)
            {
                this.Task = task;
                this.Action = action;
            }

            public bool PerformTest()
            {
                if (Task.IsCompleted)
                {
                    return true;
                }
                return false;
            }

            public void PerformInvoke()
            {
                Action.Invoke();
            }
        }
        private class DispatchTester : ITaskTester
        {
            private Action Action;
            public DispatchTester(Action action)
            {
                this.Action = action;
            }
            public bool PerformTest()
            {
                return true;
            }
            public void PerformInvoke()
            {
                Action.Invoke();
            }

        }
    }


}
