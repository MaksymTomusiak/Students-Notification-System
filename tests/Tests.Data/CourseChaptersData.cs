using Domain.CourseChapters;
using Domain.Courses;

namespace Tests.Data;

public static class CourseChaptersData
{
    public static CourseChapter MainChapter(CourseId courseId, uint number)
        => CourseChapter.New(CourseChapterId.New(), courseId, "Main test chapter", 5, number);
    
    public static CourseChapter SecondaryChapter(CourseId courseId, uint number)
        => CourseChapter.New(CourseChapterId.New(), courseId, "Secondary test chapter", 10, number);
    
    public static CourseChapter New(CourseId courseId, string name, uint estimatedTime, uint number) 
        => CourseChapter.New(CourseChapterId.New(), courseId, name, estimatedTime, number);
}