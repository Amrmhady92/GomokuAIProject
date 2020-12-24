using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KandooZ
{

    /// <summary>
    ///  This Class Contains read made methods for use anytime list of Methods::
    ///  - SetLayerRecursively: to set the layers of all gameobjects children under a parent gameObject 
    ///  - ...
    /// </summary>
    public static class Utilities
    {

        public static void SetLayerRecursively(GameObject obj, int newLayer)
        {
            if (null == obj)
            {
                return;
            }

            obj.layer = newLayer;

            foreach (Transform child in obj.transform)
            {
                if (null == child)
                {
                    continue;
                }
                SetLayerRecursively(child.gameObject, newLayer);
            }
        }

        public static Vector3 RandomNavSphere(Vector3 origin, float dist, float minDistance = 0, int layermask = -1 , bool useIterations = false, int iterationsCount = 5 , bool zeroY = true)
        {
            
            Vector3 randDirection = Random.insideUnitSphere * (dist);
            //Vector3 minDirection = ((randDirection - origin).normalized * minDistance);
            Vector3 minDirection = (randDirection).normalized * minDistance;


            randDirection += origin + minDirection;
            if (zeroY) randDirection.y = 0;

            UnityEngine.AI.NavMeshHit navHit;
            bool found = false;
            found = UnityEngine.AI.NavMesh.SamplePosition(randDirection, out navHit, dist, layermask);
            
            Vector3 newPos = navHit.position;
            if (!found)
            {
                if (useIterations)
                {
                    for (int i = 0; i < iterationsCount; i++)
                    {
                        randDirection = Random.insideUnitSphere * (dist);
                        minDirection = (randDirection).normalized * minDistance;
                        randDirection += origin + minDirection;
                        if (zeroY) randDirection.y = 0;

                        found = UnityEngine.AI.NavMesh.SamplePosition(randDirection, out navHit, dist, layermask);
                        newPos = navHit.position;
                        if (found)
                        {
                            Debug.Log("Tries = " + i);
                            break;
                        }
                    }
                }

                if(!found)
                {
                    newPos = origin; MonoBehaviour.print("Infinity Sphere moved to origin");
                }          
            }
            return newPos;
        }

        public static Vector3 RandomNavCircle(Vector3 origin, float dist, float minDistance = 0, int layermask = -1, bool useIterations = false, int iterationsCount = 5)
        {

            Vector3 randDirection = Random.insideUnitCircle * (dist);
            Vector3 minDirection = ((randDirection - origin).normalized * minDistance);


            randDirection += origin + minDirection;
            UnityEngine.AI.NavMeshHit navHit;
            bool found = false;
            found = UnityEngine.AI.NavMesh.SamplePosition(randDirection, out navHit, dist, layermask);

            Vector3 newPos = navHit.position;
            if (!found)
            {
                if (useIterations)
                {
                    for (int i = 0; i < iterationsCount; i++)
                    {
                        found = UnityEngine.AI.NavMesh.SamplePosition(randDirection, out navHit, dist, layermask);
                        newPos = navHit.position;
                        if (found) break;
                    }
                }

                if (!found)
                {
                    newPos = origin; MonoBehaviour.print("Infinity Sphere moved to origin");
                }
            }
            return newPos;
        }
        public static Vector3 CalculateBestThrowSpeed(Vector3 origin, Vector3 target, float timeToTarget)
        {
            // calculate vectors
            Vector3 toTarget = target - origin;
            Vector3 toTargetXZ = toTarget;
            toTargetXZ.y = 0;

            // calculate xz and y
            float y = toTarget.y;
            float xz = toTargetXZ.magnitude;

            // calculate starting speeds for xz and y. Physics forumulase deltaX = v0 * t + 1/2 * a * t * t
            // where a is "-gravity" but only on the y plane, and a is 0 in xz plane.
            // so xz = v0xz * t => v0xz = xz / t
            // and y = v0y * t - 1/2 * gravity * t * t => v0y * t = y + 1/2 * gravity * t * t => v0y = y / t + 1/2 * gravity * t
            float t = timeToTarget;
            float v0y = y / t + 0.5f * Physics.gravity.magnitude * t;
            float v0xz = xz / t;

            // create result vector for calculated starting speeds
            Vector3 result = toTargetXZ.normalized;        // get direction of xz but with magnitude 1
            result *= v0xz;                                // set magnitude of xz to v0xz (starting speed in xz plane)
            result.y = v0y;                                // set y to v0y (starting speed of y plane)

            return result;
        }

    }
}