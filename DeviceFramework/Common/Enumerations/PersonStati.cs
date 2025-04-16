namespace TIM.Devices.Framework.Common
{
    public enum PersonStati
    {
        NoStatus = 0,
        Member = 1,
        FormerMember = 2,
        DayGuest = 3,
        Prospective = 4,
        Employee = 5,
        FormerEmployee = 6,
        FormerStudent = 7,
        Student = 8,
        Other = 9,
        Deactivated = 10,
        Freelancer = 11,
        FormerFreelancer = 12,
        EShopper = 13
    }

    public static class PersonStatiHelper
    {
        public static bool IsEmployed(this int? personStatus)
        {
            if (personStatus.HasValue == false)
                return false;

            return
                personStatus == (int)PersonStati.Employee ||
                personStatus == (int)PersonStati.Freelancer ||
                personStatus == (int)PersonStati.Student;
        }
    }
}