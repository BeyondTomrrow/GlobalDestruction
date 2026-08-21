using System;
using System.Collections.Generic;
using MonoGame.Extended.Collections;

namespace WorldNMilSim.Core;

// Owns entity lifetime and component storage.
// Components are plain data structs/classes. systems handle logic.

public class World
{
	private int _nextId = 1;
	private readonly HashSet<int> _alive = new();
	private readonly Dictionary<Type, Dictionary<int, object>> _pools = new();

	public Entity CreateEntity()
	{
		var e = new Entity(_nextId++);
		_alive.Add(e.Id);
		return e;
	}

	public void DestroyEntity(Entity e)
	{
		_alive.Remove(e.Id);
		foreach (var pool in _pools.Values)
			pool.Remove(e.Id);
	}

	public bool IsAlive(Entity e) => _alive.Contains(e.Id);

	public void Set<T>(Entity e, T component) where T : class
	{
		Pool<T>()[e.Id] = component;
	}

	public T? Get<T>(Entity e) where T : class
	{
		return Pool<T>().TryGetValue(e.Id, out var c) ? (T)c : null;
	}

	public bool Has<T>(Entity e) where T : class => Pool<T>().ContainsKey(e.Id);

	public void Remove<T>(Entity e) where T : class => Pool<T>().Remove(e.Id);

	// Iterate all entities that have component T.
	public IEnumerable<(Entity, T)> Query<T>() where T : class
	{
		foreach (var kvp in Pool<T>())
		{
			if (_alive.Contains(kvp.Key))
				yield return (new Entity(kvp.Key), (T)kvp.Value);
		}
	}

	// Iterate entities that have both T1 and T2.
	public IEnumerable<(Entity, T1, T2)> Query<T1, T2>()
		where T1 : class where T2 : class
	{
		var poolA = Pool<T1>();
		var poolB = Pool<T2>();
		foreach (var kvp in poolA)
		{
			if (_alive.Contains(kvp.Key) && poolB.TryGetValue(kvp.Key, out var b))
				yield return (new Entity(kvp.Key), (T1)kvp.Value, (T2)b);
		}
	}

	public IEnumerable<(Entity, T1, T2, T3)> Query<T1, T2, T3>()
		where T1 : class where T2 : class where T3 : class
	{
		var poolA = Pool<T1>();
		var poolB = Pool<T2>();
		var poolC = Pool<T3>();
		foreach (var kvp in poolA)
		{
			if (_alive.Contains(kvp.Key) &&
				poolB.TryGetValue(kvp.Key, out var b) &&
				poolC.TryGetValue(kvp.Key, out var c))
			{
				yield return (new Entity(kvp.Key), (T1)kvp.Value, (T2)b, (T3)c);
			}
		}
	}

	private Dictionary<int, object> Pool<T>()
	{
		var type = typeof(T);
		if (!_pools.TryGetValue(type, out var pool))
		{
			pool = new Dictionary<int, object>();
			_pools[type] = pool;
		}
		return pool;
	}

}
