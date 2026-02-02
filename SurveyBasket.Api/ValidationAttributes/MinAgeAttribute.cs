namespace SurveyBasket.Api.ValidationAttributes
{

    // هستخدمهم علي ايه
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class MinAgeAttribute: ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            if (value is not null)
            {
                var date= (DateTime)value;
                if (DateTime.Today < date.AddYears(18)) // 2026<2018 ---->  false
                    return false;
            }
            return true;
        }
        // هروح بقي احطها فوق الاتريبيوت اللي انا عايز استخدمها فوقيه
        // [MinAge]

    }
}
