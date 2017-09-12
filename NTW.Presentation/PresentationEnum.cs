using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Windows;
using System.Windows.Data;

namespace NTW.Presentation
{
    public static class PresentationEnum
    {
        /// <summary>
        /// Поиск и формирование списка перечислений для которых следуется создать ObjectDataProvider.
        /// </summary>
        /// <param name="namespaceName">Пространство имен по которому будет сформирован список.</param>
        /// <param name="contains">Параметр, который определяет правило проверки имени пространства имен.
        /// Если true - то будет проверятся наличие строки в пространстве имен. false - полное совпадение имени.</param>
        /// <returns>Массив типов соответствующий параметрам отбора.</returns>
        private static Type[] GetEnumTypes(string namespaceName, bool contains)
        {
            List<Type> result = new List<Type>();
            if (!contains)
                result = Assembly.GetEntryAssembly().GetTypes().Where(t => t.Namespace == namespaceName && t.BaseType == typeof(Enum)).ToList();
            else
                result = Assembly.GetEntryAssembly().GetTypes().Where(t => t.Namespace.Contains(namespaceName) && t.BaseType == typeof(Enum)).ToList();

            return result.ToArray();
        }

        /// <summary>
        /// Поиск и формирование списка перечислений для которых следуется создать ObjectDataProvider.
        /// </summary>
        /// <param name="namespacesName">Именя пространств имен по которым следуется получить перечисления.</param>
        /// <returns>Массив типов соответствующий параметрам отбора.</returns>
        private static Type[] GetEnumTypes(string[] namespacesName)
        {
            List<Type> result = new List<Type>();

            result = Assembly.GetEntryAssembly().GetTypes().Where(t => namespacesName.Contains(t.Namespace) && t.BaseType == typeof(Enum)).ToList();

            return result.ToArray();
        }

        /// <summary>
        /// Создание ObjectDataProvider для перечеслений по определенным параметрам.
        /// </summary>
        /// <param name="application">Application в ресурсы которого следуется добвить ObjectDataProvider.</param>
        /// <param name="namespaceName">Пространство имен по которому будет сформирован список.</param>
        /// <param name="contains">Параметр, который определяет правило проверки имени пространства имен.
        /// Если true - то будет проверятся наличие строки в пространстве имен. false - полное совпадение имени.</param>
        /// <returns>Массив типов соответствующий параметрам отбора.</param>
        internal static void CreateDynamicResourceEnum(Application application, string namespaceName, bool contains)
        {
            foreach (Type i in GetEnumTypes(namespaceName, contains))
            {
                if (application.Resources.FindName(i.FullName) == null)
                {
                    ObjectDataProvider odp = new ObjectDataProvider();
                    odp.MethodName = "GetValues";
                    odp.MethodParameters.Add(i);
                    odp.ObjectType = typeof(Enum);

                    application.Resources.Add(i.FullName, odp);
                }
            }
        }

        /// <summary>
        /// Создание ObjectDataProvider для перечеслений по определенным параметрам.
        /// </summary>
        /// <param name="application">Application в ресурсы которого следуется добвить ObjectDataProvider.</param>
        /// <param name="namespacesName">Именя пространств имен по которым следуется получить перечисления.</param>
        internal static void CreateDynamicResourceEnum(Application application, string[] namespacesName)
        {
            foreach (Type i in GetEnumTypes(namespacesName))
            {
                if (application.Resources.FindName(i.FullName) == null)
                {
                    ObjectDataProvider odp = new ObjectDataProvider();
                    odp.MethodName = "GetValues";
                    odp.MethodParameters.Add(i);
                    odp.ObjectType = typeof(Enum);

                    application.Resources.Add(i.FullName, odp);
                }
            }
        }
    }
}
