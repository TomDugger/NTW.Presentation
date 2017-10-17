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
using System.Collections;

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

        private static List<Type> GetTypes(Type type)
        {
            List<Type> result = new List<Type>();
            NonPresentation nonPresenatry = System.Attribute.GetCustomAttributes(type).ToList().Find((x) => x is NonPresentation) as NonPresentation;

            if (nonPresenatry == null)
            {
                result.Add(type);
                foreach (var property in type.GetProperties())
                    if (!SimpleTypes.Contains(property.PropertyType) && property.PropertyType != typeof(object))
                    {
                        if (property.PropertyType.IsArray || property.PropertyType.GetInterface(typeof(IList).Name) != null)
                            result.AddRange(GetTypes(property.PropertyType.GetElementType()));
                        else if (property.PropertyType.IsGenericType)
                        {
                            foreach(var t in property.PropertyType.GetGenericArguments())
                                if (!SimpleTypes.Contains(t) && t != typeof(object))
                                result.AddRange(GetTypes(t));
                        }

                            result.AddRange(GetTypes(property.PropertyType));
                    }
            }
            return result;
        }

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
            {
                List<Type> temp = new List<Type>();
                temp = Assembly.GetEntryAssembly().GetTypes().Where(condition).Where(t => !t.IsGenericType).ToList();

                //выделяем типы для которых требуется выделить шаблон
                //result.AddRange(temp);
                foreach (Type i in temp)
                {
                    NonPresentation nonPresenatry = System.Attribute.GetCustomAttributes(i).ToList().Find((x) => x is NonPresentation) as NonPresentation;
                    if (nonPresenatry == null)
                    {
                        result.AddRange(GetTypes(i));
                    }
                }
            }

            return result.ToArray();
        }

        private static FrameworkElementFactory CreateTemplateFromType(Type type)
        {
            FrameworkElementFactory Container = new FrameworkElementFactory(typeof(Grid));
            if (type.IsClass)
            {
                if (type.IsArray)
                    return CreateArrayType(type);
                else if (type.GetInterface(typeof(IList).Name) != null)
                    return CreateListType(type);
                else if (type.GetInterface(typeof(IDictionary).Name) != null)
                    return CreateDictionaryType(type);
                else if (type.IsGenericType)
                {
                    if (type.GetInterface(typeof(IList<>).Name) != null)
                        return CreateListGenericType(type);
                    else
                        return CreateTemplateClass(type);
                }
                else
                    return CreateTemplateClass(type);
            }

            return Container;
        }

        private static FrameworkElementFactory CreateTemplateClass(Type type)
        {
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
                        else
                            ContainerPanel.AppendChild(CreateClassType(property.Name));
                        #endregion

                        Panel.AppendChild(ContainerPanel);
                    }
                }
            #endregion

            return Panel;
        }

        private static FrameworkElementFactory CreateCaption(string propertyName, PresentationInfo pAttr)
        {
            FrameworkElementFactory Caption = new FrameworkElementFactory(typeof(TextBlock));
            Caption.SetValue(TextBlock.TextProperty, pAttr != null ? pAttr.CaptionName : propertyName);
            Caption.SetValue(TextBlock.TextWrappingProperty, pAttr != null ? pAttr.PresentCaption : TextWrapping.NoWrap);
            return Caption;
        }

        #region обработки типов по признаку
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

        private static FrameworkElementFactory CreateClassType(string PropertyName) {
            FrameworkElementFactory Property = new FrameworkElementFactory(typeof(ContentControl));
            Property.SetValue(ContentControl.PaddingProperty, new Thickness(20, 0, 0, 0));
            Property.SetBinding(ContentControl.ContentProperty, new Binding(PropertyName));
            Property.SetValue(ContentControl.HorizontalContentAlignmentProperty, System.Windows.HorizontalAlignment.Stretch);
            return Property;
        }

        private static FrameworkElementFactory CreateArrayType(Type property) {
            Type AType = property.GetElementType();
            if (SimpleTypes.Contains(AType) || AType.BaseType == typeof(Enum)) {
                Type BType = typeof(ArrayItemsControl<>).MakeGenericType(AType);
                FrameworkElementFactory Property = new FrameworkElementFactory(BType);
                Property.SetValue(ScrollViewer.HorizontalScrollBarVisibilityProperty, ScrollBarVisibility.Disabled);
                Property.SetValue(ItemsControl.PaddingProperty, new Thickness(20, 0, 0, 0));
                Property.SetValue(ItemsControl.HorizontalContentAlignmentProperty, HorizontalAlignment.Stretch);
                Property.SetBinding(BaseItemsControl.ContextProperty, new Binding("."));

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
                Property.SetValue(ItemsControl.PaddingProperty, new Thickness(20, 0, 0, 0));
                Property.SetValue(ItemsControl.HorizontalContentAlignmentProperty, HorizontalAlignment.Stretch);
                Property.SetBinding(ItemsControl.ItemsSourceProperty, new Binding("."));
                Property.SetResourceReference(ItemsControl.ItemTemplateProperty, new DataTemplateKey(AType));
                return Property;
            }
        }

        private static FrameworkElementFactory CreateListType(Type property) {
            Type AType = property.GetGenericArguments()[0];
            Type BType = typeof(ListItemsControl<>).MakeGenericType(AType);
            FrameworkElementFactory Property = new FrameworkElementFactory(BType);

            Property.SetValue(ScrollViewer.HorizontalScrollBarVisibilityProperty, ScrollBarVisibility.Disabled);
            Property.SetValue(ItemsControl.HorizontalContentAlignmentProperty, HorizontalAlignment.Stretch);
            Property.SetValue(ItemsControl.PaddingProperty, new Thickness(20, 0, 0, 0));

            if (SimpleTypes.Contains(AType) || AType.BaseType == typeof(Enum))
                Property.SetBinding(BaseItemsControl.ContextProperty, new Binding("."));
            else
                Property.SetBinding(BaseItemsControl.ItemsSourceProperty, new Binding("."));
            if (AType.BaseType == typeof(Enum)) {
                AddTemplateEnumToResource(AType, true);
                Property.SetResourceReference(ItemsControl.ItemTemplateProperty, "T_" + AType.FullName);
            }
            else if (AType == typeof(bool))
                Property.SetResourceReference(ItemsControl.ItemTemplateProperty, "ItemBoolenD");
            else if (SimpleTypes.Contains(AType))
                Property.SetResourceReference(ItemsControl.ItemTemplateProperty, "ItemSimpleD");
            else {
                Property.SetResourceReference(ItemsControl.ItemTemplateProperty, "ItemClassD");
            }
            return Property;
        }

        private static FrameworkElementFactory CreateListGenericType(Type property) {
            Type AType = property.GetGenericArguments()[0];
            Type BType = typeof(ListGenericItemsControl<>).MakeGenericType(AType);
            FrameworkElementFactory Property = new FrameworkElementFactory(BType);

            Property.SetValue(ScrollViewer.HorizontalScrollBarVisibilityProperty, ScrollBarVisibility.Disabled);
            Property.SetValue(ItemsControl.HorizontalContentAlignmentProperty, HorizontalAlignment.Stretch);
            Property.SetValue(ItemsControl.PaddingProperty, new Thickness(20, 0, 0, 0));

            if (SimpleTypes.Contains(AType) || AType.BaseType == typeof(Enum))
                Property.SetBinding(BaseItemsControl.ContextProperty, new Binding("."));
            else
                Property.SetBinding(BaseItemsControl.ItemsSourceProperty, new Binding("."));
            if (AType.BaseType == typeof(Enum)) {
                AddTemplateEnumToResource(AType, true);
                Property.SetResourceReference(ItemsControl.ItemTemplateProperty, "T_" + AType.FullName);
            }
            else if (AType == typeof(bool))
                Property.SetResourceReference(ItemsControl.ItemTemplateProperty, "ItemBoolenD");
            else if (SimpleTypes.Contains(AType))
                Property.SetResourceReference(ItemsControl.ItemTemplateProperty, "ItemSimpleD");
            else {
                Property.SetResourceReference(ItemsControl.ItemTemplateProperty, "ItemClassD");
            }
            return Property;
        }

        private static FrameworkElementFactory CreateDictionaryType(Type property) { 
            Type KType = property.GetGenericArguments()[0];
            Type VType = property.GetGenericArguments()[1];
            Type MType = typeof(DictionaryItemsControl<,>).MakeGenericType(KType, VType);
            FrameworkElementFactory Property = new FrameworkElementFactory(MType);

            Property.SetValue(ScrollViewer.HorizontalScrollBarVisibilityProperty, ScrollBarVisibility.Disabled);
            Property.SetValue(ItemsControl.HorizontalContentAlignmentProperty, HorizontalAlignment.Stretch);

            Property.SetBinding(BaseItemsControl.ContextProperty, new Binding("."));

            Property.SetValue(ItemsControl.ItemTemplateProperty, GenerateItemDictionaryTemplate(VType));
            return Property;
        }
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

            Container.SetBinding(Expander.HeaderProperty, new Binding("Key.Value"));

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
                if (Application.Current.TryFindResource(new DataTemplateKey(i)) == null)
                    Application.Current.Resources.Add(new DataTemplateKey(i), new DataTemplate() { VisualTree = CreateTemplateFromType(i) });
        }
    }
}
