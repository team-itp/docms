using System;
using System.Diagnostics;
using System.Linq;

namespace Docms.Client.App.Commands
{
    internal abstract class Command
    {
        protected const string PropertyPrefix = "Argument";

        public abstract string CommandName { get; }

        public virtual void PrintHelp()
        {
            Console.WriteLine($"{Process.GetCurrentProcess().MainModule.ModuleName} {CommandName}");
        }

        public abstract void RunCommand();

        public void SetArgument(string argumentName, string argumentValue)
        {
            var propertyInfo = GetType()
                .GetProperties()
                .FirstOrDefault(prop => prop.Name.Equals(PropertyPrefix + argumentName, StringComparison.InvariantCultureIgnoreCase));
            if (propertyInfo == null)
            {
                throw new DocmssyncException("Unexpected argument encountered.");
            }
            if (!(propertyInfo.PropertyType != argumentValue.GetType()))
            {
                propertyInfo.SetValue(this, argumentValue, null);
                return;
            }
            if (propertyInfo.PropertyType == true.GetType())
            {
                throw new DocmssyncException(string.Format("Found unexpected argument text: '{0}'", argumentValue));
            }
            throw new DocmssyncException("You must specify a parameter string after this argument.");
        }
    }
}
