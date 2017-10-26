using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;

namespace ModelTest
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            NTW.Presentation.Presentation.Generation(t => t == typeof(ModelTest.Test.Presentation) || t == typeof(ModelTest.Test.ChildrenMyInterface) || t == typeof(ModelTest.Test.Children2MyInterface));// es => es.Namespace == "ModelTest");
        }
    }
}
