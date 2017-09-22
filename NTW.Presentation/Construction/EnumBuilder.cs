using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Windows.Data;
using System.Windows;

namespace NTW.Presentation.Construction
{
    internal static class EnumBuilder
    {
        /// <summary>
        /// Поиск и формирование списка перечислений для которых следуется создать ObjectDataProvider.
        /// </summary>
        /// <param name="condition">Условие отбора типов пренадлежащих enum.
        /// Если true - то будет проверятся наличие строки в пространстве имен. false - полное совпадение имени.</param>
        /// <returns>Массив типов соответствующий параметрам отбора.</returns>
        private static Type[] GetEnumTypes(Func<Type, bool> condition)
        {
            List<Type> result = new List<Type>();

            result = Assembly.GetEntryAssembly().GetTypes().Where(condition).Where(t => t.BaseType == typeof(Enum)).ToList();

            return result.ToArray();
        }

        /// <summary>
        /// Создание ObjectDataProvider для перечеслений по определенным параметрам.
        /// </summary>
        /// <param name="condition">Условие отбора типов пренадлежащих enum.
        /// Если true - то будет проверятся наличие строки в пространстве имен. false - полное совпадение имени.</param>
        /// <returns>Массив типов соответствующий параметрам отбора.</param>
        internal static void CreateDynamicResource(Func<Type, bool> condition)
        {
            foreach (Type i in GetEnumTypes(condition))
            {
                Application app = Application.Current;
                if (app.Resources.FindName(i.FullName) == null)
                {
                    ObjectDataProvider odp = new ObjectDataProvider();
                    odp.MethodName = "GetValues";
                    odp.MethodParameters.Add(i);
                    odp.ObjectType = typeof(Enum);

                    app.Resources.Add(i.FullName, odp);
                }
            }
        }
    }
}
