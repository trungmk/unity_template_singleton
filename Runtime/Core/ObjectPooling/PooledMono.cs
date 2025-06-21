using System;
using System.Collections.Generic;
using UnityEngine;
using YNWA;

namespace Core
{
    public class PooledMono : MonoBehaviour
    {
        public Action<PooledMono> OnBackToPoolEvent { get; set; }

        public string AddressName { get; private set; }
        
        public PoolItemType PoolItemType  { get; private set; }

        public void Init(string addressName = "", PoolItemType poolItemType = PoolItemType.General)
        {
            AddressName = addressName;
            PoolItemType = poolItemType;
        }

        public void ReturnToPool()
        {
            OnBackToPoolEvent?.Invoke(this);
        }
        
        public void Handle_UnPreload()
        {
            ReturnToPool();
        }
    }
}


