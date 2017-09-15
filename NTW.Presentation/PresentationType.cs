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
            typeof(string), 
            typeof(Guid)
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

        /// <summary>
        /// Возврашает шаблон Который предназначен для работы с данным типом.
        /// </summary>
        /// <param name="control">Тип данных для которого будет сформирован шаблон.</param>
        /// <returns>Шаблон</returns>
        private static DataTemplate GetControl(Application application, Type type)
        {
            DataTemplate template = new DataTemplate();
            template.DataType = type;
            PresentationMarginInfo pmi = System.Attribute.GetCustomAttributes(type).ToList().Find((x) => x is PresentationMarginInfo) as PresentationMarginInfo;
            PresentationPaddingInfo ppi = System.Attribute.GetCustomAttributes(type).ToList().Find((x) => x is PresentationPaddingInfo) as PresentationPaddingInfo;

            #region Основа (контейнер) в которую будут вкладыватся элементы
            FrameworkElementFactory Panel = new FrameworkElementFactory(typeof(StackPanel));
            Panel.Name = "BackPanel"; //на всякий случай дадим имя - мало ли придется обратится к контексту данных

            Panel.SetValue(StackPanel.MarginProperty, GetThickness(pmi));
            Panel.SetValue(Control.PaddingProperty, GetThickness(ppi));

            foreach (PropertyInfo property in type.GetProperties().Where(x => x.Name != "Template"))//откличм условие так как возможно управление способом привязки
            {
                //требуется получить список атрибутов для текущего свойства
                List<System.Attribute> attributes = System.Attribute.GetCustomAttributes(property).ToList();//List - так как обладаем метовами поиска без приобразования

                PresentationInfo pAttr = attributes.Find((x) => x is PresentationInfo) as PresentationInfo;
                PresentationCollectionInfo pcAttr = attributes.Find((x) => x is PresentationCollectionInfo) as PresentationCollectionInfo;
                PresentationMarginInfo pmAttr = attributes.Find((x) => x is PresentationMarginInfo) as PresentationMarginInfo;

                //так невозможно управлять одновременно и подписью и контентом то обернемих в общий контейнер
                FrameworkElementFactory ContainerPanel = new FrameworkElementFactory(typeof(StackPanel));
                ContainerPanel.SetValue(TextBlock.MarginProperty, GetThickness(pmAttr));
                //формируем подпись свойства
                ContainerPanel.AppendChild(CreateCaption(property.Name, pAttr));

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

                    ContainerPanel.AppendChild(Property);
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

                    ContainerPanel.AppendChild(Property);
                }
                #endregion
                #region Если относится к типу "Представления"
                else if (property.PropertyType.BaseType == typeof(Presentation) && !property.PropertyType.IsGenericType)//будем определять в составе Generic
                {
                    ContainerPanel.AppendChild(CreateContent(property.Name, pAttr));
                } 
                #endregion
                #region Если относится к типу Generic
                else if (property.PropertyType.IsGenericType)//всетаки придется вынести в отдульную обработку
                {
                    //с листом (да и со словорем) все иначе и сложнее так как есть сложная хрень с генерацией нескольких параметров
                    #region IList
                    //будем определять к какому плану будет отнасится данный тип generic
                    if (property.PropertyType.GetInterface("IList") != null)//значит относится к спискам
                    {
                        #region проверка наличия ресурса для данного объекта
                        if (application.TryFindResource(property.PropertyType.FullName) == null)
                            application.Resources.Add(property.PropertyType.FullName, GetControl(application, property.PropertyType));
                        #endregion

                        FrameworkElementFactory BaseContainer = new FrameworkElementFactory(typeof(Grid));
                        FrameworkElementFactory Container = new FrameworkElementFactory(typeof(Grid));

                        #region Формирование модели если требуется
                        //попробуем без спецефичного формирования объекта
                        //увы, без него никак
                        Type GType = property.PropertyType.GetGenericArguments()[0];
                        string propertyName = property.Name;
                        Container.AddHandler(ListBox.LoadedEvent, new RoutedEventHandler((s, e) =>
                        {
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

                        ContainerPanel.AppendChild(BaseContainer);
                    }//конечно же отдельно обработать! 
                    #endregion
                    #region IDictionary
                    else if (property.PropertyType.GetInterface("IDictionary") != null)//если является словорем
                    {
                        //по факту в качестве ключа и значения могут быть даже сложные классы, по этому стоит подумать как это реаргонизавать
                        //для начала соберем макет который будет управлятся при помощи DictionaryViewModel

                        FrameworkElementFactory Container = new FrameworkElementFactory(typeof(Grid));

                        #region Подгрузка
                        Type[] generics = property.PropertyType.GetGenericArguments();
                        string PropertyName = property.Name;

                        Container.AddHandler(Grid.LoadedEvent, new RoutedEventHandler((s, e) =>
                        {
                            var view = Activator.CreateInstance(typeof(DictionaryViewModel<,>).MakeGenericType(generics), null);
                            BindingOperations.SetBinding(view as DependencyObject, AbstractDictionaryView.ItemsProperty, new Binding(PropertyName) { Source = (s as Grid).DataContext, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });
                            (s as Grid).DataContext = view;
                        }));
                        #endregion

                        #region Строки и колонки
                        FrameworkElementFactory Row1 = new FrameworkElementFactory(typeof(RowDefinition));
                        Row1.SetValue(RowDefinition.HeightProperty, new GridLength(22));
                        Container.AppendChild(Row1);

                        FrameworkElementFactory Row2 = new FrameworkElementFactory(typeof(RowDefinition));
                        Row2.SetValue(RowDefinition.HeightProperty, new GridLength(1, GridUnitType.Star));
                        Container.AppendChild(Row2);

                        FrameworkElementFactory Row3 = new FrameworkElementFactory(typeof(RowDefinition));
                        Row3.SetValue(RowDefinition.HeightProperty, new GridLength(22));
                        Container.AppendChild(Row3);

                        FrameworkElementFactory Column1 = new FrameworkElementFactory(typeof(ColumnDefinition));
                        Column1.SetValue(ColumnDefinition.WidthProperty, new GridLength(1, GridUnitType.Star));
                        Container.AppendChild(Column1);

                        FrameworkElementFactory Column2 = new FrameworkElementFactory(typeof(ColumnDefinition));
                        Column2.SetValue(ColumnDefinition.WidthProperty, new GridLength(1, GridUnitType.Star));
                        Container.AppendChild(Column2);
                        #endregion

                        //1. ComboBox с возможностью выбора определенного ключа с дальнейшим отображением значения по ключу
                        #region Отборка
                        FrameworkElementFactory Combo = new FrameworkElementFactory(typeof(ComboBox));
                        Combo.SetValue(Grid.ColumnSpanProperty, 2);
                        Combo.SetBinding(ComboBox.ItemsSourceProperty, new Binding("MKeys"));
                        Combo.SetBinding(ComboBox.SelectedItemProperty, new Binding("SelectedKey") { Mode = BindingMode.TwoWay, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });
                        Container.AppendChild(Combo);
                        #endregion

                        //2. Label с шаблоном для отображения значения
                        #region Отображение
                        FrameworkElementFactory Lab = new FrameworkElementFactory(typeof(Label));
                        Lab.SetValue(Grid.RowProperty, 1);
                        Lab.SetValue(Grid.ColumnSpanProperty, 2);
                        Lab.SetValue(Label.HorizontalContentAlignmentProperty, System.Windows.HorizontalAlignment.Stretch);
                        //если это словарь то оно обладает двумя типами под generic и второй из них это значение
                        if (SimpleTypes.Contains(property.PropertyType.GetGenericArguments()[1]))
                        {
                            Lab.SetBinding(Label.ContentProperty, new Binding(".") { UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });
                            Lab.SetResourceReference(Label.ContentTemplateProperty, "SItemPresentation");
                        }
                        else
                        {
                            Lab.SetBinding(Label.ContentProperty, new Binding("Value") { UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });
                            Lab.SetBinding(Label.ContentTemplateProperty, new Binding("Value.Template"));
                        }

                        Container.AppendChild(Lab);
                        #endregion

                        //3. Button  с возможностью добавления
                        #region Add
                        FrameworkElementFactory Add = new FrameworkElementFactory(typeof(Button));
                        Add.SetValue(Grid.RowProperty, 2);
                        Add.SetValue(Button.ContentProperty, "Add");
                        Add.SetBinding(Button.CommandProperty, new Binding("AddCommand"));
                        Container.AppendChild(Add);
                        #endregion

                        #region Remove
                        FrameworkElementFactory Remove = new FrameworkElementFactory(typeof(Button));
                        Remove.SetValue(Grid.RowProperty, 2);
                        Remove.SetValue(Grid.ColumnProperty, 1);
                        Remove.SetValue(Button.ContentProperty, "Remove");
                        Remove.SetBinding(Button.CommandProperty, new Binding("RemoveCommand"));
                        Container.AppendChild(Remove);
                        #endregion

                        ContainerPanel.AppendChild(Container);
                    } 
                    #endregion
                    #region Simple Generic
                    else //значит относится к составным объектам
                    {
                        if (application.TryFindResource(property.PropertyType.FullName) == null)
                            application.Resources.Add(property.PropertyType.FullName, GetControl(application, property.PropertyType));

                        ContainerPanel.AppendChild(CreateContent(property.Name, pAttr));
                    } 
                    #endregion
                }
                #endregion
                #region Если относится к типу массива
                else if (property.PropertyType.IsArray)
                {
                    #region проверка наличия ресурса для данного объекта
                    if (application.TryFindResource(property.PropertyType.GetElementType().FullName) == null)
                        application.Resources.Add(property.PropertyType.GetElementType().FullName, GetControl(application, property.PropertyType.GetElementType()));
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
                    }
                    ContainerPanel.AppendChild(List);
                    #endregion

                    #region Выставление атрибутов
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
                    #endregion
                }
                #endregion
                #region Если относится к типу interface
                else if (property.PropertyType.IsInterface)
                {
                    FrameworkElementFactory Container = new FrameworkElementFactory(typeof(Label));
                    Container.SetBinding(Label.DataContextProperty, new Binding(property.Name));
                    Container.SetBinding(Label.ContentProperty, new Binding("."));
                    Container.SetBinding(Label.ContentTemplateProperty, new Binding("Template"));
                    ContainerPanel.AppendChild(Container);
                }
                #endregion

                Panel.AppendChild(ContainerPanel);
            }
            #endregion

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
            RemoveButton.SetBinding(Button.CommandProperty, new Binding("DataContext.RemoveCommand") { RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(Grid), 3) });
            RemoveButton.SetBinding(Button.CommandParameterProperty, new Binding("DataContext.Value") { ElementName = "removeButton" });
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
                    DataTemplate template = GetControl(application, t);
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
                    DataTemplate template = GetControl(application, t);
                    application.Resources.Add(t.FullName, template);
                }
            }
        }

        private static Thickness GetThickness(PresentationMarginInfo pmAttr)
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
    }
}
