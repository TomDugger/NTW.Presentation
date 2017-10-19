using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace NTW.Presentation.Attributes
{
    public class PresentationMarginInfo:System.Attribute
    {
        #region Private
        private double left = 0;
        private double right = 0;
        private double top = 0;
        private double buttom = 0;

        private double leftRight = 0;
        private double topButtom = 6;
        private double all = 0;
        #endregion

        #region Public
        public double Left
        {
            get { return left; }
            set { left = value; }
        }

        public double Right
        {
            get { return right; }
            set { right = value; }
        }

        public double Top
        {
            get { return top; }
            set { top = value; }
        }

        public double Buttom
        {
            get { return buttom; }
            set { buttom = value; }
        }

        public double LeftRight
        {
            get { return leftRight; }
            set { leftRight = value; }
        }

        public double TopButtom
        {
            get { return topButtom; }
            set { topButtom = value; }
        }

        public double All
        {
            get { return all; }
            set { all = value; }
        }
        #endregion
    }
}
