using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrannPack.Helpers.RecourcePool
{
	public partial class PoolManager : Node
	{
		private class Pool
		{
			public PackedScene Scene;
			public Queue<Node> Inactive = new Queue<Node>();
		}

		private Dictionary<string, Pool> _pools = new();

		private Node _inactiveHolder;

		public static PoolManager PoolManagerNode;
		public static Node PlayableNode;
		public static Node NPCNode;
		public static Node ProjectilesNode;

		public override void _Ready()
		{
			_inactiveHolder = new Node { Name = "InactivePool" , ProcessMode = ProcessModeEnum.Disabled};
			AddChild(_inactiveHolder);
			PoolManagerNode = this;
			if (PlayableNode == null) PlayableNode = GetTree().Root.GetNode("Root/World/Playables");
			if (NPCNode == null) NPCNode = GetTree().Root.GetNode("Root/World/NPCs");
			if (ProjectilesNode == null) ProjectilesNode = GetTree().Root.GetNode("Root/World/Projectiles");

			LoadSpawnPoolDetails();
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
				GD.Print("PRELOAD");
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
				GD.Print("POOLSPAWN");
			}
			else
			{
				node = pool.Scene.Instantiate();
				GD.Print("NEW");
			}

			if (node is IPoolable p)
			{
				p.SetPool(this, key);
				p.OnActivate();
			}
			(node as CanvasItem).Visible = true;
			node.Reparent(parent);
			node.Owner = parent;
			return node as T;
		}

		/// <summary>
		/// Returns a node to the pool.
		/// </summary>
		public void Return(string key, Node node)
		{
			if (!_pools.TryGetValue(key, out var pool))
			{
				GD.PrintErr($"No return pool found for key: {key}");
				node.QueueFree();
				return;
			}

			if (node is IPoolable p)
				p.OnDeactivate();
			(node as CanvasItem).Visible = false;
			node.Reparent(_inactiveHolder);
			node.Owner = _inactiveHolder;
			pool.Inactive.Enqueue(node);
		}

		public void LoadSpawnPoolDetails(string folderPath = "res://Scripts/Modules/")
		{
			ScanFolderRecursive(folderPath);
		}

		private void ScanFolderRecursive(string folderPath)
		{
			var dir = DirAccess.Open(folderPath);
			if (dir == null)
			{
				GD.PrintErr($"Could not open directory: {folderPath}");
				return;
			}

			dir.ListDirBegin();

			while (true)
			{
				string entry = dir.GetNext();
				if (string.IsNullOrEmpty(entry))
					break;

				if (entry == "." || entry == "..")
					continue;

				string fullPath = folderPath.TrimEnd('/') + "/" + entry;

				if (dir.CurrentIsDir())
				{
					// Recurse into subdirectory
					ScanFolderRecursive(fullPath);
				}
				else if (entry.EndsWith(".tres") || entry.EndsWith(".res"))
				{
					var resource = ResourceLoader.Load<SpawnPoolDetails>(fullPath);
					if (resource != null)
					{
						GD.Print($"Registering pool for: {resource.Name} ({resource.CodeName})");

						var scene = resource.SpawnScene;
						if (scene != null)
						{
							RegisterPool(resource.CodeName, scene, resource.ShouldPool ? resource.InitialPoolSize : 0);
						}
						else
						{
							GD.PrintErr($"SpawnScene is null for: {entry}");
						}
					}
				}
			}

			dir.ListDirEnd();
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
	public enum SpawnType { Body,Projectile,Pickup,FX,Interractable,Other}
	

}
