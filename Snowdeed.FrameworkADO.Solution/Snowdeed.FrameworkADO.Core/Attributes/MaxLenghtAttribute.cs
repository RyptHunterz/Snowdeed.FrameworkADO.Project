using System;
namespace Snowdeed.FrameworkADO.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class MaxLenghtAttribute : Attribute
	{
        private readonly int maxLenght;

        public MaxLenghtAttribute(int MaxLenght = 0)
        {
            this.maxLenght = MaxLenght;
        }

        public int MaxLenght
        {
            get { return maxLenght; }
        }
    }
}