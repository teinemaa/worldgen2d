using System;
using System.Collections.Generic;
using System.Linq;

namespace WorldGeneration.Core
{
    [Serializable]
    public class WeightedComponent <T>
    {
        public T Component;
        public float Weight;

        public static T GetRandomComponent(UnityRandom rand, IEnumerable<WeightedComponent<T>> components)
        {
            float totalWeight = components.Sum(component => component.Weight);
            float weightPosition = rand.NextSingle()*totalWeight;
            foreach (WeightedComponent<T> component in components)
            {
                weightPosition -= component.Weight;
                if (weightPosition <= 0)
                {
                    return component.Component;
                }
            }
            return default(T);
        }
    }
}
