using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Validators
{
    [AttributeUsage(AttributeTargets.Property|AttributeTargets.Field, AllowMultiple = false)]
    sealed public class NumberValidatorAttribute : ValidationAttribute
    {

        public NumberValidatorAttribute() { }


        public override bool IsValid(object? value)
        {
            bool result = false;
            if (value != null) 
            {
                if ((int)value > 0)
                    result = true;
            }
            return result;
        }
    }
}
