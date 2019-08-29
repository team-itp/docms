namespace Docms.Infrastructure.WebDav.DataModel
{
    public interface IPropertyProvider
    {
        PropertyName[] GetPropertyNames(IResource resource);
        IProperty GetAllProperties(IResource resource);
        IProperty GetProperty(IResource resource, PropertyName propertyName);
        IProperty[] GetProperties(IResource resource, PropertyName[] propertyNames);
    }
}
