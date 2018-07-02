using System;
using System.Collections.Generic;
using System.Linq;

namespace Ribbon.Client.Util
{
    public static class Utils
    {
        public static bool IsPresentAsCause(Exception exceptionToSearchIn, IReadOnlyCollection<Type> exceptionToSearchFor)
        {
            var infiniteLoopPreventionCounter = 10;
            while (exceptionToSearchIn != null && infiniteLoopPreventionCounter > 0)
            {
                infiniteLoopPreventionCounter--;
                if (exceptionToSearchFor.Any(type => type.IsInstanceOfType(exceptionToSearchIn)))
                {
                    return true;
                }

                exceptionToSearchIn = exceptionToSearchIn.GetBaseException();
            }

            return false;
        }
    }
}