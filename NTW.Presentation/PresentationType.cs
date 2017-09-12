using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using NTW.Presentation.Attribute;
using System.Windows.Media;

namespace NTW.Presentation
{
    public static class PresentationType
    {
        /// <summary>
        /// Массив типов являющимися типоми простых значений (число, строка и т.д.)
        /// </summary>
        public static Type[] SimpleTypes = new Type[] { 
            typeof(bool), 
            typeof(byte), 
            typeof(sbyte), 
            typeof(char), 
            typeof(decimal), 
            typeof(double), 
            typeof(float), 
            typeof(int), 
            typeof(uint), 
            typeof(long), 
            typeof(ulong), 
            typeof(short), 
            typeof(ushort), 
            typeof(string) 
        };

        /// <summary>
        /// Получение массива типов для последующего формирования шаблонов отображения из указанного пространства имен.
        /// </summary>
        /// <param name="namespaceName">Строковое значение пространства имен.</param>
        /// <param name="contains">Параметер определяющий способ проверки пространства имен.</param>
        /// <returns>Массив типов</returns>
        private static Type[] GetSimpleClassTypes(string namespaceName, bool contains)
        {
            List<Type> result = new List<Type>();

            if (!contains)
                result = Assembly.GetEntryAssembly().GetTypes().Where(t => t.Namespace != null && t.Namespace == namespaceName && (t.IsClass || t.IsInterface) && t.BaseType == typeof(Presentation)).ToList();
            else
                result = Assembly.GetEntryAssembly().GetTypes().Where(t => t.Namespace != null && t.Namespace.Contains(namespaceName) && (t.IsClass || t.IsInterface) && t.BaseType == typeof(Presentation)).ToList();

            return result.ToArray();
        }

        /// <summary>
        /// Получение массива типов для последующего формирования шаблонов отображения из указанных пространств имен.
        /// </summary>
        /// <param name="namespaceNames">Массив строковых значений пространств имен</param>
        /// <returns>Массив типов</returns>
        private static Type[] GetSimpleClassTypes(string[] namespaceNames)
        {
            List<Type> result = new List<Type>();

            result = Assembly.GetEntryAssembly().GetTypes().Where(t => namespaceNames.Contains(t.Namespace) && (t.IsClass || t.IsInterface) && t.BaseType == typeof(IPresentation)).ToList();

            return result.ToArray();
        }

        #region Создвние составных элементов шаблона
        private static FrameworkElementFactory CreateCaption(string propertyName, PresentationInfo pAttr)
        {
            FrameworkElementFactory Caption = new FrameworkElementFactory(typeof(TextBlock));
            Caption.SetValue(TextBlock.TextProperty, pAttr != null ? pAttr.CaptionName : propertyName);
            Caption.SetValue(TextBlock.TextWrappingProperty, pAttr != null? pAttr.PresentCaption : TextWrapping.NoWrap);
            return Caption;
        }

        private static FrameworkElementFactory CreateContent(string propertyName, PresentationInfo pAttr)
        {
            FrameworkElementFactory Property = new FrameworkElementFactory(typeof(Label));
            Property.SetBinding(Label.DataContextProperty, new Binding(propertyName));
            Property.SetBinding(Label.ContentProperty, new Binding("."));
            Property.SetBinding(Label.ContentTemplateProperty, new Binding("Template"));
            Property.SetValue(Label.HorizontalContentAlignmentProperty, System.Windows.HorizontalAlignment.Stretch);

            return Property;
        }
        #endregion

        //заложим новый класс формирования шаблона
        private static DataTemplate _GetControl(Application application, Type type)
        {
            DataTemplate template = new DataTemplate();
            template.DataType = type;

            #region Основа (контейнер) в которую будут вкладыватся элементы
            FrameworkElementFactory Panel = new FrameworkElementFactory(typeof(StackPanel));
            Panel.Name = "BackPanel"; //на всякий случай дадим имя - мало ли придется обратится к контексту данных

            foreach (PropertyInfo property in type.GetProperties().Where(x => x.Name != "Template"))//откличм условие так как возможно управление способом привязки
            {
                //требуется получить список атрибутов для текущего свойства
                List<System.Attribute> attributes = System.Attribute.GetCustomAttributes(property).ToList();//List - так как обладаем метовами поиска без приобразования

                PresentationInfo pAttr = attributes.Find((x) => x is PresentationInfo) as PresentationInfo;
                PresentationCollectionInfo pcAttr = attributes.Find((x) => x is PresentationCollectionInfo) as PresentationCollectionInfo;

                //формируем подпись свойства
                Panel.AppendChild(CreateCaption(property.Name, pAttr));

                //подставляем возможные варианты отображения свойства
                //1.  если является простым типом данных
                #region Если относится к простому типу
                if (SimpleTypes.Contains(property.PropertyType))
                {
                    //можно просто текст бокс сделать c проверкой отработки поля
                    FrameworkElementFactory Property;
                    if (!property.CanWrite)
                    {
                        Property = new FrameworkElementFactory(typeof(TextBlock));
                        Property.SetBinding(TextBlock.TextProperty, new Binding(property.Name) {
                            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                            Mode = BindingMode.OneWay
                        });
                    }
                    else
                    {
                        Property = new FrameworkElementFactory(typeof(TextBox));
                        Property.SetBinding(TextBox.TextProperty, new Binding(property.Name) {
                            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                            Mode = !property.CanWrite ? BindingMode.OneWay : BindingMode.TwoWay
                        });
                    }

                    Panel.AppendChild(Property);
                } 
                #endregion
                #region Если является перечислением
                else if(property.PropertyType.BaseType == typeof(Enum)) 
                {
                    FrameworkElementFactory Property;
                    if (!property.CanWrite)
                    {
                        Property = new FrameworkElementFactory(typeof(TextBlock));
                        Property.SetBinding(TextBlock.TextProperty, new Binding(property.Name) {
                            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                            Mode = BindingMode.OneWay
                        });
                    }
                    else
                    {
                        Property = new FrameworkElementFactory(typeof(ComboBox));
                        Property.SetResourceReference(ComboBox.DataContextProperty, property.PropertyType.FullName);
                        Property.SetBinding(ComboBox.ItemsSourceProperty, new Binding("."));
                        Property.SetBinding(ComboBox.SelectedValueProperty, new Binding("DataContext." + property.Name) { 
                            ElementName = "BackPanel", 
                            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged 
                        });
                    }

                    Panel.AppendChild(Property);
                }
                #endregion
                #region Если относится к типу "Представления"
                else if (property.PropertyType.BaseType == typeof(Presentation) && !property.PropertyType.IsGenericType)//будем определять в составе Generic
                {
                    Panel.AppendChild(CreateContent(property.Name, pAttr));
                } 
                #endregion
                #region Если относится к типу Generic
                else if (property.PropertyType.IsGenericType)//всетаки придется вынести в отдульную обработку
                {
                    //с листом (да и со словорем) все иначе и сложнее так как есть сложная хрень с генерацией нескольких параметров

                    //будем определять к какому плану будет отнасится данный тип generic
                    if (property.PropertyType.GetInterface("IList") != null)//значит относится к спискам
                    {
                        #region проверка наличия ресурса для данного объекта
                        if (application.TryFindResource(property.PropertyType.FullName) == null)
                            application.Resources.Add(property.PropertyType.FullName, _GetControl(application, property.PropertyType)); 
                        #endregion

                        FrameworkElementFactory BaseContainer = new FrameworkElementFactory(typeof(Grid));
                        FrameworkElementFactory Container = new FrameworkElementFactory(typeof(Grid));

                        #region Формирование модели если требуется
                        //попробуем без спецефичного формирования объекта
                        //увы, без него никак
                        Type GType = property.PropertyType.GetGenericArguments()[0];
                        string propertyName = property.Name;
                        Container.AddHandler(ListBox.LoadedEvent, new RoutedEventHandler((s, e) => {
                            var view = Activator.CreateInstance(typeof(CollectionViewModel<>).MakeGenericType(GType), null);
                            BindingOperations.SetBinding(view as DependencyObject, AbstractView.ItemsProperty, new Binding(propertyName) { Source = ((s as Grid).Parent as Grid).DataContext, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });
                            (s as Grid).DataContext = view;
                        }));
                        #endregion

                        //что делать если несколько типов генерации как в Dictionary?
                        #region Формирование Grid
                        FrameworkElementFactory ContainerRowProperty = new FrameworkElementFactory(typeof(RowDefinition));
                        ContainerRowProperty.SetValue(RowDefinition.HeightProperty, new GridLength(40));
                        Container.AppendChild(ContainerRowProperty);

                        FrameworkElementFactory ContainerRow1Property = new FrameworkElementFactory(typeof(RowDefinition));
                        ContainerRow1Property.SetValue(RowDefinition.HeightProperty, new GridLength(1, GridUnitType.Star));
                        Container.AppendChild(ContainerRow1Property);

                        FrameworkElementFactory ContainerColumnProperty = new FrameworkElementFactory(typeof(ColumnDefinition));
                        ContainerColumnProperty.SetValue(ColumnDefinition.WidthProperty, new GridLength(1, GridUnitType.Star));
                        Container.AppendChild(ContainerColumnProperty);

                        FrameworkElementFactory ContainerColumn1Property = new FrameworkElementFactory(typeof(ColumnDefinition));
                        ContainerColumn1Property.SetValue(ColumnDefinition.WidthProperty, new GridLength(40));
                        Container.AppendChild(ContainerColumn1Property); 
                        #endregion

                        #region Button "Add"
                        FrameworkElementFactory ButtonAdd = new FrameworkElementFactory(typeof(Button));
                        ButtonAdd.SetValue(Button.ContentProperty, "+");
                        ButtonAdd.SetBinding(Button.CommandProperty, new Binding("AddCommand"));

                        if (pcAttr != null && pcAttr.AddButtonContentTemplate != null && application.TryFindResource(pcAttr.AddButtonContentTemplate) != null)
                            ButtonAdd.SetResourceReference(Button.ContentTemplateProperty, pcAttr.AddButtonContentTemplate);

                        Container.AppendChild(ButtonAdd);
                        #endregion

                        #region Button "Clear"
                        FrameworkElementFactory ButtonClear = new FrameworkElementFactory(typeof(Button));
                        ButtonClear.Name = property.Name + "clearButton";
                        ButtonClear.SetValue(Button.ContentProperty, "clear");
                        ButtonClear.SetBinding(Button.CommandProperty, new Binding("ClearCommand"));
                        ButtonClear.SetBinding(Button.CommandParameterProperty, new Binding(".") { ElementName = property.Name + "clearButton" });
                        ButtonClear.SetValue(Grid.ColumnProperty, 1);

                        if (pcAttr != null && pcAttr.ClearButtonContentTemplate != null && application.TryFindResource(pcAttr.ClearButtonContentTemplate) != null)
                            ButtonClear.SetResourceReference(Button.ContentTemplateProperty, pcAttr.ClearButtonContentTemplate);

                        Container.AppendChild(ButtonClear);
                        #endregion

                        #region List
                        FrameworkElementFactory List = new FrameworkElementFactory(typeof(ListBox));
                        List.SetValue(ScrollViewer.HorizontalScrollBarVisibilityProperty, ScrollBarVisibility.Disabled);
                        List.SetValue(Grid.ColumnSpanProperty, 2);
                        List.SetValue(Grid.RowProperty, 1);
                        List.SetValue(ListBox.HorizontalContentAlignmentProperty, System.Windows.HorizontalAlignment.Stretch);
                        List.SetBinding(ListBox.ItemsSourceProperty, new Binding("AItems"));

                        //как быть с шаблон данных
                        if (SimpleTypes.Contains(property.PropertyType.GetGenericArguments()[0]))
                            List.SetResourceReference(ListBox.ItemTemplateProperty, "ItemPresentation");
                        else
                        {
                            List.SetResourceReference(ListBox.ItemTemplateProperty, "CustomItemPresentation");
                            //List.SetBinding(ListBox.ItemsSourceProperty, new Binding("Items"));
                        }
                        Container.AppendChild(List);
                        #endregion

                        #region обработка аттрибута коллекции
                        if (pcAttr != null)
                        {
                            if (pcAttr.MinHeight != 0)
                                BaseContainer.SetValue(Grid.MinHeightProperty, pcAttr.MinHeight);

                            if (pcAttr.MaxHeight != 0)
                                BaseContainer.SetValue(Grid.MaxHeightProperty, pcAttr.MaxHeight);

                            if (pcAttr.ItemTemplate != null && Application.Current.TryFindResource(pcAttr.ItemTemplate) != null)
                                List.SetResourceReference(ListBox.ItemTemplateProperty, pcAttr.ItemTemplate);

                            if (pcAttr != null && pcAttr.ItemStyle != null && Application.Current.TryFindResource(pcAttr.ItemStyle) != null)
                                List.SetResourceReference(ListBox.ItemContainerStyleProperty, pcAttr.ItemStyle);
                        }  
                        #endregion

                        BaseContainer.AppendChild(Container);

                        Panel.AppendChild(BaseContainer);
                    }//конечно же отдельно обработать!
                    else if (property.PropertyType.GetInterface("IDictionary") != null)//если является словорем
                    {

                    }
                    else //значит относится к составным объектам
                    {
                        if (application.TryFindResource(property.PropertyType.FullName) == null)
                            application.Resources.Add(property.PropertyType.FullName, _GetControl(application, property.PropertyType));

                        Panel.AppendChild(CreateContent(property.Name, pAttr));
                    }
                }
                #endregion
                #region Если относится к типу массива
                else if (property.PropertyType.IsArray)
                {
                    #region проверка наличия ресурса для данного объекта
                    if (application.TryFindResource(property.PropertyType.GetElementType().FullName) == null)
                        application.Resources.Add(property.PropertyType.GetElementType().FullName, _GetControl(application, property.PropertyType.GetElementType()));
                    #endregion

                    #region List
                    FrameworkElementFactory List = new FrameworkElementFactory(typeof(ListBox));
                    List.SetValue(ScrollViewer.HorizontalScrollBarVisibilityProperty, ScrollBarVisibility.Disabled);
                    List.SetValue(ListBox.HorizontalContentAlignmentProperty, System.Windows.HorizontalAlignment.Stretch);
                    List.SetBinding(ListBox.ItemsSourceProperty, new Binding("AItems"));

                    #region Формирование модели если требуется
                    //попробуем без спецефичного формирования объекта
                    //увы, без него никак
                    Type GType = property.PropertyType.GetElementType();
                    string propertyName = property.Name;
                    List.AddHandler(ListBox.LoadedEvent, new RoutedEventHandler((s, e) =>
                    {
                        var view = Activator.CreateInstance(typeof(CollectionViewModel<>).MakeGenericType(GType), null);
                        BindingOperations.SetBinding(view as DependencyObject, AbstractView.ItemsProperty, new Binding(propertyName) { Source = (s as ListBox).DataContext, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });
                        (s as ListBox).DataContext = view;
                    }));
                    #endregion

                    //как быть с шаблон данных
                    if (SimpleTypes.Contains(property.PropertyType.GetElementType()))
                        List.SetResourceReference(ListBox.ItemTemplateProperty, "SItemPresentation");
                    else
                    {
                        List.SetResourceReference(ListBox.ItemTemplateProperty, "SCustomItemPresentation");
                        //List.SetBinding(ListBox.ItemsSourceProperty, new Binding("Items"));
                    }
                    Panel.AppendChild(List);
                    #endregion

                    if (pcAttr != null)
                    {
                        if (pcAttr.MinHeight != 0)
                            List.SetValue(Grid.MinHeightProperty, pcAttr.MinHeight);

                        if (pcAttr.MaxHeight != 0)
                            List.SetValue(Grid.MaxHeightProperty, pcAttr.MaxHeight);

                        if (pcAttr.ItemTemplate != null && Application.Current.TryFindResource(pcAttr.ItemTemplate) != null)
                            List.SetResourceReference(ListBox.ItemTemplateProperty, pcAttr.ItemTemplate);

                        if (pcAttr != null && pcAttr.ItemStyle != null && Application.Current.TryFindResource(pcAttr.ItemStyle) != null)
                            List.SetResourceReference(ListBox.ItemContainerStyleProperty, pcAttr.ItemStyle);
                    } 
                }
                #endregion
            }
            #endregion

            template.VisualTree = Panel;

            return template;
        }

        /// <summary>
        /// Возврашает шаблон Который предназначен для работы с данным типом.
        /// </summary>
        /// <param name="control">Тип данных для которого будет сформирован шаблон.</param>
        /// <returns>Шаблон</returns>
        private static DataTemplate GetControl(Type control)
        {
            DataTemplate template = new DataTemplate();
            template.DataType = control;

            FrameworkElementFactory Panel = new FrameworkElementFactory(typeof(StackPanel));
            Panel.Name = "BackPanel";

            //foreach (PropertyInfo p in control.GetProperties().Where(x => x.CanRead && x.CanWrite))
            //{
            //    var atts = System.Attribute.GetCustomAttributes(p);
            //    var pi = System.Attribute.GetCustomAttribute(p, typeof(PresentationInfo)) as PresentationInfo;

            //    #region Подпись
            //    FrameworkElementFactory Caption = new FrameworkElementFactory(typeof(TextBlock));
            //    Caption.SetValue(TextBlock.TextProperty, pi != null ? pi.CaptionName : p.Name);
            //    Caption.SetValue(TextBlock.TextWrappingProperty, pi != null ? pi.PresentCaption : TextWrapping.NoWrap);
            //    Panel.AppendChild(Caption); 
            //    #endregion

            //    #region Если строковое представление
            //    if (SimpleTypes.Contains(p.PropertyType))
            //    {
            //        FrameworkElementFactory Property = new FrameworkElementFactory(typeof(TextBox));
            //        Property.SetBinding(TextBox.TextProperty, new Binding(p.Name) { UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });
            //        Panel.AppendChild(Property);
            //    } 
            //    #endregion
            //    #region Если является перечислением
            //    else if (p.PropertyType.BaseType == typeof(Enum))
            //    {

            //        FrameworkElementFactory Property = new FrameworkElementFactory(typeof(ComboBox));
            //        Property.SetResourceReference(ComboBox.DataContextProperty, p.PropertyType.FullName);
            //        Property.SetBinding(ComboBox.ItemsSourceProperty, new Binding("."));
            //        Property.SetBinding(ComboBox.SelectedValueProperty, new Binding("DataContext." + p.Name) { ElementName = "BackPanel", UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });
            //        Panel.AppendChild(Property);
            //    } 
            //    #endregion
            //    #region Если относится к типу объекта отображения
            //    else if (p.PropertyType.BaseType == typeof(IPresentation))
            //    {
            //        FrameworkElementFactory Property = new FrameworkElementFactory(typeof(Label));
            //        Property.SetBinding(Label.DataContextProperty, new Binding(p.Name));
            //        Property.SetBinding(Label.ContentProperty, new Binding("."));
            //        Property.SetBinding(Label.ContentTemplateProperty, new Binding("Template"));
            //        Property.SetValue(Label.HorizontalContentAlignmentProperty, System.Windows.HorizontalAlignment.Stretch);
            //        Panel.AppendChild(Property);
            //    } 
            //    #endregion
            //    #region Если является типом генерации определенного типа
            //    else if (p.PropertyType.IsGenericType)//считаем что это коллекция
            //    {
            //        var collatt = atts.FirstOrDefault(x => x is PresentationCollectionInfo) as PresentationCollectionInfo;

            //        FrameworkElementFactory ContainerProperty = new FrameworkElementFactory(typeof(Grid));

            //        CollectionViewModel model = new CollectionViewModel(p.PropertyType.GetGenericArguments()[0]);

            //        #region Grid
            //        FrameworkElementFactory ContainerRowProperty = new FrameworkElementFactory(typeof(RowDefinition));
            //        ContainerRowProperty.SetValue(RowDefinition.HeightProperty, new GridLength(40));
            //        ContainerProperty.AppendChild(ContainerRowProperty);

            //        FrameworkElementFactory ContainerRow1Property = new FrameworkElementFactory(typeof(RowDefinition));
            //        ContainerRow1Property.SetValue(RowDefinition.HeightProperty, new GridLength(1, GridUnitType.Star));
            //        ContainerProperty.AppendChild(ContainerRow1Property);

            //        FrameworkElementFactory ContainerColumnProperty = new FrameworkElementFactory(typeof(ColumnDefinition));
            //        ContainerColumnProperty.SetValue(ColumnDefinition.WidthProperty, new GridLength(1, GridUnitType.Star));
            //        ContainerProperty.AppendChild(ContainerColumnProperty);

            //        FrameworkElementFactory ContainerColumn1Property = new FrameworkElementFactory(typeof(ColumnDefinition));
            //        ContainerColumn1Property.SetValue(ColumnDefinition.WidthProperty, new GridLength(40));
            //        ContainerProperty.AppendChild(ContainerColumn1Property); 
            //        #endregion

            //        #region Button add
            //        FrameworkElementFactory ButtonAddProperty = new FrameworkElementFactory(typeof(Button));
            //        ButtonAddProperty.SetValue(Button.ContentProperty, "+");
            //        ButtonAddProperty.SetValue(Button.DataContextProperty, model);
            //        ButtonAddProperty.SetBinding(Button.CommandProperty, new Binding("AddCommand"));

            //        if (collatt != null && collatt.AddButtonContentTemplate != null && Application.Current.TryFindResource(collatt.AddButtonContentTemplate) != null)
            //            ButtonAddProperty.SetResourceReference(Button.ContentTemplateProperty, collatt.AddButtonContentTemplate);

            //        if (SimpleTypes.Contains(p.PropertyType.GetGenericArguments()[0]))
            //            ButtonAddProperty.SetBinding(Button.CommandParameterProperty, new Binding("DataContext") { ElementName = p.Name + "PresentList" });
            //        else
            //            ButtonAddProperty.SetBinding(Button.CommandParameterProperty, new Binding("ItemsSource") { ElementName = p.Name + "PresentList" });
            //        ButtonAddProperty.SetValue(Grid.RowProperty, 0);

            //        ContainerProperty.AppendChild(ButtonAddProperty);
            //        #endregion

            //        #region Button create/clear
            //        FrameworkElementFactory ButtonNewProperty = new FrameworkElementFactory(typeof(Button));
            //        ButtonNewProperty.Name = p.Name + "clearButton";
            //        ButtonNewProperty.SetValue(Button.ContentProperty, "new");
            //        ButtonNewProperty.SetValue(Button.DataContextProperty, model);
            //        ButtonNewProperty.SetBinding(Button.CommandProperty, new Binding("ClearCommand"));
            //        ButtonNewProperty.SetBinding(Button.CommandParameterProperty, new Binding(".") { ElementName = p.Name + "clearButton" });
            //        ButtonNewProperty.SetValue(Grid.ColumnProperty, 1);

            //        if (collatt != null && collatt.NewButtonContentTemplate != null && Application.Current.TryFindResource(collatt.NewButtonContentTemplate) != null)
            //            ButtonNewProperty.SetResourceReference(Button.ContentTemplateProperty, collatt.NewButtonContentTemplate);

            //        ContainerProperty.AppendChild(ButtonNewProperty);
            //        #endregion

            //        #region List
            //        FrameworkElementFactory Property = new FrameworkElementFactory(typeof(ListBox));
            //        Property.Name = p.Name + "PresentList";
            //        Property.SetValue(ScrollViewer.HorizontalScrollBarVisibilityProperty, ScrollBarVisibility.Disabled);
            //        Property.SetValue(Grid.ColumnSpanProperty, 2);
            //        Property.SetValue(ListBox.HorizontalContentAlignmentProperty, System.Windows.HorizontalAlignment.Stretch);

            //        #region выставление шаблона по условию является спосок из простых данных или из объектов класса
            //        if (SimpleTypes.Contains(p.PropertyType.GetGenericArguments()[0]))
            //        {
            //            Property.SetResourceReference(ListBox.ItemTemplateProperty, "ItemPresentation");

            //            string pp = p.Name;
            //            Type h = p.PropertyType.GetGenericArguments()[0];
            //            Type b = p.PropertyType;
            //            Property.AddHandler(ListBox.LoadedEvent, new RoutedEventHandler((s, e) =>
            //            {
            //                var view = Activator.CreateInstance(typeof(AbstractCollectionView<>).MakeGenericType(h), null);

            //                BindingOperations.SetBinding(view as DependencyObject, BaseAbstract.ItemsProperty, new Binding(pp) { Source = (s as ListBox).DataContext, Mode = BindingMode.TwoWay });

            //                (s as ListBox).DataContext = view;
            //            }));

            //                Property.SetBinding(ListBox.ItemsSourceProperty, new Binding("AItems"));
            //        }
            //        else
            //        {
            //            Property.SetResourceReference(ListBox.ItemTemplateProperty, "CustomItemPresentation");

            //            Property.SetBinding(ListBox.ItemsSourceProperty, new Binding(p.Name));
            //        }
            //        #endregion

            //        Property.SetValue(Grid.RowProperty, 1);

            //        ContainerProperty.AppendChild(Property);
            //        #endregion

            //        #region Отребуты управления контейнером отображения элементов
            //        if (collatt != null)
            //        {
            //            if (collatt.MinHeight != 0)
            //                ContainerProperty.SetValue(Grid.MinHeightProperty, collatt.MinHeight);

            //            if (collatt.MaxHeight != 0)
            //                ContainerProperty.SetValue(Grid.MaxHeightProperty, collatt.MaxHeight);

            //            if (collatt.ItemTemplate != null && Application.Current.TryFindResource(collatt.ItemTemplate) != null)
            //                Property.SetResourceReference(ListBox.ItemTemplateProperty, collatt.ItemTemplate);

            //            if (collatt != null && collatt.ItemStyle != null && Application.Current.TryFindResource(collatt.ItemStyle) != null)
            //                Property.SetResourceReference(ListBox.ItemContainerStyleProperty, collatt.ItemStyle);
            //        } 
            //        #endregion

            //        Panel.AppendChild(ContainerProperty);
            //    }
            //    #endregion
            //    #region если является массивом
            //    else if (p.PropertyType.IsArray)//если используется массив строго заданного размера
            //    {//отличий от предыдущего почти никакого только нет кнопок очистить, добавить и удалить
            //        var collatt = atts.FirstOrDefault(x => x is PresentationCollectionInfo) as PresentationCollectionInfo;

            //        #region List
            //        FrameworkElementFactory Property = new FrameworkElementFactory(typeof(ListBox));
            //        Property.Name = p.Name + "PresentList";
            //        Property.SetValue(ScrollViewer.HorizontalScrollBarVisibilityProperty, ScrollBarVisibility.Disabled);
            //        Property.SetValue(Grid.ColumnSpanProperty, 2);
            //        Property.SetValue(ListBox.HorizontalContentAlignmentProperty, System.Windows.HorizontalAlignment.Stretch);
            //        if (SimpleTypes.Contains(p.PropertyType.GetElementType()))
            //        {
            //            Property.SetResourceReference(ListBox.ItemTemplateProperty, "SItemPresentation");

            //            string pp = p.Name;
            //            Type h = p.PropertyType.GetElementType();
            //            Property.AddHandler(ListBox.LoadedEvent, new RoutedEventHandler((s, e) =>
            //            {
            //                var view = Activator.CreateInstance(typeof(AbstractCollectionView<>).MakeGenericType(h), null);

            //                BindingOperations.SetBinding(view as DependencyObject, AbstractCollectionView<int>.ItemsProperty, new Binding(pp) { Source = (s as ListBox).DataContext, Mode = BindingMode.TwoWay });

            //                (s as ListBox).DataContext = view;
            //            }));

            //            Property.SetBinding(ListBox.ItemsSourceProperty, new Binding("AItems"));
            //        }
            //        else
            //        {
            //            Property.SetResourceReference(ListBox.ItemTemplateProperty, p.PropertyType.GetElementType().FullName);

            //            Property.SetBinding(ListBox.ItemsSourceProperty, new Binding(p.Name) { Mode = BindingMode.TwoWay });
            //        }

            //        Property.SetValue(Grid.RowProperty, 1);

            //        #region Отребуты управления контейнером отображения элементов
            //        if (collatt != null)
            //        {
            //            if (collatt.MinHeight != 0)
            //                Property.SetValue(Grid.MinHeightProperty, collatt.MinHeight);

            //            if (collatt.MaxHeight != 0)
            //                Property.SetValue(Grid.MaxHeightProperty, collatt.MaxHeight);

            //            if (collatt.ItemTemplate != null && Application.Current.Resources.Contains(collatt.ItemTemplate))
            //                Property.SetResourceReference(ListBox.ItemTemplateProperty, collatt.ItemTemplate);

            //            if (collatt.ItemStyle != null && Application.Current.TryFindResource(collatt.ItemStyle) != null)
            //                Property.SetResourceReference(ListBox.ItemContainerStyleProperty, collatt.ItemStyle);
            //        }
            //        #endregion

            //        Panel.AppendChild(Property);
            //        #endregion
            //    }
            //    #endregion
            //}

            template.VisualTree = Panel;

            return template;
        }

        /// <summary>
        /// Шаблон данных для элемента списка без кнопки удаления.
        /// </summary>
        /// <returns>Шаблон</returns>
        private static DataTemplate GenerateMemberSControl()
        {
            DataTemplate template = new DataTemplate();

            FrameworkElementFactory Property = new FrameworkElementFactory(typeof(TextBox));
            Property.SetBinding(TextBox.TextProperty, new Binding("Value") { Mode = BindingMode.TwoWay, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });

            template.VisualTree = Property;

            return template;
        }

        /// <summary>
        /// Шаблон данных для элемента списка с кнопкой удаления.
        /// </summary>
        /// <returns>Шаблон</returns>
        private static DataTemplate GenerateMemberControl()
        {
            DataTemplate template = new DataTemplate();

            FrameworkElementFactory ContainerProperty = new FrameworkElementFactory(typeof(Grid));
            ContainerProperty.Name = "BaseContainer";

            FrameworkElementFactory ContainerColumnProperty = new FrameworkElementFactory(typeof(ColumnDefinition));
            ContainerColumnProperty.SetValue(ColumnDefinition.WidthProperty, new GridLength(1, GridUnitType.Star));
            ContainerProperty.AppendChild(ContainerColumnProperty);

            FrameworkElementFactory ContainerColumn1Property = new FrameworkElementFactory(typeof(ColumnDefinition));
            ContainerColumn1Property.SetValue(ColumnDefinition.WidthProperty, new GridLength(40));
            ContainerProperty.AppendChild(ContainerColumn1Property);

            FrameworkElementFactory Property = new FrameworkElementFactory(typeof(TextBox));
            Property.SetBinding(TextBox.TextProperty, new Binding("Value") { Mode = BindingMode.TwoWay, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });
            ContainerProperty.AppendChild(Property);

            FrameworkElementFactory RemoveButton = new FrameworkElementFactory(typeof(Button));
            RemoveButton.Name = "removeButton";
            RemoveButton.SetValue(Button.ContentProperty, "x");
            //RemoveButton.SetValue(Button.DataContextProperty, new CollectionViewModel());
            RemoveButton.SetBinding(Button.CommandProperty, new Binding("RemoveCommand"));
            RemoveButton.SetBinding(Button.CommandParameterProperty, new Binding(".") { ElementName = "removeButton" });
            RemoveButton.SetValue(Grid.ColumnProperty, 1);
            ContainerProperty.AppendChild(RemoveButton);

            template.VisualTree = ContainerProperty;

            return template;
        }

        /// <summary>
        /// Специальный шаблон для элемента списка состовного (сложного) типа. 
        /// </summary>
        /// <returns>Шаблон</returns>
        private static DataTemplate GenerateCustomControl(bool RemoveOnHeader)
        {
            DataTemplate template = new DataTemplate();

            FrameworkElementFactory ParentConatinerProperty = new FrameworkElementFactory(typeof(Expander));
            ParentConatinerProperty.SetValue(Expander.HorizontalAlignmentProperty, System.Windows.HorizontalAlignment.Stretch);
            if (RemoveOnHeader)
                ParentConatinerProperty.SetResourceReference(Expander.HeaderTemplateProperty, "CustomHeaderPresentation");
            else
                ParentConatinerProperty.SetResourceReference(Expander.HeaderTemplateProperty, "SCustomHeaderPresentation");
            ParentConatinerProperty.SetBinding(Expander.HeaderProperty, new Binding("."));
            ParentConatinerProperty.SetBinding(Expander.IsExpandedProperty, new Binding("IsSelected") { RelativeSource = new RelativeSource() { AncestorType = typeof(ListBoxItem), Mode = RelativeSourceMode.FindAncestor }, Mode = BindingMode.TwoWay, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });
            #region Content
            
            FrameworkElementFactory Property = new FrameworkElementFactory(typeof(Label));
            Property.SetValue(Label.HorizontalContentAlignmentProperty, System.Windows.HorizontalAlignment.Stretch);
            Property.SetBinding(Label.ContentProperty, new Binding("Value") { Mode = BindingMode.TwoWay, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });
            Property.SetBinding(Label.ContentTemplateProperty, new Binding("Value.Template"));
            ParentConatinerProperty.AppendChild(Property);
            template.VisualTree = ParentConatinerProperty;

            #endregion
            return template;
        }

        /// <summary>
        /// Специальный шаблон заголовка элемента списка состовного (сложного) типа.
        /// </summary>
        /// <returns>Шаблон</returns>
        private static DataTemplate GenerateHeaderCustomControl(bool buttonRemove)
        {
            DataTemplate template = new DataTemplate();

            FrameworkElementFactory ContainerProperty;

            #region Header

            if (buttonRemove)
            {
                #region Grid
                ContainerProperty = new FrameworkElementFactory(typeof(Grid));
                ContainerProperty.Name = "BaseContainer";

                FrameworkElementFactory ContainerColumnProperty = new FrameworkElementFactory(typeof(ColumnDefinition));
                ContainerColumnProperty.SetValue(ColumnDefinition.WidthProperty, new GridLength(1, GridUnitType.Star));
                ContainerProperty.AppendChild(ContainerColumnProperty);

                FrameworkElementFactory ContainerColumn1Property = new FrameworkElementFactory(typeof(ColumnDefinition));
                ContainerColumn1Property.SetValue(ColumnDefinition.WidthProperty, new GridLength(40));
                ContainerProperty.AppendChild(ContainerColumn1Property);
                #endregion

                #region TextBlock
                FrameworkElementFactory HeaderTextProperty = new FrameworkElementFactory(typeof(TextBlock));
                HeaderTextProperty.SetBinding(TextBlock.TextProperty, new Binding("Value") { UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });
                HeaderTextProperty.SetValue(TextBlock.VerticalAlignmentProperty, System.Windows.VerticalAlignment.Center);
                ContainerProperty.AppendChild(HeaderTextProperty);
                #endregion

                #region RewmoveButton
                FrameworkElementFactory RemoveButton = new FrameworkElementFactory(typeof(Button));
                RemoveButton.Name = "removeButton";
                RemoveButton.SetValue(Button.ContentProperty, "x");
                RemoveButton.SetBinding(Button.CommandProperty, new Binding("DataContext.RemoveCommand") { RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(Grid), 3) });
                RemoveButton.SetBinding(Button.CommandParameterProperty, new Binding("DataContext.Value") { ElementName = "removeButton" });
                RemoveButton.SetValue(Grid.ColumnProperty, 1);

                ContainerProperty.AppendChild(RemoveButton);
                #endregion
            }
            else
            {
                #region TextBlock
                ContainerProperty = new FrameworkElementFactory(typeof(TextBlock));
                ContainerProperty.SetBinding(TextBlock.TextProperty, new Binding("Value") { UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });
                ContainerProperty.SetValue(TextBlock.VerticalAlignmentProperty, System.Windows.VerticalAlignment.Center);
                #endregion
            }
            #endregion

            template.VisualTree = ContainerProperty;

            return template;
        }

        /// <summary>
        /// Входной метод для обработки типов данных по указанным параметрам.
        /// </summary>
        /// <param name="application">Приложения, в чьи ресурсы будету добавлены шаблоны.</param>
        /// <param name="namespaceName">Строковое значение прстранства имен.</param>
        /// <param name="contains">Параметр определяющий способ обработки пространства имен.</param>
        internal static void CreateDynamicResourceSimpleClass(Application application, string namespaceName, bool contains)
        {
            //Item template
            if (application.TryFindResource("ItemPresentation") == null)
                application.Resources.Add("ItemPresentation", GenerateMemberControl());

            if (application.TryFindResource("CustomItemPresentation") == null)
                application.Resources.Add("CustomItemPresentation", GenerateCustomControl(true));

            if (application.TryFindResource("CustomHeaderPresentation") == null)
                application.Resources.Add("CustomHeaderPresentation", GenerateHeaderCustomControl(true));

            if (application.TryFindResource("SCustomItemPresentation") == null)
                application.Resources.Add("SCustomItemPresentation", GenerateCustomControl(false));

            if (application.TryFindResource("SCustomHeaderPresentation") == null)
                application.Resources.Add("SCustomHeaderPresentation", GenerateHeaderCustomControl(false));

            if (application.TryFindResource("SItemPresentation") == null)
                application.Resources.Add("SItemPresentation", GenerateMemberSControl());

            //simple type of IPresentation
            Type[] types = GetSimpleClassTypes(namespaceName, contains);
            foreach (Type t in GetSimpleClassTypes(namespaceName, contains))
            {
                if (application.TryFindResource(t.FullName) == null)
                {
                    DataTemplate template = _GetControl(application, t);
                    application.Resources.Add(t.FullName, template);
                }
            }
        }

        /// <summary>
        /// Входной метод для обработки типов данных по указанным параметрам.
        /// </summary>
        /// <param name="application">Приложения, в чьи ресурсы будету добавлены шаблоны.</param>
        /// <param name="namespaceName">Список строковых значений пространст имен.</param>
        internal static void CreateDynamicResourceSimpleClass(Application application, string[] namespaceName)
        {
            //Item template
            if (application.TryFindResource("ItemPresentation") == null)
                application.Resources.Add("ItemPresentation", GenerateMemberControl());

            if (application.TryFindResource("CustomItemPresentation") == null)
                application.Resources.Add("CustomItemPresentation", GenerateCustomControl(true));

            if (application.TryFindResource("CustomHeaderPresentation") == null)
                application.Resources.Add("CustomHeaderPresentation", GenerateHeaderCustomControl(true));

            if (application.TryFindResource("SCustomItemPresentation") == null)
                application.Resources.Add("SCustomItemPresentation", GenerateCustomControl(false));

            if (application.TryFindResource("SCustomHeaderPresentation") == null)
                application.Resources.Add("SCustomHeaderPresentation", GenerateHeaderCustomControl(false));

            if (application.TryFindResource("SItemPresentation") == null)
                application.Resources.Add("SItemPresentation", GenerateMemberSControl());

            //simple type of IPresentation
            foreach (Type t in GetSimpleClassTypes(namespaceName))
            {
                if (application.TryFindResource(t.FullName) == null)
                {
                    DataTemplate template = GetControl(t);
                    application.Resources.Add(t.FullName, template);
                }
            }
        }
    }
}
