using System;

namespace Snowdeed.FrameworkADO.Core.Attributes
{
    public class MaxLenghtAttribute : Attribute
    {
        private int maxLenght;

        public MaxLenghtAttribute(int MaxLenght)
        {
            this.maxLenght = MaxLenght;
        }

        public int MaxLenght
        {
            get { return maxLenght; }
        }
    }
}