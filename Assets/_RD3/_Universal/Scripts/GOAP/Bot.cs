using System;
using UnityEngine;
using UnityEngine.AI;

namespace _RD3._Universal.Scripts.GOAP
{
    public class Bot : MonoBehaviour
    {
        private RaycastHit _hitInfo = new RaycastHit();
        private NavMeshAgent _agent;
        private Camera _camera;

        private void Start()
        {
            _camera = Camera.main;
            _agent = GetComponent<NavMeshAgent>();
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                Ray ray = _camera.ScreenPointToRay(Input.mousePosition);

                if (Physics.Raycast(ray, out _hitInfo))
                {
                    Debug.Log($"DESTINO: {_hitInfo.point}");

                    NavMeshPath path = new NavMeshPath();

                    bool calculated = _agent.CalculatePath(
                        _hitInfo.point,
                        path
                    );

                    Debug.Log(
                        $"CALCULATE PATH: {calculated} | " +
                        $"STATUS: {path.status} | " +
                        $"CORNERS: {path.corners.Length}"
                    );

                    for (int i = 0; i < path.corners.Length; i++)
                    {
                        Debug.Log($"CORNER {i}: {path.corners[i]}");
                    }

                    _agent.SetDestination(_hitInfo.point);
                }
            }

            if (_agent.nextOffMeshLinkData.valid)
            {
                Debug.Log("!!!!!!!! LINK ENCONTRADO !!!!!!!!");
            }

            if (_agent.isOnOffMeshLink)
            {
                Debug.Log("!!!!!!!! AGENTE ESTÁ NO LINK !!!!!!!!");
            }
        }
        
    }
}
