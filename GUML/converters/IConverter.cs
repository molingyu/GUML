namespace GUML;

public enum ConverterType
{
    Token,
    Component,
    KeyName,
    Value
}

public interface IConverter
{
    public ConverterType ConverterType { get; }

    public object Convert(object source);
}
