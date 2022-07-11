using Tridinet.Systems;
using Tridinet.Utilities.Data;
using System.Collections;
using UnityEngine;

namespace Assets.Scripts
{
    /// <summary>
    /// This is the base class for runtime
    /// world building
    /// </summary>
    public class BaseBuiltPrefab : MonoBehaviour, INode
    {
        public NodeData data { get; set; }
        public Vector3 offset { get; set; }

        public float health => data.health;

        public CompiledCode script { get; set; }

        public void InitNode(NodeData data)
        {
            this.data = data;
        }

        public void DeleteNode()
        {
            Destroy(gameObject);
        }

        //public void SetOffset(ActionList actionlist)
        //{
        //    switch (actionlist)
        //    {
        //        case ActionList.Rotation:
        //            offset = transform.eulerAngles;
        //            break;
        //        case ActionList.Position:
        //            offset = transform.position;
        //            break;
        //        case ActionList.Scale:
        //            offset = transform.localScale;
        //            break;
        //        default:
        //            break;
        //    }
        //}

        //public Vector3 Calculate(Axis axis, ActionList actionList, Vector3 difference, float multiplier)
        //{
        //    switch (axis)
        //    {
        //        case Axis.x:
        //            return SetValue(offset + (difference.x * transform.right * multiplier), actionList);
        //        case Axis.y:
        //            return SetValue(offset + (difference.y * transform.up * multiplier), actionList);
        //        case Axis.z:
        //            return SetValue(offset + (difference.y * transform.forward * multiplier), actionList);
        //        default:
        //            break;
        //    }
        //    return Vector3.zero;
        //}

        //public Vector3 SetValue(Vector3 newvector, ActionList actionList)
        //{
        //    switch (actionList)
        //    {
        //        case ActionList.Rotation:
        //            transform.eulerAngles = newvector;
        //            break;
        //        case ActionList.Position:
        //            transform.position = newvector;
        //            break;
        //        case ActionList.Scale:
        //            transform.localScale = newvector;
        //            break;
        //        default:
        //            break;
        //    }
        //    return newvector;
        //}

        public void TakeDamage(IOffensive offensive)
        {
            data.health -= offensive.damageAmmount;
            if (data.health <= 0f)
            {
                //do something
            }
        }
    }
}