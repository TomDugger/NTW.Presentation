using System.Windows;

namespace NTW.Presentation
{
    /// <summary>
    /// Базовый класс абстракции представления. Требуется наследовать в собственном классе
    /// для возможности формирования отображения.
    /// Содержит единственное свойство, которое извлевает шаблон для класса.
    /// </summary>
    public abstract class Presentation : IPresentation
    {
        /// <summary>
        /// Соойство возврашающее шаблон.
        /// </summary>
        public object Template
        {
            get
            {
                string cName = this.GetType().FullName;
                return Application.Current.TryFindResource(cName);
            }
        }

        public static void Generation(string namespaceNameType, bool contains)
        {
            NTW.Presentation.PresentationType.CreateDynamicResourceSimpleClass(Application.Current, namespaceNameType, contains);
        }

        public static void Generation(string namespaceNameType, string namespaceNameEnum, bool containsType, bool containsEnum)
        {
            NTW.Presentation.PresentationEnum.CreateDynamicResourceEnum(Application.Current, namespaceNameEnum, containsEnum);
            NTW.Presentation.PresentationType.CreateDynamicResourceSimpleClass(Application.Current, namespaceNameType, containsType);
        }

        public static void Generation(string[] namespaceNamesType, string[] namespaceNamesEnum)
        {
            NTW.Presentation.PresentationEnum.CreateDynamicResourceEnum(Application.Current, namespaceNamesEnum);
            NTW.Presentation.PresentationType.CreateDynamicResourceSimpleClass(Application.Current, namespaceNamesType);
        }
    }
}
