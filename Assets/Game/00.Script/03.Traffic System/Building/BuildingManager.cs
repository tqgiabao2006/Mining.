using System;
using System.Collections;
using System.Collections.Generic;
using Game._00.Script._00.Manager.Observer;
using Game._00.Script._02.Grid_setting;
using Game._00.Script._03.Traffic_System.Car_spawner_system.CarSpawner_ECS;
using Unity.Entities;
using UnityEngine;

namespace Game._00.Script._03.Traffic_System.Building
{

    public class BuildingManager: SubjectBase
    {
        //Directed graph => adjacent list => building type + its output
        private Dictionary<BuildingType, List<BuildingType>> _inputMap = new Dictionary<BuildingType, List<BuildingType>>();
       
        private Dictionary<BuildingType, List<BuildingBase>> _currentBuildings = new Dictionary<BuildingType, List<BuildingBase>>();
        public Dictionary<BuildingType, List<BuildingBase>> CurrentBuildings
        {
            get => _currentBuildings;
        }

        private Dictionary<GameObject, List<Node>> _unconnectedBuildings;
        
        //Use dictionary because two roads can be connected but not to others
        private Dictionary<int, List<BuildingBase>> _connectedBuildings; 
        
        
        private void Start()
        {
            InputOutputMapSetup();
            ObserversSetup();
            _connectedBuildings = new Dictionary<int, List<BuildingBase>>();
            _unconnectedBuildings = new Dictionary<GameObject, List<Node>>();
        }
        
        #region Set up
        private void InputOutputMapSetup()
        {
            //Business has no output
            _inputMap.Add(BuildingType.BusinessRed, new List<BuildingType>() { BuildingType.HomeRed });
            _inputMap.Add(BuildingType.BusinessYellow, new List<BuildingType>() { BuildingType.HomeYellow });
            _inputMap.Add(BuildingType.BusinessBlue, new List<BuildingType>() { BuildingType.HomeBlue });
            
        }
        
      
        public override void ObserversSetup()
        {
            // Get the CarSpawnSystem
            IObserver spawnSystemInstance = World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<CarSpawnSystem>();
            if (spawnSystemInstance != null)
            {
                _observers.Add(spawnSystemInstance);
            }
        }
        #endregion
        
        public void RegisterBuilding(BuildingBase building)
        {
            if (_currentBuildings.ContainsKey(building.BuildingType))
            {
                _currentBuildings[building.BuildingType].Add(building);
            }
            else
            {
                _currentBuildings.Add(building.BuildingType, new List<BuildingBase>() { building });
            }
            
            _unconnectedBuildings.Add(building.gameObject, building.ParkingNodes);
        }

        public List<BuildingBase> GetInputBuildings(BuildingType buildingType)
        {
            List<BuildingBase> buildings = new List<BuildingBase>();
            List<BuildingType> buildingTypes = _inputMap[buildingType];
            foreach (BuildingType type in buildingTypes)
            {
                if (_currentBuildings.TryGetValue(type, out var building))
                {
                    buildings.AddRange(building);
                }
            }
            return buildings;   
        }

        public bool IsOutput(BuildingType business, BuildingType home)
        {
            if (_inputMap.TryGetValue(home, out var buildings))
            {
                if (buildings.Contains(business))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Spawn multiple cars have waiting time between by notifying spawn car system through time
        /// Can not bring this function to the system itself because it makes system ignore other notification when 2, 3 cars spawned
        /// in the same time, job can't work for structural change like instantiate entity
        /// </summary>
        /// <returns></returns>
        public void SpawnCarWaves(Vector3 startNodePosition, Quaternion rotation,string carFlag)
        {
           Notify((startNodePosition, rotation,carFlag), NotificationFlags.SpawnCar); 
        }

        /// <summary>
        /// Notify ECS Spawner System to spawn car find path buildingDirection in it
        /// </summary>
        /// <param name="data"></param>
        /// <param name="flag"></param>
        public void OnNotified(object data, string flag)
        {
        //     if (flag == NotificationFlags.CheckingConnection &&
        //         data is (Func<List<Node>, Node, Node>))
        //     {
        //         //Check all in unconnected graph:
        //         Func<List<Node>, Node, Node> givenData = (Func<List<Node>, Node, Node>)data;
        //         List<GameObject> removedObj = new List<GameObject>();
        //     
        //         foreach (GameObject buildingObj in _unconnectedBuildings.Keys)
        //         {
        //             //Get all output buildings' parking nodes
        //             BuildingBase building = buildingObj.GetComponent<BuildingBase>();
        //             List<Node> roadNodes = new List<Node>();
        //             foreach (BuildingBase b in GetInputBuildings(building.BuildingType))
        //             {
        //                roadNodes.Add(b.RoadNode);
        //             }
        //             
        //             if (roadNodes.Count == 0)
        //             {
        //                 continue;
        //             }
        //
        //             Node startNode = building.RoadNode;
        //             Node endNode = givenData(roadNodes, building.RoadNode);
        //             
        //             if (endNode != null)
        //             {
        //                 CarSpawnInfo carSpawnInfo = _carSpawnInfos[building.BuildingType];
        //                 StartCoroutine(SpawnCarWaves(startNode, endNode, carSpawnInfo));
        //                
        //                 //Add to remove list to remove later
        //                 if (_unconnectedBuildings.ContainsKey(endNode.BelongedBuilding))
        //                 {
        //                     removedObj.Add(endNode.BelongedBuilding);
        //                 }
        //                 removedObj.Add(building.gameObject);
        //                 
        //                 //Add to connected:
        //                 if (!_connectedBuildings.ContainsKey(building.OriginBuildingNode.GraphIndex))
        //                 {
        //                    _connectedBuildings.Add(building.OriginBuildingNode.GraphIndex, new List<BuildingBase>());
        //                 }
        //                 _connectedBuildings[building.OriginBuildingNode.GraphIndex].Add(building);
        //                 _connectedBuildings[building.OriginBuildingNode.GraphIndex].Add(endNode.BelongedBuilding.GetComponent<BuildingBase>());
        //             }
        //         }
        //         //Remove unconnected building
        //         foreach (GameObject buildingObj in removedObj)
        //         {
        //             _unconnectedBuildings.Remove(buildingObj);
        //         }
        //     }
        }
    }

}