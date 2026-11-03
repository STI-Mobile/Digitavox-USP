namespace Digitavox.Domain.Catalog;

/// <summary>Permanent identities retired in catalog version 1. They must never be reused.</summary>
public static class ContentAvailability
{
    public static bool IsRemoved(string courseId, string lessonId = null) =>
        courseId is "curso_abnt2_basico" or "curso_abnt2_intermediario" ||
        courseId == "musicas" && lessonId == "LICAO5";
}
