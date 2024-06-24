namespace Snowdeed.FrameworkADO.Core.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class ForeignKeyAttribute(string Table, string KeyName, string Schema = "dbo") : Attribute
{
    private readonly string schema = Schema;
    private readonly string table = Table;
    private readonly string keyName = KeyName;

    public string Schema
    {
        get { return schema; }
    }

    public string Table
    {
        get { return table; }
    }

    public string KeyName
    {
        get { return keyName; }
    }
}