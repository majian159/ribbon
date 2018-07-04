using System;
using System.Collections.Generic;
using System.Linq;

namespace Ribbon.Client.Util
{
    public static class Utils
    {
        public static TTarget GetInnerException<TTarget>(Exception exceptionToSearchIn) where TTarget : Exception
        {
            var infiniteLoopPreventionCounter = 10;
            while (exceptionToSearchIn != null && infiniteLoopPreventionCounter > 0)
            {
                infiniteLoopPreventionCounter--;

                if (exceptionToSearchIn is TTarget target)
                {
                    return target;
                }

                exceptionToSearchIn = exceptionToSearchIn.InnerException;
            }

            return default(TTarget);
        }

        public static bool IsPresentAsException(Exception exceptionToSearchIn, IReadOnlyCollection<Type> exceptionToSearchFor)
        {
            var infiniteLoopPreventionCounter = 10;
            while (exceptionToSearchIn != null && infiniteLoopPreventionCounter > 0)
            {
                infiniteLoopPreventionCounter--;
                if (exceptionToSearchFor.Any(type => type.IsInstanceOfType(exceptionToSearchIn)))
                {
                    return true;
                }

                exceptionToSearchIn = exceptionToSearchIn.InnerException;
            }

            return false;
        }
    }
}