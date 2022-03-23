﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Tilemaps;

namespace NavMeshComponents.Extensions
{
    [ExecuteAlways]
    [AddComponentMenu("Navigation/NavMeshCollectSources2d", 30)]
    public class NavMeshCollectSources2d: NevMeshExtension
    {
        [SerializeField]
        bool m_OverrideByGrid;
        public bool overrideByGrid { get { return m_OverrideByGrid; } set { m_OverrideByGrid = value; } }

        [SerializeField]
        GameObject m_UseMeshPrefab;
        public GameObject useMeshPrefab { get { return m_UseMeshPrefab; } set { m_UseMeshPrefab = value; } }

        [SerializeField]
        bool m_CompressBounds;
        public bool compressBounds { get { return m_CompressBounds; } set { m_CompressBounds = value; } }

        [SerializeField]
        Vector3 m_OverrideVector = Vector3.one;
        public Vector3 overrideVector { get { return m_OverrideVector; } set { m_OverrideVector = value; } }

        public override void CalculateWorldBounds(NavMeshSurface surface, List<NavMeshBuildSource> sources, NavMeshBuilderState navNeshState)
        {
            if ((int)surface.collectObjects != (int)CollectObjects2d.Children)
            {
                navNeshState.result.Encapsulate(CalculateGridWorldBounds(surface, navNeshState.worldToLocal));
            }
        }

        private static Bounds CalculateGridWorldBounds(NavMeshSurface surface, Matrix4x4 worldToLocal)
        {
            var bounds = new Bounds();
            var grid = FindObjectOfType<Grid>();
            var tilemaps = grid.GetComponentsInChildren<Tilemap>();
            if (tilemaps == null || tilemaps.Length < 1)
            {

                throw new NullReferenceException("Add at least one tilemap");
            }
            foreach (var tilemap in tilemaps)
            {
                //Debug.Log($"From Local Bounds [{tilemap.name}]: {tilemap.localBounds}");
                var lbounds = NavMeshSurface.GetWorldBounds(worldToLocal * tilemap.transform.localToWorldMatrix, tilemap.localBounds);
                bounds.Encapsulate(lbounds);
                //Debug.Log($"To World Bounds: {bounds}");
            }
            bounds.Expand(0.1f);
            return bounds;
        }
        public override void CollectSources(NavMeshSurface surface, List<NavMeshBuildSource> sources, NavMeshBuilderState navNeshState)
        {
            if (!surface.hideEditorLogs && !Mathf.Approximately(transform.eulerAngles.x, 270f))
                Debug.LogWarning("NavMeshSurface2d is not rotated respectively to (x-90;y0;z0). Apply rotation unless intended.");
            var builder = new NavMeshBuilder2dState();
            builder.defaultArea = surface.defaultArea;
            builder.layerMask = surface.layerMask;
            builder.agentID = surface.agentTypeID;
            builder.useMeshPrefab = useMeshPrefab;
            builder.overrideByGrid = overrideByGrid;
            builder.compressBounds = compressBounds;
            builder.overrideVector = overrideVector;
            builder.CollectGeometry = surface.useGeometry;
            builder.CollectObjects = (CollectObjects2d)(int)surface.collectObjects;
            builder.parent = surface.gameObject;
            builder.hideEditorLogs = surface.hideEditorLogs;
            NavMeshBuilder2d.CollectSources(sources, builder);
        }
    }
}
