using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Windows;
using NTW.Presentation.Attributes;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Controls.Primitives;

namespace NTW.Presentation.Construction
{
    internal static class TypeBuilder
    {
        /// <summary>
        /// Массив типов являющимися типоми простых значений (число, строка и т.д.)
        /// </summary>
        internal static Type[] SimpleTypes = new Type[] { 
            typeof(byte), typeof(sbyte),  
            typeof(short), typeof(ushort), 
            typeof(int), typeof(uint),
            typeof(long), typeof(ulong),
            typeof(float), typeof(double), typeof(decimal),
            typeof(char), typeof(string),
            typeof(bool)
        };

        /// <summary>
        /// Получение массива типов для последующего формирования шаблонов отображения из указанного пространства имен.
        /// </summary>
        /// <param name="namespaceName">Строковое значение пространства имен.</param>
        /// <param name="contains">Параметер определяющий способ проверки пространства имен.</param>
        /// <returns>Массив типов</returns>
        private static Type[] GetTypes(Func<Type, bool> condition)
        {
            List<Type> result = new List<Type>();

            if (condition != null)
                result = Assembly.GetEntryAssembly().GetTypes().Where(condition).Where(t => !t.IsGenericType).ToList();

            return result.ToArray();
        }

        private static DataTemplate CreateTemplate(Type type)
        {
            DataTemplate template = new DataTemplate();
            template.DataType = type;

            #region Атребуты (над классом)
            NonPresentation nonPresenatry = System.Attribute.GetCustomAttributes(type).ToList().Find((x) => x is NonPresentation) as NonPresentation;
            PresentationMarginInfo marginPresentary = System.Attribute.GetCustomAttributes(type).ToList().Find((x) => x is PresentationMarginInfo) as PresentationMarginInfo;
            PresentationPaddingInfo paddingPresentary = System.Attribute.GetCustomAttributes(type).ToList().Find((x) => x is PresentationPaddingInfo) as PresentationPaddingInfo;
            #endregion

            #region Основа (контейнер) в которую будут вкладыватся элементы
            FrameworkElementFactory Panel = new FrameworkElementFactory(typeof(StackPanel));
            Panel.Name = "BackPanel";

            Panel.SetValue(StackPanel.MarginProperty, GetThickness(marginPresentary));
            Panel.SetValue(Control.PaddingProperty, GetThickness(paddingPresentary));
            #endregion

            #region Свойства
            if(nonPresenatry == null)
                foreach (PropertyInfo property in type.GetProperties())
                {
                    #region извлечение атрибутов
                    List<System.Attribute> attributes = System.Attribute.GetCustomAttributes(property).ToList();

                    NonPresentation nattr = attributes.Find((x) => x is NonPresentation) as NonPresentation;
                    PresentationInfo pAttr = attributes.Find((x) => x is PresentationInfo) as PresentationInfo;
                    PresentationCollectionInfo pcAttr = attributes.Find((x) => x is PresentationCollectionInfo) as PresentationCollectionInfo;
                    PresentationMarginInfo pmAttr = attributes.Find((x) => x is PresentationMarginInfo) as PresentationMarginInfo;
                    #endregion
                    if (nattr == null)
                    {
                        #region Основной контейнер для свойств
                        FrameworkElementFactory ContainerPanel = new FrameworkElementFactory(typeof(StackPanel));
                        ContainerPanel.SetValue(TextBlock.MarginProperty, GetThickness(pmAttr));

                        ContainerPanel.AppendChild(CreateCaption(property.Name, pAttr));
                        #endregion

                        #region конструирование шаблона для свойств
                        if (SimpleTypes.Contains(property.PropertyType))
                            ContainerPanel.AppendChild(CreateSimpleType(property));

                        else if (property.PropertyType.BaseType == typeof(Enum))
                            ContainerPanel.AppendChild(CreateEnumType(property));

                        else if (property.PropertyType.IsClass) {

                            var interfaces = property.PropertyType.GetInterfaces();

                            if (property.PropertyType.IsArray)
                                ContainerPanel.AppendChild(CreateArrayType(property));
                            else if (property.PropertyType.GetInterface("IList") != null)
                                ContainerPanel.AppendChild(CreateListType(property));
                            else if(property.PropertyType.GetInterface("IDictionary") != null)
                                ContainerPanel.AppendChild(CreateDictionaryType(property));
                            else if (property.PropertyType.IsGenericType)
                                if (property.PropertyType.GetInterface((typeof(IList<>).Name)) != null)
                                    ContainerPanel.AppendChild(CreateListGenericType(property));
                                else
                                    ContainerPanel.AppendChild(CreateClassType(property));
                        }
                        #endregion

                        Panel.AppendChild(ContainerPanel);
                    }
                }
            #endregion

            template.VisualTree = Panel;

            return template;
        }

        private static FrameworkElementFactory CreateCaption(string propertyName, PresentationInfo pAttr)
        {
            FrameworkElementFactory Caption = new FrameworkElementFactory(typeof(TextBlock));
            Caption.SetValue(TextBlock.TextProperty, pAttr != null ? pAttr.CaptionName : propertyName);
            Caption.SetValue(TextBlock.TextWrappingProperty, pAttr != null ? pAttr.PresentCaption : TextWrapping.NoWrap);
            return Caption;
        }

        #region обработки типов по признаку
        //private static FrameworkElementFactory CreateCommandType(PropertyInfo property)
        //{
        //    FrameworkElementFactory Property = new FrameworkElementFactory(typeof(Button));
        //    //вызов одинарной команды
        //    Property.SetBinding(Button.CommandProperty, new Binding(property.Name));
        //    Property.SetValue(Button.ContentProperty, "Run");
        //    return Property;
        //}

        private static FrameworkElementFactory CreateSimpleType(PropertyInfo property)
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
                if (property.PropertyType == typeof(bool))
                {
                    Property = new FrameworkElementFactory(typeof(ToggleButton));
                    Property.SetBinding(ToggleButton.ContentProperty, new Binding(property.Name));
                    Property.SetBinding(CheckBox.IsCheckedProperty, new Binding(property.Name) {
                        UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                        Mode = BindingMode.TwoWay
                    });
                }
                else
                {
                    Property = new FrameworkElementFactory(typeof(TextBox));
                    Property.SetBinding(TextBox.TextProperty, new Binding(property.Name) {
                        UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                        Mode = BindingMode.TwoWay
                    });
                }
            }
            return Property;

        }

        private static FrameworkElementFactory CreateEnumType(PropertyInfo property) {
            FrameworkElementFactory Property;
            if (!property.CanWrite) {
                Property = new FrameworkElementFactory(typeof(TextBlock));
                Property.SetBinding(TextBlock.TextProperty, new Binding(property.Name) {
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                    Mode = BindingMode.OneWay
                });
            }
            else {
                Property = new FrameworkElementFactory(typeof(ComboBox));
                Property.SetResourceReference(ComboBox.DataContextProperty, property.PropertyType.FullName);
                Property.SetBinding(ComboBox.ItemsSourceProperty, new Binding("."));

                EnumBuilder.AddEnumResource(property.PropertyType);

                Property.SetBinding(ComboBox.SelectedValueProperty, new Binding("DataContext." + property.Name) {
                    ElementName = "BackPanel",
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                });
            }
            return Property;
        }

        private static FrameworkElementFactory CreateClassType(PropertyInfo property) {
            FrameworkElementFactory Property = new FrameworkElementFactory(typeof(Label));
            AddTemplateToResource(property.PropertyType);
            Property.SetBinding(Label.DataContextProperty, new Binding(property.Name));
            Property.SetBinding(Label.ContentProperty, new Binding("."));
            Property.SetValue(Label.HorizontalContentAlignmentProperty, System.Windows.HorizontalAlignment.Stretch);
            return Property;
        }

        private static FrameworkElementFactory CreateArrayType(PropertyInfo property) {
            Type AType = property.PropertyType.GetElementType();
            if (SimpleTypes.Contains(AType) || AType.BaseType == typeof(Enum)) {
                Type BType = typeof(ArrayItemsControl<>).MakeGenericType(AType);
                FrameworkElementFactory Property = new FrameworkElementFactory(BType);
                Property.SetValue(ScrollViewer.HorizontalScrollBarVisibilityProperty, ScrollBarVisibility.Disabled);
                Property.SetValue(ItemsControl.HorizontalContentAlignmentProperty, HorizontalAlignment.Stretch);
                Property.SetBinding(BaseItemsControl.ContextProperty, new Binding(property.Name));

                if (AType.BaseType == typeof(Enum)) {
                    AddTemplateEnumToResource(AType);
                    Property.SetResourceReference(ItemsControl.ItemTemplateProperty, "T_" + AType.FullName);
                }
                else if (AType == typeof(bool))
                    Property.SetResourceReference(ItemsControl.ItemTemplateProperty, "ItemBoolen");
                else
                    Property.SetResourceReference(ItemsControl.ItemTemplateProperty, "ItemSimple");

                return Property;
            }
            else {
                FrameworkElementFactory Property = new FrameworkElementFactory(typeof(ItemsControl));
                Property.SetValue(ScrollViewer.HorizontalScrollBarVisibilityProperty, ScrollBarVisibility.Disabled);
                Property.SetValue(ItemsControl.HorizontalContentAlignmentProperty, HorizontalAlignment.Stretch);
                AddTemplateToResource(AType);
                Property.SetBinding(ItemsControl.ItemsSourceProperty, new Binding(property.Name));
                Property.SetResourceReference(ItemsControl.ItemTemplateProperty, new DataTemplateKey(AType));
                return Property;
            }
        }

        private static FrameworkElementFactory CreateListType(PropertyInfo property) {
            Type AType = property.PropertyType.GetGenericArguments()[0];
            Type BType = typeof(ListItemsControl<>).MakeGenericType(AType);
            FrameworkElementFactory Property = new FrameworkElementFactory(BType);

            Property.SetValue(ScrollViewer.HorizontalScrollBarVisibilityProperty, ScrollBarVisibility.Disabled);
            Property.SetValue(ItemsControl.HorizontalContentAlignmentProperty, HorizontalAlignment.Stretch);

            if (SimpleTypes.Contains(AType) || AType.BaseType == typeof(Enum))
                Property.SetBinding(BaseItemsControl.ContextProperty, new Binding(property.Name));
            else
                Property.SetBinding(BaseItemsControl.ItemsSourceProperty, new Binding(property.Name));
            if (AType.BaseType == typeof(Enum)) {
                AddTemplateEnumToResource(AType, true);
                Property.SetResourceReference(ItemsControl.ItemTemplateProperty, "T_" + AType.FullName);
            }
            else if (AType == typeof(bool))
                Property.SetResourceReference(ItemsControl.ItemTemplateProperty, "ItemBoolenD");
            else if (SimpleTypes.Contains(AType))
                Property.SetResourceReference(ItemsControl.ItemTemplateProperty, "ItemSimpleD");
            else {
                AddTemplateToResource(AType);
                Property.SetResourceReference(ItemsControl.ItemTemplateProperty, "ItemClassD");
            }
            return Property;
        }

        private static FrameworkElementFactory CreateListGenericType(PropertyInfo property) {
            Type AType = property.PropertyType.GetGenericArguments()[0];
            Type BType = typeof(ListGenericItemsControl<>).MakeGenericType(AType);
            FrameworkElementFactory Property = new FrameworkElementFactory(BType);

            Property.SetValue(ScrollViewer.HorizontalScrollBarVisibilityProperty, ScrollBarVisibility.Disabled);
            Property.SetValue(ItemsControl.HorizontalContentAlignmentProperty, HorizontalAlignment.Stretch);

            if (SimpleTypes.Contains(AType) || AType.BaseType == typeof(Enum))
                Property.SetBinding(BaseItemsControl.ContextProperty, new Binding(property.Name));
            else
                Property.SetBinding(BaseItemsControl.ItemsSourceProperty, new Binding(property.Name));
            if (AType.BaseType == typeof(Enum)) {
                AddTemplateEnumToResource(AType, true);
                Property.SetResourceReference(ItemsControl.ItemTemplateProperty, "T_" + AType.FullName);
            }
            else if (AType == typeof(bool))
                Property.SetResourceReference(ItemsControl.ItemTemplateProperty, "ItemBoolenD");
            else if (SimpleTypes.Contains(AType))
                Property.SetResourceReference(ItemsControl.ItemTemplateProperty, "ItemSimpleD");
            else {
                AddTemplateToResource(AType);
                Property.SetResourceReference(ItemsControl.ItemTemplateProperty, "ItemClassD");
            }
            return Property;
        }

        private static FrameworkElementFactory CreateDictionaryType(PropertyInfo property) { 
            Type KType = property.PropertyType.GetGenericArguments()[0];
            Type VType = property.PropertyType.GetGenericArguments()[1];
            Type MType = typeof(DictionaryItemsControl<,>).MakeGenericType(KType, VType);
            FrameworkElementFactory Property = new FrameworkElementFactory(MType);

            Property.SetValue(ScrollViewer.HorizontalScrollBarVisibilityProperty, ScrollBarVisibility.Disabled);
            Property.SetValue(ItemsControl.HorizontalContentAlignmentProperty, HorizontalAlignment.Stretch);

            Property.SetBinding(BaseItemsControl.ContextProperty, new Binding(property.Name));

            Property.SetValue(ItemsControl.ItemTemplateProperty, GenerateItemDictionaryTemplate(VType));
            return Property;
        }

        //private static FrameworkElementFactory CreateGenericListType(PropertyInfo property, PresentationCollectionInfo pcAttr)
        //{
        //    ListTypeToCreate.Add(property.PropertyType.GetGenericArguments()[0]);

        //    FrameworkElementFactory BaseContainer = new FrameworkElementFactory(typeof(Grid));
        //    FrameworkElementFactory Container = new FrameworkElementFactory(typeof(Grid));

        //    #region Формирование модели если требуется
        //    //попробуем без спецефичного формирования объекта
        //    //увы, без него никак
        //    Type GType = property.PropertyType.GetGenericArguments()[0];
        //    string propertyName = property.Name;
        //    Container.AddHandler(ListBox.LoadedEvent, new RoutedEventHandler((s, e) =>
        //    {
        //        var view = Activator.CreateInstance(typeof(CollectionViewModel<>).MakeGenericType(GType), null);
        //        BindingOperations.SetBinding(view as DependencyObject, AbstractView.ItemsProperty, new Binding(propertyName) { Source = ((s as Grid).Parent as Grid).DataContext, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });
        //        (s as Grid).DataContext = view;
        //    }));
        //    #endregion

        //    //что делать если несколько типов генерации как в Dictionary?
        //    #region Формирование Grid
        //    FrameworkElementFactory ContainerRowProperty = new FrameworkElementFactory(typeof(RowDefinition));
        //    ContainerRowProperty.SetValue(RowDefinition.HeightProperty, new GridLength(40));
        //    Container.AppendChild(ContainerRowProperty);

        //    FrameworkElementFactory ContainerRow1Property = new FrameworkElementFactory(typeof(RowDefinition));
        //    ContainerRow1Property.SetValue(RowDefinition.HeightProperty, new GridLength(1, GridUnitType.Star));
        //    Container.AppendChild(ContainerRow1Property);

        //    FrameworkElementFactory ContainerColumnProperty = new FrameworkElementFactory(typeof(ColumnDefinition));
        //    ContainerColumnProperty.SetValue(ColumnDefinition.WidthProperty, new GridLength(1, GridUnitType.Star));
        //    Container.AppendChild(ContainerColumnProperty);

        //    FrameworkElementFactory ContainerColumn1Property = new FrameworkElementFactory(typeof(ColumnDefinition));
        //    ContainerColumn1Property.SetValue(ColumnDefinition.WidthProperty, new GridLength(40));
        //    Container.AppendChild(ContainerColumn1Property);
        //    #endregion

        //    #region Button "Add"
        //    FrameworkElementFactory ButtonAdd = new FrameworkElementFactory(typeof(Button));
        //    ButtonAdd.SetValue(Button.ContentProperty, "+");
        //    ButtonAdd.SetBinding(Button.CommandProperty, new Binding("AddCommand"));

        //    if (pcAttr != null && pcAttr.AddButtonContentTemplate != null && Application.Current.TryFindResource(pcAttr.AddButtonContentTemplate) != null)
        //        ButtonAdd.SetResourceReference(Button.ContentTemplateProperty, pcAttr.AddButtonContentTemplate);

        //    Container.AppendChild(ButtonAdd);
        //    #endregion

        //    #region Button "Clear"
        //    FrameworkElementFactory ButtonClear = new FrameworkElementFactory(typeof(Button));
        //    ButtonClear.Name = property.Name + "clearButton";
        //    ButtonClear.SetValue(Button.ContentProperty, "clear");
        //    ButtonClear.SetBinding(Button.CommandProperty, new Binding("ClearCommand"));
        //    ButtonClear.SetBinding(Button.CommandParameterProperty, new Binding(".") { ElementName = property.Name + "clearButton" });
        //    ButtonClear.SetValue(Grid.ColumnProperty, 1);

        //    if (pcAttr != null && pcAttr.ClearButtonContentTemplate != null && Application.Current.TryFindResource(pcAttr.ClearButtonContentTemplate) != null)
        //        ButtonClear.SetResourceReference(Button.ContentTemplateProperty, pcAttr.ClearButtonContentTemplate);

        //    Container.AppendChild(ButtonClear);
        //    #endregion

        //    #region List
        //    FrameworkElementFactory List = new FrameworkElementFactory(typeof(ListBox));
        //    List.SetValue(ScrollViewer.HorizontalScrollBarVisibilityProperty, ScrollBarVisibility.Disabled);
        //    List.SetValue(Grid.ColumnSpanProperty, 2);
        //    List.SetValue(Grid.RowProperty, 1);
        //    List.SetValue(ListBox.HorizontalContentAlignmentProperty, System.Windows.HorizontalAlignment.Stretch);
        //    List.SetBinding(ListBox.ItemsSourceProperty, new Binding("AItems"));

        //    //как быть с шаблон данных
        //    if (SimpleTypes.Contains(property.PropertyType.GetGenericArguments()[0]))
        //        List.SetResourceReference(ListBox.ItemTemplateProperty, "ItemPresentation");
        //    else
        //    {
        //        List.SetResourceReference(ListBox.ItemTemplateProperty, "CustomItemPresentation");
        //        //List.SetBinding(ListBox.ItemsSourceProperty, new Binding("Items"));
        //    }
        //    Container.AppendChild(List);
        //    #endregion

        //    #region обработка аттрибута коллекции
        //    if (pcAttr != null)
        //    {
        //        if (pcAttr.MinHeight != 0)
        //            BaseContainer.SetValue(Grid.MinHeightProperty, pcAttr.MinHeight);

        //        if (pcAttr.MaxHeight != 0)
        //            BaseContainer.SetValue(Grid.MaxHeightProperty, pcAttr.MaxHeight);

        //        if (pcAttr.ItemTemplate != null && Application.Current.TryFindResource(pcAttr.ItemTemplate) != null)
        //            List.SetResourceReference(ListBox.ItemTemplateProperty, pcAttr.ItemTemplate);

        //        if (pcAttr != null && pcAttr.ItemStyle != null && Application.Current.TryFindResource(pcAttr.ItemStyle) != null)
        //            List.SetResourceReference(ListBox.ItemContainerStyleProperty, pcAttr.ItemStyle);
        //    }
        //    #endregion

        //    BaseContainer.AppendChild(Container);

        //    return BaseContainer;
        //}

        //private static FrameworkElementFactory CreateGenericDictionaryType(PropertyInfo property)
        //{
        //    FrameworkElementFactory Container = new FrameworkElementFactory(typeof(Grid));

        //    #region Подгрузка
        //    Type[] generics = property.PropertyType.GetGenericArguments();
        //    string PropertyName = property.Name;

        //    Container.AddHandler(Grid.LoadedEvent, new RoutedEventHandler((s, e) =>
        //    {
        //        var view = Activator.CreateInstance(typeof(DictionaryViewModel<,>).MakeGenericType(generics), null);
        //        BindingOperations.SetBinding(view as DependencyObject, AbstractDictionaryView.ItemsProperty, new Binding(PropertyName) { Source = (s as Grid).DataContext, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });
        //        (s as Grid).DataContext = view;
        //    }));
        //    #endregion

        //    #region Строки и колонки
        //    FrameworkElementFactory Row1 = new FrameworkElementFactory(typeof(RowDefinition));
        //    Row1.SetValue(RowDefinition.HeightProperty, new GridLength(22));
        //    Container.AppendChild(Row1);

        //    FrameworkElementFactory Row2 = new FrameworkElementFactory(typeof(RowDefinition));
        //    Row2.SetValue(RowDefinition.HeightProperty, new GridLength(1, GridUnitType.Star));
        //    Container.AppendChild(Row2);

        //    FrameworkElementFactory Row3 = new FrameworkElementFactory(typeof(RowDefinition));
        //    Row3.SetValue(RowDefinition.HeightProperty, new GridLength(22));
        //    Container.AppendChild(Row3);

        //    FrameworkElementFactory Column1 = new FrameworkElementFactory(typeof(ColumnDefinition));
        //    Column1.SetValue(ColumnDefinition.WidthProperty, new GridLength(1, GridUnitType.Star));
        //    Container.AppendChild(Column1);

        //    FrameworkElementFactory Column2 = new FrameworkElementFactory(typeof(ColumnDefinition));
        //    Column2.SetValue(ColumnDefinition.WidthProperty, new GridLength(1, GridUnitType.Star));
        //    Container.AppendChild(Column2);
        //    #endregion

        //    //1. ComboBox с возможностью выбора определенного ключа с дальнейшим отображением значения по ключу
        //    #region Отборка
        //    FrameworkElementFactory Combo = new FrameworkElementFactory(typeof(ComboBox));
        //    Combo.SetValue(Grid.ColumnSpanProperty, 2);
        //    Combo.SetBinding(ComboBox.ItemsSourceProperty, new Binding("MKeys"));
        //    Combo.SetBinding(ComboBox.SelectedItemProperty, new Binding("SelectedKey") { Mode = BindingMode.TwoWay, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });
        //    Container.AppendChild(Combo);
        //    #endregion

        //    //2. Label с шаблоном для отображения значения
        //    #region Отображение
        //    FrameworkElementFactory Lab = new FrameworkElementFactory(typeof(Label));
        //    Lab.SetValue(Grid.RowProperty, 1);
        //    Lab.SetValue(Grid.ColumnSpanProperty, 2);
        //    Lab.SetValue(Label.HorizontalContentAlignmentProperty, System.Windows.HorizontalAlignment.Stretch);
        //    //если это словарь то оно обладает двумя типами под generic и второй из них это значение
        //    if (SimpleTypes.Contains(property.PropertyType.GetGenericArguments()[1]))
        //    {
        //        Lab.SetBinding(Label.ContentProperty, new Binding(".") { UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });
        //        Lab.SetResourceReference(Label.ContentTemplateProperty, "SItemPresentation");
        //    }
        //    else
        //    {
        //        Lab.SetBinding(Label.ContentProperty, new Binding("Value") { UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });
        //    }

        //    Container.AppendChild(Lab);
        //    #endregion

        //    //3. Button  с возможностью добавления
        //    #region Add
        //    FrameworkElementFactory Add = new FrameworkElementFactory(typeof(Button));
        //    Add.SetValue(Grid.RowProperty, 2);
        //    Add.SetValue(Button.ContentProperty, "Add");
        //    Add.SetBinding(Button.CommandProperty, new Binding("AddCommand"));
        //    Container.AppendChild(Add);
        //    #endregion

        //    #region Remove
        //    FrameworkElementFactory Remove = new FrameworkElementFactory(typeof(Button));
        //    Remove.SetValue(Grid.RowProperty, 2);
        //    Remove.SetValue(Grid.ColumnProperty, 1);
        //    Remove.SetValue(Button.ContentProperty, "Remove");
        //    Remove.SetBinding(Button.CommandProperty, new Binding("RemoveCommand"));
        //    Container.AppendChild(Remove);
        //    #endregion

        //    return Container;
        //}

        //private static FrameworkElementFactory CreateGenericSimpleType(PropertyInfo property)
        //{
        //    //добавить в очередь на добавление
        //    ListTypeToCreate.Add(property.PropertyType);
        //    return CreateClassType(property);
        //}

        //private static FrameworkElementFactory CreateClassType(PropertyInfo property)
        //{
        //    FrameworkElementFactory Property = new FrameworkElementFactory(typeof(Label));
        //    Property.SetBinding(Label.DataContextProperty, new Binding(property.Name));
        //    Property.SetBinding(Label.ContentProperty, new Binding("."));
        //    Property.SetValue(Label.HorizontalContentAlignmentProperty, System.Windows.HorizontalAlignment.Stretch);
        //    return Property;
        //}

        //private static FrameworkElementFactory CreateArrayType(PropertyInfo property, PresentationCollectionInfo pcAttr)
        //{
        //    ListTypeToCreate.Add(property.PropertyType.GetElementType());

        //    #region List
        //    FrameworkElementFactory List = new FrameworkElementFactory(typeof(ListBox));

        //    List.SetValue(ScrollViewer.HorizontalScrollBarVisibilityProperty, ScrollBarVisibility.Disabled);
        //    List.SetValue(ListBox.HorizontalContentAlignmentProperty, System.Windows.HorizontalAlignment.Stretch);
        //    List.SetBinding(ListBox.ItemsSourceProperty, new Binding("AItems"));

        //    #region Формирование модели если требуется
        //    //попробуем без спецефичного формирования объекта
        //    //увы, без него никак
        //    Type GType = property.PropertyType.GetElementType();
        //    string propertyName = property.Name;
        //    List.AddHandler(ListBox.LoadedEvent, new RoutedEventHandler((s, e) =>
        //    {
        //        var view = Activator.CreateInstance(typeof(CollectionViewModel<>).MakeGenericType(GType), null);
        //        BindingOperations.SetBinding(view as DependencyObject, AbstractView.ItemsProperty, new Binding(propertyName) { Source = (s as ListBox).DataContext, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });
        //        (s as ListBox).DataContext = view;
        //    }));
        //    #endregion

        //    //как быть с шаблон данных
        //    if (SimpleTypes.Contains(property.PropertyType.GetElementType()))
        //        List.SetResourceReference(ListBox.ItemTemplateProperty, "SItemPresentation");
        //    else
        //    {
        //        List.SetResourceReference(ListBox.ItemTemplateProperty, "SCustomItemPresentation");
        //    }
        //    #endregion

        //    #region Выставление атрибутов
        //    if (pcAttr != null)
        //    {
        //        if (pcAttr.MinHeight != 0)
        //            List.SetValue(Grid.MinHeightProperty, pcAttr.MinHeight);

        //        if (pcAttr.MaxHeight != 0)
        //            List.SetValue(Grid.MaxHeightProperty, pcAttr.MaxHeight);

        //        if (pcAttr.ItemTemplate != null && Application.Current.TryFindResource(pcAttr.ItemTemplate) != null)
        //            List.SetResourceReference(ListBox.ItemTemplateProperty, pcAttr.ItemTemplate);

        //        if (pcAttr != null && pcAttr.ItemStyle != null && Application.Current.TryFindResource(pcAttr.ItemStyle) != null)
        //            List.SetResourceReference(ListBox.ItemContainerStyleProperty, pcAttr.ItemStyle);
        //    }
        //    #endregion

        //    return List;
        //}
        #endregion

        #region обертки
        private static DataTemplate GenerateItemSimpleTemplate(bool delete = false) {
            DataTemplate template = new DataTemplate();
            FrameworkElementFactory Container = new FrameworkElementFactory(typeof(Grid));
            if (delete) {
                FrameworkElementFactory ContainerColumnProperty = new FrameworkElementFactory(typeof(ColumnDefinition));
                ContainerColumnProperty.SetValue(ColumnDefinition.WidthProperty, new GridLength(1, GridUnitType.Star));
                Container.AppendChild(ContainerColumnProperty);

                FrameworkElementFactory ContainerColumn1Property = new FrameworkElementFactory(typeof(ColumnDefinition));
                ContainerColumn1Property.SetValue(ColumnDefinition.WidthProperty, new GridLength(40));
                Container.AppendChild(ContainerColumn1Property);

                FrameworkElementFactory DeleteButton = new FrameworkElementFactory(typeof(Button));
                DeleteButton.SetValue(Button.ContentProperty, "X");
                DeleteButton.SetValue(Grid.ColumnProperty, 1);
                DeleteButton.SetBinding(Button.CommandProperty, new Binding("RemoveCommand") {
                    RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(BaseItemsControl), 1),
                });
                DeleteButton.SetBinding(Button.CommandParameterProperty, new Binding("."));

                Container.AppendChild(DeleteButton);
            }
            FrameworkElementFactory Property = new FrameworkElementFactory(typeof(TextBox));
            Property.SetBinding(TextBox.TextProperty, new Binding("Value") { Mode = BindingMode.TwoWay, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });
            Container.AppendChild(Property);
            template.VisualTree = Container;
            return template;
        }

        private static DataTemplate GenerateItemBoolenTemplate(bool delete = false) {
            DataTemplate template = new DataTemplate();
            FrameworkElementFactory Container = new FrameworkElementFactory(typeof(Grid));
            if (delete) {
                FrameworkElementFactory ContainerColumnProperty = new FrameworkElementFactory(typeof(ColumnDefinition));
                ContainerColumnProperty.SetValue(ColumnDefinition.WidthProperty, new GridLength(1, GridUnitType.Star));
                Container.AppendChild(ContainerColumnProperty);

                FrameworkElementFactory ContainerColumn1Property = new FrameworkElementFactory(typeof(ColumnDefinition));
                ContainerColumn1Property.SetValue(ColumnDefinition.WidthProperty, new GridLength(40));
                Container.AppendChild(ContainerColumn1Property);

                FrameworkElementFactory DeleteButton = new FrameworkElementFactory(typeof(Button));
                DeleteButton.SetValue(Button.ContentProperty, "X");
                DeleteButton.SetValue(Grid.ColumnProperty, 1);
                DeleteButton.SetBinding(Button.CommandProperty, new Binding("RemoveCommand") {
                    RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(BaseItemsControl), 1),
                });
                DeleteButton.SetBinding(Button.CommandParameterProperty, new Binding("."));

                Container.AppendChild(DeleteButton);
            }
            FrameworkElementFactory Property = new FrameworkElementFactory(typeof(ToggleButton));
            Property.SetBinding(ToggleButton.ContentProperty, new Binding("Value"));
            Property.SetBinding(ToggleButton.IsCheckedProperty, new Binding("Value"));
            Container.AppendChild(Property);
            template.VisualTree = Container;
            return template;
        }

        private static DataTemplate GenerateItemClassTemplateFromDelete() {
            DataTemplate template = new DataTemplate();
            FrameworkElementFactory Container = new FrameworkElementFactory(typeof(Grid));


            FrameworkElementFactory ContainerColumnProperty = new FrameworkElementFactory(typeof(ColumnDefinition));
            ContainerColumnProperty.SetValue(ColumnDefinition.WidthProperty, new GridLength(1, GridUnitType.Star));
            Container.AppendChild(ContainerColumnProperty);

            FrameworkElementFactory ContainerColumn1Property = new FrameworkElementFactory(typeof(ColumnDefinition));
            ContainerColumn1Property.SetValue(ColumnDefinition.WidthProperty, new GridLength(40));
            Container.AppendChild(ContainerColumn1Property);

            FrameworkElementFactory DeleteButton = new FrameworkElementFactory(typeof(Button));
            DeleteButton.SetValue(Button.ContentProperty, "X");
            DeleteButton.SetValue(Grid.ColumnProperty, 1);
            DeleteButton.SetValue(Button.HeightProperty, 24.0);
            DeleteButton.SetValue(Button.VerticalAlignmentProperty, VerticalAlignment.Top);
            DeleteButton.SetBinding(Button.CommandProperty, new Binding("RemoveCommand") {
                RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(BaseItemsControl), 1),
            });
            DeleteButton.SetBinding(Button.CommandParameterProperty, new Binding("."));

            Container.AppendChild(DeleteButton);

            FrameworkElementFactory Property = new FrameworkElementFactory(typeof(Label));
            Property.SetBinding(Label.ContentProperty, new Binding("."));
            Property.SetValue(Label.HorizontalContentAlignmentProperty, HorizontalAlignment.Stretch);
            Property.SetValue(Label.PaddingProperty, new Thickness(0));
            Container.AppendChild(Property);

            template.VisualTree = Container;
            return template;
        }

        private static DataTemplate GenerateItemDictionaryTemplate(Type type) {
            DataTemplate template = new DataTemplate();
            FrameworkElementFactory Container = new FrameworkElementFactory(typeof(Expander));

            Container.SetBinding(Expander.HeaderProperty, new Binding("Key"));

            FrameworkElementFactory Property = new FrameworkElementFactory(typeof(Label));
            Property.SetBinding(Label.ContentProperty, new Binding("."));

            Property.SetValue(Label.HorizontalContentAlignmentProperty, HorizontalAlignment.Stretch);

            if (type.BaseType == typeof(Enum)) {
                AddTemplateEnumToResource(type, true);
                Property.SetResourceReference(Label.ContentTemplateProperty, "T_" + type.FullName);
            }
            else if (type == typeof(bool))
                Property.SetResourceReference(Label.ContentTemplateProperty, "ItemBoolen");
            else if (SimpleTypes.Contains(type))
                Property.SetResourceReference(Label.ContentTemplateProperty, "ItemSimple");
            else {
                AddTemplateToResource(type);
                Property.SetBinding(Label.ContentProperty, new Binding("Value"));
                Property.SetResourceReference(Label.ContentTemplateProperty, "ItemClass");
            }

            Container.AppendChild(Property);
            template.VisualTree = Container;
            return template;
        }

        private static DataTemplate CreateEnumTemplate(Type i, bool delete) {
            DataTemplate template = new DataTemplate(i);
            FrameworkElementFactory Container = new FrameworkElementFactory(typeof(Grid));

            if (delete) {
                FrameworkElementFactory ContainerColumnProperty = new FrameworkElementFactory(typeof(ColumnDefinition));
                ContainerColumnProperty.SetValue(ColumnDefinition.WidthProperty, new GridLength(1, GridUnitType.Star));
                Container.AppendChild(ContainerColumnProperty);

                FrameworkElementFactory ContainerColumn1Property = new FrameworkElementFactory(typeof(ColumnDefinition));
                ContainerColumn1Property.SetValue(ColumnDefinition.WidthProperty, new GridLength(40));
                Container.AppendChild(ContainerColumn1Property);

                FrameworkElementFactory DeleteButton = new FrameworkElementFactory(typeof(Button));
                DeleteButton.SetValue(Button.ContentProperty, "X");
                DeleteButton.SetValue(Grid.ColumnProperty, 1);
                DeleteButton.SetBinding(Button.CommandProperty, new Binding("RemoveCommand") {
                    RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(BaseItemsControl), 1),
                });
                DeleteButton.SetBinding(Button.CommandParameterProperty, new Binding("."));

                Container.AppendChild(DeleteButton);
            }

            FrameworkElementFactory Property = new FrameworkElementFactory(typeof(ComboBox));
            Property.SetResourceReference(ComboBox.DataContextProperty, i.FullName);
            Property.SetBinding(ComboBox.ItemsSourceProperty, new Binding("."));

            EnumBuilder.AddEnumResource(i);

            Property.SetBinding(ComboBox.SelectedValueProperty, new Binding("DataContext.Value") {
                RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(Grid), 1),
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            });

            Container.AppendChild(Property);

            template.VisualTree = Container;
            return template;
        }

        #endregion

        #region Helps
        private static Thickness GetThickness(PresentationMarginInfo pmAttr)
        {
            Thickness th = new Thickness();
            if (pmAttr != null)
            {
                double l = 0, t = 0, r = 0, b = 0;
                if (pmAttr.All != 0)
                    l = t = r = b = pmAttr.All;
                else if (pmAttr.LeftRight != 0 || pmAttr.TopButtom != 0)
                {
                    if (pmAttr.LeftRight != 0)
                        l = r = pmAttr.LeftRight;

                    if (pmAttr.TopButtom != 0)
                        t = b = pmAttr.TopButtom;
                }
                else
                {
                    l = pmAttr.Left; t = pmAttr.Top; r = pmAttr.Right; b = pmAttr.Buttom;
                }
                th = new Thickness(l, t, r, b);
            }
            return th;
        }

        private static Thickness GetThickness(PresentationPaddingInfo pmAttr)
        {
            Thickness th = new Thickness();
            if (pmAttr != null)
            {
                double l = 0, t = 0, r = 0, b = 0;
                //и так, разбераем все по этапно с последовательностью проверки от еденичного значения к двочно и общему
                if (pmAttr.All != 0)
                    l = t = r = b = pmAttr.All;
                else if (pmAttr.LeftRight != 0 || pmAttr.TopButtom != 0)
                {
                    if (pmAttr.LeftRight != 0)
                        l = r = pmAttr.LeftRight;

                    if (pmAttr.TopButtom != 0)
                        t = b = pmAttr.TopButtom;
                }
                else
                {
                    l = pmAttr.Left; t = pmAttr.Top; r = pmAttr.Right; b = pmAttr.Buttom;
                }
                th = new Thickness(l, t, r, b);
            }
            return th;
        }
        #endregion

        internal static void AddTemplateToResource(Type i) {
            if (Application.Current.TryFindResource(new DataTemplateKey(i)) == null)
                Application.Current.Resources.Add(new DataTemplateKey(i), CreateTemplate(i));
        }

        internal static void AddTemplateEnumToResource(Type i, bool delete = false) {
            if (Application.Current.TryFindResource("T_" + i.FullName) == null)
                Application.Current.Resources.Add("T_" + i.FullName, CreateEnumTemplate(i, delete));
        }

        internal static void CreateDynamicResource(Func<Type, bool> condition)
        {
            Application app = Application.Current;

            app.Resources.Add("ItemSimple", GenerateItemSimpleTemplate());
            app.Resources.Add("ItemSimpleD", GenerateItemSimpleTemplate(true));

            app.Resources.Add("ItemBoolen", GenerateItemBoolenTemplate());
            app.Resources.Add("ItemBoolenD", GenerateItemBoolenTemplate(true));

            app.Resources.Add("ItemClassD", GenerateItemClassTemplateFromDelete());

            foreach (Type i in GetTypes(condition))
                AddTemplateToResource(i);
        }
    }
}
