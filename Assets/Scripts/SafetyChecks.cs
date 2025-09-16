using System;

namespace ScorpionSteps 
{
    public static class SafetyChecks
    {
        public static bool IsNull(object obj)
        {
            if (System.Object.ReferenceEquals(obj, null))
                return true;

            return false;
        }

        public static bool IsNotNull(object obj)
        {
            if (System.Object.ReferenceEquals(obj, null))
                return false;

            return true;
        }

        public static bool IsSameOrSubclass(Type potentialBase, Type potentialDescendant)
        {
            return potentialDescendant.IsSubclassOf(potentialBase)
                   || potentialDescendant == potentialBase;
        }
    }
}
