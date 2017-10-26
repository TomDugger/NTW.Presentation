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
using System.Windows.Input;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Windows.Media.Animation;

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
                if (!SimpleTypes.Contains(type) && type != typeof(object) && !type.IsEnum)
                {
                    result.Add(type);
                    foreach (var property in type.GetProperties())
                        if (!SimpleTypes.Contains(property.PropertyType) && property.PropertyType != typeof(object) && !property.PropertyType.IsEnum)
                        {
                            if (property.PropertyType.IsArray)
                                result.AddRange(GetTypes(property.PropertyType.GetElementType()));
                            else if (property.PropertyType.GetInterface(typeof(IList).Name) != null)
                                result.AddRange(GetTypes(property.PropertyType.GetElementType() == null ? property.PropertyType.GetGenericArguments()[0] : property.PropertyType.GetElementType()));
                            else if (property.PropertyType.IsGenericType)
                            {
                                foreach (var t in property.PropertyType.GetGenericArguments())
                                    if (!SimpleTypes.Contains(t) && t != typeof(object))
                                        result.AddRange(GetTypes(t));
                            }

                            result.AddRange(GetTypes(property.PropertyType));
                        }
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
            if (type == typeof(String))
                return null;
            else if (type.IsValueType && !type.IsEnum)
            {
                return CreateTemplateClass(type);
            }
            else if (type.IsClass)
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
                else if (type.GetInterface(typeof(ICommand).Name) != null)
                    return CreateCommandType(type);
                else
                    return CreateTemplateClass(type);
            }
            else if (type.IsInterface)
            {
                return CreateTemplateClass(type);
            }

            return null;
        }

        private static FrameworkElementFactory CreateTemplateClass(Type type)
        {
            #region Атребуты (над классом)
            NonPresentation nonPresenatry = System.Attribute.GetCustomAttributes(type).ToList().Find((x) => x is NonPresentation) as NonPresentation;
            PresentationMarginInfo marginPresentary = System.Attribute.GetCustomAttributes(type).ToList().Find((x) => x is PresentationMarginInfo) as PresentationMarginInfo;
            #endregion

            #region Основа (контейнер) в которую будут вкладыватся элементы
            FrameworkElementFactory Panel = new FrameworkElementFactory(typeof(StackPanel));
            Panel.Name = "BackPanel";

            Panel.SetValue(StackPanel.MarginProperty, GetThickness(marginPresentary));
            #endregion

            #region Свойства
            Dictionary<string, FrameworkElementFactory> Groups = new Dictionary<string, FrameworkElementFactory>();
            if(nonPresenatry == null)
                foreach (PropertyInfo property in type.GetProperties())
                {
                    #region извлечение атрибутов
                    List<System.Attribute> attributes = System.Attribute.GetCustomAttributes(property).ToList();

                    NonPresentation nattr = attributes.Find((x) => x is NonPresentation) as NonPresentation;
                    PresentationInfo pAttr = attributes.Find((x) => x is PresentationInfo) as PresentationInfo;
                    PresentationCollectionInfo pcAttr = attributes.Find((x) => x is PresentationCollectionInfo) as PresentationCollectionInfo;
                    PresentationMarginInfo pmAttr = attributes.Find((x) => x is PresentationMarginInfo) as PresentationMarginInfo;
                    PresentationGroupInfo pgi = attributes.Find((x) => x is PresentationGroupInfo) as PresentationGroupInfo;
                    PresentationBinding pbind = attributes.Find((x) => x is PresentationBinding) as PresentationBinding;
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
                        else if (property.PropertyType.GetInterface(typeof(ICommand).Name) != null)
                            ContainerPanel.AppendChild(CreateCommandType(property));
                        else
                            ContainerPanel.AppendChild(CreateClassType(property.Name, pAttr, pbind));
                        #endregion

                        if (pgi != null)
                        {
                            if (!Groups.ContainsKey(pgi.GroupName))
                            {
                                var t = CreateGroups(pgi.GroupName);
                                Panel.AppendChild(t.Item1);
                                Groups.Add(pgi.GroupName, t.Item2);
                            }
                            Groups[pgi.GroupName].AppendChild(ContainerPanel);
                        }
                        else
                            Panel.AppendChild(ContainerPanel);
                    }
                }
            #endregion

            return Panel;
        }

        private static Tuple<FrameworkElementFactory, FrameworkElementFactory> CreateGroups(string GroupName)
        {
            FrameworkElementFactory Groups = new FrameworkElementFactory(typeof(Expander));
            Groups.SetValue(Expander.HeaderProperty, GroupName);

            FrameworkElementFactory ContentGroupsPanel = new FrameworkElementFactory(typeof(StackPanel));

            Groups.AppendChild(ContentGroupsPanel);

            return Tuple.Create<FrameworkElementFactory, FrameworkElementFactory>(Groups, ContentGroupsPanel);
        }

        private static FrameworkElementFactory CreateCaption(string propertyName, PresentationInfo pAttr)
        {
            FrameworkElementFactory Caption = new FrameworkElementFactory(typeof(TextBlock));
            Caption.SetValue(TextBlock.FontWeightProperty, System.Windows.FontWeights.Bold);
            Caption.SetValue(TextBlock.TextProperty, pAttr != null ? pAttr.CaptionName : propertyName);
            Caption.SetValue(TextBlock.TextWrappingProperty, pAttr != null ? pAttr.PresentCaption : TextWrapping.NoWrap);
            return Caption;
        }

        private static Style CreateStyleFromAsync()
        {
            Style style = new Style(typeof(ContentControl));

            ControlTemplate cTemplate = new ControlTemplate(typeof(ContentControl));
            FrameworkElementFactory grid = new FrameworkElementFactory(typeof(Grid));
            #region rec1
            FrameworkElementFactory rectangle = new FrameworkElementFactory(typeof(Rectangle));
            rectangle.SetValue(Rectangle.NameProperty, "rec1");
            rectangle.SetValue(Rectangle.FillProperty, new SolidColorBrush(Colors.DodgerBlue));
            rectangle.SetValue(Rectangle.WidthProperty, 46.0);
            rectangle.SetValue(Rectangle.HeightProperty, 46.0);

            RotateTransform rt = new RotateTransform();
            rt.CenterX = 23;
            rt.CenterY = 23;

            rectangle.SetValue(Rectangle.RenderTransformProperty, rt);

            grid.AppendChild(rectangle); 
            #endregion

            #region rec2
            FrameworkElementFactory rectangle1 = new FrameworkElementFactory(typeof(Rectangle));
            rectangle1.SetValue(Rectangle.NameProperty, "rec2");
            rectangle1.SetValue(Rectangle.FillProperty, new SolidColorBrush(Colors.Blue));
            rectangle1.SetValue(Rectangle.WidthProperty, 46.0);
            rectangle1.SetValue(Rectangle.HeightProperty, 46.0);

            RotateTransform rt1 = new RotateTransform();
            rt1.CenterX = 23;
            rt1.CenterY = 23;

            ScaleTransform sc1 = new ScaleTransform();
            sc1.CenterX = 23;
            sc1.CenterY = 23;

            TransformGroup gr1 = new TransformGroup();
            gr1.Children.Add(rt1);
            gr1.Children.Add(sc1);

            rectangle1.SetValue(Rectangle.RenderTransformProperty, gr1);

            grid.AppendChild(rectangle1); 
            #endregion

            cTemplate.VisualTree = grid;

            #region style-rec1
            Storyboard sb = new Storyboard();
            DoubleAnimation da = new DoubleAnimation();
            da.From = 0;
            da.To = 360;
            da.Duration = new Duration(new TimeSpan(0, 0, 2));
            da.EasingFunction = new PowerEase() { EasingMode = EasingMode.EaseInOut };
            da.RepeatBehavior = RepeatBehavior.Forever;
            sb.Children.Add(da);
            Storyboard.SetTargetProperty(da, new PropertyPath("RenderTransform.Angle"));

            BeginStoryboard bsb = new BeginStoryboard();
            bsb.Storyboard = sb;

            EventTrigger trig = new EventTrigger(Control.LoadedEvent);
            trig.Actions.Add(bsb);

            Style st = new Style(typeof(Rectangle));
            st.Triggers.Add(trig);
            rectangle.SetValue(Rectangle.StyleProperty, st); 
            #endregion

            #region style-rec2
            #region sb1
            Storyboard sb1 = new Storyboard();
            DoubleAnimation da1 = new DoubleAnimation();
            da1.From = 0;
            da1.To = 360;
            da1.Duration = new Duration(new TimeSpan(0, 0, 2));
            da1.EasingFunction = new PowerEase() { EasingMode = EasingMode.EaseInOut };
            da1.RepeatBehavior = RepeatBehavior.Forever;
            sb1.Children.Add(da1);
            Storyboard.SetTargetProperty(da1, new PropertyPath("RenderTransform.Children[0].Angle"));

            DoubleAnimation da2 = new DoubleAnimation();
            da2.From = 0.9;
            da2.To = 0.2;
            da2.Duration = new Duration(new TimeSpan(0, 0, 2));
            da2.EasingFunction = new PowerEase() { EasingMode = EasingMode.EaseInOut };
            da2.RepeatBehavior = RepeatBehavior.Forever;
            da2.AutoReverse = true;
            sb1.Children.Add(da2);
            Storyboard.SetTargetProperty(da2, new PropertyPath("RenderTransform.Children[1].ScaleX"));

            DoubleAnimation da3 = new DoubleAnimation();
            da3.From = 0.9;
            da3.To = 0.2;
            da3.Duration = new Duration(new TimeSpan(0, 0, 2));
            da3.EasingFunction = new PowerEase() { EasingMode = EasingMode.EaseInOut };
            da3.RepeatBehavior = RepeatBehavior.Forever;
            da3.AutoReverse = true;
            sb1.Children.Add(da3);
            Storyboard.SetTargetProperty(da3, new PropertyPath("RenderTransform.Children[1].ScaleY"));
            #endregion

            BeginStoryboard bsb1 = new BeginStoryboard();
            bsb1.Storyboard = sb1; 
            #endregion

            EventTrigger trig1 = new EventTrigger(Control.LoadedEvent);
            trig1.Actions.Add(bsb1);

            Style st1 = new Style(typeof(Rectangle));
            st1.Triggers.Add(trig1);
            rectangle1.SetValue(Rectangle.StyleProperty, st1); 

            Trigger tg = new Trigger();
            tg.Property = ContentControl.ContentProperty;
            tg.Value = null;
            tg.Setters.Add(new Setter(ContentControl.TemplateProperty, cTemplate));

            style.Triggers.Add(tg);

            return style;
        }

        #region обработки типов по признаку
        private static FrameworkElementFactory CreateSimpleType(PropertyInfo property)
        {
            PresentationBinding pBind = System.Attribute.GetCustomAttributes(property).ToList().Find((x) => x is PresentationBinding) as PresentationBinding;
            FrameworkElementFactory Property;
            if (!property.CanWrite)
            {
                Property = new FrameworkElementFactory(typeof(TextBlock));
                Property.SetBinding(TextBlock.TextProperty, new Binding(property.Name) {
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                    Mode = BindingMode.OneWay, 
                    IsAsync = (pBind != null ? pBind.IsAsync: false)
                });
            }
            else
            {
                if (property.PropertyType == typeof(bool))
                {
                    Property = new FrameworkElementFactory(typeof(ToggleButton));
                    Property.SetBinding(ToggleButton.ContentProperty, new Binding(property.Name) { UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged});
                    Property.SetBinding(CheckBox.IsCheckedProperty, new Binding(property.Name) {
                        UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                        Mode = BindingMode.TwoWay,
                        IsAsync = (pBind != null ? pBind.IsAsync : false)
                    });
                }
                else
                {
                    Property = new FrameworkElementFactory(typeof(TextBox));
                    Property.SetBinding(TextBox.TextProperty, new Binding(property.Name) {
                        UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                        Mode = BindingMode.TwoWay,
                        IsAsync = (pBind != null ? pBind.IsAsync : false)
                    });
                }
            }
            return Property;

        }

        private static FrameworkElementFactory CreateEnumType(PropertyInfo property) {
            PresentationBinding pBind = System.Attribute.GetCustomAttributes(property).ToList().Find((x) => x is PresentationBinding) as PresentationBinding;
            FrameworkElementFactory Property;
            if (!property.CanWrite) {
                Property = new FrameworkElementFactory(typeof(TextBlock));
                Property.SetBinding(TextBlock.TextProperty, new Binding(property.Name) {
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                    Mode = BindingMode.OneWay,
                    IsAsync = pBind != null ? pBind.IsAsync : false
                });
            }
            else {
                Property = new FrameworkElementFactory(typeof(ComboBox));
                EnumBuilder.AddEnumResource(property.PropertyType);
                Property.SetResourceReference(ComboBox.DataContextProperty, property.PropertyType.FullName);

                Property.SetBinding(ComboBox.ItemsSourceProperty, new Binding("."));

                Property.SetBinding(ComboBox.SelectedValueProperty, new Binding("DataContext." + property.Name) {
                    ElementName = "BackPanel",
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                    IsAsync = (pBind != null ? pBind.IsAsync : false)
                });
            }
            return Property;
        }

        private static FrameworkElementFactory CreateClassType(string PropertyName, PresentationInfo pinfo = null, PresentationBinding pbind = null) {

            FrameworkElementFactory Property = new FrameworkElementFactory(typeof(ContentControl));

            if (pinfo != null)
            {
                if (pinfo.MinHeight > 0)
                    Property.SetValue(ContentControl.MinHeightProperty, pinfo.MinHeight);
                if (pinfo.MaxHeight > 0)
                    Property.SetValue(ContentControl.MaxHeightProperty, pinfo.MaxHeight);

                if (pinfo.MinWidth > 0)
                    Property.SetValue(ContentControl.MinWidthProperty, pinfo.MinWidth);
                if (pinfo.MaxWidth > 0)
                    Property.SetValue(ContentControl.MaxWidthProperty, pinfo.MaxWidth);
            }

            Property.SetValue(ContentControl.PaddingProperty, new Thickness(20, 0, 0, 0));
            Property.SetBinding(ContentControl.ContentProperty, new Binding(PropertyName) { IsAsync = (pbind != null ? pbind.IsAsync : false)});
            if (pbind != null && pbind.IsAsync)
                Property.SetResourceReference(ContentControl.StyleProperty, "IsAsyncStyle");
            Property.SetValue(ContentControl.HorizontalContentAlignmentProperty, System.Windows.HorizontalAlignment.Stretch);
            return Property;
        }

        private static FrameworkElementFactory CreateArrayType(Type property)
        {
            PresentationBinding pbind = System.Attribute.GetCustomAttributes(property).ToList().Find((x) => x is PresentationBinding) as PresentationBinding; 
            Type AType = property.GetElementType();
            if (SimpleTypes.Contains(AType) || AType.BaseType == typeof(Enum)) {
                Type BType = typeof(ArrayItemsControl<>).MakeGenericType(AType);
                FrameworkElementFactory Property = new FrameworkElementFactory(BType);
                Property.SetValue(ScrollViewer.HorizontalScrollBarVisibilityProperty, ScrollBarVisibility.Disabled);
                Property.SetValue(ItemsControl.PaddingProperty, new Thickness(20, 0, 0, 0));

                Property.SetValue(ItemsControl.HorizontalContentAlignmentProperty, HorizontalAlignment.Stretch);
                Property.SetBinding(BaseItemsControl.ContextProperty, new Binding(".") { IsAsync = pbind != null ? pbind.IsAsync : false});

                if (AType.BaseType == typeof(Enum)) {
                    AddTemplateEnumToResource(AType, "T");
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
                Property.SetBinding(ItemsControl.ItemsSourceProperty, new Binding(".") { IsAsync = pbind != null ? pbind.IsAsync : false });
                Property.SetResourceReference(ItemsControl.ItemTemplateProperty, new DataTemplateKey(AType));
                return Property;
            }
        }

        private static FrameworkElementFactory CreateListType(Type property)
        {
            PresentationBinding pbind = System.Attribute.GetCustomAttributes(property).ToList().Find((x) => x is PresentationBinding) as PresentationBinding;
            Type AType = property.GetGenericArguments()[0];
            Type BType = typeof(ListItemsControl<>).MakeGenericType(AType);
            FrameworkElementFactory Property = new FrameworkElementFactory(BType);

            Property.SetValue(ScrollViewer.HorizontalScrollBarVisibilityProperty, ScrollBarVisibility.Disabled);
            Property.SetValue(ItemsControl.HorizontalContentAlignmentProperty, HorizontalAlignment.Stretch);
            Property.SetValue(ItemsControl.PaddingProperty, new Thickness(20, 0, 0, 0));

            if (SimpleTypes.Contains(AType) || AType.BaseType == typeof(Enum))
                Property.SetBinding(BaseItemsControl.ContextProperty, new Binding(".") { UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged, IsAsync = pbind != null ? pbind.IsAsync : false});
            else
                Property.SetBinding(BaseItemsControl.ItemsSourceProperty, new Binding(".") { IsAsync = pbind != null ? pbind.IsAsync : false });
            if (AType.BaseType == typeof(Enum)) {
                AddTemplateEnumToResource(AType, "L", true);
                Property.SetResourceReference(ItemsControl.ItemTemplateProperty, "L_" + AType.FullName);
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

        private static FrameworkElementFactory CreateListGenericType(Type property)
        {
            PresentationBinding pbind = System.Attribute.GetCustomAttributes(property).ToList().Find((x) => x is PresentationBinding) as PresentationBinding;

            Type AType = property.GetGenericArguments()[0];
            Type BType = typeof(ListGenericItemsControl<>).MakeGenericType(AType);
            FrameworkElementFactory Property = new FrameworkElementFactory(BType);

            Property.SetValue(ScrollViewer.HorizontalScrollBarVisibilityProperty, ScrollBarVisibility.Disabled);
            Property.SetValue(ItemsControl.HorizontalContentAlignmentProperty, HorizontalAlignment.Stretch);
            Property.SetValue(ItemsControl.PaddingProperty, new Thickness(20, 0, 0, 0));

            if (SimpleTypes.Contains(AType) || AType.BaseType == typeof(Enum))
                Property.SetBinding(BaseItemsControl.ContextProperty, new Binding(".") { UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged, IsAsync = pbind != null ? pbind.IsAsync : false });
            else
                Property.SetBinding(BaseItemsControl.ItemsSourceProperty, new Binding(".") { IsAsync = pbind != null ? pbind.IsAsync : false });
            if (AType.BaseType == typeof(Enum)) {
                AddTemplateEnumToResource(AType, "L", true);
                Property.SetResourceReference(ItemsControl.ItemTemplateProperty, "L_" + AType.FullName);
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

        private static FrameworkElementFactory CreateDictionaryType(Type property)
        {
            PresentationBinding pbind = System.Attribute.GetCustomAttributes(property).ToList().Find((x) => x is PresentationBinding) as PresentationBinding;
            Type KType = property.GetGenericArguments()[0];
            Type VType = property.GetGenericArguments()[1];
            Type MType = typeof(DictionaryItemsControl<,>).MakeGenericType(KType, VType);
            FrameworkElementFactory Property = new FrameworkElementFactory(MType);

            Property.SetValue(ScrollViewer.HorizontalScrollBarVisibilityProperty, ScrollBarVisibility.Disabled);
            Property.SetValue(ItemsControl.HorizontalContentAlignmentProperty, HorizontalAlignment.Stretch);

            Property.SetBinding(BaseItemsControl.ContextProperty, new Binding(".") { IsAsync = pbind != null ? pbind.IsAsync : false });

            Property.SetValue(ItemsControl.ItemTemplateProperty, GenerateItemDictionaryTemplate(VType));
            return Property;
        }

        private static FrameworkElementFactory CreateCommandType(PropertyInfo property)
        {
            PresentationBinding pbind = System.Attribute.GetCustomAttributes(property).ToList().Find((x) => x is PresentationBinding) as PresentationBinding;
            FrameworkElementFactory Property = new FrameworkElementFactory(typeof(Button));
            Property.SetValue(Button.ContentProperty, "Run");
            Property.SetBinding(Button.CommandProperty, new Binding(property.Name) { IsAsync = pbind != null ? pbind.IsAsync : false });
            return Property;
        }

        private static FrameworkElementFactory CreateCommandType(Type property)
        {
            PresentationBinding pbind = System.Attribute.GetCustomAttributes(property).ToList().Find((x) => x is PresentationBinding) as PresentationBinding;
            FrameworkElementFactory Property = new FrameworkElementFactory(typeof(Button));
            Property.SetValue(Button.ContentProperty, "Run");
            Property.SetBinding(Button.CommandProperty, new Binding(".") { IsAsync = pbind != null ? pbind.IsAsync : false });
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
                    IsAsync = true
                });
                DeleteButton.SetBinding(Button.CommandParameterProperty, new Binding(".") { IsAsync = true });

                Container.AppendChild(DeleteButton);
            }
            FrameworkElementFactory Property = new FrameworkElementFactory(typeof(TextBox));
            Binding bn = new Binding("Value") { Mode = BindingMode.TwoWay, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged, IsAsync = true };
            bn.ValidationRules.Add(new DataErrorValidationRule());
            Property.SetBinding(TextBox.TextProperty, bn);
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
                    IsAsync = true
                });
                DeleteButton.SetBinding(Button.CommandParameterProperty, new Binding(".") { IsAsync = true });

                Container.AppendChild(DeleteButton);
            }
            FrameworkElementFactory Property = new FrameworkElementFactory(typeof(ToggleButton));

            Binding bn = new Binding("Value") { Mode = BindingMode.TwoWay, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged, IsAsync = true };
            bn.ValidationRules.Add(new DataErrorValidationRule());

            Property.SetBinding(ToggleButton.ContentProperty, bn);
            Property.SetBinding(ToggleButton.IsCheckedProperty, bn);
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
                IsAsync = true
            });
            DeleteButton.SetBinding(Button.CommandParameterProperty, new Binding(".") { IsAsync = true});

            Container.AppendChild(DeleteButton);

            FrameworkElementFactory Property = new FrameworkElementFactory(typeof(Label));
            Binding bn = new Binding(".") { Mode = BindingMode.TwoWay, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged, IsAsync = true };
            bn.ValidationRules.Add(new DataErrorValidationRule());
            Property.SetBinding(Label.ContentProperty, bn);
            Property.SetValue(Label.HorizontalContentAlignmentProperty, HorizontalAlignment.Stretch);
            Property.SetValue(Label.PaddingProperty, new Thickness(0));
            Container.AppendChild(Property);

            template.VisualTree = Container;
            return template;
        }

        private static DataTemplate GenerateItemDictionaryTemplate(Type type) {
            DataTemplate template = new DataTemplate();
            FrameworkElementFactory Container = new FrameworkElementFactory(typeof(Expander));

            Container.SetBinding(Expander.HeaderProperty, new Binding("Key.Value") { IsAsync = true});

            FrameworkElementFactory Property = new FrameworkElementFactory(typeof(Label));
            Property.SetBinding(Label.ContentProperty, new Binding(".") { IsAsync = true });

            Property.SetValue(Label.HorizontalContentAlignmentProperty, HorizontalAlignment.Stretch);

            if (type.BaseType == typeof(Enum)) {
                AddTemplateEnumToResource(type, "L", true);
                Property.SetResourceReference(Label.ContentTemplateProperty, "L_" + type.FullName);
            }
            else if (type == typeof(bool))
                Property.SetResourceReference(Label.ContentTemplateProperty, "ItemBoolen");
            else if (SimpleTypes.Contains(type))
                Property.SetResourceReference(Label.ContentTemplateProperty, "ItemSimple");
            else {
                Property.SetBinding(Label.ContentProperty, new Binding("Value") { IsAsync = true });
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
                    IsAsync = true
                });
                DeleteButton.SetBinding(Button.CommandParameterProperty, new Binding(".") { IsAsync = true });

                Container.AppendChild(DeleteButton);
            }

            FrameworkElementFactory Property = new FrameworkElementFactory(typeof(ComboBox));
            Property.SetResourceReference(ComboBox.DataContextProperty, i.FullName);
            Property.SetBinding(ComboBox.ItemsSourceProperty, new Binding(".") { IsAsync = true });

            EnumBuilder.AddEnumResource(i);

            Property.SetBinding(ComboBox.SelectedValueProperty, new Binding("DataContext.Value") {
                RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(Grid), 1),
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                IsAsync = true
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
        #endregion

        internal static void AddTemplateEnumToResource(Type i, string pref, bool delete = false) {
            if (Application.Current.TryFindResource(pref + "_" + i.FullName) == null)
                Application.Current.Resources.Add(pref + "_" + i.FullName, CreateEnumTemplate(i, delete));
        }

        internal static void CreateDynamicResource(Func<Type, bool> condition)
        {
            Application app = Application.Current;

            app.Resources.Add("IsAsyncStyle", CreateStyleFromAsync());

            app.Resources.Add("ItemSimple", GenerateItemSimpleTemplate());
            app.Resources.Add("ItemSimpleD", GenerateItemSimpleTemplate(true));

            app.Resources.Add("ItemBoolen", GenerateItemBoolenTemplate());
            app.Resources.Add("ItemBoolenD", GenerateItemBoolenTemplate(true));

            app.Resources.Add("ItemClassD", GenerateItemClassTemplateFromDelete());

            var items = GetTypes(condition);
            foreach (Type i in GetTypes(condition))
                if (Application.Current.TryFindResource(new DataTemplateKey(i)) == null)
                {
                    FrameworkElementFactory Container;
                    if ((Container = CreateTemplateFromType(i)) != null)
                        Application.Current.Resources.Add(new DataTemplateKey(i), new DataTemplate() { VisualTree = Container });
                }
        }
    }
}
