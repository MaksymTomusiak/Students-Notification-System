using Domain.CourseChapters;
using Domain.CourseSubChapters;

namespace Tests.Data;

public static class CourseSubChaptersData
{
    public static CourseSubChapter MainSubChapter(CourseChapterId chapterId, uint number)
        => CourseSubChapter.New(
            CourseSubChapterId.New(),
            chapterId, 
            "Main test subchapter",
            "Main test subchapter content", 
            60, 
            number);
    
    public static CourseSubChapter SecondarySubChapter(CourseChapterId chapterId, uint number)
        => CourseSubChapter.New(
            CourseSubChapterId.New(),
            chapterId, 
            "Secondary test subchapter",
            "Secondary test subchapter content", 
            60, 
            number);

    public static CourseSubChapter New(CourseChapterId chapterId, string name, string content,
        uint estimatedLearningTimeMinutes, uint number) 
        => CourseSubChapter.New(
            CourseSubChapterId.New(),
            chapterId,
            name,
            content,
            estimatedLearningTimeMinutes,
            number
    );
}