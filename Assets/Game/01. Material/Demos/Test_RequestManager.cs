using UnityEngine;

namespace Game._00.Script.Demos
{
    public class Test_RequestManager : MonoBehaviour {

        // Queue<PathRequest> pathRequestQueue = new Queue<PathRequest>();
        // PathRequest currentPathRequest;
        //
        // static Test_RequestManager instance;
        // PathFinding pathfinding;
        //
        // bool isProcessingPath;
        //
        // void Awake() {
        //     instance = this;
        //     // pathfinding = GameManager.Instance.04. PathFinding;
        // }
        //
        // public static void RequestPath(Vector3 pathStart, Vector3 pathEnd, Action<Vector3[], bool> callback) {
        //     PathRequest newRequest = new PathRequest(pathStart,pathEnd,callback);
        //     instance.pathRequestQueue.Enqueue(newRequest);
        //     instance.TryProcessNext();
        // }
        //
        // void TryProcessNext() {
        //     if (!isProcessingPath && pathRequestQueue.Count > 0) {
        //         currentPathRequest = pathRequestQueue.Dequeue();
        //         isProcessingPath = true;
        //         pathfinding.StartFindPath(currentPathRequest.pathStart, currentPathRequest.pathEnd);
        //     }
        // }
        //
        // public void FinishedProcessingPath(Vector3[] path, bool success) {
        //     currentPathRequest.callback(path,success);
        //     isProcessingPath = false;
        //     TryProcessNext();
        // }
        //
        // struct PathRequest {
        //     public Vector3 pathStart;
        //     public Vector3 pathEnd;
        //     public Action<Vector3[], bool> callback;
        //
        //     public PathRequest(Vector3 _start, Vector3 _end, Action<Vector3[], bool> _callback) {
        //         pathStart = _start;
        //         pathEnd = _end;
        //         callback = _callback;
        //     }
        //
        // }
    }
}