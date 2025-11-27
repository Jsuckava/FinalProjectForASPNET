using System;
using FinalAspNetProj.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FinalAspNetProj.Data;

public partial class AspnetfpDbContext : IdentityDbContext
{
    public AspnetfpDbContext(DbContextOptions<AspnetfpDbContext> options)
           : base(options)
    {
    }

    public virtual DbSet<DownloadableFile> DownloadableFiles { get; set; }
    public virtual DbSet<Survey> Surveys { get; set; }
    public virtual DbSet<SurveyAnalysis> SurveyAnalysis { get; set; }
    public virtual DbSet<SurveyResponse> SurveyResponses { get; set; }
    public DbSet<SurveyTemplate> SurveyTemplates { get; set; } = null!;
    public DbSet<Question> Questions { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<DownloadableFile>(entity =>
        {
            entity.HasKey(e => e.FileId).HasName("PK__Download__6F0F98BF04137FF4");
            entity.ToTable("DownloadableFile");
            entity.Property(e => e.FileName).HasMaxLength(255);
            entity.Property(e => e.FilePath).HasMaxLength(1000);
            entity.Property(e => e.FileType).HasMaxLength(50);
        });


        modelBuilder.Entity<Survey>(entity =>
        {
            entity.HasKey(e => e.SurveyId).HasName("PK__Survey__A5481F7D1DE96033");
            entity.ToTable("Survey");
            entity.Property(e => e.RespondentName).HasMaxLength(255);
        });

        modelBuilder.Entity<SurveyAnalysis>(entity =>
        {
            entity.HasKey(e => e.AnalysisId).HasName("PK__SurveyAn__5B789DC82519D4ED");
            entity.ToTable("SurveyAnalysis");
            entity.HasIndex(e => e.SurveyId, "UQ__SurveyAn__A5481F7C82E76A2A").IsUnique();
            entity.Property(e => e.FlawProbability).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.PercentageScore).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.ReUseProbability).HasColumnType("decimal(5, 2)");

            entity.HasOne(d => d.Survey).WithOne(p => p.SurveyAnalysis)
                .HasForeignKey<SurveyAnalysis>(d => d.SurveyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SurveyAnalysis_Survey");
        });

        modelBuilder.Entity<SurveyTemplate>(entity =>
        {
            entity.HasKey(e => e.SurveyTemplateID);
            entity.ToTable("SurveyTemplates");
            entity.Property(e => e.Title).IsRequired();
        });

        modelBuilder.Entity<Question>(entity =>
        {
            entity.HasKey(e => e.QuestionId);
            entity.ToTable("Questions");
            entity.Property(e => e.Text).IsRequired();

            entity.HasOne(d => d.SurveyTemplate)
                .WithMany(p => p.Questions)
                .HasForeignKey(d => d.SurveyTemplateID);
        });

        modelBuilder.Entity<SurveyResponse>(entity =>
        {
            entity.HasKey(e => e.ResponseId).HasName("PK__SurveyRe__1AAA646C25AEEA33");
            entity.ToTable("SurveyResponse");

            entity.HasOne(d => d.Question).WithMany(p => p.SurveyResponses)
                .HasForeignKey(d => d.QuestionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SurveyResponse_Question");

            entity.HasOne(d => d.Survey).WithMany(p => p.SurveyResponses)
                .HasForeignKey(d => d.SurveyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SurveyResponse_Survey");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}