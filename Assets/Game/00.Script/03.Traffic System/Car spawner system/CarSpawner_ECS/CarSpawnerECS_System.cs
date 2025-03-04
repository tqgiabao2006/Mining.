using System;
using System.Collections.Generic;
using Game._00.Script._00.Manager.Observer;
using Game._00.Script._03.Traffic_System.PathFinding;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Game._00.Script._02.Grid_setting;
using Game._00.Script._03.Traffic_System.Building;
using Unity.Transforms;

namespace  Game._00.Script._03.Traffic_System.Car_spawner_system.CarSpawner_ECS
{
   [BurstCompile]
   [UpdateInGroup (typeof(PresentationSystemGroup))]
    public partial class CarSpawnSystem : SystemBase, IObserver
    {
        private PathRequestManager _pathRequestManager;
        private bool _isNotified = false;
        //use this for parallel spawn waves in different places to spawn multiple car in the same building
        protected override void OnCreate()
        {
            RequireForUpdate<SpawnGameObjectHolder>();
        }

     
        public void OnNotified(object data, string flag)
        {
            if (data is ValueTuple<Node, Node, string> && flag == NotificationFlags.SpawnCar)
            {
                if (!_isNotified) //To avoid duplicate OnNotified call
                {
                    _isNotified = true;
                }
                else
                {
                    return;
                }    
                
                ValueTuple< Node, Node, string> startEndBuildings = (ValueTuple< Node, Node, string>)data;
                Vector3[] waypoints = _pathRequestManager.GetPathWaypoints(startEndBuildings.Item1.WorldPosition, startEndBuildings.Item2.WorldPosition);
                BlobAssetReference<BlobArray<float3>> waypointsBlob = CreateWaypointsBlob(waypoints);
                SpawnCarEntity(startEndBuildings.Item3, new SpawnData()
                {
                    StartPos = new float3(startEndBuildings.Item1.WorldPosition.x,
                        startEndBuildings.Item1.WorldPosition.y, 0),
                    EndPos = new float3(startEndBuildings.Item2.WorldPosition.x,
                        startEndBuildings.Item2.WorldPosition.y, 0),
                    Waypoints = waypointsBlob,
                });
                _isNotified = false;
            }
        }

        protected override void OnUpdate()
        {
            //Delay time to get the _pathRequestManager because OnCreate() is called before Awake() to initialize the class
            if (_pathRequestManager == null)
            {
                _pathRequestManager = PathRequestManager.Instance;
            }
        }
        
        /// <summary>
        /// Using entity manager to run this in main thread, moving car in jobs => more optimized
        /// </summary>
        /// <param name="objectFlags"></param>
        /// <param name="spawnData"></param>
        public void SpawnCarEntity(string objectFlags, SpawnData spawnData)
        {  
            SpawnGameObjectHolder objectHolder = SystemAPI.GetSingleton<SpawnGameObjectHolder>();

            Entity spawnedEntity = Entity.Null;
            if (objectFlags == ObjectFlags.RedBlood)
            {
                spawnedEntity = EntityManager.Instantiate(objectHolder.RedBlood);
            }else if (objectFlags == ObjectFlags.BlueBlood)
            {
                spawnedEntity = EntityManager.Instantiate(objectHolder.BlueBlood);
            }
            float3 spawnPosition = new float3(spawnData.StartPos.x, spawnData.StartPos.y, 0);
            
            // Set components directly using EntityManager
            EntityManager.SetComponentData(spawnedEntity, new LocalTransform
            {
                Position = spawnPosition,
                Rotation = quaternion.identity,
                Scale = 0.4f
            });
            
            BlobBuilder blobBuilder = new BlobBuilder(Allocator.Temp);
            ref FollowPathWaypointBlob followPathWaypointBlob = ref blobBuilder.ConstructRoot<FollowPathWaypointBlob>();

            BlobBuilderArray<CarSpawner_ECS.PathWaypoint> blobBuilderArray = blobBuilder.Allocate(ref followPathWaypointBlob.Waypoints, spawnData.Waypoints.Value.Length);
            for (int i = 0; i < spawnData.Waypoints.Value.Length; i++)
            {
                blobBuilderArray[i] = new PathWaypoint { Value = spawnData.Waypoints.Value[i] };
            }

            BlobAssetReference<FollowPathWaypointBlob> followPathWaypointBlobReference = blobBuilder.CreateBlobAssetReference<FollowPathWaypointBlob>(Allocator.Temp);

            EntityManager.AddComponentData(spawnedEntity, new FollowPathData()
            {
                WaypointsBlob = followPathWaypointBlobReference
            });

            blobBuilder.Dispose();
            
            EntityManager.SetComponentData(spawnedEntity, new CanRun()
            {
                Value = true,
            });
            
            EntityManager.SetComponentData(spawnedEntity, new OriginBuildingRoad()
            {
                Position = spawnData.StartPos
            });
        }
        
        /// <summary>
        /// Convert Vector3[] waypoints to BlobAsset more optimized for ECS and Job system
        /// </summary>
        /// <param name="waypoints"></param>
        /// <returns></returns>
        public BlobAssetReference<BlobArray<float3>> CreateWaypointsBlob(Vector3[] waypoints)
        {
            using (var builder = new BlobBuilder(Allocator.Temp))
            {
                ref var root = ref builder.ConstructRoot<BlobArray<float3>>();
                var waypointArray = builder.Allocate(ref root, waypoints.Length);

                for (int i = 0; i < waypoints.Length; i++)
                {
                    waypointArray[i] = new float3(waypoints[i].x, waypoints[i].y, waypoints[i].z);
                }

                return builder.CreateBlobAssetReference<BlobArray<float3>>(Allocator.Persistent);
            }
        }
    }
    
}