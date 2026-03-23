using UnityEngine;

namespace AgilityDogs.Data
{
    [CreateAssetMenu(fileName = "NewHandlerData", menuName = "Agility Dogs/Handler Data")]
    public class HandlerData : ScriptableObject
    {
        [Header("Handler Info")]
        public string handlerName;
        public string displayName;
        [TextArea(2, 4)]
        public string backstory;

        [Header("Stats")]
        [Range(1, 10)]
        public int speedStat = 5;
        [Range(1, 10)]
        public int commandPrecisionStat = 5;
        [Range(1, 10)]
        public int courseReadingStat = 5;
        [Range(1, 10)]
        public int pressureHandlingStat = 5;

        [Header("Movement")]
        public float handlerSpeed = 5f;
        public float handlerSprintSpeed = 8f;

        [Header("Unlock")]
        public bool isUnlockedByDefault = true;
        public int unlockCost = 0;

        [Header("Visual")]
        public GameObject prefab;
        public Sprite portrait;
    }
}
