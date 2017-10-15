using System.Windows;
using System;
using NTW.Presentation.Construction;

namespace NTW.Presentation
{
    public static class Presentation 
    {
        public static void Generation(Func<Type, bool> TypesCondition, Func<Type, bool> EnumCondition)
        {
            EnumBuilder.CreateDynamicResource(EnumCondition);

            TypeBuilder.CreateDynamicResource(TypesCondition);
        }

        public static void Generation(Func<Type, bool> TypesCondition)
        {
            TypeBuilder.CreateDynamicResource(TypesCondition);
        }
    }
}
