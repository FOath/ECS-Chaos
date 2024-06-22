using Unity.Entities;
using Unity.Transforms;
using Unity.Burst;
using System;
using Unity.Mathematics;

public partial struct SpawnerSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {

    }

    public void OnDestroy(ref SystemState state) 
    {
    
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer.ParallelWriter ecb = GetEntityCommandBuffer(ref state);
        new ProcessSpawnerJob
        {
            ElapsedTime = SystemAPI.Time.ElapsedTime,
            Ecb = ecb
        }.ScheduleParallel();
    }

    private EntityCommandBuffer.ParallelWriter GetEntityCommandBuffer(ref SystemState state)
    {
        var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);
        return ecb.AsParallelWriter();
    }
}

public partial struct ProcessSpawnerJob : IJobEntity
{
    public EntityCommandBuffer.ParallelWriter Ecb;
    public double ElapsedTime;

    private void Execute([ChunkIndexInQuery] int chunkIndex, ref Spawner spawner)
    {
        if (spawner.NextSpawnTime < ElapsedTime)
        {
            Unity.Mathematics.Random r = new((uint)(ElapsedTime * 31415));

            Entity newEntity = Ecb.Instantiate(chunkIndex, spawner.Prefab);

            Ecb.SetComponent(chunkIndex, newEntity, LocalTransform.FromPosition(r.NextFloat3() * 10 - 5));

            spawner.NextSpawnTime = (float)ElapsedTime + spawner.SpawnRate;
        }
    }
}