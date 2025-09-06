using System;

public static class TimeUtils
{
    public static string CalculateTimeAgo(this DateTime endTime)
    {
        return CalculateTimeAgo(endTime - DateTime.UtcNow);
    }
    
    public static string CalculateTimeAgo(this DateTime startTime, DateTime endTime)
    {
        return CalculateTimeAgo(endTime - startTime);
    }
    
    public static string CalculateTimeAgo(this TimeSpan timeSpan)
    {
        string timeString;
        if (timeSpan.TotalDays >= 365)
        {
            int years = (int)(timeSpan.TotalDays / 365);
            if (years == 1)
            {
                timeString = "1 year ago";
            }
            else
            {
                timeString = years + " years ago";
            }
        }
        else if (timeSpan.TotalDays >= 7)
        {
            int weeks = (int)(timeSpan.TotalDays / 7);
            timeString = weeks + " weeks ago";
        }
        else if (timeSpan.TotalDays > 1)
        {
            timeString = (int)timeSpan.TotalDays + " days ago";
        }
        else if (timeSpan.TotalHours >= 1)
        {
            timeString = (int)timeSpan.TotalHours + " hours ago";
        }
        else if (timeSpan.TotalMinutes >= 1)
        {
            timeString = (int)timeSpan.TotalMinutes + " minutes ago";
        }
        else
        {
            timeString = "< 1 minute ago";
        }
        
        return timeString;
    }
    
    public static string CalculateTime(this TimeSpan timeSpan)
    {
        string timeString;

        if (timeSpan.TotalDays > 7)
        {
            int weeks = (int)(timeSpan.TotalDays / 7);
            timeString = weeks + " weeks";
            
            int remainingDays = (int)(timeSpan.TotalDays % 7);
            if (remainingDays > 0)
            {
                timeString += " " + remainingDays + " days";
            }
        }
        else if (timeSpan.TotalDays > 1)
        {
            timeString = (int)timeSpan.TotalDays + " days";
            int remainingHours = (int)(timeSpan.TotalHours % 24);
            if (remainingHours > 0)
            {
                timeString += " " + remainingHours + " hours";
            }
        }
        else if (timeSpan.TotalHours >= 1)
        {
            timeString = (int)timeSpan.TotalHours + " hours";
            int remainingMinutes = (int)(timeSpan.TotalMinutes % 60);
            if (remainingMinutes > 0)
            {
                timeString += " " + remainingMinutes + " minutes";
            }
        }
        else if (timeSpan.TotalMinutes >= 1)
        {
            timeString = (int)timeSpan.TotalMinutes + " minutes";
            int remainingSeconds = (int)(timeSpan.TotalSeconds % 60);
            if (remainingSeconds > 0)
            {
                timeString += " " + remainingSeconds + " seconds";
            }
        }
        else
        {
            timeString = (int)timeSpan.TotalSeconds + " seconds";
        }
        
        
        return timeString;
    }
    
    public static string CalculateShortTime(this TimeSpan timeSpan)
    {
        string timeString;

        if (timeSpan.TotalDays > 7)
        {
            int weeks = (int)(timeSpan.TotalDays / 7);
            timeString = weeks + "w";
            
            int remainingDays = (int)(timeSpan.TotalDays % 7);
            if (remainingDays > 0)
            {
                timeString += " " + remainingDays + "d";
            }
        }
        else if (timeSpan.TotalDays > 1)
        {
            timeString = (int)timeSpan.TotalDays + "s";
            int remainingHours = (int)(timeSpan.TotalHours % 24);
            if (remainingHours > 0)
            {
                timeString += " " + remainingHours + "h";
            }
        }
        else if (timeSpan.TotalHours >= 1)
        {
            timeString = (int)timeSpan.TotalHours + "h";
            int remainingMinutes = (int)(timeSpan.TotalMinutes % 60);
            if (remainingMinutes > 0)
            {
                timeString += " " + remainingMinutes + "m";
            }
        }
        else if (timeSpan.TotalMinutes >= 1)
        {
            timeString = (int)timeSpan.TotalMinutes + "m";
            int remainingSeconds = (int)(timeSpan.TotalSeconds % 60);
            if (remainingSeconds > 0)
            {
                timeString += " " + remainingSeconds + "s";
            }
        }
        else
        {
            timeString = (int)timeSpan.TotalSeconds + "s";
        }
        
        
        return timeString;
    }
}