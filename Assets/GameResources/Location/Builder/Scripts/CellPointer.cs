using System;
using GameResources.Location.Island.Scripts;
using UnityEngine;

namespace GameResources.Location.Builder.Scripts
{
    public class CellPointer : MonoBehaviour
    {
        public event Action OnCellPointed;
        public event Action OnNoCellPointed; 

        public bool IsCellPointedNow { get; private set; }

        public LocationGridProvider PointedGrid { get; private set; }
        
        private LocationCell _pointedCell;
        public LocationCell PointedCell
        {
            get
            {
                return _pointedCell;
            }

            private set
            {
                IsCellPointedNow = true;
                
                if (_pointedCell == value)
                {
                    return;
                }

                _pointedCell = value;
                
                OnCellPointed?.Invoke();
            }
        }
        
        [SerializeField] 
        private Camera raycastCamera;

        private const float MAX_RAYCAST_DISTANCE = 50;

        private RaycastHit[] _hits;

        private void Awake()
        {
            _hits = new RaycastHit[5];
        }

        private void Update() => GetPointedCell();

        private void GetPointedCell()
        {
            if (TryGetPointedCell(out var grid, out var cell))
            {
                PointedGrid = grid;
                PointedCell = cell;

                return;
            }

            IsCellPointedNow = false;
            OnNoCellPointed?.Invoke();
        }

        private bool TryGetPointedCell(out LocationGridProvider grid, out LocationCell locationCell)
        {
            var ray = raycastCamera.ScreenPointToRay(Input.mousePosition);

            var size = Physics.RaycastNonAlloc(ray, _hits, MAX_RAYCAST_DISTANCE);
            
            if (size == 0)
            {
                grid = null;
                locationCell = null;
                
                return false;
            }
            
            if (TryGetHitOnGrid(_hits, size, out grid, out var hit) == false)
            {
                locationCell = null;
                
                return false;
            }

            var localPoint = grid.transform.InverseTransformPoint(hit.point);

            return grid.LocationGrid.TryGetPointedCell(localPoint, out locationCell);
        }

        private bool TryGetHitOnGrid(in RaycastHit[] hits, in int size, out LocationGridProvider grid, out RaycastHit hit)
        {
            grid = null;
            hit = new RaycastHit();
            
            var closestGridHitDistance = MAX_RAYCAST_DISTANCE;

            for (var i = 0; i < size; ++i)
            {
                if (hits[i].collider.gameObject.TryGetComponent(out LocationGridProvider possibleGrid) == false)
                {
                    continue;
                }

                var hitDistance = Vector3.Distance(raycastCamera.transform.position, hits[i].point);

                if (closestGridHitDistance <= hitDistance)
                {
                    continue;
                }

                hit = hits[i];
                grid = possibleGrid;
                closestGridHitDistance = hitDistance;
            }

            return MAX_RAYCAST_DISTANCE - closestGridHitDistance > float.Epsilon;
        }
    }
}
