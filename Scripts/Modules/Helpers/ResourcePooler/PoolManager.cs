using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrannPack.Helpers.RecourcePool
{
    public partial class PoolManager:Node
    {
        private class Pool
        {
            public PackedScene Scene;
            public Queue<Node> Inactive = new Queue<Node>();
        }

        private Dictionary<string, Pool> _pools = new();

        private Node _inactiveHolder;

        public static Node PoolManagerNode;
        public static Node PlayableNode;
        public static Node NPCNode;
        public static Node ProjectilesNode;

        public override void _Ready()
        {
            _inactiveHolder = new Node { Name = "InactivePool" };
            AddChild(_inactiveHolder);
            PoolManagerNode = this;
            if (PlayableNode == null) PlayableNode=GetNode("root/World/Playables");
            if (NPCNode == null) NPCNode = GetNode("root/World/NPCs");
            if (ProjectilesNode == null) PlayableNode = GetNode("root/World/Projectiles");
        }

        /// <summary>
        /// Registers a resource to be pooled under a key.
        /// </summary>
        public void RegisterPool(string key, PackedScene scene, int preloadCount = 0)
        {
            if (_pools.ContainsKey(key)) return;

            Pool pool = new Pool { Scene = scene };
            _pools[key] = pool;

            for (int i = 0; i < preloadCount; i++)
            {
                Node node = scene.Instantiate();
                if (node is IPoolable p)
                {
                    p.SetPool(this, key);
                    p.OnDeactivate();
                }
                _inactiveHolder.AddChild(node);
                pool.Inactive.Enqueue(node);
            }
        }

        /// <summary>
        /// Spawns a node from the pool or creates a new one.
        /// </summary>
        public T Spawn<T>(string key, Node parent) where T : Node
        {
            if (!_pools.TryGetValue(key, out var pool))
            {
                GD.PrintErr($"No pool found for key: {key}");
                return null;
            }

            Node node;
            if (pool.Inactive.Count > 0)
            {
                node = pool.Inactive.Dequeue();
            }
            else
            {
                node = pool.Scene.Instantiate();
            }

            if (node is IPoolable p)
            {
                p.SetPool(this, key);
                p.OnActivate();
            }

            parent.AddChild(node);
            return node as T;
        }

        /// <summary>
        /// Returns a node to the pool.
        /// </summary>
        public void Return(string key, Node node)
        {
            if (!_pools.TryGetValue(key, out var pool))
            {
                GD.PrintErr($"No pool found for key: {key}");
                node.QueueFree();
                return;
            }

            if (node is IPoolable p)
                p.OnDeactivate();

            _inactiveHolder.AddChild(node);
            pool.Inactive.Enqueue(node);
        }
    }

    public interface IPoolable
    {
        /// <summary>
        /// Called when the node is reused from the pool.
        /// </summary>
        public virtual void OnActivate() { IsInUse = true; }

        /// <summary>
        /// Called when the node is returned to the pool.
        /// </summary>
        public virtual void OnDeactivate() { IsInUse = false; }

        /// <summary>
        /// Optional hook for when the node should return itself (e.g. after animation/timeout).
        /// </summary>
        void SetPool(PoolManager pool, string poolKey);

        public bool IsInUse { get; set; }


    }

}
